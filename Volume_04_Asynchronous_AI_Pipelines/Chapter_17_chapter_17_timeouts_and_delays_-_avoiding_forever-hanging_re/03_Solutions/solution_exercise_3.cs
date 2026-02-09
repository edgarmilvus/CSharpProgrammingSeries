
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

# Source File: solution_exercise_3.cs
# Description: Solution for Exercise 3
# ==========================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

public class StreamStalledException : Exception
{
    public StreamStalledException(string message) : base(message) { }
}

public class StreamingClient
{
    private readonly HttpClient _httpClient;

    public StreamingClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async IAsyncEnumerable<string> GetStreamAsync(string url, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken externalToken)
    {
        using var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, externalToken);
        using var stream = await response.Content.ReadAsStreamAsync();
        using var reader = new StreamReader(stream);

        // Linked token source bridges external cancellation and internal logic
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(externalToken);
        
        while (!externalToken.IsCancellationRequested)
        {
            // Create a specific delay task for this iteration
            var delayTask = Task.Delay(TimeSpan.FromSeconds(10), linkedCts.Token);
            var readTask = reader.ReadLineAsync();

            var completedTask = await Task.WhenAny(readTask, delayTask);

            if (completedTask == delayTask)
            {
                // Timeout occurred
                throw new StreamStalledException("No data received within the 10-second heartbeat window.");
            }

            // ReadLineAsync completed
            var line = await readTask;

            if (line == null) 
                yield break; // Stream ended naturally

            yield return line;
            
            // Reset loop: The delay task is discarded, a new one is created in the next iteration
        }
    }
}
