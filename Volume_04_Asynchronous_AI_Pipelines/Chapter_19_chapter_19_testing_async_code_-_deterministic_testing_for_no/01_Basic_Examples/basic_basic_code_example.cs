
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

// Source File: basic_basic_code_example.cs
// Description: Basic Code Example
// ==========================================

using System.Collections.Concurrent;
using System.Diagnostics;
using Microsoft.Extensions.AI; // Requires NuGet: Microsoft.Extensions.AI
using Moq;                    // Requires NuGet: Moq

// 1. THE PROBLEM CONTEXT
// Imagine a service that summarizes news articles. It calls an external AI model.
// The test must verify that:
//   a) Successful summaries are collected.
//   b) Failed articles are tracked as errors.
//   c) The entire process respects a timeout (doesn't hang).
// We cannot rely on the real AI (too slow/non-deterministic), so we mock it.

public class NewsSummarizer
{
    private readonly IChatClient _aiClient;
    private readonly ILogger<NewsSummarizer> _logger;

    public NewsSummarizer(IChatClient aiClient, ILogger<NewsSummarizer> logger)
    {
        _aiClient = aiClient;
        _logger = logger;
    }

    // 2. THE METHOD UNDER TEST
    // Uses Parallel.ForEachAsync to process articles concurrently.
    // Uses a CancellationTokenSource to enforce a timeout.
    public async Task<SummaryResult> SummarizeBatchAsync(
        IEnumerable<string> articles, 
        TimeSpan timeout)
    {
        var results = new ConcurrentBag<string>();
        var errors = new ConcurrentBag<string>();
        
        // Create a timeout token. If the operation takes longer than 'timeout', it cancels.
        using var cts = new CancellationTokenSource(timeout);

        try
        {
            // Modern C# Parallelism: Process items asynchronously in parallel.
            await Parallel.ForEachAsync(articles, new ParallelOptions 
            { 
                MaxDegreeOfParallelism = 3, // Limit concurrency to avoid overwhelming the mock/LLM
                CancellationToken = cts.Token 
            }, async (article, token) =>
            {
                try
                {
                    // Call the AI (which will be mocked in our test)
                    var response = await _aiClient.GetResponseAsync(
                        $"Summarize this news: {article}", 
                        cancellationToken: token);
                    
                    var summary = response.Text;
                    results.Add(summary);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogWarning("Processing timed out for an article.");
                    errors.Add("Timeout");
                    throw; // Re-throw to stop the parallel operation
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error summarizing article.");
                    errors.Add(ex.Message);
                }
            });
        }
        catch (OperationCanceledException)
        {
            _logger.LogError("Batch processing was cancelled due to timeout.");
        }

        return new SummaryResult(results.ToList(), errors.ToList());
    }
}

// Simple DTO for the result
public record SummaryResult(List<string> Summaries, List<string> Errors);

// Dummy Logger implementation for the example to be runnable
public class ConsoleLogger<T> : ILogger<T>
{
    public IDisposable BeginScope<TState>(TState state) => null!;
    public bool IsEnabled(LogLevel logLevel) => true;
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
    {
        // In a real test, we'd capture logs. Here we just print.
        Console.WriteLine($"[{logLevel}] {formatter(state, exception)}");
    }
}

// 3. THE TEST
public class DeterministicTest
{
    public static async Task Main()
    {
        // --- SETUP ---
        // Create the mock for the IChatClient
        var mockClient = new Mock<IChatClient>();
        
        // Define the behavior: "Deterministic Mocking"
        // We configure the mock to return specific values based on specific inputs.
        // This removes randomness.
        mockClient
            .Setup(c => c.GetResponseAsync(
                It.Is<string>(s => s.Contains("Article A")), 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AIResponse(new ChatMessage(ChatRole.Assistant, "Summary A")));

        mockClient
            .Setup(c => c.GetResponseAsync(
                It.Is<string>(s => s.Contains("Article B")), 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AIResponse(new ChatMessage(ChatRole.Assistant, "Summary B")));

        // Setup a "slow" article to test our timeout logic
        mockClient
            .Setup(c => c.GetResponseAsync(
                It.Is<string>(s => s.Contains("Article C")), 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(async () => 
            {
                await Task.Delay(2000); // Simulate 2 seconds of latency
                return new AIResponse(new ChatMessage(ChatRole.Assistant, "Summary C"));
            });

        // Setup a "failing" article
        mockClient
            .Setup(c => c.GetResponseAsync(
                It.Is<string>(s => s.Contains("Article D")), 
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("Network unstable"));

        var logger = new ConsoleLogger<NewsSummarizer>();
        var service = new NewsSummarizer(mockClient.Object, logger);

        // --- EXECUTION ---
        var articles = new[] { "Article A", "Article B", "Article C", "Article D" };
        
        // We set a strict timeout of 1 second.
        // Article A and B should pass instantly.
        // Article C takes 2 seconds (should timeout).
        // Article D throws immediately (should be caught and logged).
        var result = await service.SummarizeBatchAsync(articles, TimeSpan.FromSeconds(1));

        // --- ASSERTIONS ---
        Console.WriteLine("\n--- TEST RESULTS ---");
        Console.WriteLine($"Successes: {string.Join(", ", result.Summaries)}");
        Console.WriteLine($"Errors: {string.Join(", ", result.Errors)}");

        // Verify the mock was called correctly
        mockClient.Verify(
            x => x.GetResponseAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), 
            Times.Exactly(4)); // Called for all 4 articles
    }
}
