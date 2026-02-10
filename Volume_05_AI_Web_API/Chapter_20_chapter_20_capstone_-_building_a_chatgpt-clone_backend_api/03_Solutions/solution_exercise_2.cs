
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

using Polly;
using Polly.CircuitBreaker;
using Polly.Extensions.Http;
using Microsoft.Extensions.Logging;

// 1. Policy Definition (Extension method for clean registration)
public static class ResiliencePolicies
{
    public static IAsyncPolicy<HttpResponseMessage> GetRetryAndCircuitBreakerPolicy(ILogger logger)
    {
        // Retry with exponential backoff and jitter
        var retryPolicy = HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)) 
                                                 + TimeSpan.FromMilliseconds(new Random().Next(0, 1000)),
                onRetry: (outcome, timespan, retryCount, context) =>
                {
                    logger.LogWarning("Retry {RetryCount} after {Delay}ms due to {StatusCode}", 
                        retryCount, timespan.TotalMilliseconds, outcome.Result?.StatusCode);
                });

        // Circuit Breaker: Open after 5 failures, hold for 30s
        var circuitBreakerPolicy = HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: 5,
                durationOfBreak: TimeSpan.FromSeconds(30),
                onBreak: (ex, breakDelay) => logger.LogError("Circuit opened for {Delay}s", breakDelay.TotalSeconds),
                onReset: () => logger.LogInformation("Circuit closed. Resuming normal operations."),
                onHalfOpen: () => logger.LogInformation("Circuit half-open. Testing next request.")
            );

        // Timeout: 60 seconds per request
        var timeoutPolicy = Policy.TimeoutAsync<HttpResponseMessage>(60, TimeoutStrategy.Pessimistic);

        // Wrap policies: Timeout -> (Retry + CircuitBreaker)
        return Policy.WrapAsync(timeoutPolicy, retryPolicy, circuitBreakerPolicy);
    }
}

// 2. HTTP Client Integration (In Program.cs or ServiceExtensions)
public static class ServiceExtensions
{
    public static IServiceCollection AddResilientAiService(this IServiceCollection services, IConfiguration config)
    {
        // Define the fallback logic
        var fallbackResponse = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
        {
            Content = new StringContent("{\"error\": \"I'm currently overloaded, please try again later.\"}")
        };

        var fallbackPolicy = Policy<HttpResponseMessage>
            .Handle<Exception>()
            .OrResult(_ => false) // Handle any result that isn't success
            .FallbackAsync(fallbackResponse, onFallbackAsync: (outcome, context) => 
            {
                // Log the fallback trigger
                return Task.CompletedTask;
            });

        services.AddHttpClient("AIClient", client =>
        {
            client.BaseAddress = new Uri(config["AiService:Endpoint"]);
            client.Timeout = TimeSpan.FromSeconds(65); // Slightly higher than policy timeout
        })
        .AddPolicyHandler((serviceProvider, request) =>
        {
            var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
            var resiliencePolicy = ResiliencePolicies.GetRetryAndCircuitBreakerPolicy(logger);
            
            // Order: Fallback wraps the resilience strategy
            return Policy.WrapAsync(fallbackPolicy, resiliencePolicy);
        });

        return services;
    }
}
