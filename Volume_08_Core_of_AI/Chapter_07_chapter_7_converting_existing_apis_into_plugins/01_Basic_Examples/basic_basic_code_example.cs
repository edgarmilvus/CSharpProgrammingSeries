
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

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Plugins.OpenApi;
using System.Text.Json;
using System.Text.Json.Serialization;

// 1. Define the data model for the API response.
// This helps the AI understand the structure of the data it receives.
public record WeatherForecast(
    [property: JsonPropertyName("city")] string City,
    [property: JsonPropertyName("date")] DateOnly Date,
    [property: JsonPropertyName("temperature_celsius")] double TemperatureCelsius,
    [property: JsonPropertyName("description")] string Description
);

// 2. Define the OpenAPI specification for our weather service.
// In a real scenario, this would be a URL or a file path to a .json file.
// For this self-contained example, we define it as a string.
const string OpenApiSpec = """
{
  "openapi": "3.0.1",
  "info": {
    "title": "Weather Forecast API",
    "version": "1.0.0"
  },
  "servers": [
    {
      "url": "https://api.weatherexample.com"
    }
  ],
  "paths": {
    "/forecast": {
      "get": {
        "summary": "Get weather forecast for a city",
        "operationId": "getForecast",
        "parameters": [
          {
            "name": "city",
            "in": "query",
            "required": true,
            "schema": {
              "type": "string"
            }
          },
          {
            "name": "days",
            "in": "query",
            "required": false,
            "schema": {
              "type": "integer",
              "default": 1
            }
          }
        ],
        "responses": {
          "200": {
            "description": "A successful response",
            "content": {
              "application/json": {
                "schema": {
                  "type": "array",
                  "items": {
                    "$ref": "#/components/schemas/WeatherForecast"
                  }
                }
              }
            }
          }
        }
      }
    }
  },
  "components": {
    "schemas": {
      "WeatherForecast": {
        "type": "object",
        "properties": {
          "city": { "type": "string" },
          "date": { "type": "string", "format": "date" },
          "temperature_celsius": { "type": "number" },
          "description": { "type": "string" }
        }
      }
    }
  }
}
""";

// 3. Create a mock HttpClient to simulate the API response.
// This makes the example runnable without an actual external service.
public class MockWeatherHttpClient : HttpClient
{
    public override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // Simulate network delay
        await Task.Delay(100, cancellationToken);

        // Check if the request is to our specific endpoint
        if (request.RequestUri?.AbsolutePath.Contains("/forecast") == true)
        {
            // Parse query parameters to simulate dynamic response
            var query = System.Web.HttpUtility.ParseQueryString(request.RequestUri.Query);
            var city = query["city"] ?? "Unknown";
            var days = int.Parse(query["days"] ?? "1");

            // Generate mock forecast data
            var forecasts = Enumerable.Range(0, days)
                .Select(i => new WeatherForecast(
                    City: city,
                    Date: DateOnly.FromDateTime(DateTime.Now.AddDays(i)),
                    TemperatureCelsius: 20 + Random.Shared.NextDouble() * 10,
                    Description: i % 2 == 0 ? "Sunny" : "Cloudy"
                ))
                .ToList();

            var jsonResponse = JsonSerializer.Serialize(forecasts);
            return new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent(jsonResponse, System.Text.Encoding.UTF8, "application/json")
            };
        }

        return new HttpResponseMessage(System.Net.HttpStatusCode.NotFound);
    }
}

// 4. The main execution block
public class Program
{
    public static async Task Main(string[] args)
    {
        // --- A. Kernel Setup ---
        // Create a kernel builder to register services and plugins.
        var builder = Kernel.CreateBuilder();
        
        // Register our mock HTTP client for dependency injection.
        // In production, you would register a real HttpClient with appropriate handlers.
        builder.Services.AddSingleton<HttpClient>(new MockWeatherHttpClient());
        
        var kernel = builder.Build();

        // --- B. Plugin Creation from OpenAPI ---
        // Create a stream from the OpenAPI spec string.
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(OpenApiSpec));
        
        // Import the API as a plugin. Semantic Kernel will parse the spec,
        // create a function for the 'getForecast' operation, and map parameters.
        // The 'executionParameters' configure how the plugin behaves.
        var weatherPlugin = await kernel.ImportPluginFromOpenApiAsync(
            "WeatherPlugin",
            stream,
            new OpenApiFunctionExecutionParameters()
            {
                // Use our mock HTTP client instead of the default one.
                HttpClient = kernel.Services.GetRequiredService<HttpClient>(),
                // Enable automatic parameter mapping from function arguments to HTTP request.
                EnableDynamicPayload = true,
                // Allow the AI to use the plugin without explicit function calling if needed.
                EnableFunctions = true
            }
        );

        Console.WriteLine("✅ Weather plugin loaded successfully.");
        Console.WriteLine($"   Available functions: {string.Join(", ", weatherPlugin.Select(p => p.Name))}");

        // --- C. Invoking the Plugin via the AI Agent ---
        // Create a prompt that requires the AI to use the weather plugin.
        // The AI will see the function definition and decide to call it.
        var prompt = "What's the weather forecast for London for the next 3 days?";

        Console.WriteLine($"\n🤖 User Query: {prompt}");
        
        // Execute the prompt. The AI (e.g., GPT-4) will analyze the prompt,
        // see the 'WeatherPlugin.getForecast' function, and invoke it with the correct arguments.
        var result = await kernel.InvokePromptAsync(prompt);

        Console.WriteLine("\n✨ AI Agent Response:");
        Console.WriteLine(result.ToString());

        // --- D. Direct Function Invocation (Alternative Approach) ---
        // Sometimes you want to call the plugin directly without AI reasoning.
        Console.WriteLine("\n--- Direct Function Call Example ---");
        
        var function = weatherPlugin["getForecast"];
        var arguments = new KernelArguments
        {
            ["city"] = "Tokyo",
            ["days"] = "2"
        };

        var directResult = await kernel.InvokeAsync(function, arguments);
        
        Console.WriteLine($"Direct API call result for Tokyo:");
        Console.WriteLine(directResult.ToString());
    }
}
