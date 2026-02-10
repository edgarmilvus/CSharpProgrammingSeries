
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

// Source File: solution_exercise_2.cs
// Description: Solution for Exercise 2
// ==========================================

// File: WeatherPlugin.cs
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;
using System.Text.Json;

public class WeatherPlugin
{
    private readonly IHttpClientFactory _httpClientFactory;

    // Constructor for Dependency Injection
    public WeatherPlugin(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    [KernelFunction("get_current_temperature")]
    public string GetCurrentTemperature(string location)
    {
        // Handle edge cases
        if (string.IsNullOrWhiteSpace(location))
            return "Location is required.";

        // Simulated logic (fallback if no HTTP client is available)
        // In a real scenario, we would use the injected client
        var hash = location.Aggregate(0, (a, b) => a + b);
        return $"{(hash % 100)}°F";
    }

    [KernelFunction("get_forecast")]
    public IEnumerable<WeatherForecast> GetForecast(string location)
    {
        if (string.IsNullOrWhiteSpace(location))
            return Enumerable.Empty<WeatherForecast>();

        // Return simple records
        return [
            new WeatherForecast("Monday", 72),
            new WeatherForecast("Tuesday", 75),
            new WeatherForecast("Wednesday", 68)
        ];
    }

    // Interactive Challenge: Real HTTP Request
    [KernelFunction("get_live_temperature")]
    public async Task<string> GetLiveTemperatureAsync(string location)
    {
        if (string.IsNullOrWhiteSpace(location)) return "Unknown location";

        try 
        {
            var client = _httpClientFactory.CreateClient();
            // Using Open-Meteo API (no key required) for demonstration
            var response = await client.GetAsync($"https://api.open-meteo.com/v1/forecast?latitude=40.71&longitude=-74.00&current_weather=true");
            response.EnsureSuccessStatusCode();
            
            var content = await response.Content.ReadAsStringAsync();
            // Parse JSON (simplified for brevity)
            using var doc = JsonDocument.Parse(content);
            var temp = doc.RootElement.GetProperty("current_weather").GetProperty("temperature").GetDouble();
            
            return $"{temp}°C";
        }
        catch (Exception ex)
        {
            return $"Error fetching weather: {ex.Message}";
        }
    }
}

public record WeatherForecast(string Day, int Temp);

// File: Program.cs
using Microsoft.SemanticKernel;
using Microsoft.Extensions.DependencyInjection;

// Setup Dependency Injection container
var services = new ServiceCollection();
services.AddHttpClient(); // Required for IHttpClientFactory
services.AddTransient<WeatherPlugin>(); // Register the plugin

var serviceProvider = services.BuildServiceProvider();

// Initialize Kernel
var kernel = Kernel.CreateBuilder()
    .AddAzureOpenAIChatCompletion("deployment-name", "https://endpoint", "key") // Placeholder
    .Build();

// Register the plugin using the service provider to satisfy DI
var weatherPlugin = serviceProvider.GetRequiredService<WeatherPlugin>();
var plugin = kernel.ImportPluginFromObject(weatherPlugin, "weather");

Console.WriteLine($"Plugin '{plugin.Name}' registered with {plugin.Functions.Count} functions.");

// Verify registration
foreach (var func in plugin.Functions)
{
    Console.WriteLine($" - Function: {func.Name}");
}
