
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

using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;
using System;
using System.Net.Http;
using System.Threading.Tasks;

// ---------------------------------------------------------
// 1. DIAGNOSIS & TYPED CLIENT IMPLEMENTATION
// ---------------------------------------------------------

// The Typed Client class. It is typically registered as Transient or Scoped.
// The HttpClient is injected by the factory.
public class ExternalAiService
{
    private readonly HttpClient _httpClient;

    public ExternalAiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string> GetAiResponseAsync(string prompt)
    {
        // Simulate calling external API
        var response = await _httpClient.GetAsync($"https://api.external-ai.com/predict?input={prompt}");
        return await response.Content.ReadAsStringAsync();
    }
}

// ---------------------------------------------------------
// 2. CONFIGURATION & RESILIENCE
// ---------------------------------------------------------
public static class HttpClientConfiguration
{
    public static void Configure(IServiceCollection services)
    {
        // Register the Typed Client with Resilience Policies
        services.AddHttpClient<ExternalAiService>(client =>
        {
            // Basic configuration
            client.BaseAddress = new Uri("https://api.external-ai.com/");
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        })
        .AddPolicyHandler(GetRetryPolicy())
        .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
        {
            // 3. LIFECYCLE ANALYSIS:
            // PooledConnectionLifetime controls how long a connection stays in the pool.
            // Setting this enables DNS rotation. If null (default), connections are kept open indefinitely,
            // caching DNS forever. Setting a time limit forces the pool to refresh connections periodically.
            PooledConnectionLifetime = TimeSpan.FromMinutes(1) 
        });
    }

    // Exponential Backoff Retry Policy (Polly)
    private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError() // Handles 5xx, 408
            .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.TooManyRequests) // Handles 429
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), // 2s, 4s, 8s
                onRetry: (outcome, timespan, retryCount, context) =>
                {
                    // Log retry attempt
                    Console.WriteLine($"Retry {retryCount} after {timespan.TotalSeconds}s due to {outcome.Exception?.Message}");
                });
    }
}

// ---------------------------------------------------------
// LIFECYCLE ANALYSIS (Comment Block)
// ---------------------------------------------------------
/*
 * LIFECYCLE ANALYSIS: HttpClient vs Typed Client
 * 
 * 1. HttpClient:
 *    - Managed by IHttpClientFactory.
 *    - The factory maintains a pool of HttpMessageHandler instances.
 *    - Handlers are reused to preserve connection pools (avoid socket exhaustion).
 *    - Handlers are rotated based on PooledConnectionLifetime to update DNS.
 *    - The HttpClient instance itself is lightweight and effectively Transient.
 * 
 * 2. Typed Client (ExternalAiService):
 *    - Registered as Transient (or Scoped).
 *    - It receives a fresh HttpClient instance from the factory on creation.
 *    - Because the heavy resources (TCP connections) are managed by the Handler pool 
 *      and not the HttpClient, creating many Typed Clients is cheap.
 *    - It is SAFE to register the Typed Client as Scoped. 
 *      Even though the HttpClient is "Transient" (new instance per resolution), 
 *      the underlying connections are shared efficiently via the handler pool.
 *      Scoped registration ensures the service lives for the request, providing 
 *      consistent behavior without the overhead of creating a new connection for every injection.
 */

// Dummy Program.cs context
public class Program
{
    public static void Main(string[] args)
    {
        var services = new ServiceCollection();
        HttpClientConfiguration.Configure(services);
    }
}
