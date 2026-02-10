
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

// Source File: project_advanced_application_script.cs
// Description: Advanced Application Script
// ==========================================

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HighThroughputModelServing
{
    /// <summary>
    /// Simulates an AI Model Inference Engine.
    /// In a real scenario, this would wrap a PyTorch/TensorFlow runtime or an ONNX model.
    /// </summary>
    public class ModelInferenceEngine
    {
        // Simulating model weights or parameters loaded into memory.
        // In a real high-performance API, these would be pinned in memory to avoid GC overhead.
        private readonly double[] _modelWeights;
        private readonly Random _random = new Random();

        public ModelInferenceEngine()
        {
            // Initialize with dummy weights to simulate a loaded model (e.g., 1.5B parameters).
            // We use an array of doubles here as per basic C# concepts.
            _modelWeights = new double[1000];
            for (int i = 0; i < _modelWeights.Length; i++)
            {
                _modelWeights[i] = _random.NextDouble();
            }
        }

        /// <summary>
        /// Simulates the computational cost of running an inference pass.
        /// </summary>
        /// <param name="inputData">The tokenized input string.</param>
        /// <returns>A generated response string.</returns>
        public string Generate(string inputData)
        {
            // SIMULATION: In a real AI workload, this involves matrix multiplications
            // and activation functions. We simulate the latency here.
            // Typical LLM inference latency ranges from 50ms to 500ms depending on hardware.
            int processingTimeMs = 100; 
            Thread.Sleep(processingTimeMs);

            // Simulate a simple calculation using the weights to "process" the input.
            double result = 0;
            for (int i = 0; i < _modelWeights.Length && i < inputData.Length; i++)
            {
                result += _modelWeights[i] * inputData[i];
            }

            return $"[AI Response] Processed input '{inputData}' with confidence score: {Math.Abs(result % 1):P2}";
        }
    }

    /// <summary>
    /// Represents a raw HTTP Request context.
    /// In Minimal APIs, this is abstracted by HttpContext. Here we model it explicitly
    /// to demonstrate the overhead of parsing and object creation.
    /// </summary>
    public class HttpRequest
    {
        public string Method { get; set; }
        public string Path { get; set; }
        public string Body { get; set; }
    }

    /// <summary>
    /// Represents a raw HTTP Response context.
    /// </summary>
    public class HttpResponse
    {
        public int StatusCode { get; set; }
        public string Body { get; set; }
        public string ContentType { get; set; } = "application/json";
    }

    /// <summary>
    /// Simulates the Minimal API Pipeline.
    /// Minimal APIs rely heavily on Source Generators to reduce reflection overhead.
    /// This class simulates the direct, low-level routing logic.
    /// </summary>
    public class MinimalApiPipeline
    {
        private readonly ModelInferenceEngine _modelEngine;

        public MinimalApiPipeline(ModelInferenceEngine engine)
        {
            _modelEngine = engine;
        }

        /// <summary>
        /// Processes a request directly with minimal abstraction layers.
        /// </summary>
        public HttpResponse HandleRequest(HttpRequest request)
        {
            // 1. Routing: Direct string comparison (Highly performant).
            if (request.Path == "/api/chat" && request.Method == "POST")
            {
                return HandleChatEndpoint(request);
            }
            
            return new HttpResponse { StatusCode = 404, Body = "Not Found" };
        }

        private HttpResponse HandleChatEndpoint(HttpRequest request)
        {
            // 2. Validation: Explicit checks (No attribute overhead).
            if (string.IsNullOrEmpty(request.Body))
            {
                return new HttpResponse { StatusCode = 400, Body = "Bad Request: Empty Body" };
            }

            // 3. Processing: Direct call to the engine.
            string responseText = _modelEngine.Generate(request.Body);

            // 4. Serialization: Manual JSON construction (Lowest overhead).
            // In real Minimal APIs, System.Text.Json is used with source generation.
            string jsonResponse = $"{{ \"response\": \"{responseText}\" }}";

            return new HttpResponse 
            { 
                StatusCode = 200, 
                Body = jsonResponse 
            };
        }
    }

    /// <summary>
    /// Simulates the Controller-Based Pipeline.
    /// Controllers introduce layers of abstraction: Action Invokers, Filters, Model Binding.
    /// </summary>
    public class ControllerPipeline
    {
        private readonly ModelInferenceEngine _modelEngine;

        public ControllerPipeline(ModelInferenceEngine engine)
        {
            _modelEngine = engine;
        }

        /// <summary>
        /// Processes a request through the Controller lifecycle.
        /// </summary>
        public HttpResponse ProcessRequest(HttpRequest request)
        {
            // 1. Routing: Matches route templates (e.g., "api/{controller}/{action}").
            // This involves more string parsing than direct comparison.
            if (request.Path.StartsWith("/Controllers/Chat") && request.Method == "POST")
            {
                // 2. Controller Activation: Creates a new instance of the controller.
                // This allocates memory on the heap (GC pressure).
                var controller = new ChatController(_modelEngine);

                // 3. Action Invocation: Uses Reflection (or cached delegates) to call the method.
                // This adds latency compared to direct method calls.
                return controller.Post(request);
            }

            return new HttpResponse { StatusCode = 404, Body = "Not Found" };
        }
    }

    /// <summary>
    /// A traditional Controller class.
    /// </summary>
    public class ChatController
    {
        private readonly ModelInferenceEngine _modelEngine;

        public ChatController(ModelInferenceEngine engine)
        {
            _modelEngine = engine;
        }

        public HttpResponse Post(HttpRequest request)
        {
            // Model Binding: Parsing the body into a DTO (Data Transfer Object).
            // In real ASP.NET Core, this uses reflection to map properties.
            // We simulate the overhead by creating a new object.
            var chatRequest = new ChatRequestDto { Message = request.Body };

            // Validation Attributes: In real apps, attributes trigger validation logic.
            if (string.IsNullOrEmpty(chatRequest.Message))
            {
                return new HttpResponse { StatusCode = 400, Body = "Validation Failed" };
            }

            // Business Logic
            string result = _modelEngine.Generate(chatRequest.Message);

            // Result Execution: OkObjectResult, JsonResult, etc.
            return new HttpResponse 
            { 
                StatusCode = 200, 
                Body = $"{{ \"response\": \"{result}\" }}" 
            };
        }
    }

    // Simple DTO for simulation
    public class ChatRequestDto
    {
        public string Message { get; set; }
    }

    /// <summary>
    /// Main Program to Benchmark and Demonstrate the Architectural Differences.
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== High-Throughput AI Model Serving Simulation ===");
            Console.WriteLine("Comparing Minimal APIs vs Controller-Based Architecture\n");

            // Initialize Shared Resources
            var engine = new ModelInferenceEngine();
            var minimalPipeline = new MinimalApiPipeline(engine);
            var controllerPipeline = new ControllerPipeline(engine);

            // Test Data
            var testPayload = "What is the capital of AI?";
            var requestMinimal = new HttpRequest { Method = "POST", Path = "/api/chat", Body = testPayload };
            var requestController = new HttpRequest { Method = "POST", Path = "/Controllers/Chat/Index", Body = testPayload };

            // --- BENCHMARK 1: Minimal API ---
            Console.WriteLine("--- Testing Minimal API (Direct Pipeline) ---");
            var watch = Stopwatch.StartNew();
            
            // Simulate 1000 concurrent requests
            for (int i = 0; i < 1000; i++)
            {
                var response = minimalPipeline.HandleRequest(requestMinimal);
                if (response.StatusCode != 200) Console.WriteLine("Error in Minimal API");
            }
            
            watch.Stop();
            long minimalTime = watch.ElapsedMilliseconds;
            Console.WriteLine($"Total Time (1000 reqs): {minimalTime} ms");
            Console.WriteLine($"Avg Latency: {minimalTime / 1000.0} ms/req\n");

            // --- BENCHMARK 2: Controller API ---
            Console.WriteLine("--- Testing Controller-Based API (Abstraction Overhead) ---");
            watch.Restart();

            for (int i = 0; i < 1000; i++)
            {
                var response = controllerPipeline.ProcessRequest(requestController);
                if (response.StatusCode != 200) Console.WriteLine("Error in Controller API");
            }

            watch.Stop();
            long controllerTime = watch.ElapsedMilliseconds;
            Console.WriteLine($"Total Time (1000 reqs): {controllerTime} ms");
            Console.WriteLine($"Avg Latency: {controllerTime / 1000.0} ms/req\n");

            // --- ANALYSIS ---
            Console.WriteLine("=== Performance Analysis ===");
            double overhead = ((controllerTime - minimalTime) / (double)minimalTime) * 100;
            Console.WriteLine($"Controller Overhead: {overhead:F2}%");
            
            Console.WriteLine("\n--- Architectural Decision ---");
            if (overhead > 10)
            {
                Console.WriteLine("RECOMMENDATION: Use Minimal APIs.");
                Console.WriteLine("Reason: High overhead in object allocation and reflection impacts");
                Console.WriteLine("throughput significantly when serving high-volume AI requests.");
            }
            else
            {
                Console.WriteLine("RECOMMENDATION: Either pattern is acceptable.");
            }

            // Generate DOT Diagram for Visualization
            Console.WriteLine("\n=== Architecture Diagram (Graphviz) ===");
            Console.WriteLine(GenerateDotDiagram());
        }

        static string GenerateDotDiagram()
        {
            return @"


[ERROR: Failed to render diagram.]

";
        }
    }
}
