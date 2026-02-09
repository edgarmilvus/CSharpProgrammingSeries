
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

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using System.Text.Json.Serialization;

// --- 1. Define the Data Models ---
// Using records for immutable data transfer objects (DTOs).
public record PredictionRequest(string Text);
public record PredictionResult(string Sentiment, double Confidence);

// --- 2. Define the Model Service ---
// This simulates a loaded AI model. In a real app, this would be a 
// complex class like an ONNX runtime session or a TensorFlow model wrapper.
public interface ISentimentModel
{
    PredictionResult Predict(string text);
}

public class MockSentimentModel : ISentimentModel
{
    // A simple dictionary to mock model inference logic.
    private static readonly Dictionary<string, PredictionResult> _knowledgeBase = new()
    {
        ["I love this product"] = new PredictionResult("Positive", 0.98),
        ["This is terrible"] = new PredictionResult("Negative", 0.95),
        ["It's okay"] = new PredictionResult("Neutral", 0.60)
    };

    public PredictionResult Predict(string text)
    {
        // Simulate computational delay (e.g., matrix multiplication)
        Thread.Sleep(10); 

        if (_knowledgeBase.TryGetValue(text, out var result))
        {
            return result;
        }

        // Default fallback for unknown text
        return new PredictionResult("Unknown", 0.50);
    }
}

// --- 3. Application Entry Point & Configuration ---
var builder = WebApplication.CreateBuilder(args);

// Register the model as a Singleton. 
// CRITICAL: The model is heavy; we load it once and share it across all requests.
builder.Services.AddSingleton<ISentimentModel, MockSentimentModel>();

// Configure JSON serialization options for consistent casing (camelCase).
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
});

var app = builder.Build();

// --- 4. Define the Minimal API Endpoint ---
// This replaces the entire Controller class structure.
app.MapPost("/predict", async (HttpContext context, ISentimentModel model) =>
{
    // Read the request body asynchronously
    var request = await context.Request.ReadFromJsonAsync<PredictionRequest>();
    
    if (request?.Text is null || string.IsNullOrWhiteSpace(request.Text))
    {
        // Explicit validation handling
        context.Response.StatusCode = 400;
        await context.Response.WriteAsJsonAsync(new { error = "Text is required." });
        return Results.BadRequest();
    }

    // Execute the AI model prediction
    var result = model.Predict(request.Text);

    // Return the result with HTTP 200 OK
    return Results.Ok(result);
});

// --- 5. Run the Application ---
app.Run();
