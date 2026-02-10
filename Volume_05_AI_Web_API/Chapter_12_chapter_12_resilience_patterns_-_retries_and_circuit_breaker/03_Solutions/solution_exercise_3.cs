
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

using Polly;
using Polly.CircuitBreaker;
using System.Net;

namespace CircuitBreakerExercise
{
    public class CircuitBreakerService
    {
        private readonly HttpClient _httpClient;
        private readonly AsyncCircuitBreakerPolicy<HttpResponseMessage> _circuitBreakerPolicy;

        public CircuitBreakerService(HttpClient httpClient)
        {
            _httpClient = httpClient;

            // Requirement 2: Configure Circuit Breaker
            _circuitBreakerPolicy = Policy<HttpResponseMessage>
                .Handle<HttpRequestException>()
                .OrResult(r => r.StatusCode >= HttpStatusCode.InternalServerError)
                .CircuitBreakerAsync(
                    exceptionsAllowedBeforeBreaking: 3,
                    durationOfBreak: TimeSpan.FromSeconds(15),
                    onBreak: (exception, breakDelay) =>
                    {
                        Console.WriteLine($"[Circuit Open] Service broken for {breakDelay.TotalSeconds}s due to: {exception.Exception?.Message}");
                    },
                    onReset: () => Console.WriteLine("[Circuit Closed] Service recovered."),
                    onHalfOpen: () => Console.WriteLine("[Circuit Half-Open] Testing service recovery...")
                );
        }

        public async Task<string> GetResponseAsync()
        {
            try
            {
                // Requirement 3: Handle BrokenCircuitException logic
                // The policy automatically throws BrokenCircuitException if the circuit is open.
                var response = await _circuitBreakerPolicy.ExecuteAsync(async () =>
                {
                    Console.WriteLine("Attempting HTTP call...");
                    // Simulating a failing endpoint
                    return await _httpClient.GetAsync("https://httpstat.us/503");
                });

                return response.IsSuccessStatusCode ? "Success" : $"Failed: {response.StatusCode}";
            }
            catch (BrokenCircuitException)
            {
                // Requirement 3: Fail fast without making HTTP call
                return "Circuit Open: Service is currently unavailable. Returning cached/fallback response.";
            }
        }
    }

    public class Program
    {
        public static async Task Main(string[] args)
        {
            var service = new CircuitBreakerService(new HttpClient());

            // Requirement 4: Simulation Loop
            
            // 1. First 3 calls fail -> Circuit Opens
            Console.WriteLine("--- Phase 1: Triggering Circuit Break ---");
            for (int i = 1; i <= 3; i++)
            {
                Console.WriteLine($"Call {i}:");
                var result = await service.GetResponseAsync();
                Console.WriteLine($"    Result: {result}\n");
                await Task.Delay(500); // Small delay between calls
            }

            // 2. Subsequent calls should fail immediately
            Console.WriteLine("--- Phase 2: Circuit Open (Fast Fail) ---");
            for (int i = 4; i <= 6; i++)
            {
                Console.WriteLine($"Call {i}:");
                var result = await service.GetResponseAsync();
                Console.WriteLine($"    Result: {result}\n");
            }

            // 3. Wait for recovery (16 seconds > 15s break duration)
            Console.WriteLine("--- Phase 3: Waiting for Reset (16s)... ---");
            await Task.Delay(16000);

            // 4. Attempt recovery
            Console.WriteLine("--- Phase 4: Recovery Attempt ---");
            Console.WriteLine("Call 7:");
            // Note: In a real scenario, the endpoint needs to return success here to close the circuit.
            // Since we are using a static failing endpoint, it will likely fail again or transition to Half-Open.
            // For demonstration, we assume the downstream service is fixed.
            var result7 = await service.GetResponseAsync();
            Console.WriteLine($"    Result: {result7}");
        }
    }
}
