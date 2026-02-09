
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

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

public class ApiSimulator
{
    private int _callCount = 0;

    // Simulates an API call that fails after 3 attempts
    public async Task<string> FetchChunkAsync(CancellationToken ct)
    {
        await Task.Delay(100, ct); // Simulate network latency
        _callCount++;
        
        if (_callCount > 3)
        {
            throw new HttpRequestException("API limit reached");
        }
        
        return $"DataChunk_{_callCount}";
    }
}

public class DataStreamer
{
    private readonly ApiSimulator _api;

    public DataStreamer(ApiSimulator api)
    {
        _api = api;
    }

    // Implementation of GetStreamedDataAsync with internal exception handling
    public async IAsyncEnumerable<string> GetStreamedDataAsync(CancellationToken ct)
    {
        int itemsYielded = 0;
        while (itemsYielded < 5)
        {
            try
            {
                // Attempt to fetch data
                string data = await _api.FetchChunkAsync(ct);
                yield return data;
                itemsYielded++;
            }
            catch (HttpRequestException ex)
            {
                // Internal Handling: Catch the exception to prevent it from bubbling up 
                // and crashing the stream immediately.
                Console.WriteLine($"Iterator caught API error: {ex.Message}");
                
                // Option: Yield a default value or error indicator before breaking
                yield return "Error_Fallback_Data";
                
                // Break the loop to terminate the stream gracefully
                break; 
            }
        }
    }
}

public class Consumer
{
    public async Task ProcessStreamAsync(DataStreamer streamer, CancellationToken ct)
    {
        Console.WriteLine("Starting stream consumption...");
        
        // Iterate using await foreach
        await foreach (var item in streamer.GetStreamedDataAsync(ct))
        {
            // If the iterator handles the exception internally, 
            // the consumer sees the fallback data and continues normally.
            Console.WriteLine($"Received: {item}");
        }

        Console.WriteLine("Stream finished.");
    }
}
