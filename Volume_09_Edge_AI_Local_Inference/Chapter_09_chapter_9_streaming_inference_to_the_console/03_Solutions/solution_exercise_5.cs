
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

# Source File: solution_exercise_5.cs
# Description: Solution for Exercise 5
# ==========================================

using Microsoft.ML.OnnxRuntime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

public class RobustModelLoader
{
    // Custom Exception
    public class ModelNotFoundException : Exception
    {
        public ModelNotFoundException(string message) : base(message) { }
    }

    // 1. Async Initialization Pattern (AsyncLazy)
    // This ensures the model is loaded once and asynchronously.
    private readonly AsyncLazy<InferenceSession> _sessionLoader;

    public RobustModelLoader(string modelPath)
    {
        // 2. File Integrity Check (Immediate)
        if (!File.Exists(modelPath))
        {
            throw new ModelNotFoundException($"Model file not found at '{modelPath}'. Please download the model weights.");
        }

        if (new FileInfo(modelPath).Length == 0)
        {
            throw new ModelNotFoundException("Model file is empty.");
        }

        _sessionLoader = new AsyncLazy<InferenceSession>(async () =>
        {
            // 3. Async Loading
            // InferenceSession constructor is blocking, so offload to Task.Run
            return await Task.Run(() => 
            {
                var options = new SessionOptions();
                // Enable logging if needed
                // options.LogSeverityLevel = OrtLoggingLevel.ORT_LOGGING_LEVEL_WARNING;
                return new InferenceSession(modelPath, options);
            });
        });
    }

    public async Task<InferenceSession> GetSessionAsync()
    {
        return await _sessionLoader.ValueAsync();
    }

    public async IAsyncEnumerable<string> GenerateWithErrorHandlingAsync(string prompt)
    {
        InferenceSession session = null;
        try
        {
            session = await GetSessionAsync();
            
            // 4. Model Warm-up
            await WarmUpModelAsync(session);

            // Generation Loop (Mocked)
            var tokens = new List<int> { 1 };
            for (int i = 0; i < 5; i++)
            {
                // Simulate a potential error condition (e.g., 10% chance)
                if (new Random().Next(100) < 10) 
                    throw new OnnxRuntimeException("Runtime Error: GPU Memory Allocation Failed");

                // Simulate inference
                await Task.Delay(100);
                
                yield return " token";
            }
        }
        catch (OnnxRuntimeException ex)
        {
            // 5. Specific Error Handling
            // Yield an error token/message before breaking
            yield return $"\n[ERROR: {ex.Message}]";
            yield break; // Stop the stream
        }
        catch (ModelNotFoundException)
        {
            // This would usually be caught in the constructor or UI startup
            // But if propagated here:
            yield return "\n[CRITICAL: Model Missing]";
            yield break;
        }
        finally
        {
            // Cleanup if necessary (Session is usually kept alive, but disposed when app closes)
        }
    }

    private async Task WarmUpModelAsync(InferenceSession session)
    {
        // Run a dummy inference
        await Task.Run(() => 
        {
            var inputTensor = new DenseTensor<long>(new long[] { 1, 2 }, new[] { 1, 2 });
            var inputs = new List<NamedOnnxValue> { NamedOnnxValue.CreateFromTensor("input_ids", inputTensor) };
            // We ignore the output, we just want to trigger the JIT/Allocation
            using var outputs = session.Run(inputs);
        });
    }
}

// Helper class for AsyncLazy
public class AsyncLazy<T>
{
    private readonly Lazy<Task<T>> _lazy;
    public AsyncLazy(Func<T> factory) => _lazy = new Lazy<Task<T>>(() => Task.Run(factory));
    public AsyncLazy(Func<Task<T>> factory) => _lazy = new Lazy<Task<T>>(factory);
    public Task<T> ValueAsync() => _lazy.Value;
}
