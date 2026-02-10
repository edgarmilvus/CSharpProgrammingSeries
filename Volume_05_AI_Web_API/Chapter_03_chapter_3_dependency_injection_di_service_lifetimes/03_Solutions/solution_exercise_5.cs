
/*
#
# These sources are part of the "C# Programming Series" by Edgar Milvus, 
# for additional info, new volumes, link to stores:
# https://github.com/edgarmilvus/CSharpProgrammingSeries
#
# MIT License
# Copyright (c) 2026 Edgar Milvus
# Permission is hereby granted, free of charge, to any person obtaining a copy
# of this software and associated documentation files (the "Software"), to deal
# in the Software without restriction, including without limitation the rights
# to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
# copies of the Software, and to permit persons to whom the Software is
# furnished to do so, subject to the following conditions:
#
# The above copyright notice and this permission notice shall be included in all
# copies or substantial portions of the Software.
#
# THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
# IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
# FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
# AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
# LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
# OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
# SOFTWARE.
*/

// Source File: solution_exercise_5.cs
// Description: Solution for Exercise 5
// ==========================================

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

// ---------------------------------------------------------
// 1. MODEL REGISTRY & RESOURCE MANAGEMENT
// ---------------------------------------------------------
public class OnnxModelSession : IDisposable
{
    private readonly SemaphoreSlim _usageLock = new SemaphoreSlim(1, 1);
    private int _activeRequests = 0;
    private bool _disposed = false;

    // Underlying engine (simplified)
    public object Engine { get; private set; } 

    public OnnxModelSession(Stream modelStream)
    {
        // Load model into Engine (e.g., ONNX Runtime InferenceSession)
        // Engine = new InferenceSession(modelStream);
    }

    // Increment counter when prediction starts
    public void Acquire() 
    {
        if (_disposed) throw new ObjectDisposedException(nameof(OnnxModelSession));
        Interlocked.Increment(ref _activeRequests);
    }

    // Decrement counter when prediction finishes
    public void Release()
    {
        Interlocked.Decrement(ref _activeRequests);
    }

    // ZOMBIE DETECTOR: Safe Disposal
    public void Dispose()
    {
        _usageLock.Wait();
        try
        {
            if (_disposed) return;
            
            // Wait for active requests to finish
            while (_activeRequests > 0)
            {
                Thread.Sleep(10); // Busy wait (or use Task.Delay in async context)
            }

            // Engine?.Dispose(); // Release native memory
            _disposed = true;
        }
        finally
        {
            _usageLock.Release();
        }
    }
}

public class ModelRegistry : IDisposable
{
    // Thread-safe dictionary
    private readonly ConcurrentDictionary<string, OnnxModelSession> _sessions = new();
    // ReaderWriterLockSlim allows multiple readers but exclusive writers
    private readonly ReaderWriterLockSlim _rwLock = new ReaderWriterLockSlim();

    public OnnxModelSession GetSession(string modelId)
    {
        if (_sessions.TryGetValue(modelId, out var session))
        {
            return session;
        }
        return null;
    }

    public void ReloadModel(string modelId, Stream modelStream)
    {
        _rwLock.EnterWriteLock(); // Exclusive lock for reloading
        try
        {
            // Dispose old session if exists
            if (_sessions.TryRemove(modelId, out var oldSession))
            {
                oldSession.Dispose(); // Safe disposal via reference counting
            }

            // Load new session
            var newSession = new OnnxModelSession(modelStream);
            _sessions[modelId] = newSession;
        }
        finally
        {
            _rwLock.ExitWriteLock();
        }
    }

    public void Dispose()
    {
        foreach (var session in _sessions.Values)
        {
            session.Dispose();
        }
    }
}

// ---------------------------------------------------------
// 4. DI INTEGRATION & BACKGROUND CLEANUP
// ---------------------------------------------------------
public class ModelAccessor
{
    private readonly ModelRegistry _registry;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ModelAccessor(ModelRegistry registry, IHttpContextAccessor httpContextAccessor)
    {
        _registry = registry;
        _httpContextAccessor = httpContextAccessor;
    }

    public OnnxModelSession GetCurrentSession()
    {
        // Retrieve model version from header (e.g., "X-Model-Version: v2")
        var modelId = _httpContextAccessor.HttpContext?.Request.Headers["X-Model-Version"].ToString() ?? "default";
        return _registry.GetSession(modelId);
    }
}

public class AppShutdownService : IHostedService
{
    private readonly IHostApplicationLifetime _lifetime;
    private readonly ModelRegistry _registry;

    public AppShutdownService(IHostApplicationLifetime lifetime, ModelRegistry registry)
    {
        _lifetime = lifetime;
        _registry = registry;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _lifetime.ApplicationStopping.Register(OnShutdown);
        return Task.CompletedTask;
    }

    private void OnShutdown()
    {
        // Clean up all loaded models when app stops
        _registry.Dispose();
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}

// ---------------------------------------------------------
// INTERACTIVE CHALLENGE: REFERENCE COUNTING SIMULATION
// ---------------------------------------------------------
/*
 * Pseudo-code for usage in Controller:
 * 
 * public IActionResult Predict([FromBody] ModelInput input)
 * {
 *     var session = _modelAccessor.GetCurrentSession();
 *     if (session == null) return NotFound();
 * 
 *     try 
 *     {
 *         session.Acquire(); // Increment counter
 *         // Perform prediction using session.Engine
 *         var result = ... 
 *         return Ok(result);
 *     }
 *     finally 
 *     {
 *         session.Release(); // Decrement counter
 *     }
 * }
 * 
 * BACKGROUND RELOAD LOGIC:
 * The Admin API calls _registry.ReloadModel("v2", stream).
 * The ReloadModel method takes an EXCLUSIVE write lock.
 * 1. It waits for the ReaderWriterLockSlim.
 * 2. It removes the old session.
 * 3. It calls Dispose() on the old session.
 * 4. Dispose() checks _activeRequests. If > 0, it waits (or times out).
 * 5. Once 0, it releases native memory.
 * 6. New session is added.
 * 7. Write lock is released.
 */

// Dummy classes for compilation
public class HttpContext { public Request Request { get; } = new Request(); }
public class Request { public IHeaderDictionary Headers { get; } = new HeaderDictionary(); }
public interface IHttpContextAccessor { HttpContext HttpContext { get; } }
public class HeaderDictionary : System.Collections.Generic.Dictionary<string, Microsoft.Extensions.Primitives.StringValues> { }
