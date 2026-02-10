
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

// Source File: solution_exercise_3.cs
// Description: Solution for Exercise 3
// ==========================================

using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using Polly.Fallback;
using Polly.Retry;

public class OrchestratorService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<OrchestratorService> _logger;
    private readonly AsyncRetryPolicy _retryPolicy;
    private readonly AsyncCircuitBreakerPolicy _circuitBreakerPolicy;
    private readonly AsyncFallbackPolicy<string> _fallbackPolicy;

    public OrchestratorService(HttpClient httpClient, ILogger<OrchestratorService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;

        // 2. Retry Policy: Exponential Backoff
        _retryPolicy = Policy
            .Handle<HttpRequestException>()
            .OrResult<HttpResponseMessage>(r => r.StatusCode >= System.Net.HttpStatusCode.InternalServerError)
            .WaitAndRetryAsync(
                retryCount: 3, 
                sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)), // 2s, 4s, 8s
                onRetry: (outcome, timespan, retryCount, context) =>
                {
                    _logger.LogWarning(
                        "Retry {RetryCount} after {Delay}ms due to: {Exception}", 
                        retryCount, timespan.TotalMilliseconds, outcome.Exception?.Message ?? outcome.Result.StatusCode.ToString());
                });

        // 3. Circuit Breaker Policy: 5 failures open circuit for 30s
        _circuitBreakerPolicy = Policy
            .Handle<HttpRequestException>()
            .OrResult<HttpResponseMessage>(r => r.StatusCode >= System.Net.HttpStatusCode.InternalServerError)
            .CircuitBreakerAsync(
                exceptionsAllowedBeforeBreaking: 5, 
                durationOfBreak: TimeSpan.FromSeconds(30),
                onBreak: (ex, breakDelay) => _logger.LogError("Circuit opened for {Delay}s. Exception: {Ex}", breakDelay.TotalSeconds, ex.Message),
                onReset: () => _logger.LogInformation("Circuit closed. Normal operation resumed.")
            );

        // 4. Fallback Policy: Return empty context if KnowledgeRetriever fails
        _fallbackPolicy = Policy<string>
            .Handle<HttpRequestException>()
            .OrResult<HttpResponseMessage>(r => r.StatusCode >= System.Net.HttpStatusCode.InternalServerError)
            .FallbackAsync(
                fallbackValue: string.Empty, // The fallback result
                onFallback: (outcome, context) => _logger.LogWarning("KnowledgeRetriever failed. Using empty context. Reason: {Reason}", outcome.Exception?.Message)
            );
    }

    public async Task<string> CoordinateWorkflow(string prompt)
    {
        // 5. Structured Logging with Correlation ID
        // In a real app, this ID would come from HttpContext or Activity.Current.TraceId
        var correlationId = Guid.NewGuid().ToString();
        using var scope = _logger.BeginScope(new Dictionary<string, object> { ["CorrelationId"] = correlationId });

        _logger.LogInformation("Starting workflow for prompt: {Prompt}", prompt);

        try
        {
            // 1. Call PlanningAgent (Retry + Circuit Breaker)
            // Note: We wrap the specific call that needs resilience.
            var planResult = await _retryPolicy.ExecuteAndCaptureAsync(async () => 
                await _circuitBreakerPolicy.ExecuteAsync(async () =>
                {
                    _logger.LogInformation("Calling PlanningAgent...");
                    return await _httpClient.GetAsync($"http://planning-agent/api/plan?prompt={prompt}");
                })
            );

            if (planResult.Outcome == OutcomeType.Failure)
            {
                _logger.LogError("PlanningAgent failed permanently after retries/circuit break.");
                return "Workflow failed: Planning Agent unavailable.";
            }

            var plan = await planResult.Result.Content.ReadAsStringAsync();
            _logger.LogInformation("Got plan: {Plan}", plan);

            // 2. Call KnowledgeRetriever (Retry + Fallback)
            // We apply retry, but if it still fails, the fallback catches it.
            var context = await _retryPolicy.ExecuteAsync(async () =>
            {
                _logger.LogInformation("Calling KnowledgeRetriever...");
                var response = await _httpClient.GetAsync($"http://knowledge-retriever/api/retrieve?query={plan}");
                // Ensure success for the retry policy to consider it a success
                response.EnsureSuccessStatusCode(); 
                return await response.Content.ReadAsStringAsync();
            });

            // Apply fallback manually or wrap the execution. 
            // Since Fallback is defined for string, we can execute the retry logic inside the fallback wrapper:
            context = await _fallbackPolicy.ExecuteAsync(async () => 
                await _retryPolicy.ExecuteAsync(async () =>
                {
                    _logger.LogInformation("Calling KnowledgeRetriever...");
                    var response = await _httpClient.GetAsync($"http://knowledge-retriever/api/retrieve?query={plan}");
                    response.EnsureSuccessStatusCode();
                    return await response.Content.ReadAsStringAsync();
                })
            );

            _logger.LogInformation("Got context: {Context}", context);

            // 3. Call ExecutionAgent (Retry only)
            var executionResult = await _retryPolicy.ExecuteAsync(async () =>
            {
                _logger.LogInformation("Calling ExecutionAgent...");
                return await _httpClient.GetAsync($"http://execution-agent/api/execute?plan={plan}&context={context}");
            });

            var result = await executionResult.Content.ReadAsStringAsync();
            _logger.LogInformation("Final result: {Result}", result);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Workflow coordination failed.");
            throw;
        }
    }
}
