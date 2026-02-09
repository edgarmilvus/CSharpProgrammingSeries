
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

using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AIWebApiChapter3Advanced
{
    // ---------------------------------------------------------
    // REAL-WORLD PROBLEM CONTEXT
    // ---------------------------------------------------------
    // Scenario: An AI Web API serves a high-traffic sentiment analysis service.
    // It loads a heavy ONNX model (simulated here) that consumes significant memory.
    // Multiple concurrent users (HTTP requests) access this model simultaneously.
    // The challenge is to manage the model's lifecycle efficiently to prevent:
    // 1. Memory leaks (loading the model for every request is too slow).
    // 2. Thread safety issues (multiple threads modifying the same model state).
    // 3. Stale data (users seeing other users' prediction results).
    // ---------------------------------------------------------

    // ---------------------------------------------------------
    // 1. DOMAIN MODELS & SERVICES
    // ---------------------------------------------------------

    /// <summary>
    /// Represents the data payload for a sentiment analysis request.
    /// In a real app, this would be a DTO (Data Transfer Object).
    /// </summary>
    public class AnalysisRequest
    {
        public string Text { get; set; }
        public string UserId { get; set; }
    }

    /// <summary>
    /// Represents the result of the analysis.
    /// </summary>
    public class AnalysisResult
    {
        public double SentimentScore { get; set; } // 0.0 (Negative) to 1.0 (Positive)
        public string ProcessedBy { get; set; }    // To track scope/instance
    }

    /// <summary>
    /// Interface for the heavy AI Inference Engine.
    /// </summary>
    public interface IInferenceEngine
    {
        AnalysisResult Predict(AnalysisRequest request);
        string GetModelId();
    }

    /// <summary>
    /// CONCEPT: SINGLETON LIFETIME.
    /// Simulates a heavy ONNX/ML.NET model loader.
    /// This is expensive to initialize and must be thread-safe.
    /// </summary>
    public class OnnxInferenceEngine : IInferenceEngine
    {
        private readonly string _modelId;
        private readonly object _lock = new object(); // Ensures thread safety for shared resources

        public OnnxInferenceEngine()
        {
            // Simulate expensive model loading (e.g., loading ONNX file into memory)
            Console.WriteLine("   [Singleton] Loading heavy ONNX model into memory...");
            Thread.Sleep(500); 
            _modelId = Guid.NewGuid().ToString().Substring(0, 8);
            Console.WriteLine($"   [Singleton] Model {_modelId} loaded and ready.");
        }

        public string GetModelId() => _modelId;

        public AnalysisResult Predict(AnalysisRequest request)
        {
            // CRITICAL: Thread Safety in Singleton
            // Since this instance is shared across all HTTP requests (threads),
            // we must lock if the underlying ML library isn't thread-safe.
            lock (_lock)
            {
                // Simulate inference computation
                Thread.Sleep(100); 
                
                // Deterministic fake logic for demonstration
                double score = request.Text.Contains("good") ? 0.9 : 0.1;
                
                return new AnalysisResult
                {
                    SentimentScore = score,
                    ProcessedBy = $"Singleton-Engine-{_modelId}"
                };
            }
        }
    }

    /// <summary>
    /// Interface for a logging service.
    /// </summary>
    public interface ILoggerService
    {
        void Log(string message);
        string GetSessionId();
    }

    /// <summary>
    /// CONCEPT: SCOPED LIFETIME.
    /// Represents a request-specific logging context (e.g., a database transaction scope).
    /// </summary>
    public class RequestLogger : ILoggerService
    {
        private readonly string _sessionId;

        public RequestLogger()
        {
            // Simulating a unique ID for this specific HTTP request scope
            _sessionId = Guid.NewGuid().ToString().Substring(0, 6);
            Console.WriteLine($"      [Scoped] Logger Session {_sessionId} created.");
        }

        public void Log(string message)
        {
            Console.WriteLine($"      [Scoped Log] [{_sessionId}]: {message}");
        }

        public string GetSessionId() => _sessionId;
    }

    /// <summary>
    /// CONCEPT: TRANSIENT LIFETIME.
    /// Represents a lightweight helper, like a data validator or a specific calculation unit.
    /// Created fresh every time it is requested.
    /// </summary>
    public interface IRequestValidator
    {
        bool Validate(AnalysisRequest request);
    }

    public class TextValidator : IRequestValidator
    {
        private readonly string _instanceId;

        public TextValidator()
        {
            _instanceId = Guid.NewGuid().ToString().Substring(0, 4);
            // Console.WriteLine($"   [Transient] Validator {_instanceId} instantiated.");
        }

        public bool Validate(AnalysisRequest request)
        {
            // In a real app, this might check length, profanity, etc.
            return !string.IsNullOrEmpty(request?.Text);
        }
    }

    // ---------------------------------------------------------
    // 2. CORE LOGIC (THE "CONTROLLER" SIMULATION)
    // ---------------------------------------------------------

    /// <summary>
    /// The main service orchestrating the AI workflow.
    /// This simulates an ASP.NET Core Controller or a MediatR Handler.
    /// </summary>
    public class SentimentAnalysisService
    {
        private readonly IInferenceEngine _inferenceEngine;
        private readonly ILoggerService _logger;
        private readonly IRequestValidator _validator;

        // Dependency Injection Constructor
        public SentimentAnalysisService(
            IInferenceEngine inferenceEngine, 
            ILoggerService logger, 
            IRequestValidator validator)
        {
            _inferenceEngine = inferenceEngine;
            _logger = logger;
            _validator = validator;
        }

        public AnalysisResult ExecuteAnalysis(AnalysisRequest request)
        {
            // 1. Validate Input (Using Transient Service)
            if (!_validator.Validate(request))
            {
                _logger.Log("Validation failed.");
                return null;
            }

            // 2. Perform Inference (Using Singleton Service)
            // Note: The Singleton ensures we don't reload the heavy model for every request.
            var result = _inferenceEngine.Predict(request);

            // 3. Log Metadata (Using Scoped Service)
            // We can correlate the result with the specific request scope.
            _logger.Log($"Prediction complete. Score: {result.SentimentScore}");
            
            return result;
        }
    }

    // ---------------------------------------------------------
    // 3. APPLICATION ORCHESTRATION (MAIN PROGRAM)
    // ---------------------------------------------------------

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== AI Web API DI Lifetime Simulation ===\n");

            // -------------------------------------------------
            // SETUP: Service Collection (Simulating Startup.cs)
            // -------------------------------------------------
            var services = new ServiceCollection();

            // Register Services with specific Lifetimes
            
            // SINGLETON: The heavy AI model.
            // One instance shared for the entire application lifetime.
            // Thread safety must be enforced inside the implementation.
            services.AddSingleton<IInferenceEngine, OnnxInferenceEngine>();

            // SCOPED: The request logger.
            // One instance created per "Scope" (simulating one HTTP Request).
            // Different requests get different loggers, but the same request shares it.
            services.AddScoped<ILoggerService, RequestLogger>();

            // TRANSIENT: The validator.
            // A new instance created every time it is requested (even within the same scope).
            // Stateless and lightweight.
            services.AddTransient<IRequestValidator, TextValidator>();

            // SCOPED: The main service (The API Controller equivalent).
            // It receives dependencies via constructor injection.
            services.AddScoped<SentimentAnalysisService>();

            var serviceProvider = services.BuildServiceProvider();

            Console.WriteLine("Services Registered.\n");

            // -------------------------------------------------
            // SIMULATION: HTTP Request Pipeline
            // -------------------------------------------------

            // SCENARIO 1: User A makes a request
            Console.WriteLine("--- Request 1 (User A) ---");
            using (var scope1 = serviceProvider.CreateScope())
            {
                // Inside a scope (HTTP Request), we resolve the service.
                // ASP.NET Core does this automatically, but we simulate it here.
                var service1 = scope1.ServiceProvider.GetRequiredService<SentimentAnalysisService>();
                
                var requestA = new AnalysisRequest { Text = "The AI model is good.", UserId = "UserA" };
                var result1 = service1.ExecuteAnalysis(requestA);

                if (result1 != null)
                {
                    Console.WriteLine($"Result: Score={result1.SentimentScore}, Processed By={result1.ProcessedBy}");
                }
            }
            // scope1 is disposed. Scoped services (Logger) are disposed here.

            Console.WriteLine();

            // SCENARIO 2: User B makes a request (Concurrent simulation)
            Console.WriteLine("--- Request 2 (User B) ---");
            using (var scope2 = serviceProvider.CreateScope())
            {
                var service2 = scope2.ServiceProvider.GetRequiredService<SentimentAnalysisService>();

                var requestB = new AnalysisRequest { Text = "This is terrible.", UserId = "UserB" };
                var result2 = service2.ExecuteAnalysis(requestB);

                if (result2 != null)
                {
                    Console.WriteLine($"Result: Score={result2.SentimentScore}, Processed By={result2.ProcessedBy}");
                }
            }

            Console.WriteLine();

            // SCENARIO 3: Demonstrating Transient vs Scoped within a single request
            Console.WriteLine("--- Request 3 (Internal Scope Check) ---");
            using (var scope3 = serviceProvider.CreateScope())
            {
                // 1. Resolve the main service (Scoped)
                var analysisService = scope3.ServiceProvider.GetRequiredService<SentimentAnalysisService>();
                
                // 2. Resolve a Validator (Transient) manually to show it's different
                var validator1 = scope3.ServiceProvider.GetRequiredService<IRequestValidator>();
                var validator2 = scope3.ServiceProvider.GetRequiredService<IRequestValidator>();

                Console.WriteLine($"   Validator 1 Instance: {validator1.GetHashCode()}");
                Console.WriteLine($"   Validator 2 Instance: {validator2.GetHashCode()}");
                Console.WriteLine("   Note: Transient services are distinct instances even within the same scope.");

                var requestC = new AnalysisRequest { Text = "Neutral text.", UserId = "UserC" };
                analysisService.ExecuteAnalysis(requestC);
            }

            Console.WriteLine("\n=== Simulation Complete ===");
        }
    }
}
