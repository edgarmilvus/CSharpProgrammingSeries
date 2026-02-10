
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
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

// This example demonstrates a minimal OpenAI-compatible plugin implementation.
// Scenario: A "Weather Service" plugin that LLMs (like ChatGPT) can call to get the current weather.
// It exposes two key endpoints:
// 1. /.well-known/ai-plugin.json: The manifest file describing the plugin.
// 2. /weather/current: The actual API endpoint for fetching weather data.

var builder = WebApplication.CreateBuilder(args);

// 1. Dependency Injection Setup
// We register the Swagger generator to dynamically generate the OpenAPI schema required by the plugin spec.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "Weather Plugin", 
        Version = "v1",
        Description = "A simple plugin to get current weather conditions."
    });
});

var app = builder.Build();

// 2. The Plugin Manifest Endpoint
// OpenAI plugins require a manifest file at the well-known path: /.well-known/ai-plugin.json
// This JSON file tells the LLM how to interact with your API.
app.MapGet("/.well-known/ai-plugin.json", () =>
{
    // Define the manifest object structure
    var manifest = new
    {
        schema_version = "v1",
        name_for_model = "weather",
        name_for_human = "Weather Plugin",
        description_for_model = "Plugin for retrieving current weather data for a specific location. Use this when users ask about the weather.",
        description_for_human = "Get the current weather for a city.",
        auth = new
        {
            type = "none" // No authentication for this simple example
        },
        api = new
        {
            type = "openapi",
            // Point to the OpenAPI schema endpoint we will define next
            url = "/swagger/v1/swagger.json", 
            is_user_authenticated = false
        },
        // URL for the logo (can be a local file or external)
        logo_url = "/logo.png", 
        contact_email = "support@example.com",
        legal_info_url = "https://example.com/legal"
    };

    return Results.Json(manifest);
});

// 3. The OpenAPI Schema Endpoint
// The manifest points to a Swagger/OpenAPI JSON file. 
// We utilize the built-in Swagger generator to provide this.
// This schema defines the parameters and return types for the LLM.
app.MapGet("/swagger/v1/swagger.json", (HttpContext httpContext) =>
{
    // Generate the OpenAPI document
    var swaggerProvider = app.Services.GetRequiredService<ISwaggerProvider>();
    var swaggerDoc = swaggerProvider.GetSwagger("v1");
    
    // Ensure the server URL is correct (handling reverse proxies/localhost)
    swaggerDoc.Servers = new List<OpenApiServer> 
    { 
        new OpenApiServer { Url = $"{httpContext.Request.Scheme}://{httpContext.Request.Host}" } 
    };

    return Results.Json(swaggerDoc);
});

// 4. The Actual API Endpoint (The Tool)
// This is the function the LLM will actually call.
// We use minimal API syntax for conciseness.
app.MapGet("/weather/current", (string location) =>
{
    // Simulate a database lookup or external API call
    // In a real app, you would inject a service here.
    var weatherData = new WeatherResponse
    {
        Location = location,
        Temperature = 22.5,
        Unit = "Celsius",
        Condition = "Sunny",
        Humidity = 45
    };

    return Results.Ok(weatherData);
})
.WithName("GetWeather") // Important: Name the endpoint for Swagger reference
.WithOpenApi(); // Adds OpenAPI metadata to this endpoint

// 5. Swagger UI Setup (Optional but recommended for testing)
// This allows humans to test the plugin easily in the browser.
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Weather Plugin V1");
    c.RoutePrefix = "swagger"; // Access via /swagger
});

app.Run();

// DTO for the response
public class WeatherResponse
{
    [JsonPropertyName("location")]
    public string Location { get; set; } = string.Empty;

    [JsonPropertyName("temperature")]
    public double Temperature { get; set; }

    [JsonPropertyName("unit")]
    public string Unit { get; set; } = string.Empty;

    [JsonPropertyName("condition")]
    public string Condition { get; set; } = string.Empty;

    [JsonPropertyName("humidity")]
    public int Humidity { get; set; }
}
