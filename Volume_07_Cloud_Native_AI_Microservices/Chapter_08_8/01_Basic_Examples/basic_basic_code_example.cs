
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
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AiInferenceService
{
    // 1. Data Transfer Object (DTO) for the incoming request payload.
    // This represents the structured data expected from a client calling the AI service.
    public class InferenceRequest
    {
        [JsonPropertyName("prompt")]
        public string Prompt { get; set; } = string.Empty;

        [JsonPropertyName("max_tokens")]
        public int MaxTokens { get; set; } = 50;
    }

    // 2. Data Transfer Object (DTO) for the outgoing response payload.
    // This structures the AI's generated output for the client.
    public class InferenceResponse
    {
        [JsonPropertyName("result")]
        public string Result { get; set; } = string.Empty;

        [JsonPropertyName("model_version")]
        public string ModelVersion { get; set; } = "v1.0-basic";
    }

    // 3. The core AI Logic Service.
    // In a production environment, this would interface with a heavy ML model (e.g., ONNX, TensorFlow).
    // For this "Hello World" example, we simulate inference logic.
    public interface IInferenceService
    {
        Task<InferenceResponse> GenerateAsync(InferenceRequest request);
    }

    public class MockInferenceService : IInferenceService
    {
        public async Task<InferenceResponse> GenerateAsync(InferenceRequest request)
        {
            // Simulate network latency or model processing time
            await Task.Delay(100); 

            // Basic deterministic logic to simulate an AI model
            var response = new InferenceResponse
            {
                Result = $"Processed: '{request.Prompt}' (Simulated AI response)"
            };

            return response;
        }
    }

    // 4. Program Entry Point.
    // Configures the web host, dependency injection, and request pipeline.
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Register the mock inference service into the Dependency Injection container.
            // This allows controllers or endpoints to request IInferenceService without knowing the concrete implementation.
            builder.Services.AddSingleton<IInferenceService, MockInferenceService>();

            var app = builder.Build();

            // 5. Define the API Endpoint.
            // Maps a POST request to /inference to handle the AI workload.
            app.MapPost("/inference", async (HttpContext context, IInferenceService inferenceService) =>
            {
                // Parse the incoming JSON body into the InferenceRequest DTO
                var request = await context.Request.ReadFromJsonAsync<InferenceRequest>();

                if (request == null || string.IsNullOrWhiteSpace(request.Prompt))
                {
                    context.Response.StatusCode = 400; // Bad Request
                    await context.Response.WriteAsync("Invalid request: Prompt is required.");
                    return;
                }

                // Execute the AI inference logic
                var response = await inferenceService.GenerateAsync(request);

                // Return the result as JSON
                await context.Response.WriteAsJsonAsync(response);
            });

            // 6. Start the Web Server.
            // Kestrel is the default cross-platform web server for ASP.NET Core.
            app.Run();
        }
    }
}
