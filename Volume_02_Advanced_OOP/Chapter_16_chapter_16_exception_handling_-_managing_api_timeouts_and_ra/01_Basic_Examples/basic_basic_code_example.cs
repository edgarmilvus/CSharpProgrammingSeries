
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

# Source File: basic_basic_code_example.cs
# Description: Basic Code Example
# ==========================================

using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Threading;

public class AiDefinitionService
{
    private readonly HttpClient _httpClient;
    private const int MaxRetries = 3;
    private const int InitialDelayMs = 1000; // 1 second initial delay

    // Constructor injecting the HttpClient dependency
    public AiDefinitionService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    // Main method to fetch a definition with retry logic
    public async Task<string?> GetDefinitionWithRetryAsync(string term)
    {
        // We define the retry logic as a delegate (lambda expression)
        // This encapsulates the retry strategy, making it reusable.
        Func<Task<string?>> retryLogic = async () =>
        {
            for (int attempt = 1; attempt <= MaxRetries; attempt++)
            {
                try
                {
                    // Simulate an API call (In reality, this would be _httpClient.GetAsync(...))
                    // We use a helper method to simulate network conditions for this example.
                    return await SimulateApiCall(term);
                }
                catch (HttpRequestException ex) when (ex.Message.Contains("429"))
                {
                    // Specific handling for Rate Limits (HTTP 429 Too Many Requests)
                    Console.WriteLine($"[Attempt {attempt}] Rate limited. Waiting...");
                    
                    // Calculate exponential backoff delay
                    int delay = InitialDelayMs * (int)Math.Pow(2, attempt - 1);
                    await Task.Delay(delay);
                }
                catch (HttpRequestException ex)
                {
                    // General network errors (timeouts, 500 errors)
                    Console.WriteLine($"[Attempt {attempt}] Network error: {ex.Message}");
                    
                    int delay = InitialDelayMs * (int)Math.Pow(2, attempt - 1);
                    await Task.Delay(delay);
                }
                catch (Exception ex)
                {
                    // Catch-all for unexpected errors
                    Console.WriteLine($"[Attempt {attempt}] Unexpected error: {ex.Message}");
                    // We might not retry on unexpected errors, or we might. 
                    // For this example, we will break the loop.
                    break;
                }
            }
            
            // If we exhaust retries, return null or throw a custom exception
            Console.WriteLine("Failed to retrieve definition after all retries.");
            return null;
        };

        // Execute the defined retry logic
        return await retryLogic();
    }

    // Helper method to simulate the API behavior
    private async Task<string?> SimulateApiCall(string term)
    {
        Random rand = new Random();
        int outcome = rand.Next(1, 10); // Random number between 1 and 9

        await Task.Delay(500); // Simulate network latency

        if (outcome <= 3) 
        {
            // Simulate a Timeout or Connection Error
            throw new HttpRequestException("Connection timed out.");
        }
        else if (outcome <= 5) 
        {
            // Simulate Rate Limit (429)
            throw new HttpRequestException("429 Too Many Requests");
        }
        else if (outcome == 9)
        {
            throw new InvalidOperationException("Critical internal error.");
        }
        
        // Success
        return $"Definition of {term}: A complex system of neurons.";
    }
}

// Example Usage
public class Program
{
    public static async Task Main()
    {
        using var client = new HttpClient();
        var service = new AiDefinitionService(client);

        Console.WriteLine("Fetching definition for 'Neural Network'...");
        string? definition = await service.GetDefinitionWithRetryAsync("Neural Network");

        if (definition != null)
        {
            Console.WriteLine($"SUCCESS: {definition}");
        }
        else
        {
            Console.WriteLine("FAILED: Could not retrieve definition.");
        }
    }
}
