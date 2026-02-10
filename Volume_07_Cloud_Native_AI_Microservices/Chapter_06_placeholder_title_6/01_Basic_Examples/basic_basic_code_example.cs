
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

// Source File: basic_basic_code_example.cs
// Description: Basic Code Example
// ==========================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace ContainerizedAiAgent
{
    // Represents the incoming request payload from a client or another microservice.
    public class InferenceRequest
    {
        public string Prompt { get; set; } = string.Empty;
        public Dictionary<string, object>? Parameters { get; set; }
    }

    // Represents the outgoing response payload containing the inference result.
    public class InferenceResponse
    {
        public string Result { get; set; } = string.Empty;
        public long ProcessingTimeMs { get; set; }
        public DateTime Timestamp { get; set; }
    }

    // The core service responsible for processing inputs and generating outputs.
    // In a real-world scenario, this would interface with a loaded ML model (e.g., ONNX, TensorFlow.NET).
    public class InferenceService
    {
        // Mock method simulating a complex AI model inference.
        // In a production environment, this would involve tensor operations and GPU acceleration.
        public async Task<InferenceResponse> ProcessRequestAsync(InferenceRequest request)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            // Simulate network latency or model computation time (e.g., 100-500ms)
            await Task.Delay(new Random().Next(100, 500)); 

            // Simulate AI logic: A simple transformation based on the prompt.
            string processedResult = string.IsNullOrEmpty(request.Prompt) 
                ? "I received an empty prompt." 
                : $"Processed: {request.Prompt.ToUpper()}";

            stopwatch.Stop();

            return new InferenceResponse
            {
                Result = processedResult,
                ProcessingTimeMs = stopwatch.ElapsedMilliseconds,
                Timestamp = DateTime.UtcNow
            };
        }
    }

    // The entry point of the containerized application.
    // It acts as the HTTP server (e.g., Kestrel) listening for incoming requests.
    public class Program
    {
        public static async Task Main(string[] args)
        {
            Console.WriteLine("Starting Containerized AI Agent...");
            Console.WriteLine("Agent is listening on http://localhost:8080");

            var inferenceService = new InferenceService();

            // Mock HTTP listener loop. 
            // In a real ASP.NET Core app, this logic is handled by the HostBuilder and Middleware pipeline.
            // Here we simulate the lifecycle for a standalone executable context.
            while (true)
            {
                try
                {
                    // Simulate receiving a request (e.g., from a Service Mesh sidecar like Envoy)
                    var mockRequest = new InferenceRequest
                    {
                        Prompt = "Hello Kubernetes",
                        Parameters = new Dictionary<string, object> { { "temperature", 0.7 } }
                    };

                    Console.WriteLine($"Received request: {JsonSerializer.Serialize(mockRequest)}");

                    // Delegate to the inference engine
                    var response = await inferenceService.ProcessRequestAsync(mockRequest);

                    // Output the result (simulating sending HTTP 200 OK response)
                    Console.WriteLine($"Response: {JsonSerializer.Serialize(response)}");
                    
                    // Simulate a 5-second interval between health checks or batch processing
                    await Task.Delay(5000);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Critical Error: {ex.Message}");
                    // In a containerized environment, this might trigger a restart if the health check fails.
                }
            }
        }
    }
}
