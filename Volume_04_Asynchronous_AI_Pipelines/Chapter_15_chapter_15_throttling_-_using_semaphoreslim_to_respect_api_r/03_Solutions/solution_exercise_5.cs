
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

using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

public class LeakyBucketHandler : DelegatingHandler
{
    private readonly SemaphoreSlim _bucket;
    private readonly int _capacity;
    private readonly Timer _replenishTimer;

    public LeakyBucketHandler(int burstCapacity, TimeSpan leakRate) : base()
    {
        _capacity = burstCapacity;
        _bucket = new SemaphoreSlim(burstCapacity, burstCapacity);

        // Timer to replenish the bucket every 'leakRate' interval
        _replenishTimer = new Timer(ReplenishBucket, null, leakRate, leakRate);
    }

    private void ReplenishBucket(object? state)
    {
        // Try to release a slot if the bucket is not full
        // Release() increments the count. If count is already at capacity, 
        // Release() throws an exception, so we check CurrentCount.
        if (_bucket.CurrentCount < _capacity)
        {
            _bucket.Release();
        }
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // Wait for a slot in the bucket
        await _bucket.WaitAsync(cancellationToken);

        try
        {
            var response = await base.SendAsync(request, cancellationToken);

            // Handle 429 (Too Many Requests) specifically
            if (response.StatusCode == HttpStatusCode.TooManyRequests)
            {
                // In a real scenario, parse Retry-After header here.
                // For this simulation, we will force a delay to simulate backoff.
                // We also artificially "drain" the bucket by waiting longer 
                // effectively slowing down the leak rate temporarily.
                
                Console.WriteLine("Received 429: Backing off...");
                await Task.Delay(2000, cancellationToken); 
            }

            return response;
        }
        catch
        {
            // If an exception occurs (network error, etc.), we might want to 
            // replenish immediately or handle logic. 
            // For Leaky Bucket, usually, we don't release on error unless 
            // it's a specific retry logic, but standard practice is to let the slot close.
            throw;
        }
    }

    // Cleanup
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _replenishTimer.Dispose();
            _bucket.Dispose();
        }
        base.Dispose(disposing);
    }
}

public class Program
{
    public static async Task Main()
    {
        // Configuration: Burst of 10, replenish 1 slot every 500ms
        var handler = new LeakyBucketHandler(10, TimeSpan.FromMilliseconds(500));
        var client = new HttpClient(handler);
        
        // Mock a request sender
        var tasks = new System.Collections.Generic.List<Task>();
        var stopwatch = Stopwatch.StartNew();

        Console.WriteLine($"Starting burst of 20 requests at {DateTime.Now:HH:mm:ss.fff}");

        // 1. Burst Phase (First 10 should go through immediately)
        // 2. Leaky Phase (Next 10 should be delayed by the timer)
        
        for (int i = 0; i < 20; i++)
        {
            int id = i;
            tasks.Add(Task.Run(async () => 
            {
                try 
                {
                    // Simulate sending request
                    // Note: We are mocking the response logic inside the handler for this demo
                    // In real usage, this would be client.GetAsync(...)
                    var msg = new HttpRequestMessage(HttpMethod.Get, "http://example.com");
                    await client.SendAsync(msg);
                    Console.WriteLine($"Request {id} completed at {stopwatch.ElapsedMilliseconds}ms");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Request {id} failed: {ex.Message}");
                }
            }));
        }

        await Task.WhenAll(tasks);
        stopwatch.Stop();
        
        Console.WriteLine($"Total time: {stopwatch.ElapsedMilliseconds}ms");
        // Expected: First 10 immediate. Next 10 spaced 500ms apart.
        // Total time roughly > 4.5 seconds (9 intervals * 500ms).
    }
}
