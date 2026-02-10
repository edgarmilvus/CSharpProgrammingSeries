
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

using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using System.Diagnostics;

namespace ResilienceObservability
{
    // 1. Metrics Collector
    public class ResilienceMetrics
    {
        public int TotalRequests { get; private set; }
        public int TotalRetries { get; private set; }
        public int CircuitBreakerOpens { get; private set; }
        public Stopwatch OpenStateStopwatch { get; private set; } = new Stopwatch();
        public long TotalOpenStateDurationMs => OpenStateStopwatch.ElapsedMilliseconds;
        
        // Latency tracking
        private List<long> _successfulLatencies = new List<long>();
        public double AverageLatency => _successfulLatencies.Count > 0 ? _successfulLatencies.Average() : 0;

        public void RecordRequest() => TotalRequests++;
        public void RecordRetry() => TotalRetries++;
        public void RecordSuccess(long latencyMs) => _successfulLatencies.Add(latencyMs);
        
        public void CircuitOpened()
        {
            CircuitBreakerOpens++;
            OpenStateStopwatch.Restart(); // Start tracking open duration
        }

        public void CircuitClosed()
        {
            OpenStateStopwatch.Stop();
        }

        public string GenerateReport()
        {
            return $@"
--- Resilience Metrics Report ---
Total Requests:          {TotalRequests}
Total Retries Performed: {TotalRetries}
Circuit Breaker Opens:   {CircuitBreakerOpens}
Total Open State Time:   {TotalOpenStateDurationMs} ms
Average Success Latency: {AverageLatency:F2} ms
---------------------------------";
        }
    }

    public class ObservableResilienceService
    {
        private readonly AsyncRetryPolicy _retryPolicy;
        private readonly AsyncCircuitBreakerPolicy _circuitBreakerPolicy;
        private readonly ResilienceMetrics _metrics;

        public ObservableResilienceService(ResilienceMetrics metrics)
        {
            _metrics = metrics;

            // 2. Circuit Breaker with Observability
            _circuitBreakerPolicy = Policy
                .Handle<HttpRequestException>()
                .CircuitBreakerAsync(
                    exceptionsAllowedBeforeBreaking: 3,
                    durationOfBreak: TimeSpan.FromSeconds(5), // Shortened for simulation
                    onBreak: (ex, duration) => _metrics.CircuitOpened(),
                    onReset: () => _metrics.CircuitClosed()
                );

            // 3. Retry Policy with Observability
            _retryPolicy = Policy
                .Handle<HttpRequestException>()
                .WaitAndRetryAsync(
                    retryCount: 3,
                    sleepDurationProvider: attempt => TimeSpan.FromSeconds(1),
                    onRetry: (ex, delay) => _metrics.RecordRetry()
                );
        }

        public async Task<string> ExecuteRequestAsync(Func<Task<string>> action)
        {
            _metrics.RecordRequest();
            var sw = Stopwatch.StartNew();
            
            try
            {
                // Wrap retry inside circuit breaker (or vice versa depending on design)
                // Here: CircuitBreaker wraps Retry to count final failures after retries
                var result = await _circuitBreakerPolicy.ExecuteAsync(async () =>
                {
                    return await _retryPolicy.ExecuteAsync(async () =>
                    {
                        return await action();
                    });
                });
                
                sw.Stop();
                _metrics.RecordSuccess(sw.ElapsedMilliseconds);
                return result;
            }
            catch (BrokenCircuitException)
            {
                // Circuit open, no execution happened
                throw;
            }
            catch
            {
                // Request failed after all retries
                throw;
            }
        }
    }

    public class Program
    {
        public static async Task Main(string[] args)
        {
            var metrics = new ResilienceMetrics();
            var service = new ObservableResilienceService(metrics);

            // 4. Simulation Loop
            Console.WriteLine("Starting Observability Simulation...");
            
            // Helper to simulate a failing request
            int attemptCount = 0;
            Func<Task<string>> failingAction = async () =>
            {
                attemptCount++;
                // Fail first 3 requests to trigger circuit breaker
                if (attemptCount < 5) // First 3 fail -> Open. 4th is blocked by CB.
                {
                    throw new HttpRequestException("Simulated API Failure");
                }
                // Simulate recovery
                await Task.Delay(100); // Simulate network latency
                return "Success";
            };

            try
            {
                // Phase 1: Fail fast to open circuit
                for (int i = 0; i < 5; i++)
                {
                    await service.ExecuteRequestAsync(failingAction);
                }
            }
            catch { /* Ignore expected exceptions for simulation */ }

            // Phase 2: Wait for circuit to reset (5s + buffer)
            Console.WriteLine("Waiting for circuit reset...");
            await Task.Delay(6000);

            // Phase 3: Successful request
            try 
            {
                await service.ExecuteRequestAsync(failingAction);
            }
            catch { }

            // 5. Generate Report
            Console.WriteLine(metrics.GenerateReport());
        }
    }
}
