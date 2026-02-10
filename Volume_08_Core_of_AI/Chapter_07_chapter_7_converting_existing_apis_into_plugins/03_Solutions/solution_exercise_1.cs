
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

// Source File: solution_exercise_1.cs
// Description: Solution for Exercise 1
// ==========================================

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Plugins.OpenApi;
using System.Text.Json;
using System.Net.Http;
using System.Text.RegularExpressions;

// 1. Data Modeling: Define records for deserialization
public record WeatherForecast(
    string City,
    DateTime Date,
    string Condition,
    double TemperatureC,
    double PrecipitationProbability // Nullable handled via default value
);

public record WeatherResponse(
    List<WeatherForecast> Forecast,
    string Status
);

// 2. Semantic Wrapper: Helper to parse natural language
public static class NaturalLanguageParser
{
    public static string ExtractCity(string input)
    {
        // Simple regex to find a capitalized word sequence (e.g., "Tokyo", "New York")
        // In a real scenario, use an NLP library like Microsoft.Recognizers.Text
        var match = Regex.Match(input, @"in\s+([A-Z][a-z]+(?:\s+[A-Z][a-z]+)*)", RegexOptions.IgnoreCase);
        return match.Success ? match.Groups[1].Value : throw new ArgumentException("Could not determine city from request.");
    }
}

// 3. Plugin Implementation
public class WeatherPlugin
{
    private readonly Kernel _kernel;
    private readonly KernelFunction _forecastFunction;

    public WeatherPlugin(Kernel kernel, string openApiFilePath)
    {
        _kernel = kernel;
        
        // Load the OpenAPI spec
        // Note: In a real app, we would use Kernel.ImportPluginFromOpenApiAsync
        // For this exercise, we simulate the function creation logic.
        
        // Mocking the OpenAPI import process for the solution structure:
        // Actual implementation would look like:
        // var plugin = await kernel.ImportPluginFromOpenApiAsync("WeatherPlugin", new Uri(openApiFilePath), new OpenApiFunctionExecutionParameters());
        // _forecastFunction = plugin["GetForecastByCity"];
        
        // Since we cannot run actual HTTP calls without the spec file, 
        // we define the function manually to demonstrate the required logic.
        _forecastFunction = CreateForecastFunction();
    }

    private KernelFunction CreateForecastFunction()
    {
        // Define the function signature
        return KernelFunctionFactory.CreateFromMethod(
            async (string city) =>
            {
                // Simulate API call logic (In reality, this is handled by the OpenApiRunner)
                var apiKey = Environment.GetEnvironmentVariable("WEATHER_API_KEY");
                if (string.IsNullOrEmpty(apiKey)) throw new Exception("API Key missing.");

                // Simulate HTTP Request
                using var client = new HttpClient();
                client.DefaultRequestHeaders.Add("X-API-Key", apiKey);
                
                // Simulate endpoint call: GET /forecast/{city}
                // var response = await client.GetAsync($"https://api.weather.com/forecast/{city}");
                
                // MOCK RESPONSE FOR EXERCISE SOLUTION
                var mockJson = $$"""
                {
                    "Status": "Success",
                    "Forecast": [
                        { "City": "{{city}}", "Date": "2023-10-27", "Condition": "Rain", "TemperatureC": 18.5, "PrecipitationProbability": 0.8 },
                        { "City": "{{city}}", "Date": "2023-10-28", "Condition": "Sunny", "TemperatureC": 22.0, "PrecipitationProbability": 0.0 }
                    ]
                }
                """;
                
                // Error Simulation Logic (Uncomment to test error handling)
                // if (city.Equals("Unknown", StringComparison.OrdinalIgnoreCase)) 
                //     throw new HttpRequestException("404", null, System.Net.HttpStatusCode.NotFound);

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var result = JsonSerializer.Deserialize<WeatherResponse>(mockJson, options);

                if (result == null || result.Status != "Success")
                    throw new Exception("API returned invalid data.");

                return result;
            },
            new KernelFunctionMetadata("GetForecastByCity")
            {
                Parameters = new List<KernelParameterMetadata> 
                { 
                    new KernelParameterMetadata("city") { ParameterType = typeof(string), Description = "The city name" } 
                },
                ReturnParameter = new KernelReturnParameterMetadata { ParameterType = typeof(WeatherResponse) }
            }
        );
    }

    // 4. Error Handling Wrapper
    public async Task<WeatherResponse> GetForecastSafeAsync(string city)
    {
        try
        {
            return await _kernel.InvokeAsync<WeatherResponse>(_forecastFunction, new KernelArguments { ["city"] = city });
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            throw new Exception($"Error: City '{city}' not found.");
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            throw new Exception("Error: Authentication failed. Check API Key.");
        }
    }

    // 5. Semantic Wrapper Function
    public async Task<string> GetWeatherSummary(string request)
    {
        try
        {
            // Step 1: Parse Natural Language
            string city = NaturalLanguageParser.ExtractCity(request);
            
            // Step 2: Call underlying plugin
            var forecast = await GetForecastSafeAsync(city);
            
            // Step 3: Format Summary
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"Weather Summary for {city}:");
            foreach (var day in forecast.Forecast)
            {
                sb.AppendLine($"- {day.Date:ddd}: {day.Condition}, {day.TemperatureC}°C (Precip: {day.PrecipitationProbability:P0})");
            }
            
            return sb.ToString();
        }
        catch (Exception ex)
        {
            return $"Failed to get weather: {ex.Message}";
        }
    }
}

// Usage Example
/*
var kernel = new Kernel();
var weatherPlugin = new WeatherPlugin(kernel, "weather-api.yaml");
var summary = await weatherPlugin.GetWeatherSummary("What's the weather like in Tokyo next week?");
Console.WriteLine(summary);
*/
