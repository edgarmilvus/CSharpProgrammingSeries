
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

// Source File: solution_exercise_4.cs
// Description: Solution for Exercise 4
// ==========================================

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Polly; // Requires Polly NuGet package
using Polly.Retry;
using System.Net.Http;

public class ResilientChatService
{
    private readonly IChatCompletionService _innerService;
    private readonly AsyncRetryPolicy _retryPolicy;

    public ResilientChatService(IChatCompletionService innerService)
    {
        _innerService = innerService;

        // Define the retry policy with exponential backoff
        _retryPolicy = Policy
            .Handle<HttpOperationException>() // Specific SK exception for HTTP errors
            .Or<TimeoutException>()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt => 
                {
                    var delay = Math.Pow(2, retryAttempt - 1); // 2^(attempt-1)
                    TimeSpan timeSpan = TimeSpan.FromSeconds(delay);
                    Console.WriteLine($"[Retry Policy] Waiting {timeSpan.TotalSeconds}s before retry #{retryAttempt}...");
                    return timeSpan;
                },
                onRetry: (exception, timeSpan, retryCount, context) =>
                {
                    Console.WriteLine($"[Retry Policy] Retry {retryCount} triggered by: {exception.Message}");
                });
    }

    public async Task<string> GetResponseWithRetryAsync(string prompt)
    {
        // Wrap the execution in the policy
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            Console.WriteLine($"Executing request: {prompt}");
            var response = await _innerService.GetChatMessageContentAsync(prompt);
            
            if (string.IsNullOrEmpty(response.Content))
                throw new InvalidOperationException("Empty response received"); // Simulate failure if needed
            
            return response.Content;
        });
    }
}

// Mock Service for Interactive Challenge
public class MockFailingChatService : IChatCompletionService
{
    private int _callCount = 0;

    public async Task<ChatMessageContent> GetChatMessageContentAsync(string prompt, PromptExecutionSettings executionSettings = null, Kernel kernel = null, CancellationToken cancellationToken = default)
    {
        _callCount++;
        Console.WriteLine($"[Mock Service] Call #{_callCount} received.");

        // Fail first 2 times, succeed on 3rd
        if (_callCount < 3)
        {
            await Task.Delay(100); // Simulate network latency
            throw new HttpOperationException(System.Net.HttpStatusCode.TooManyRequests, "Rate limit exceeded");
        }

        await Task.Delay(100);
        return new ChatMessageContent(AuthorRole.Assistant, "This is the successful response.");
    }

    // Interface implementation details (required but not the focus)
    public IReadOnlyDictionary<string, object> Attributes => throw new NotImplementedException();
    public Task<IReadOnlyList<ChatMessageContent>> GetChatMessageContentsAsync(string prompt, PromptExecutionSettings executionSettings = null, Kernel kernel = null, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    public IAsyncEnumerable<StreamingChatMessageContent> GetStreamingChatMessageContentsAsync(string prompt, PromptExecutionSettings executionSettings = null, Kernel kernel = null, CancellationToken cancellationToken = default) => throw new NotImplementedException();
}

class Program
{
    static async Task Main(string[] args)
    {
        // 1. Setup Mock Service
        var mockService = new MockFailingChatService();

        // 2. Wrap with Resilience
        var resilientService = new ResilientChatService(mockService);

        Console.WriteLine("--- Testing Resilience Strategy ---");
        
        try
        {
            // 3. Execute
            string result = await resilientService.GetResponseWithRetryAsync("Test Prompt");
            Console.WriteLine($"\nSuccess! Final Result: {result}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Request failed after retries: {ex.Message}");
        }
    }
}
