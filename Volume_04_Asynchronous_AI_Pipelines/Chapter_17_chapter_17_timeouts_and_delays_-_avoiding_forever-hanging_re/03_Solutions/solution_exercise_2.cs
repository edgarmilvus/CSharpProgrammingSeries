
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
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

public class RetryPolicy
{
    private readonly int _maxRetries;
    private readonly ILogger<RetryPolicy> _logger; // Assuming DI/Logger abstraction
    private readonly Random _random = new();

    public RetryPolicy(int maxRetries, ILogger<RetryPolicy> logger)
    {
        _maxRetries = maxRetries;
        _logger = logger;
    }

    public async Task<T> ExecuteWithRetryAsync<T>(Func<Task<T>> operation)
    {
        int attempt = 0;

        while (true)
        {
            try
            {
                return await operation();
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.TooManyRequests)
            {
                attempt++;
                if (attempt > _maxRetries)
                {
                    _logger.LogError("Max retries exceeded. Throwing exception.");
                    throw;
                }

                TimeSpan delay = CalculateDelay(ex.ResponseHeaders, attempt);
                
                _logger.LogWarning($"Rate limit hit. Retrying in {delay.TotalSeconds:F2}s. Attempt {attempt}/{_maxRetries}");
                
                await Task.Delay(delay);
            }
            // Add other catch blocks for transient errors if needed
        }
    }

    private TimeSpan CalculateDelay(HttpResponseHeaders headers, int attempt)
    {
        // 1. Check for Retry-After header (Priority)
        if (headers.TryGetValues("Retry-After", out var values))
        {
            if (int.TryParse(values.First(), out int retryAfterSeconds))
            {
                return TimeSpan.FromSeconds(retryAfterSeconds);
            }
        }

        // 2. Exponential Backoff with Jitter
        // Base delay (e.g., 1 second)
        double baseDelayMs = 1000; 
        double exponent = Math.Pow(2, attempt - 1); // 2^0, 2^1, 2^2...
        double backoffMs = baseDelayMs * exponent;
        
        // Jitter: Add random 0-1000ms
        double jitterMs = _random.Next(0, 1000);
        
        return TimeSpan.FromMilliseconds(backoffMs + jitterMs);
    }
}
