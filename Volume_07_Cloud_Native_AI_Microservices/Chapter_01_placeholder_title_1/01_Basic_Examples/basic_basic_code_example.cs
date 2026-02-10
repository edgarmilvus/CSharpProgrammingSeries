
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
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.Json.Serialization;

// 1. Define the Data Contracts
// These classes represent the structure of the data exchanged between the client and the service.
// They are simple POCOs (Plain Old CLR Objects) suitable for JSON serialization.
public class AnalysisRequest
{
    [JsonPropertyName("text")]
    public required string Text { get; set; }
}

public class AnalysisResult
{
    [JsonPropertyName("sentiment")]
    public string Sentiment { get; set; } = "Neutral";

    [JsonPropertyName("confidence")]
    public double Confidence { get; set; }

    [JsonPropertyName("processedAt")]
    public DateTime ProcessedAt { get; set; }
}

// 2. Define the Inference Logic Interface
// In a real-world scenario, this abstraction allows us to swap out the inference engine
// (e.g., from ML.NET to ONNX Runtime or Azure Cognitive Services) without changing the API layer.
public interface IInferenceEngine
{
    AnalysisResult Analyze(string text);
}

// 3. Implement the Mock Inference Engine
// For this "Hello World" example, we simulate an AI model. 
// In production, this would load a trained model file (e.g., .zip for ML.NET or .onnx).
public class MockInferenceEngine : IInferenceEngine
{
    private readonly ILogger<MockInferenceEngine> _logger;

    public MockInferenceEngine(ILogger<MockInferenceEngine> logger)
    {
        _logger = logger;
    }

    public AnalysisResult Analyze(string text)
    {
        _logger.LogInformation("Analyzing text: {Text}", text);

        // Simulate model inference logic
        // In a real scenario, this would involve vectorizing text and running a prediction.
        bool isPositive = text.Contains("good", StringComparison.OrdinalIgnoreCase) || 
                          text.Contains("great", StringComparison.OrdinalIgnoreCase) || 
                          text.Contains("love", StringComparison.OrdinalIgnoreCase);

        bool isNegative = text.Contains("bad", StringComparison.OrdinalIgnoreCase) || 
                          text.Contains("terrible", StringComparison.OrdinalIgnoreCase) || 
                          text.Contains("hate", StringComparison.OrdinalIgnoreCase);

        string sentiment = "Neutral";
        double confidence = 0.5;

        if (isPositive)
        {
            sentiment = "Positive";
            confidence = 0.95;
        }
        else if (isNegative)
        {
            sentiment = "Negative";
            confidence = 0.95;
        }

        return new AnalysisResult
        {
            Sentiment = sentiment,
            Confidence = confidence,
            ProcessedAt = DateTime.UtcNow
        };
    }
}

// 4. The Main Application Entry Point
// This sets up the web host, dependency injection, and request pipeline.
public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Configure Services
        // We register our InferenceEngine as a Singleton. 
        // In a stateless microservice, Singleton is acceptable for stateless logic or 
        // long-lived clients (like database connections), but be careful with transient state.
        builder.Services.AddSingleton<IInferenceEngine, MockInferenceEngine>();
        
        // Add Logging
        builder.Services.AddLogging(config =>
        {
            config.AddConsole();
            config.AddDebug();
        });

        var app = builder.Build();

        // 5. Define the API Endpoint
        // This maps the HTTP POST request to our logic.
        app.MapPost("/analyze", async (HttpContext context, IInferenceEngine engine) =>
        {
            try
            {
                // Deserialize the request body
                var request = await JsonSerializer.DeserializeAsync<AnalysisRequest>(context.Request.Body);

                if (request == null || string.IsNullOrWhiteSpace(request.Text))
                {
                    context.Response.StatusCode = 400; // Bad Request
                    await context.Response.WriteAsync("Invalid request: Text is required.");
                    return;
                }

                // Execute the inference logic
                var result = engine.Analyze(request.Text);

                // Serialize and return the response
                context.Response.ContentType = "application/json";
                await JsonSerializer.SerializeAsync(context.Response.Body, result);
            }
            catch (Exception ex)
            {
                // Global error handling (simplified for example)
                context.Response.StatusCode = 500;
                await context.Response.WriteAsync($"Internal Server Error: {ex.Message}");
            }
        });

        // 6. Run the Application
        // Kestrel is the cross-platform web server included with .NET.
        app.Run();
    }
}
