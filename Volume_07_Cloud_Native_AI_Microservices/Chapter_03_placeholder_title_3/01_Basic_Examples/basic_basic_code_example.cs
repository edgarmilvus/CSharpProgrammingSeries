
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
using System.Text.Json;
using System.Text.Json.Serialization;

// 1. Define the Data Contracts
// We use records for immutable data transfer objects (DTOs).
public record InferenceRequest(
    [property: JsonPropertyName("text")] string Text
);

public record InferenceResult(
    [property: JsonPropertyName("label")] string Label,
    [property: JsonPropertyName("confidence")] double Confidence
);

// 2. Define the AI Service Interface
// Abstraction allows us to swap the implementation later (e.g., from Mock to ONNX).
public interface IInferenceService
{
    Task<InferenceResult> PredictAsync(string text, CancellationToken cancellationToken);
}

// 3. Implement the AI Service
// This service simulates loading a model and running inference.
public class MockInferenceService : IInferenceService
{
    private readonly ILogger<MockInferenceService> _logger;
    private bool _modelLoaded = false;

    public MockInferenceService(ILogger<MockInferenceService> logger)
    {
        _logger = logger;
    }

    // Simulate expensive model loading on startup
    public void Initialize()
    {
        _logger.LogInformation("Loading AI model into memory...");
        // In reality: _model = OnnxRuntime.Load("model.onnx");
        Thread.Sleep(2000); // Simulate 2-second load time
        _modelLoaded = true;
        _logger.LogInformation("AI Model loaded and ready.");
    }

    public async Task<InferenceResult> PredictAsync(string text, CancellationToken cancellationToken)
    {
        if (!_modelLoaded)
        {
            throw new InvalidOperationException("Model not initialized.");
        }

        // Simulate inference latency (GPU/CPU computation)
        await Task.Delay(100, cancellationToken); 

        // Mock Logic: Simple keyword-based classification
        string label;
        double confidence;

        if (text.Contains("great", StringComparison.OrdinalIgnoreCase) || 
            text.Contains("love", StringComparison.OrdinalIgnoreCase))
        {
            label = "Positive";
            confidence = 0.95;
        }
        else if (text.Contains("bad", StringComparison.OrdinalIgnoreCase) || 
                 text.Contains("hate", StringComparison.OrdinalIgnoreCase))
        {
            label = "Negative";
            confidence = 0.92;
        }
        else
        {
            label = "Neutral";
            confidence = 0.65;
        }

        _logger.LogInformation("Inference completed for text: '{Text}' -> {Label}", text, label);
        
        return new InferenceResult(label, confidence);
    }
}

// 4. The Application Entry Point
public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container
        builder.Services.AddControllers();
        
        // Register the Inference Service as a Singleton.
        // CRITICAL: We use Singleton because loading the AI model is expensive.
        // We want to load it once and reuse it for all requests.
        builder.Services.AddSingleton<IInferenceService, MockInferenceService>();

        var app = builder.Build();

        // 5. Lifecycle Hook: Initialize the Model
        // We hook into the ApplicationStarted event to load the model 
        // before the server starts accepting traffic.
        var inferenceService = app.Services.GetRequiredService<IInferenceService>();
        if (inferenceService is MockInferenceService mockService)
        {
            mockService.Initialize();
        }

        // 6. Define the API Endpoint
        app.MapPost("/api/inference", async (HttpContext context, IInferenceService inferenceService) =>
        {
            try
            {
                // Deserialize request
                var request = await JsonSerializer.DeserializeAsync<InferenceRequest>(
                    context.Request.Body, 
                    cancellationToken: context.RequestAborted);

                if (request is null || string.IsNullOrWhiteSpace(request.Text))
                {
                    context.Response.StatusCode = 400;
                    await context.Response.WriteAsync("Invalid request body.");
                    return;
                }

                // Run Inference
                var result = await inferenceService.PredictAsync(request.Text, context.RequestAborted);

                // Serialize response
                context.Response.ContentType = "application/json";
                await JsonSerializer.SerializeAsync(context.Response.Body, result, cancellationToken: context.RequestAborted);
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = 500;
                await context.Response.WriteAsync($"Internal Server Error: {ex.Message}");
            }
        });

        // 7. Start the Server
        // Maps to port 8080 (standard for containers)
        app.Run("http://0.0.0.0:8080");
    }
}
