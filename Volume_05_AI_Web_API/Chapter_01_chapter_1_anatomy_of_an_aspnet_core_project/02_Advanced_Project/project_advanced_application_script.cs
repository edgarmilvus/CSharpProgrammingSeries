
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

# Source File: project_advanced_application_script.cs
# Description: Advanced Application Script
# ==========================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;

namespace AI_API_Simulator
{
    // ---------------------------------------------------------
    // 1. Domain Models (Simulating AI Data Structures)
    // ---------------------------------------------------------
    // In a real ASP.NET Core project, these would be defined in a 
    // separate namespace or project (e.g., AI_API_Simulator.Models).
    // We use basic classes here to represent the data transfer objects (DTOs).
    public class AIRequest
    {
        public string Prompt { get; set; }
        public double Temperature { get; set; }
        public int MaxTokens { get; set; }

        public AIRequest(string prompt, double temp, int tokens)
        {
            Prompt = prompt;
            Temperature = temp;
            MaxTokens = tokens;
        }
    }

    public class AIResponse
    {
        public string Result { get; set; }
        public string ModelVersion { get; set; }
        public DateTime CreatedAt { get; set; }

        public AIResponse(string result, string version)
        {
            Result = result;
            ModelVersion = version;
            CreatedAt = DateTime.UtcNow;
        }
    }

    // ---------------------------------------------------------
    // 2. Service Layer (Business Logic)
    // ---------------------------------------------------------
    // This simulates the "Service" dependency injected into controllers.
    // It handles the complex logic of interacting with the AI model.
    public interface IModelService
    {
        AIResponse Generate(AIRequest request);
    }

    public class AIModelService : IModelService
    {
        private readonly string _modelVersion;

        public AIModelService(string modelVersion)
        {
            _modelVersion = modelVersion;
        }

        public AIResponse Generate(AIRequest request)
        {
            // Simulate processing time
            System.Threading.Thread.Sleep(100);

            // Basic logic to simulate model behavior based on parameters
            string generatedText;
            if (request.Temperature > 0.8)
            {
                generatedText = $"Creative Output: {request.Prompt} [High Temp]";
            }
            else if (request.Temperature < 0.2)
            {
                generatedText = $"Deterministic Output: {request.Prompt} [Low Temp]";
            }
            else
            {
                generatedText = $"Balanced Output: {request.Prompt}";
            }

            // Truncate based on MaxTokens (simulated by string length)
            if (generatedText.Length > request.MaxTokens)
            {
                generatedText = generatedText.Substring(0, request.MaxTokens);
            }

            return new AIResponse(generatedText, _modelVersion);
        }
    }

    // ---------------------------------------------------------
    // 3. Middleware Simulation (HTTP Pipeline)
    // ---------------------------------------------------------
    // Simulates the ASP.NET Core Middleware pipeline.
    // Each component processes the request and passes it to the next.
    public class HttpContext
    {
        public string RequestPath { get; set; }
        public string RequestMethod { get; set; }
        public string RequestBody { get; set; }
        public string ResponseBody { get; set; }
        public int StatusCode { get; set; }
        public Dictionary<string, string> Headers { get; set; }

        public HttpContext(string method, string path, string body)
        {
            RequestMethod = method;
            RequestPath = path;
            RequestBody = body;
            Headers = new Dictionary<string, string>();
            StatusCode = 200; // Default OK
        }
    }

    public delegate RequestDelegate(HttpContext context);

    // Middleware Base
    public abstract class Middleware
    {
        protected Middleware _next;

        public Middleware(Middleware next)
        {
            _next = next;
        }

        public abstract void Invoke(HttpContext context);
    }

    // 3a. Exception Handling Middleware
    public class ExceptionHandlerMiddleware : Middleware
    {
        public ExceptionHandlerMiddleware(Middleware next) : base(next) { }

        public override void Invoke(HttpContext context)
        {
            try
            {
                // Try to process the rest of the pipeline
                if (_next != null) _next.Invoke(context);
            }
            catch (Exception ex)
            {
                context.StatusCode = 500;
                context.ResponseBody = JsonSerializer.Serialize(new { error = ex.Message, stackTrace = ex.StackTrace });
                context.Headers["Content-Type"] = "application/json";
                Console.WriteLine($"[Error] Exception caught: {ex.Message}");
            }
        }
    }

    // 3b. Routing Middleware
    public class RoutingMiddleware : Middleware
    {
        private readonly IModelService _modelService;

        public RoutingMiddleware(Middleware next, IModelService service) : base(next)
        {
            _modelService = service;
        }

        public override void Invoke(HttpContext context)
        {
            // Simple routing logic
            if (context.RequestPath == "/api/chat/generate" && context.RequestMethod == "POST")
            {
                HandleChatEndpoint(context);
            }
            else if (context.RequestPath == "/health" && context.RequestMethod == "GET")
            {
                context.ResponseBody = "Healthy";
                context.StatusCode = 200;
            }
            else
            {
                context.StatusCode = 404;
                context.ResponseBody = "Not Found";
            }
        }

        private void HandleChatEndpoint(HttpContext context)
        {
            // 1. Deserialize Request (Simulating Model Binding)
            // In real ASP.NET Core, this is done automatically by the framework.
            AIRequest request = ParseRequest(context.RequestBody);

            // 2. Validate Input
            if (request == null || string.IsNullOrEmpty(request.Prompt))
            {
                context.StatusCode = 400;
                context.ResponseBody = "Bad Request: Prompt is required.";
                return;
            }

            if (request.Temperature < 0 || request.Temperature > 1)
            {
                context.StatusCode = 400;
                context.ResponseBody = "Bad Request: Temperature must be between 0 and 1.";
                return;
            }

            // 3. Execute Business Logic
            AIResponse response = _modelService.Generate(request);

            // 4. Serialize Response
            context.ResponseBody = JsonSerializer.Serialize(response);
            context.Headers["Content-Type"] = "application/json";
            context.StatusCode = 200;
        }

        private AIRequest ParseRequest(string body)
        {
            // Rudimentary JSON parsing for simulation purposes
            // In production, use System.Text.Json or Newtonsoft.Json
            try
            {
                // Very basic manual parsing to avoid complex libraries for this exercise
                if (!body.Contains("\"Prompt\"")) return null;
                
                // Extracting values manually (simulating deserialization)
                int startPrompt = body.IndexOf(":\"") + 2;
                int endPrompt = body.IndexOf("\"", startPrompt);
                string prompt = body.Substring(startPrompt, endPrompt - startPrompt);

                // Default values for simulation
                return new AIRequest(prompt, 0.7, 100);
            }
            catch
            {
                return null;
            }
        }
    }

    // 3c. Logging Middleware
    public class LoggingMiddleware : Middleware
    {
        public LoggingMiddleware(Middleware next) : base(next) { }

        public override void Invoke(HttpContext context)
        {
            Console.WriteLine($"[Log] Request: {context.RequestMethod} {context.RequestPath}");
            
            var startTime = DateTime.Now;

            if (_next != null) _next.Invoke(context);

            var duration = DateTime.Now - startTime;
            Console.WriteLine($"[Log] Response: {context.StatusCode} in {duration.TotalMilliseconds}ms");
        }
    }

    // ---------------------------------------------------------
    // 4. Application Entry Point (Program.cs Simulation)
    // ---------------------------------------------------------
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting AI API Simulator...\n");

            // 1. Configuration (Simulating appsettings.json)
            string modelVersion = "AI-Model-v1.2.0";

            // 2. Dependency Injection Setup (Simulating Startup/Program.cs)
            // We manually construct the dependency graph here.
            IModelService aiService = new AIModelService(modelVersion);

            // 3. Building the Middleware Pipeline
            // The order matters: Exception Handler wraps everything.
            // Logging is usually the first to start and last to finish.
            // Routing is usually inner, closer to the endpoint logic.
            Middleware pipeline = new ExceptionHandlerMiddleware(
                new LoggingMiddleware(
                    new RoutingMiddleware(null, aiService)
                )
            );

            // 4. Simulating Incoming Requests
            SimulateRequest(pipeline, "GET", "/health", null);
            SimulateRequest(pipeline, "POST", "/api/chat/generate", "{ \"Prompt\": \"Explain AI\" }");
            SimulateRequest(pipeline, "POST", "/api/chat/generate", "{ \"Prompt\": \"\" }"); // Invalid
            SimulateRequest(pipeline, "GET", "/invalid", null); // 404
            SimulateRequest(pipeline, "POST", "/api/chat/generate", "Malformed JSON"); // Exception simulation
        }

        static void SimulateRequest(Middleware pipeline, string method, string path, string body)
        {
            Console.WriteLine("\n--------------------------------------------------");
            Console.WriteLine($"Simulating: {method} {path}");
            
            var context = new HttpContext(method, path, body);
            
            // Execute the pipeline
            pipeline.Invoke(context);

            // Output Results
            Console.WriteLine($"Status Code: {context.StatusCode}");
            if (!string.IsNullOrEmpty(context.ResponseBody))
            {
                Console.WriteLine($"Response Body: {context.ResponseBody}");
            }
        }
    }
}
