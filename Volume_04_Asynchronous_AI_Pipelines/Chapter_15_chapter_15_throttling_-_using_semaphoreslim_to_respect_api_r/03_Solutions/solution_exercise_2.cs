
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

// Source File: solution_exercise_2.cs
// Description: Solution for Exercise 2
// ==========================================

using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

// 1. Create the RateLimitHandler
public class RateLimitHandler : DelegatingHandler
{
    private readonly SemaphoreSlim _semaphore;

    public RateLimitHandler(int maxConcurrency) : base()
    {
        _semaphore = new SemaphoreSlim(maxConcurrency, maxConcurrency);
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // Wait asynchronously for a slot in the semaphore
        await _semaphore.WaitAsync(cancellationToken);

        try
        {
            // Proceed with the request to the inner handler (or server)
            return await base.SendAsync(request, cancellationToken);
        }
        finally
        {
            // Release the slot
            _semaphore.Release();
        }
    }
}

// 2. Modified LlmClient (Simplified, no internal semaphore logic)
public class LlmClient
{
    private readonly HttpClient _httpClient;

    public LlmClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string> GetCompletionAsync(string prompt)
    {
        // Simulate the API call using the provided HttpClient
        // In a real scenario, this would be an HTTP POST/GET
        // For this exercise, we simulate latency within the client logic 
        // (though in a real DelegatingHandler, the latency is part of the HTTP call).
        
        // We add a small artificial delay here to simulate processing time 
        // because we aren't actually hitting a network endpoint.
        await Task.Delay(250); 
        return $"Processed: {prompt}";
    }
}

public class Program
{
    public static async Task Main()
    {
        // 3. Configure the HttpClient with the RateLimitHandler
        var handler = new RateLimitHandler(maxConcurrency: 5);
        
        // Create the HttpClient with the custom handler
        var httpClient = new HttpClient(handler);
        
        // Instantiate the client
        var llmClient = new LlmClient(httpClient);

        var tasks = new System.Collections.Generic.List<Task<string>>();
        var stopwatch = Stopwatch.StartNew();

        Console.WriteLine("Starting 20 concurrent requests via DelegatingHandler...");

        // 4. Spawn 20 concurrent tasks
        for (int i = 0; i < 20; i++)
        {
            int index = i;
            tasks.Add(llmClient.GetCompletionAsync($"Prompt {index}"));
        }

        await Task.WhenAll(tasks);
        stopwatch.Stop();

        Console.WriteLine($"Total execution time: {stopwatch.ElapsedMilliseconds}ms");
        
        // 5. Discussion (in comments):
        // This architectural change decouples the rate limiting concern from the LlmClient.
        // We can now swap LlmClient for a different provider's client (e.g., 'LegacyAiClient') 
        // and attach the same RateLimitHandler to its HttpClient pipeline.
        // This adheres to the Single Responsibility Principle and allows middleware-style logic.
    }
}
