
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

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace AiInferenceService
{
    // 1. Data Models: Defines the structure of the request and response.
    public class InferenceRequest
    {
        public string Text { get; set; } = string.Empty;
    }

    public class InferenceResult
    {
        public string Label { get; set; } = string.Empty;
        public float Score { get; set; }
    }

    // 2. AI Service Interface: Abstraction for the inference logic.
    public interface IInferenceService
    {
        Task<InferenceResult> PredictAsync(string text);
    }

    // 3. Mock AI Service: Simulates a real model (e.g., BERT/Transformer) 
    // without requiring heavy dependencies or GPU access for this example.
    public class MockInferenceService : IInferenceService
    {
        // Simulating a model vocabulary and weights for simple keyword matching
        private readonly Dictionary<string, float> _sentimentWeights = new()
        {
            { "good", 0.8f },
            { "great", 0.9f },
            { "excellent", 1.0f },
            { "bad", -0.8f },
            { "terrible", -1.0f },
            { "awful", -0.9f }
        };

        public Task<InferenceResult> PredictAsync(string text)
        {
            // Normalize input
            var words = text.ToLower().Split(new[] { ' ', '.', ',', '!' }, StringSplitOptions.RemoveEmptyEntries);
            
            float score = 0;
            foreach (var word in words)
            {
                if (_sentimentWeights.TryGetValue(word, out var weight))
                {
                    score += weight;
                }
            }

            // Determine label based on score
            string label = score > 0.1f ? "Positive" : (score < -0.1f ? "Negative" : "Neutral");

            // Simulate processing delay (common in real AI inference)
            // This highlights the need for async processing in microservices.
            return Task.Delay(50).ContinueWith(_ => 
                new InferenceResult { Label = label, Score = Math.Clamp(score, -1.0f, 1.0f) }
            );
        }
    }

    // 4. Program Entry Point: Configures the web host and dependency injection.
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Register services into the Dependency Injection container.
            // Singleton ensures the model is loaded once in memory (crucial for AI models).
            builder.Services.AddSingleton<IInferenceService, MockInferenceService>();
            
            // Add Controllers (if using MVC pattern, though we use Minimal API here for brevity)
            builder.Services.AddControllers();

            var app = builder.Build();

            // 5. Minimal API Endpoint: The entry point for the microservice.
            // This handles HTTP POST requests to /predict
            app.MapPost("/predict", async (HttpContext context, IInferenceService inferenceService) =>
            {
                // Parse the incoming JSON request
                var request = await JsonSerializer.DeserializeAsync<InferenceRequest>(
                    context.Request.Body, 
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );

                if (request == null || string.IsNullOrWhiteSpace(request.Text))
                {
                    context.Response.StatusCode = 400;
                    await context.Response.WriteAsync("Invalid request: Text is required.");
                    return;
                }

                // Execute the AI inference
                var result = await inferenceService.PredictAsync(request.Text);

                // Return the result as JSON
                context.Response.ContentType = "application/json";
                await JsonSerializer.SerializeAsync(context.Response.Body, result);
            });

            // 6. Health Check Endpoint: Essential for Kubernetes liveness/readiness probes.
            app.MapGet("/health", () => "Service is healthy.");

            // Start the server (default port 5000)
            app.Run("http://0.0.0.0:5000");
        }
    }
}
