
#
# These sources are part of the "C# Programming Series" by Edgar Milvus, 
# you can find it on stores: 
# 
# https://www.amazon.com/dp/B0GKJ3NYL6 or https://tinyurl.com/CSharpProgrammingBooks or 
# https://leanpub.com/u/edgarmilvus (quantity discounts)
# 
# New books info: https://linktr.ee/edgarmilvus 
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

# Source File: solution_exercise_4.cs
# Description: Solution for Exercise 4
# ==========================================

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR; // If using SignalR, but here we use SSE via Response
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

// Service to manage active connections
public interface IActiveConnectionManager
{
    bool TryAddConnection(string key, CancellationTokenSource cts);
    void RemoveConnection(string key);
    void CancelAllForApiKey(string apiKeyId);
}

public class ActiveConnectionManager : IActiveConnectionManager
{
    // Key: "user_{userId}" or "apikey_{keyId}"
    private readonly ConcurrentDictionary<string, CancellationTokenSource> _activeStreams = new();

    public bool TryAddConnection(string key, CancellationTokenSource cts)
    {
        // If a connection already exists for this user/key, reject it
        if (_activeStreams.ContainsKey(key)) return false;
        
        _activeStreams.TryAdd(key, cts);
        return true;
    }

    public void RemoveConnection(string key)
    {
        _activeStreams.TryRemove(key, out _);
    }

    public void CancelAllForApiKey(string apiKeyId)
    {
        var keyPrefix = $"apikey_{apiKeyId}";
        foreach (var kvp in _activeStreams.Where(x => x.Key.StartsWith(keyPrefix)))
        {
            kvp.Value.Cancel();
        }
    }
}

[ApiController]
[Route("api/chat")]
[Authorize] // Applies Hybrid Auth from Exercise 2
public class ChatStreamController : ControllerBase
{
    private readonly IActiveConnectionManager _connectionManager;
    private readonly ITokenBucket _rateLimiter; // Injected or created per request

    public ChatStreamController(IActiveConnectionManager connectionManager)
    {
        _connectionManager = connectionManager;
    }

    [HttpGet("stream")]
    public async Task StreamChat([FromQuery] string prompt)
    {
        // 1. Identify Client Key (User or API Key)
        string clientKey = User.Identity?.IsAuthenticated == true 
            ? (User.FindFirst("auth_method")?.Value == "api_key" 
                ? $"apikey_{User.Identity.Name}" 
                : $"user_{User.Identity.NameIdentifier}")
            : throw new UnauthorizedAccessException();

        // 2. Connection Count Limiter (Concurrent Stream Limit)
        var cts = CancellationTokenSource.CreateLinkedTokenSource(HttpContext.RequestAborted);
        
        if (!_connectionManager.TryAddConnection(clientKey, cts))
        {
            HttpContext.Response.StatusCode = 429;
            await HttpContext.Response.WriteAsync("Concurrent stream limit exceeded.");
            return;
        }

        try
        {
            // Setup SSE headers
            HttpContext.Response.Headers.ContentType = "text/event-stream";
            HttpContext.Response.Headers.CacheControl = "no-cache";
            
            // Simulate AI Stream
            await foreach (var chunk in GenerateAiStreamAsync(prompt, cts.Token))
            {
                // Check if client disconnected
                if (HttpContext.RequestAborted.IsCancellationRequested) break;

                await HttpContext.Response.WriteAsync($"data: {chunk}\n\n");
                await HttpContext.Response.Body.FlushAsync();
            }
        }
        catch (OperationCanceledException)
        {
            // Client disconnected or Kill Switch triggered
        }
        finally
        {
            _connectionManager.RemoveConnection(clientKey);
        }
    }

    private async IAsyncEnumerable<string> GenerateAiStreamAsync(string prompt, [EnumeratorCancellation] CancellationToken token)
    {
        // Simulate processing time
        for (int i = 0; i < 10; i++)
        {
            token.ThrowIfCancellationRequested();
            await Task.Delay(1000, token);
            yield return $"Chunk {i} for prompt: {prompt}";
        }
    }
}

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")] // Only Admins can access
public class AdminController : ControllerBase
{
    private readonly IActiveConnectionManager _connectionManager;

    public AdminController(IActiveConnectionManager connectionManager)
    {
        _connectionManager = connectionManager;
    }

    [HttpPost("revoke-key/{keyId}")]
    public IActionResult RevokeKey(string keyId)
    {
        _connectionManager.CancelAllForApiKey(keyId);
        return Ok($"Terminated active streams for Key ID: {keyId}");
    }
}
