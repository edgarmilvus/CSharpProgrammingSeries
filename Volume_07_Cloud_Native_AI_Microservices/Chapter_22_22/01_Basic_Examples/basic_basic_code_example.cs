
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

// 1. Define the data contracts for the API
public record InferenceRequest(
    [property: JsonPropertyName("prompt")] string Prompt
);

public record InferenceResponse(
    [property: JsonPropertyName("result")] string Result,
    [property: JsonPropertyName("model_version")] string ModelVersion
);

// 2. Implement the core AI Agent Logic
public class SentimentAgent
{
    // In a real scenario, this would load a ONNX model or call an LLM.
    // For this containerization example, we simulate inference.
    public async Task<string> AnalyzeAsync(string prompt)
    {
        // Simulate model loading delay
        await Task.Delay(50); 
        
        // Simple heuristic logic
        if (prompt.Contains("great") || prompt.Contains("love"))
            return "POSITIVE";
        
        if (prompt.Contains("bad") || prompt.Contains("hate"))
            return "NEGATIVE";
            
        return "NEUTRAL";
    }
}

// 3. The Application Entry Point
var builder = WebApplication.CreateBuilder(args);

// Register the agent as a Singleton service (one instance per container)
builder.Services.AddSingleton<SentimentAgent>();

var app = builder.Build();

// 4. Define the API Endpoint
app.MapPost("/api/infer", async (InferenceRequest request, SentimentAgent agent) =>
{
    try 
    {
        var result = await agent.AnalyzeAsync(request.Prompt);
        var response = new InferenceResponse(result, "v1.0.0");
        return Results.Ok(response);
    }
    catch (Exception ex)
    {
        // In containerized environments, logging to stdout is crucial for observability
        Console.WriteLine($"[Error] Inference failed: {ex.Message}");
        return Results.Problem("Inference failed");
    }
});

// Health check endpoint for Kubernetes Liveness/Readiness probes
app.MapGet("/health", () => Results.Ok("Healthy"));

// Start the server
app.Run();
