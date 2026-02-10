
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

// ==========================================
// Native Functions: Calling C# Code from LLMs
// Basic Code Example
// ==========================================

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization;

// 1. Define the data structure for our native function.
// This class represents the input parameters required for the function.
// We use System.Text.Json for serialization, which is the default in modern .NET.
public class WeatherRequest
{
    [JsonPropertyName("city")]
    [Description("The name of the city to retrieve weather for.")]
    public string City { get; set; } = string.Empty;

    [JsonPropertyName("unit")]
    [Description("The unit of temperature: 'Celsius' or 'Fahrenheit'. Defaults to Celsius.")]
    public string Unit { get; set; } = "Celsius";
}

// 2. Define the response data structure.
// This represents the output of our native function.
public class WeatherResponse
{
    [JsonPropertyName("city")]
    public string City { get; set; } = string.Empty;

    [JsonPropertyName("temperature")]
    public double Temperature { get; set; }

    [JsonPropertyName("unit")]
    public string Unit { get; set; } = string.Empty;

    [JsonPropertyName("condition")]
    public string Condition { get; set; } = string.Empty;
}

// 3. Create the Native Plugin Class.
// This class contains the actual C# logic we want the LLM to invoke.
public class WeatherPlugin
{
    // The [KernelFunction] attribute marks this method as invokable by the Semantic Kernel.
    // The [Description] attribute is crucial: it tells the LLM what this function does.
    [KernelFunction, Description("Retrieves the current weather for a specified city.")]
    public async Task<WeatherResponse> GetWeatherAsync(
        // Parameter binding: The kernel maps the LLM's arguments to these parameters.
        [Description("The city to get weather for")] string city,
        [Description("The unit of temperature")] string unit = "Celsius")
    {
        // Simulate a database or API call.
        // In a real app, this would be an HttpClient call to a weather service.
        await Task.Delay(100); // Simulate network latency

        // Mock logic for demonstration purposes.
        // Deterministic code execution happens here.
        var random = new Random();
        double temp = random.NextDouble() * 40 - 10; // Random temp between -10 and 30
        string condition = random.Next(0, 3) switch
        {
            0 => "Sunny",
            1 => "Cloudy",
            _ => "Rainy"
        };

        // Convert temperature if requested (simple logic for example)
        if (unit.Equals("Fahrenheit", StringComparison.OrdinalIgnoreCase))
        {
            temp = (temp * 9 / 5) + 32;
        }

        return new WeatherResponse
        {
            City = city,
            Temperature = Math.Round(temp, 1),
            Unit = unit,
            Condition = condition
        };
    }
}

// 4. Main Execution Context
// This demonstrates how to register the plugin and invoke it via the Kernel.
class Program
{
    static async Task Main(string[] args)
    {
        // Setup: Initialize the Kernel.
        // We use a mock connector here to avoid needing real API keys for the example.
        var kernel = Kernel.CreateBuilder()
            .AddOpenAIChatCompletion(
                modelId: "gpt-4o-mini", 
                apiKey: "fake-api-key-for-demo") // Placeholder
            .Build();

        // Step A: Import the Native Plugin.
        // The Kernel scans the class for [KernelFunction] attributes.
        var plugin = kernel.ImportPluginFromObject(new WeatherPlugin(), "Weather");

        // Step B: Prepare arguments for the function.
        // The Semantic Kernel handles JSON serialization of these arguments.
        var arguments = new KernelArguments
        {
            ["city"] = "Seattle",
            ["unit"] = "Celsius"
        };

        Console.WriteLine("--- Invoking Native Function Directly ---");
        
        // Step C: Execute the function.
        // This bypasses the LLM and calls the C# code directly.
        // This is useful for testing or deterministic execution paths.
        var result = await kernel.InvokeAsync(plugin["GetWeatherAsync"], arguments);

        // Step D: Process the result.
        // The result is automatically deserialized from JSON (or the raw object) back to a usable format.
        Console.WriteLine($"Result: {result}");
        
        // Note: In a full agentic flow, the LLM would decide to call this function.
        // The flow would look like this:
        // 1. User: "What's the weather in Seattle?"
        // 2. LLM: Analyzes intent -> identifies 'GetWeatherAsync' function is needed.
        // 3. LLM: Generates JSON arguments: { "city": "Seattle", "unit": "Celsius" }.
        // 4. Semantic Kernel: Deserializes JSON -> invokes C# method.
        // 5. C# Method: Executes logic -> returns WeatherResponse object.
        // 6. Semantic Kernel: Serializes response -> sends to LLM.
        // 7. LLM: Natural language response: "It's currently 22°C and Sunny in Seattle."
    }
}
