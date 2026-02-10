
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

using System.Diagnostics;
using Polly;
using Polly.CircuitBreaker;

public class ResilientAgentClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ResilientAgentClient> _logger;
    
    // Circuit Breaker State
    private int _failureCount = 0;
    private DateTime? _lastFailureTime;
    private CircuitState _state = CircuitState.Closed;
    private const int FailureThreshold = 5;
    private readonly TimeSpan _durationOfBreak = TimeSpan.FromSeconds(30);

    public ResilientAgentClient(HttpClient httpClient, ILogger<ResilientAgentClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<string> CommunicateWithCoder(string message)
    {
        // Check Circuit Breaker State manually (Interactive Challenge requirement)
        if (_state == CircuitState.Open)
        {
            if (DateTime.UtcNow - _lastFailureTime > _durationOfBreak)
            {
                _state = CircuitState.HalfOpen; // Transition to Half-Open to test if service recovered
                _logger.LogInformation("Circuit Breaker entering Half-Open state.");
            }
            else
            {
                throw new CircuitBreakerException("Circuit is Open. Request blocked.");
            }
        }

        try
        {
            // Simulate Service Mesh Headers
            var request = new HttpRequestMessage(HttpMethod.Post, "http://coder-service/process");
            request.Headers.Add("x-request-id", Guid.NewGuid().ToString());
            request.Headers.Add("b3-trace-id", Activity.Current?.TraceId.ToString());
            request.Content = new StringContent($"{{\"message\": \"{message}\"}}");

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            // Success: Reset Circuit Breaker logic
            if (_state == CircuitState.HalfOpen)
            {
                _state = CircuitState.Closed;
                _failureCount = 0;
                _logger.LogInformation("Circuit Breaker reset to Closed state.");
            }

            return await response.Content.ReadAsStringAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Request failed to Coder service.");
            HandleFailure();
            throw;
        }
    }

    private void HandleFailure()
    {
        _failureCount++;
        _lastFailureTime = DateTime.UtcNow;

        if (_state == CircuitState.HalfOpen || _failureCount >= FailureThreshold)
        {
            _state = CircuitState.Open;
            _logger.LogWarning($"Circuit Breaker OPENED. Failures: {_failureCount}");
        }
    }
}

// Custom Exception for Circuit Breaker
public class CircuitBreakerException : Exception
{
    public CircuitBreakerException(string message) : base(message) { }
}
