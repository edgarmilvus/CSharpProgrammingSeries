
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

public class ModelLoader : IDisposable
{
    private bool _disposed = false;
    private FileStream _resourceHandle; // Simulated unmanaged resource

    public async Task<bool> LoadModelAsync(CancellationToken ct)
    {
        // Simulate allocating a resource
        _resourceHandle = new FileStream("dummy_model.bin", FileMode.Create, FileAccess.Write, FileShare.None, 4096, true);
        Console.WriteLine("Resource allocated.");

        try
        {
            // Simulate loading delay (5 seconds) with cancellation support
            // We use a loop to check cancellation more frequently than the full 5 seconds
            for (int i = 0; i < 50; i++)
            {
                ct.ThrowIfCancellationRequested();
                await Task.Delay(100, ct);
            }
            
            Console.WriteLine("Model loaded successfully.");
            return true;
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Loading cancelled.");
            throw; // Re-throw to let the caller know it was cancelled
        }
        finally
        {
            // CRITICAL: Ensure cleanup runs even if cancelled
            // In a real scenario, we might check if loading completed successfully before disposing
            // but for cancellation, we usually want to clean up partial resources.
            Dispose();
        }
    }

    public void Dispose()
    {
        if (_disposed) return;

        Console.WriteLine("Disposing resources...");
        
        // Clean up managed resources
        _resourceHandle?.Dispose();
        _resourceHandle = null;

        _disposed = true;
        Console.WriteLine("Resource cleanup completed.");
        GC.SuppressFinalize(this);
    }
}

public class Program
{
    public static async Task Main(string[] args)
    {
        using (var loader = new ModelLoader())
        {
            // Timeout after 2 seconds (shorter than the 5 second load)
            using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2)))
            {
                try
                {
                    Console.WriteLine("Starting model load (timeout in 2s)...");
                    await loader.LoadModelAsync(cts.Token);
                }
                catch (TaskCanceledException)
                {
                    Console.WriteLine("Main caught TaskCanceledException.");
                }
                catch (OperationCanceledException)
                {
                    Console.WriteLine("Main caught OperationCanceledException.");
                }
            }
        }
        
        // Verify Dispose was called via the 'using' block or the finally block in LoadModelAsync
        Console.WriteLine("Program finished.");
    }
}
