
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
using System.Text.Json;
using System.Threading.Tasks;

namespace AI_API_ErrorHandling_Simulation
{
    // ---------------------------------------------------------
    // 1. Domain Models (Simulating AI Service Responses)
    // ---------------------------------------------------------

    /// <summary>
    /// Represents a request sent to an AI model endpoint.
    /// </summary>
    public class AIChatRequest
    {
        public string Prompt { get; set; }
        public string Model { get; set; }
        public int MaxTokens { get; set; }
    }

    /// <summary>
    /// Represents a successful response from the AI model.
    /// </summary>
    public class AIChatResponse
    {
        public string ResponseText { get; set; }
        public string ModelVersion { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // ---------------------------------------------------------
    // 2. RFC 7807 Problem Details Implementation
    // ---------------------------------------------------------

    /// <summary>
    /// Standardized error response format per RFC 7807.
    /// Used by clients to parse errors programmatically.
    /// </summary>
    public class ProblemDetails
    {
        public string Type { get; set; }       // URI identifying the error type
        public string Title { get; set; }      // Short, human-readable summary
        public int Status { get; set; }        // HTTP Status Code
        public string Detail { get; set; }     // Specific error description
        public string Instance { get; set; }   // URI specific to this occurrence

        // Extension for AI-specific context
        public string ErrorCode { get; set; }
    }

    // ---------------------------------------------------------
    // 3. Custom Exception Definitions
    // ---------------------------------------------------------

    public class AIContentFilteredException : Exception
    {
        public string BlockedCategory { get; }
        public double SeverityScore { get; }

        public AIContentFilteredException(string message, string category, double score) 
            : base(message)
        {
            BlockedCategory = category;
            SeverityScore = score;
        }
    }

    public class InvalidModelException : Exception
    {
        public InvalidModelException(string message) : base(message) { }
    }

    // ---------------------------------------------------------
    // 4. The "Advanced Application Script" - Core Logic
    // ---------------------------------------------------------

    public class Program
    {
        // Entry point simulating an API request pipeline
        public static async Task Main(string[] args)
        {
            Console.WriteLine("--- AI API Error Handling Simulation ---\n");

            // Simulate 3 different request scenarios
            var testRequests = new List<AIChatRequest>
            {
                new AIChatRequest { Prompt = "Hello, world!", Model = "gpt-4", MaxTokens = 50 }, // Success
                new AIChatRequest { Prompt = "How to hack a bank?", Model = "gpt-4", MaxTokens = 50 }, // Content Filtered
                new AIChatRequest { Prompt = "Tell me a story", Model = "unknown-model-v1", MaxTokens = 50 } // Invalid Model
            };

            foreach (var request in testRequests)
            {
                Console.WriteLine($"Processing Request: Model='{request.Model}', Prompt='{request.Prompt.Substring(0, Math.Min(20, request.Prompt.Length))}...'");
                
                try
                {
                    // Simulate the API Controller calling a Service
                    var response = await ProcessAIRequestAsync(request);
                    
                    // Simulate returning 200 OK
                    Console.WriteLine($"  [SUCCESS] {JsonSerializer.Serialize(response)}\n");
                }
                catch (Exception ex)
                {
                    // ---------------------------------------------------------
                    // THE ERROR HANDLING PIPELINE (Concept from Chapter 19)
                    // ---------------------------------------------------------
                    // In a real ASP.NET Core app, this logic is centralized in 
                    // IExceptionHandler or Middleware. Here, we simulate that 
                    // transformation logic explicitly.
                    
                    var problem = MapExceptionToProblemDetails(ex);
                    var jsonError = JsonSerializer.Serialize(problem, new JsonSerializerOptions { WriteIndented = true });
                    
                    // Simulate HTTP 400/500 response
                    Console.WriteLine($"  [ERROR - HTTP {problem.Status}] {jsonError}\n");
                }
            }
        }

        // ---------------------------------------------------------
        // 5. Business Logic & Service Simulation
        // ---------------------------------------------------------

        /// <summary>
        /// Simulates the core AI service logic.
        /// Throws specific exceptions based on input validation and safety checks.
        /// </summary>
        static async Task<AIChatResponse> ProcessAIRequestAsync(AIChatRequest request)
        {
            // Simulate async I/O
            await Task.Delay(100); 

            // 1. Input Validation (Simulating FluentValidation or Attributes)
            if (string.IsNullOrWhiteSpace(request.Prompt))
                throw new ArgumentException("Prompt cannot be empty.");

            // 2. Model Availability Check
            if (request.Model != "gpt-4" && request.Model != "gpt-3.5-turbo")
            {
                throw new InvalidModelException($"The model '{request.Model}' is not supported. Available models: gpt-4, gpt-3.5-turbo.");
            }

            // 3. AI Safety / Content Filtering Simulation
            if (request.Prompt.Contains("hack") || request.Prompt.Contains("bank"))
            {
                // In a real scenario, this comes from the AI Service SDK (e.g., Azure OpenAI ContentFilterResult)
                throw new AIContentFilteredException(
                    "Prompt was flagged by the safety system.", 
                    "Hate", 
            }

            // 4. Success Case
            return new AIChatResponse
            {
                ResponseText = $"Generated response for: {request.Prompt}",
                ModelVersion = request.Model,
                CreatedAt = DateTime.UtcNow
            };
        }

        // ---------------------------------------------------------
        // 6. Exception Handler / Middleware Simulation
        // ---------------------------------------------------------

        /// <summary>
        /// Centralized logic to convert internal exceptions into RFC 7807 Problem Details.
        /// Mimics the behavior of registering IExceptionHandler or Middleware in ASP.NET Core.
        /// </summary>
        static ProblemDetails MapExceptionToProblemDetails(Exception ex)
        {
            var problem = new ProblemDetails
            {
                Instance = $"/requests/{Guid.NewGuid()}", // Simulating a unique request trace ID
                Title = "An error occurred while processing your request."
            };

            // Pattern Matching switch expression (Modern C# Feature)
            switch (ex)
            {
                case AIContentFilteredException aiEx:
                    problem.Status = 400; // Bad Request (Client needs to adjust prompt)
                    problem.Type = "https://api.example.com/errors/content-filtered";
                    problem.Title = "Request blocked by Safety System";
                    problem.Detail = aiEx.Message;
                    problem.ErrorCode = "SAFETY_VIOLATION";
                    // Adding custom extensions for AI context
                    // (In ASP.NET Core, this would be part of the ProblemDetails.Extensions dictionary)
                    break;

                case InvalidModelException modelEx:
                    problem.Status = 400; // Bad Request
                    problem.Type = "https://api.example.com/errors/invalid-model";
                    problem.Title = "Invalid AI Model";
                    problem.Detail = modelEx.Message;
                    problem.ErrorCode = "MODEL_INVALID";
                    break;

                case ArgumentException argEx:
                    problem.Status = 400;
                    problem.Type = "https://api.example.com/errors/invalid-input";
                    problem.Title = "Invalid Argument";
                    problem.Detail = argEx.Message;
                    problem.ErrorCode = "VALIDATION_ERROR";
                    break;

                case Exception _:
                    // Catch-all for unexpected errors (e.g., downstream service outage)
                    problem.Status = 500;
                    problem.Type = "https://api.example.com/errors/internal-server-error";
                    problem.Title = "Internal Server Error";
                    problem.Detail = "An unexpected error occurred. Please try again later.";
                    problem.ErrorCode = "INTERNAL_ERROR";
                    break;
            }

            return problem;
        }
    }
}
