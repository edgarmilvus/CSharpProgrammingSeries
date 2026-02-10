
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

// 1. Define the request and response models for the API
public record InferenceRequest(string Prompt);

public record InferenceResponse
{
    [JsonPropertyName("response")]
    public string Response { get; set; } = string.Empty;

    [JsonPropertyName("model_version")]
    public string ModelVersion { get; set; } = "v1.0";

    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; }
}

// 2. Define the core AI Agent interface and implementation
public interface IInferenceAgent
{
    Task<string> ProcessPromptAsync(string prompt);
}

public class SimpleEchoAgent : IInferenceAgent
{
    // Simulating a stateful context (e.g., a memory store or model session)
    private readonly string _agentId = Guid.NewGuid().ToString();

    public async Task<string> ProcessPromptAsync(string prompt)
    {
        // Simulate processing delay (e.g., model inference time)
        await Task.Delay(100);

        // Basic logic: Echo the prompt with a context-aware prefix
        if (string.IsNullOrWhiteSpace(prompt))
            return "I received an empty prompt. Please provide input.";

        return $"[Agent {_agentId}]: I processed your request: '{prompt}'. Status: Inference Complete.";
    }
}

// 3. Configure the Dependency Injection container and HTTP Pipeline
var builder = WebApplication.CreateBuilder(args);

// Register the agent as a Singleton to maintain state across requests within the same pod
// In a real scenario, this might be Scoped or Transient depending on memory requirements.
builder.Services.AddSingleton<IInferenceAgent, SimpleEchoAgent>();

var app = builder.Build();

// 4. Define the API Endpoint
app.MapPost("/api/v1/inference", async (HttpContext context, IInferenceAgent agent) =>
{
    try
    {
        // Deserialize the incoming JSON request
        var request = await JsonSerializer.DeserializeAsync<InferenceRequest>(context.Request.Body);

        if (request == null || string.IsNullOrWhiteSpace(request.Prompt))
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsync("Invalid request: Prompt is required.");
            return;
        }

        // Execute the agent logic
        var result = await agent.ProcessPromptAsync(request.Prompt);

        // Construct the response
        var response = new InferenceResponse
        {
            Response = result,
            Timestamp = DateTime.UtcNow
        };

        // Serialize and return the JSON response
        context.Response.ContentType = "application/json";
        await JsonSerializer.SerializeAsync(context.Response.Body, response);
    }
    catch (Exception ex)
    {
        context.Response.StatusCode = 500;
        await context.Response.WriteAsync($"Internal Server Error: {ex.Message}");
    }
});

// 5. Start the server
// In a containerized environment, we typically listen on all interfaces
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
app.Run($"http://0.0.0.0:{port}");
