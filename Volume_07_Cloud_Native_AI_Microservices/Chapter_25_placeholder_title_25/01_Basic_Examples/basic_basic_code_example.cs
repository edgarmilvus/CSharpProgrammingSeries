
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

// 1. Define the data contracts for the AI Agent interaction.
// This separates the internal logic from the external API surface.
public record InferenceRequest(string Prompt);

public record InferenceResponse
{
    [JsonPropertyName("agent_id")]
    public string AgentId { get; init; } = string.Empty;

    [JsonPropertyName("response")]
    public string Response { get; init; } = string.Empty;

    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; init; }
}

// 2. Define the core AI Agent interface.
// In a real microservice, this would be implemented by a class wrapping an ONNX model or an LLM client.
public interface IInferenceAgent
{
    Task<InferenceResponse> ProcessAsync(InferenceRequest request, CancellationToken cancellationToken);
}

// 3. Implement the mock AI Agent.
// This simulates a stateful inference workload (e.g., loading a model into memory).
public class MockInferenceAgent : IInferenceAgent
{
    private readonly string _agentId;

    public MockInferenceAgent()
    {
        // Simulate model loading latency and unique instance identification (Pod identity).
        _agentId = Guid.NewGuid().ToString()[..8];
        // In a real scenario, we would load the model weights here (e.g., using TorchSharp or ML.NET).
    }

    public async Task<InferenceResponse> ProcessAsync(InferenceRequest request, CancellationToken cancellationToken)
    {
        // Simulate compute-bound inference latency (e.g., GPU processing).
        await Task.Delay(100, cancellationToken);

        return new InferenceResponse
        {
            AgentId = _agentId,
            Response = $"Processed: '{request.Prompt}' by Agent {_agentId}",
            Timestamp = DateTime.UtcNow
        };
    }
}

// 4. The Microservice Entry Point.
// This sets up the dependency injection container and the HTTP pipeline.
public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Register the AI Agent as a Singleton.
        // CRITICAL: This ensures the model stays loaded in memory for the lifetime of the container.
        // If this were 'Scoped', the model would be reloaded for every HTTP request, destroying performance.
        builder.Services.AddSingleton<IInferenceAgent, MockInferenceAgent>();

        var app = builder.Build();

        // 5. Define the API Endpoint.
        // We use minimal APIs for high-performance, low-overhead request handling.
        app.MapPost("/api/inference", async (
            InferenceRequest request,
            IInferenceAgent agent,
            CancellationToken cancellationToken) =>
        {
            // Validate input (basic guard clause).
            if (string.IsNullOrWhiteSpace(request.Prompt))
            {
                return Results.BadRequest("Prompt cannot be empty.");
            }

            try
            {
                // Delegate to the agent service.
                var result = await agent.ProcessAsync(request, cancellationToken);
                
                // Return JSON response.
                return Results.Ok(result);
            }
            catch (Exception ex)
            {
                // Log error (in a real app, use ILogger<T>)
                return Results.Problem($"Inference failed: {ex.Message}");
            }
        });

        // 6. Start the server.
        // Kestrel is the cross-platform web server included with .NET.
        app.Run();
    }
}
