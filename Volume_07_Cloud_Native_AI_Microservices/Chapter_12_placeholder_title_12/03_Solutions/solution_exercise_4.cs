
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

// Source File: solution_exercise_4.cs
// Description: Solution for Exercise 4
// ==========================================

// C# Client Console App
using Grpc.Net.Client;
using Polly;
using Polly.CircuitBreaker;
using Triton.InferenceServer.Grpc; // Assuming generated stubs or placeholder

class Program
{
    static async Task Main(string[] args)
    {
        // 1. Define Circuit Breaker Policy
        var circuitBreakerPolicy = Policy
            .Handle<Exception>() // Or specific RpcException
            .CircuitBreakerAsync(
                exceptionsAllowedBeforeBreaking: 2,
                durationOfBreak: TimeSpan.FromSeconds(10),
                onBreak: (ex, breakDelay) => 
                    Console.WriteLine($"Circuit broken! Will retry after {breakDelay.TotalSeconds}s"),
                onReset: () => Console.WriteLine("Circuit reset. Service is back."),
                onHalfOpen: () => Console.WriteLine("Circuit half-open. Testing connection...")
            );

        var channel = GrpcChannel.ForAddress("http://triton-service:8001");
        var client = new GRPCInferenceService.GRPCInferenceServiceClient(channel);

        Console.WriteLine("Press Ctrl+C to exit.");

        while (true)
        {
            try
            {
                // 2. Execute with Circuit Breaker
                await circuitBreakerPolicy.ExecuteAsync(async () =>
                {
                    Console.WriteLine("Sending inference request...");

                    // Construct Triton Request
                    var request = new ModelInferRequest
                    {
                        ModelName = "my_model",
                        ModelVersion = "1"
                    };
                    
                    // Add inputs (dummy data)
                    // ... serialization logic ...

                    var response = await client.ModelInferAsync(request);
                    Console.WriteLine($"Inference successful. Output shape: {response.Outputs[0].Shape}");
                });
            }
            catch (BrokenCircuitException)
            {
                Console.WriteLine("Request blocked by Circuit Breaker. Failing fast.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Request failed: {ex.Message}");
            }

            await Task.Delay(2000);
        }
    }
}
