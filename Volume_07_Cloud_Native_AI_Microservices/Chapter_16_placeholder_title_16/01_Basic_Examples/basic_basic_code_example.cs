
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
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AiAgentMicroservice
{
    // 1. Domain Model: Represents the data structure for the AI Agent's input and output.
    public class SentimentRequest
    {
        [JsonPropertyName("text")]
        public required string Text { get; set; }
    }

    public class SentimentResponse
    {
        [JsonPropertyName("sentiment")]
        public string Sentiment { get; set; } = string.Empty;
        
        [JsonPropertyName("confidence")]
        public double Confidence { get; set; }
    }

    // 2. The AI Logic: A mock inference engine. 
    // In a real scenario, this would load a TensorFlow/PyTorch model or call a specialized inference server.
    public interface IInferenceEngine
    {
        SentimentResponse Analyze(string text);
    }

    public class SimpleInferenceEngine : IInferenceEngine
    {
        // Deterministic logic for "Hello World" purposes.
        public SentimentResponse Analyze(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return new SentimentResponse { Sentiment = "Neutral", Confidence = 0.0 };

            var lower = text.ToLowerInvariant();
            
            if (lower.Contains("good") || lower.Contains("great") || lower.Contains("happy"))
                return new SentimentResponse { Sentiment = "Positive", Confidence = 0.95 };
            
            if (lower.Contains("bad") || lower.Contains("terrible") || lower.Contains("sad"))
                return new SentimentResponse { Sentiment = "Negative", Confidence = 0.92 };

            return new SentimentResponse { Sentiment = "Neutral", Confidence = 0.5 };
        }
    }

    // 3. The Web API: Exposes the agent via HTTP for Kubernetes to route traffic to.
    public class Program
    {
        public static void Main(string args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Register the inference engine into the Dependency Injection container.
            // This makes the AI logic testable and swappable.
            builder.Services.AddSingleton<IInferenceEngine, SimpleInferenceEngine>();

            var app = builder.Build();

            // Define the API endpoint.
            // Kubernetes Health Checks will hit this root.
            app.MapGet("/", () => "AI Agent Microservice is running.");

            // The actual inference endpoint.
            app.MapPost("/analyze", async (HttpContext context, IInferenceEngine engine) =>
            {
                // Deserialize the incoming JSON request.
                var request = await context.Request.ReadFromJsonAsync<SentimentRequest>();
                
                if (request == null || string.IsNullOrWhiteSpace(request.Text))
                {
                    context.Response.StatusCode = 400;
                    await context.Response.WriteAsync("Invalid request body.");
                    return;
                }

                // Perform the AI inference.
                var result = engine.Analyze(request.Text);

                // Return the result as JSON.
                await context.Response.WriteAsJsonAsync(result);
            });

            // Listen on all interfaces (crucial for Docker container networking).
            // Default port is 8080, often used in containerized environments.
            app.Run("http://0.0.0.0:8080");
        }
    }
}
