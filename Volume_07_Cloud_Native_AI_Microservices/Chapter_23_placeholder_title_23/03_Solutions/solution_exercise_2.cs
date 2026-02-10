
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
using System.Collections.Concurrent;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

public interface IServiceMeshClient
{
    Task<TResponse> SendAsync<TRequest, TResponse>(
        string destinationService, 
        TRequest payload, 
        CancellationToken ct,
        Func<TRequest, string>? routingStrategy = null);
}

public class ServiceMeshClient : IServiceMeshClient, IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly ConcurrentDictionary<string, CircuitBreakerState> _circuitBreakers;
    private readonly Random _random = new Random();
    
    // Configuration constants
    private const int FailureThreshold = 5;
    private const int CooldownSeconds = 30;

    public ServiceMeshClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _circuitBreakers = new ConcurrentDictionary<string, CircuitBreakerState>();
    }

    public async Task<TResponse> SendAsync<TRequest, TResponse>(
        string destinationService, 
        TRequest payload, 
        CancellationToken ct,
        Func<TRequest, string>? routingStrategy = null)
    {
        // Content-Aware Routing: Determine the actual target URL based on payload.
        string targetEndpoint = routingStrategy != null 
            ? routingStrategy(payload) 
            : destinationService;

        // Get or create circuit breaker state for this specific endpoint.
        var cbState = _circuitBreakers.GetOrAdd(targetEndpoint, _ => new CircuitBreakerState());

        // Check Circuit Breaker State
        if (cbState.IsOpen)
        {
            if (DateTime.UtcNow - cbState.LastFailureTime < TimeSpan.FromSeconds(CooldownSeconds))
            {
                throw new HttpRequestException($"Circuit Breaker OPEN for {targetEndpoint}. Cooldown active.");
            }
            // Cooldown expired, allow test request (Half-Open logic simulated here by closing it)
            cbState.IsOpen = false;
            cbState.FailureCount = 0;
        }

        // Retry Policy (Exponential Backoff Simulation)
        int retryCount = 3;
        for (int attempt = 0; attempt < retryCount; attempt++)
        {
            try
            {
                // Inject Trace Context
                using var requestMessage = new HttpRequestMessage(HttpMethod.Post, targetEndpoint);
                requestMessage.Headers.Add("traceparent", $"00-{Guid.NewGuid()}-{Guid.NewGuid()}-01");
                
                if (payload != null)
                {
                    var content = JsonSerializer.Serialize(payload);
                    requestMessage.Content = new StringContent(content, System.Text.Encoding.UTF8, "application/json");
                }

                // Simulate Downstream Failure (30% chance)
                if (_random.Next(100) < 30)
                {
                    throw new HttpRequestException("Simulated downstream failure");
                }

                // Execute Call
                var response = await _httpClient.SendAsync(requestMessage, ct);
                response.EnsureSuccessStatusCode();

                // Success: Reset Circuit Breaker
                cbState.FailureCount = 0;
                
                var responseBody = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<TResponse>(responseBody)!;
            }
            catch (Exception ex) when (ex is HttpRequestException || ex is TaskCanceledException)
            {
                // Handle Failure
                Interlocked.Increment(ref cbState.FailureCount);
                cbState.LastFailureTime = DateTime.UtcNow;

                // Open Circuit if threshold reached
                if (cbState.FailureCount >= FailureThreshold)
                {
                    cbState.IsOpen = true;
                    throw new HttpRequestException($"Circuit Breaker OPENED for {targetEndpoint} after {FailureThreshold} failures.", ex);
                }

                // Exponential Backoff
                var delay = TimeSpan.FromSeconds(Math.Pow(2, attempt));
                await Task.Delay(delay, ct);
            }
        }

        throw new HttpRequestException($"Failed to send request to {targetEndpoint} after {retryCount} retries.");
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
    }

    private class CircuitBreakerState
    {
        public int FailureCount;
        public bool IsOpen;
        public DateTime LastFailureTime;
    }
}
