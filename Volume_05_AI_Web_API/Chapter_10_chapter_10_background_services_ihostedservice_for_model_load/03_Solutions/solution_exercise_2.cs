
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

# Source File: solution_exercise_2.cs
# Description: Solution for Exercise 2
# ==========================================

using System;
using System.Threading;
using System.Threading.Tasks;

public class ModelService : IDisposable
{
    // Using Lazy<Task<T>> is the modern, idiomatic way to handle async lazy initialization.
    // It ensures thread safety and single execution without manual locking.
    private readonly Lazy<Task<bool>> _initializationTask;
    private bool _disposed;

    public ModelService()
    {
        _initializationTask = new Lazy<Task<bool>>(() => InitializeAsync(CancellationToken.None), 
            LazyThreadSafetyMode.ExecutionAndPublication);
    }

    // Called by BackgroundService or on first request
    public async Task<bool> InitializeAsync(CancellationToken cancellationToken)
    {
        // Simulate initialization logic
        try
        {
            await Task.Delay(2000, cancellationToken); // Simulate work
            // Check if resources are valid (e.g., GPU memory check)
            return true; 
        }
        catch (OperationCanceledException)
        {
            // Propagate cancellation to all waiting callers
            throw; 
        }
    }

    // Called by Controllers
    public async Task GetModelAsync(CancellationToken cancellationToken)
    {
        // Await the lazy task. If initialization is in progress, this waits.
        // If already initialized, this returns immediately.
        // If initialization failed, the exception is thrown here.
        await _initializationTask.Value;
        
        // Simulate usage
        if (cancellationToken.IsCancellationRequested)
            throw new OperationCanceledException(cancellationToken);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        // Cleanup unmanaged resources here
    }
}

// Unit Test Scenario (Conceptual)
public class ModelServiceTests
{
    public async Task Concurrent_Access_Initializes_Once()
    {
        var service = new ModelService();
        int initializationCount = 0;

        // Mock the InitializeAsync to increment a counter
        // In a real test, use a Mock framework or a wrapper.
        
        var tasks = new List<Task>();
        for (int i = 0; i < 10; i++)
        {
            tasks.Add(Task.Run(() => service.GetModelAsync(CancellationToken.None)));
        }

        await Task.WhenAll(tasks);

        // Verify that the initialization logic ran exactly once
        // (This would be asserted against the mock counter)
    }
}
