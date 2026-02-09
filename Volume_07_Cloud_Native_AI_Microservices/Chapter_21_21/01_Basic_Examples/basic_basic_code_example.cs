
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

# Source File: basic_basic_code_example.cs
# Description: Basic Code Example
# ==========================================

// ============================================================
// BASIC CODE EXAMPLE: Containerized AI Inference Microservice
// ============================================================
// CONTEXT: In a cloud-native AI system, an "agent" (e.g., a sentiment analyzer)
// receives text, processes it via a model, and returns a result.
// This code demonstrates a minimal, self-contained microservice using ASP.NET Core.
// It simulates an AI model inference call and exposes it via an HTTP endpoint.
// This is the foundational unit that will be containerized and scaled in Kubernetes.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.Json.Serialization;

// 1. Define the Data Contracts (DTOs)
// -----------------------------------------------------------
// Real-world context: The API needs structured input and output.
// We use Records (C# 9+) for immutable, concise data models.
public record InferenceRequest(string Text);
public record InferenceResult(string Sentiment, double Confidence);

// 2. Define the "AI Model" Service
// -----------------------------------------------------------
// Real-world context: In a real scenario, this would load a ONNX model
// or call an external service like Azure Cognitive Services.
// Here, we simulate the inference logic for the "Hello World" example.
public interface IInferenceService
{
    Task<InferenceResult> AnalyzeAsync(InferenceRequest request);
}

public class MockSentimentModel : IInferenceService
{
    private readonly ILogger<MockSentimentModel> _logger;

    public MockSentimentModel(ILogger<MockSentimentModel> logger)
    {
        _logger = logger;
    }

    public async Task<InferenceResult> AnalyzeAsync(InferenceRequest request)
    {
        // Simulate network latency or GPU processing time
        await Task.Delay(50); 

        // Basic keyword-based simulation (not a real model)
        var text = request.Text.ToLowerInvariant();
        double confidence = 0.5;
        string sentiment = "Neutral";

        if (text.Contains("good") || text.Contains("great") || text.Contains("excellent"))
        {
            sentiment = "Positive";
            confidence = 0.95;
        }
        else if (text.Contains("bad") || text.Contains("terrible") || text.Contains("poor"))
        {
            sentiment = "Negative";
            confidence = 0.92;
        }

        _logger.LogInformation("Analyzed text: '{Text}' -> {Sentiment} ({Confidence:P})", 
            request.Text, sentiment, confidence);

        return new InferenceResult(sentiment, confidence);
    }
}

// 3. Define the API Controller
// -----------------------------------------------------------
// Real-world context: This is the entry point for the microservice.
// It handles HTTP requests, validates input, and delegates to the service.
[ApiController]
[Route("[controller]")]
public class InferenceController : ControllerBase
{
    private readonly IInferenceService _inferenceService;

    public InferenceController(IInferenceService inferenceService)
    {
        _inferenceService = inferenceService;
    }

    [HttpPost("analyze")]
    [ProducesResponseType(typeof(InferenceResult), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Analyze([FromBody] InferenceRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Text))
        {
            return BadRequest("Text cannot be empty.");
        }

        var result = await _inferenceService.AnalyzeAsync(request);
        return Ok(result);
    }
}

// 4. Program Entry Point (Minimal API Style)
// -----------------------------------------------------------
// Real-world context: This sets up the dependency injection container,
// configures logging, and starts the HTTP server.
public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container
        builder.Services.AddControllers();
        builder.Services.AddSingleton<IInferenceService, MockSentimentModel>();

        // Configure JSON options for cleaner API responses
        builder.Services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            options.SerializerOptions.WriteIndented = true;
        });

        var app = builder.Build();

        // Configure the HTTP request pipeline
        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.MapControllers();

        // Start the service
        app.Run("http://0.0.0.0:8080"); // Listen on all interfaces, port 8080
    }
}
