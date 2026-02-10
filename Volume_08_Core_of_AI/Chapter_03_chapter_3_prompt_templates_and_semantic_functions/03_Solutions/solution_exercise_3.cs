
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

// Source File: solution_exercise_3.cs
// Description: Solution for Exercise 3
// ==========================================

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Planning;
using System.ComponentModel;

// 1. Native Plugin Definition
public class TravelPlugins
{
    // Hardcoded coordinates for demonstration
    private static readonly Dictionary<string, (double Lat, double Lon)> _cities = new()
    {
        ["Paris"] = (48.8566, 2.3522),
        ["Lyon"] = (45.7640, 4.8357)
    };

    [KernelFunction, Description("Calculate distance between two cities in kilometers")]
    public double CalculateDistance(
        [Description("The origin city")] string origin, 
        [Description("The destination city")] string destination)
    {
        if (!_cities.ContainsKey(origin) || !_cities.ContainsKey(destination))
            return 0; // Fallback

        var (lat1, lon1) = _cities[origin];
        var (lat2, lon2) = _cities[destination];

        // Haversine Formula
        const double R = 6371; // Earth radius in km
        var dLat = ToRadians(lat2 - lat1);
        var dLon = ToRadians(lon2 - lon1);
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return R * c;
    }

    [KernelFunction, Description("Get current weather for a city")]
    public string GetWeather([Description("The city name")] string city)
    {
        // Simulated API response
        return $"Sunny, 25°C in {city}";
    }

    private static double ToRadians(double deg) => deg * Math.PI / 180;
}

class Program
{
    static async Task Main(string[] args)
    {
        var builder = Kernel.CreateBuilder();
        
        // Add Planner Services (Required for Function Calling Stepwise Planner)
        builder.Services.AddFunctionCallingStepwisePlanner();
        
        // Add AI Connector (Mocked for this example, replace with real one)
        // builder.AddOpenAIChatCompletion(...);
        
        var kernel = builder.Build();

        // 2. Import Native Plugin
        kernel.ImportPluginFromObject(new TravelPlugins(), "Travel");

        // 3. Define Semantic Function (Inline for simplicity)
        // Note: In a Planner scenario, we don't usually manually invoke this. 
        // The Planner generates the plan to call native functions, then we might use a semantic function to format the final output.
        // However, the exercise asks to pass results manually first, then use Planner.
        
        // MANUAL EXECUTION FLOW
        Console.WriteLine("--- Manual Execution Flow ---");
        var origin = "Paris";
        var destination = "Lyon";
        
        // Retrieve native functions manually
        var distanceFunc = kernel.Plugins["Travel"]["CalculateDistance"];
        var weatherFunc = kernel.Plugs["Travel"]["GetWeather"];
        
        // Invoke Native Functions
        var distanceResult = await kernel.InvokeAsync(distanceFunc, new() { ["origin"] = origin, ["destination"] = destination });
        var weatherResult = await kernel.InvokeAsync(weatherFunc, new() { ["city"] = destination });
        
        // Generate Itinerary using Semantic Function
        var itineraryPrompt = "Based on the distance of {{distance}} km and weather conditions of {{weather}}, suggest a packing list and travel mode.";
        var itineraryFn = kernel.CreateFunctionFromPrompt(itineraryPrompt);
        
        var finalPlan = await kernel.InvokeAsync(itineraryFn, new() 
        { 
            ["distance"] = distanceResult, 
            ["weather"] = weatherResult 
        });
        
        Console.WriteLine(finalPlan);

        // 4. PLANNER EXECUTION FLOW (Interactive Challenge)
        Console.WriteLine("\n--- Planner Execution Flow ---");
        
        var planner = new FunctionCallingStepwisePlanner(new FunctionCallingStepwisePlannerOptions { 
            MaxIterations = 5, 
            MaxTokens = 2000 
        });

        // The LLM sees the user query and the available functions
        var query = $"Plan a trip from {origin} to {destination}.";
        
        try 
        {
            // The planner will internally call the LLM, which decides to call CalculateDistance and GetWeather
            var result = await planner.ExecuteAsync(kernel, query);
            
            Console.WriteLine("Planner Result:");
            Console.WriteLine(result.Result);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Planner failed: {ex.Message}");
        }
    }
}
