
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

using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

// A simple 'Hello World' example demonstrating how to configure and use
// IHttpClientFactory to call an external AI service (simulated here).
// This approach prevents socket exhaustion and allows for centralized configuration.

// 1. Define the request model (The "Builder" pattern concept)
public record class AiPromptRequest(
    [property: JsonPropertyName("prompt")] string Prompt,
    [property: JsonPropertyName("max_tokens")] int MaxTokens = 50
);

// 2. Define the response model
public record class AiResponse(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("choices")] List<AiChoice> Choices
);

public record class AiChoice(
    [property: JsonPropertyName("text")] string Text
);

// 3. The Typed Client Service
// This service encapsulates the logic for communicating with the AI provider.
public class AiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AiService> _logger;

    public AiService(HttpClient httpClient, ILogger<AiService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<string> GetCompletionAsync(string prompt)
    {
        try
        {
            // Construct the request payload
            var request = new AiPromptRequest(prompt);
            
            // PostAsJsonAsync handles serialization automatically
            var response = await _httpClient.PostAsJsonAsync("v1/completions", request);

            // Ensure success status code (throws HttpRequestException on failure)
            response.EnsureSuccessStatusCode();

            // Deserialize the response
            var aiResponse = await response.Content.ReadFromJsonAsync<AiResponse>();
            
            // Return the first choice's text
            return aiResponse?.Choices?.FirstOrDefault()?.Text ?? "No response generated.";
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP request failed while calling AI service.");
            throw; // Re-throw to let the caller handle the UI feedback
        }
    }
}

// 4. Program Setup (Minimal API style)
// This simulates the Startup/Program.cs configuration.
var builder = WebApplication.CreateBuilder(args);

// CRITICAL: Configure IHttpClientFactory
// We register the Typed Client 'AiService' and configure its HttpClient.
builder.Services.AddHttpClient<AiService>(client =>
{
    // Base address for the external API
    client.BaseAddress = new Uri("https://api.example-ai-provider.com/");
    
    // Set common headers (e.g., API Key)
    // In a real app, retrieve this from IConfiguration or Azure Key Vault.
    var apiKey = builder.Configuration["ApiKey"] ?? "sk-12345";
    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
    
    // Set timeout to prevent hanging indefinitely
    client.Timeout = TimeSpan.FromSeconds(30);
});

// Add logging
builder.Services.AddLogging();

var app = builder.Build();

// 5. Define a simple endpoint to trigger the service
app.MapGet("/chat", async (AiService aiService, string prompt) =>
{
    var response = await aiService.GetCompletionAsync(prompt);
    return Results.Ok(new { response });
});

app.Run();
