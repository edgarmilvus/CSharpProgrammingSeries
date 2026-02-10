
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
using System.Text;
using System.Threading.Tasks;

namespace AiApiSimulator
{
    // 1. Core Data Structures
    // Represents an HTTP-like request context.
    public class RequestContext
    {
        public string Path { get; set; }
        public string Method { get; set; }
        public Dictionary<string, string> Headers { get; set; }
        public string Body { get; set; }
        public bool IsAuthenticated { get; set; }
        public List<string> Errors { get; set; }

        public RequestContext(string method, string path)
        {
            Method = method;
            Path = path;
            Headers = new Dictionary<string, string>();
            Errors = new List<string>();
            Body = string.Empty;
        }
    }

    // Represents the response context.
    public class ResponseContext
    {
        public int StatusCode { get; set; }
        public string Content { get; set; }
        public List<string> StreamChunks { get; set; }

        public ResponseContext()
        {
            StreamChunks = new List<string>();
            Content = string.Empty;
        }
    }

    // 2. The Pipeline Delegate
    // A delegate representing a middleware component. It takes a context and a "next" delegate.
    public delegate Task PipelineDelegate(RequestContext context, Func<RequestContext, Task> next);

    // 3. The Pipeline Builder
    // Simulates the ASP.NET Core application builder.
    public class PipelineBuilder
    {
        private readonly List<PipelineDelegate> _middlewares = new List<PipelineDelegate>();

        public void Use(PipelineDelegate middleware)
        {
            _middlewares.Add(middleware);
        }

        public async Task RunAsync(RequestContext context)
        {
            // Create a chain of delegates. The "next" in the last middleware is a no-op.
            Func<RequestContext, Task> next = ctx => Task.CompletedTask;

            // Build the pipeline in reverse order so that _middlewares[0] is executed first,
            // calling _middlewares[1], etc.
            for (int i = _middlewares.Count - 1; i >= 0; i--)
            {
                var currentMiddleware = _middlewares[i];
                var previousNext = next; // Capture closure
                next = ctx => currentMiddleware(ctx, previousNext);
            }

            // Execute the first middleware
            await next(context);
        }
    }

    // 4. Middleware Implementations

    public class AuthMiddleware
    {
        public async Task Invoke(RequestContext context, Func<RequestContext, Task> next)
        {
            Console.WriteLine("[Middleware] Checking API Key...");

            // Basic logic to check headers
            if (context.Headers.ContainsKey("X-API-KEY") && context.Headers["X-API-KEY"] == "SECRET-123")
            {
                context.IsAuthenticated = true;
                Console.WriteLine("[Middleware] Authentication Successful.");
            }
            else
            {
                context.IsAuthenticated = false;
                context.Errors.Add("Unauthorized: Invalid or missing API Key.");
                // We do not call 'next' here; the pipeline stops.
                Console.WriteLine("[Middleware] Authentication Failed. Stopping pipeline.");
                return;
            }

            await next(context);
        }
    }

    public class ValidationMiddleware
    {
        public async Task Invoke(RequestContext context, Func<RequestContext, Task> next)
        {
            Console.WriteLine("[Middleware] Validating Input...");

            if (!context.IsAuthenticated)
            {
                // Short circuit if auth failed
                return;
            }

            // Simulate validation rules
            if (string.IsNullOrEmpty(context.Body) && context.Method == "POST")
            {
                context.Errors.Add("Validation Error: Body cannot be empty.");
                Console.WriteLine("[Middleware] Validation Failed.");
                return;
            }

            if (context.Path != "/api/chat" && context.Path != "/api/model")
            {
                context.Errors.Add("Validation Error: Invalid Endpoint.");
                Console.WriteLine("[Middleware] Invalid Endpoint.");
                return;
            }

            Console.WriteLine("[Middleware] Validation Successful.");
            await next(context);
        }
    }

    public class ExceptionHandlerMiddleware
    {
        public async Task Invoke(RequestContext context, Func<RequestContext, Task> next)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Middleware] Global Exception Caught: {ex.Message}");
                // Reset context to error state
                context.Errors.Clear();
                context.Errors.Add($"System Error: {ex.Message}");
            }
        }
    }

    // 5. Endpoint Handlers (The "Terminal" logic)

    public class ChatEndpoint
    {
        // Simulates a high-latency AI model call with streaming response
        public async Task HandleRequest(RequestContext context, ResponseContext response)
        {
            Console.WriteLine("[Endpoint] Processing Chat Request...");

            // Simulate AI Model Processing Time
            await Task.Delay(500); 

            // Simulate Streaming Tokens
            string[] tokens = new string[] { "Hello", ", ", "user", ". ", "How", " can", " I", " help", " you", " today?" };
            
            foreach (var token in tokens)
            {
                response.StreamChunks.Add(token);
                // Simulate network latency per token
                await Task.Delay(50); 
            }

            response.StatusCode = 200;
            Console.WriteLine("[Endpoint] Chat Generation Complete.");
        }
    }

    public class ModelEndpoint
    {
        public async Task HandleRequest(RequestContext context, ResponseContext response)
        {
            Console.WriteLine("[Endpoint] Processing Model Info Request...");
            await Task.Delay(100);
            response.Content = "{ \"model\": \"GPT-4-Simulator\", \"status\": \"active\" }";
            response.StatusCode = 200;
        }
    }

    // 6. Main Application Logic

    public class Program
    {
        public static async Task Main(string[] args)
        {
            Console.WriteLine("=== AI API Simulator Starting ===\n");

            // Initialize Pipeline
            var builder = new PipelineBuilder();

            // Register Middleware (Order matters!)
            var exceptionHandler = new ExceptionHandlerMiddleware();
            var auth = new AuthMiddleware();
            var validation = new ValidationMiddleware();

            // Note: In a real app, we'd wrap these in lambda expressions to match the delegate signature.
            // Here we simulate the method injection by passing the specific invoke method.
            builder.Use(exceptionHandler.Invoke);
            builder.Use(auth.Invoke);
            builder.Use(validation.Invoke);

            // 7. Test Scenarios
            
            // Scenario 1: Successful Chat Request with Streaming
            Console.WriteLine("--- TEST CASE 1: Valid Chat Request ---");
            var req1 = new RequestContext("POST", "/api/chat");
            req1.Headers["X-API-KEY"] = "SECRET-123";
            req1.Body = "Tell me a joke";
            await ExecutePipeline(builder, req1);

            // Scenario 2: Unauthorized Request
            Console.WriteLine("\n--- TEST CASE 2: Unauthorized Request ---");
            var req2 = new RequestContext("GET", "/api/model");
            // Missing API Key
            await ExecutePipeline(builder, req2);

            // Scenario 3: Validation Error
            Console.WriteLine("\n--- TEST CASE 3: Validation Error ---");
            var req3 = new RequestContext("POST", "/api/chat");
            req3.Headers["X-API-KEY"] = "SECRET-123";
            req3.Body = ""; // Empty body
            await ExecutePipeline(builder, req3);

            // Scenario 4: Exception Handling
            Console.WriteLine("\n--- TEST CASE 4: Simulated System Exception ---");
            var req4 = new RequestContext("POST", "/api/model");
            req4.Headers["X-API-KEY"] = "SECRET-123";
            req4.Body = "Trigger Error"; // Special flag to trigger error in simulation
            await ExecutePipeline(builder, req4);

            Console.WriteLine("\n=== Simulation Complete ===");
        }

        // Helper to execute the pipeline and handle the response
        private static async Task ExecutePipeline(PipelineBuilder builder, RequestContext context)
        {
            var response = new ResponseContext();

            // Run the middleware pipeline
            await builder.RunAsync(context);

            // Check if pipeline resulted in errors
            if (context.Errors.Count > 0)
            {
                response.StatusCode = 400; // Bad Request or 401
                response.Content = string.Join(", ", context.Errors);
                Console.WriteLine($"[Response] Status: {response.StatusCode}, Content: {response.Content}");
            }
            else
            {
                // If valid, route to specific endpoint
                // (In a real framework, this routing logic would be another middleware)
                if (context.Path == "/api/chat")
                {
                    var endpoint = new ChatEndpoint();
                    await endpoint.HandleRequest(context, response);
                    
                    // Simulate Streaming Output
                    Console.Write("[Response] Streaming: ");
                    foreach (var chunk in response.StreamChunks)
                    {
                        Console.Write(chunk);
                        // Flush logic simulated here
                    }
                    Console.WriteLine($"\n[Response] Status: {response.StatusCode}");
                }
                else if (context.Path == "/api/model")
                {
                    // Simulate Exception Trigger
                    if (context.Body == "Trigger Error")
                    {
                        throw new InvalidOperationException("Simulated Model Crash");
                    }

                    var endpoint = new ModelEndpoint();
                    await endpoint.HandleRequest(context, response);
                    Console.WriteLine($"[Response] Status: {response.StatusCode}, Content: {response.Content}");
                }
            }
        }
    }
}
