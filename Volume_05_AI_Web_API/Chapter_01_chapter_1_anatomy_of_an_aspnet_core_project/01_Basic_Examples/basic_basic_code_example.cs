
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
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AIApiDemo
{
    // Represents a simple AI model request payload
    public class AIModelRequest
    {
        [JsonPropertyName("prompt")]
        public required string Prompt { get; set; }

        [JsonPropertyName("max_tokens")]
        public int MaxTokens { get; set; } = 100;

        [JsonPropertyName("temperature")]
        public float Temperature { get; set; } = 0.7f;
    }

    // Represents the AI model response payload
    public class AIModelResponse
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [JsonPropertyName("generated_text")]
        public string GeneratedText { get; set; } = string.Empty;

        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    // Minimal API entry point
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // 1. Dependency Injection Setup
            // Registering a mock AI service as a singleton to maintain state if needed
            builder.Services.AddSingleton<IAIModelService, MockAIModelService>();

            // 2. Configuration Binding
            // Bind a custom configuration section to a strongly-typed object
            var configSection = builder.Configuration.GetSection("AIOptions");
            builder.Services.Configure<AIOptions>(configSection);

            // 3. Build the Application
            var app = builder.Build();

            // 4. Middleware Pipeline Configuration
            // Enable detailed error pages for development
            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // Custom middleware to log incoming requests
            app.Use(async (context, next) =>
            {
                var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
                logger.LogInformation("Received request: {Method} {Path}", context.Request.Method, context.Request.Path);
                await next.Invoke();
            });

            // 5. Endpoint Definition
            // Define a POST endpoint for the AI Chat
            app.MapPost("/api/chat/generate", async (HttpContext httpContext, IAIModelService aiService, AIOptions options) =>
            {
                // Read and deserialize the request body
                var request = await JsonSerializer.DeserializeAsync<AIModelRequest>(
                    httpContext.Request.Body,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (request == null || string.IsNullOrWhiteSpace(request.Prompt))
                {
                    httpContext.Response.StatusCode = 400;
                    await httpContext.Response.WriteAsync("Invalid request: Prompt is required.");
                    return;
                }

                // Process the request via the injected service
                var response = await aiService.GenerateAsync(request);

                // Serialize and write the response
                httpContext.Response.ContentType = "application/json";
                await JsonSerializer.SerializeAsync(httpContext.Response.Body, response);
            });

            // 6. Run the Application
            app.Run();
        }
    }

    // Strongly-typed configuration class
    public class AIOptions
    {
        public string ModelName { get; set; } = "DefaultModel";
        public int RateLimitPerMinute { get; set; } = 60;
    }

    // Service Abstraction
    public interface IAIModelService
    {
        Task<AIModelResponse> GenerateAsync(AIModelRequest request);
    }

    // Mock Implementation (Simulating a real AI engine)
    public class MockAIModelService : IAIModelService
    {
        private readonly ILogger<MockAIModelService> _logger;
        private readonly AIOptions _options;

        public MockAIModelService(ILogger<MockAIModelService> logger, IOptions<AIOptions> options)
        {
            _logger = logger;
            _options = options.Value;
        }

        public Task<AIModelResponse> GenerateAsync(AIModelRequest request)
        {
            _logger.LogInformation("Generating response using model: {Model}", _options.ModelName);

            // Simulate processing delay
            return Task.FromResult(new AIModelResponse
            {
                GeneratedText = $"Mock AI Response to '{request.Prompt}' (Model: {_options.ModelName})",
                CreatedAt = DateTime.UtcNow
            });
        }
    }
}
