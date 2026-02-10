
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

// Source File: solution_exercise_5.cs
// Description: Solution for Exercise 5
// ==========================================

using System;
using System.Threading.Tasks;
using System.Diagnostics;

namespace ResilientFetching
{
    // 1. Custom Exception
    public class DocumentFetchError : Exception
    {
        public DocumentFetchError(string message) : base(message) { }
    }

    // 2. Circuit Breaker State
    public enum CircuitState { Closed, Open, HalfOpen }

    public class CircuitBreaker
    {
        private int _failureCount = 0;
        private readonly int _failureThreshold;
        private readonly TimeSpan _resetTimeout;
        private DateTime _lastFailureTime = DateTime.MinValue;
        private CircuitState _state = CircuitState.Closed;
        private readonly object _lock = new object();

        public CircuitBreaker(int failureThreshold, double resetTimeoutSeconds)
        {
            _failureThreshold = failureThreshold;
            _resetTimeout = TimeSpan.FromSeconds(resetTimeoutSeconds);
        }

        public async Task<T> ExecuteAsync<T>(Func<Task<T>> func)
        {
            lock (_lock)
            {
                if (_state == CircuitState.Open)
                {
                    if (DateTime.UtcNow - _lastFailureTime > _resetTimeout)
                    {
                        _state = CircuitState.HalfOpen;
                    }
                    else
                    {
                        throw new DocumentFetchError("Circuit Breaker is OPEN. Request blocked.");
                    }
                }
            }

            try
            {
                var result = await func();
                
                // Success logic
                lock (_lock)
                {
                    _failureCount = 0;
                    if (_state == CircuitState.HalfOpen) _state = CircuitState.Closed;
                }
                return result;
            }
            catch (Exception ex)
            {
                // Failure logic
                lock (_lock)
                {
                    _failureCount++;
                    _lastFailureTime = DateTime.UtcNow;

                    if (_failureCount >= _failureThreshold)
                    {
                        _state = CircuitState.Open;
                        Console.WriteLine($"Circuit Breaker OPENED due to repeated failures.");
                    }
                }
                
                // Re-throw for the retry logic to catch
                if (ex is DocumentFetchError) throw;
                throw new DocumentFetchError($"Fetch failed: {ex.Message}");
            }
        }
    }

    public static class ResilientFetcher
    {
        // 3. Retry Decorator Logic
        public static async Task<T> RetryAsync<T>(
            Func<Task<T>> action, 
            int maxAttempts = 3, 
            double backoffFactor = 2)
        {
            int attempt = 0;
            Exception? lastException = null;

            while (attempt < maxAttempts)
            {
                try
                {
                    return await action();
                }
                catch (DocumentFetchError ex)
                {
                    lastException = ex;
                    attempt++;
                    
                    if (attempt >= maxAttempts) break;

                    // Exponential Backoff: 1s, 2s, 4s...
                    double delaySeconds = backoffFactor * Math.Pow(2, attempt - 1);
                    Console.WriteLine($"Attempt {attempt} failed. Retrying in {delaySeconds}s...");
                    await Task.Delay(TimeSpan.FromSeconds(delaySeconds));
                }
            }

            throw new DocumentFetchError($"Failed after {maxAttempts} attempts. Last error: {lastException?.Message}");
        }
    }

    public class Program
    {
        private static readonly Random _random = new Random();
        private static readonly CircuitBreaker _breaker = new CircuitBreaker(failureThreshold: 3, resetTimeoutSeconds: 5);

        // Simulated fetch that randomly fails
        public static async Task<string> FetchDocumentAsync(string url)
        {
            // Wrap the logic in the Circuit Breaker
            return await _breaker.ExecuteAsync(async () =>
            {
                await Task.Delay(200); // Network latency
                
                // Simulate 70% failure rate to test logic
                if (_random.NextDouble() < 0.7) 
                {
                    throw new DocumentFetchError($"Network timeout fetching {url}");
                }
                
                return $"Success: {url}";
            });
        }

        public static async Task Main()
        {
            Console.WriteLine("Starting resilient fetch test...");

            // Define the operation to retry
            Func<Task<string>> operation = () => FetchDocumentAsync("https://example.com/doc/1");

            try
            {
                // 4. Apply Retry Logic
                string result = await ResilientFetcher.RetryAsync(operation, maxAttempts: 3, backoffFactor: 2);
                Console.WriteLine($"Result: {result}");
            }
            catch (DocumentFetchError e)
            {
                Console.WriteLine($"Final Error: {e.Message}");
            }
        }
    }
}
