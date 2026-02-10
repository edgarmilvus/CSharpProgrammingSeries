
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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// 1. Register Swagger generator services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "AI Chat API",
        Version = "v1",
        Description = "A simple API serving an AI chat model endpoint."
    });

    // Define the complex ChatRequest schema manually to ensure accuracy
    c.SchemaGeneratorOptions.CustomTypeMappings.Add(typeof(ChatRequest), () =>
    {
        var schema = new OpenApiSchema
        {
            Type = "object",
            Properties = new Dictionary<string, OpenApiSchema>
            {
                ["messages"] = new OpenApiSchema
                {
                    Type = "array",
                    Description = "The conversation history.",
                    Items = new OpenApiSchema
                    {
                        Type = "object",
                        Properties = new Dictionary<string, OpenApiSchema>
                        {
                            ["role"] = new OpenApiSchema { Type = "string", Enum = new List<object> { "user", "assistant", "system" } },
                            ["content"] = new OpenApiSchema { Type = "string" }
                        },
                        Required = new HashSet<string> { "role", "content" }
                    }
                },
                ["temperature"] = new OpenApiSchema { Type = "number", Format = "float", Default = 0.7f }
            },
            Required = new HashSet<string> { "messages" }
        };
        return schema;
    });
});

var app = builder.Build();

// 2. Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "AI Chat API v1");
        c.RoutePrefix = string.Empty; // Serve Swagger UI at the root
    });
}

// 3. Define the Data Models (Request/Response)
public record ChatMessage(string Role, string Content);
public record ChatRequest(List<ChatMessage> Messages, float Temperature = 0.7f);
public record ChatResponse(string Id, string Content, string Model);

// 4. Define the AI Service (Mock Implementation)
public static class AiService
{
    public static async Task<ChatResponse> GenerateResponseAsync(ChatRequest request)
    {
        // Simulate AI processing delay
        await Task.Delay(500);
        
        // Simple mock logic: echo the last message or return a canned response
        var lastMessage = request.Messages.LastOrDefault();
        var responseContent = lastMessage?.Role == "user" 
            ? $"Mock AI response to: {lastMessage.Content}" 
            : "I am ready to assist.";
            
        return new ChatResponse(
            Id: Guid.NewGuid().ToString(),
            Content: responseContent,
            Model: "mock-model-v1"
        );
    }
}

// 5. Define the API Endpoint
app.MapPost("/api/chat", async (ChatRequest request) =>
{
    // Input validation (basic)
    if (request == null || request.Messages == null || request.Messages.Count == 0)
        return Results.BadRequest("Messages cannot be empty.");

    // Call the AI service
    var response = await AiService.GenerateResponseAsync(request);
    
    // Return the result
    return Results.Ok(response);
})
.WithName("ChatCompletion")
.WithOpenApi(); // Ensures this endpoint is included in the Swagger document

app.Run();
