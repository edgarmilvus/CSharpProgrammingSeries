
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

// Source File: solution_exercise_4.cs
// Description: Solution for Exercise 4
// ==========================================

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

public class WeatherReporter
{
    private static readonly Random _random = new Random();

    public static async Task RunAsync()
    {
        var builder = Kernel.CreateBuilder();
        builder.AddOpenAIChatCompletion("gpt-4o-mini", Environment.GetEnvironmentVariable("OPENAI_API_KEY"));
        var kernel = builder.Build();

        var cities = new List<string> { "London", "Tokyo", "New York" };
        var resultsLog = new List<string>();
        var successfulReports = new List<string>();

        Console.WriteLine("Starting Weather Check Sequence...\n");

        // Process each city with retry logic
        foreach (var city in cities)
        {
            try
            {
                // Call the retry wrapper
                string weatherData = await CallWithRetry(kernel, city, maxRetries: 3);
                
                // If we got here without exception, it was a success
                successfulReports.Add($"The weather in {city} is {weatherData}.");
                resultsLog.Add($"{city}: Success on final attempt.");
            }
            catch (Exception ex)
            {
                // Failed after all retries
                resultsLog.Add($"{city}: Failed after 3 attempts. Error: {ex.Message}");
                successfulReports.Add($"The weather in {city} is currently unavailable.");
            }
        }

        // 3. Aggregation via Agent
        Console.WriteLine("\n--- Generating Summary Report ---");
        var reporterAgent = new ChatCompletionAgent(kernel)
        {
            Name = "Reporter",
            Instructions = "Summarize the weather data provided into a concise, readable paragraph."
        };

        string inputContext = string.Join("\n", successfulReports);
        var summary = await reporterAgent.InvokeAsync(inputContext);

        // 4. Output
        Console.WriteLine("\nExecution Log:");
        foreach (var log in resultsLog)
        {
            Console.WriteLine(log);
        }

        Console.WriteLine("\nFinal Report:");
        Console.WriteLine(summary.Message.Content);
    }

    // The Retry Logic Wrapper
    private static async Task<string> CallWithRetry(Kernel kernel, string cityName, int maxRetries)
    {
        int attempt = 0;
        while (attempt < maxRetries)
        {
            try
            {
                attempt++;
                // Simulate Unreliable Tool
                var result = await SimulateUnreliableWeatherCheck(cityName);
                
                // If successful, return immediately
                return result;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"  [Attempt {attempt} for {cityName}] Failed: {ex.Message}");
                
                // If this was the last attempt, rethrow to be caught by the outer loop
                if (attempt == maxRetries) 
                {
                    throw; 
                }

                // Exponential Backoff: 1s, 2s, 4s...
                double delaySeconds = Math.Pow(2, attempt - 1);
                Console.WriteLine($"  -> Retrying in {delaySeconds} seconds...");
                await Task.Delay(TimeSpan.FromSeconds(delaySeconds));
            }
        }
        return "Weather data unavailable";
    }

    // Simulated Unreliable Tool (50% failure rate)
    private static async Task<string> SimulateUnreliableWeatherCheck(string city)
    {
        await Task.Delay(200); // Simulate network latency
        
        // 50% chance of failure
        if (_random.Next(0, 2) == 0)
        {
            throw new HttpRequestException("Network timeout");
        }

        // Return mock weather
        var conditions = new[] { "Sunny", "Cloudy", "Rainy", "Windy" };
        return conditions[_random.Next(conditions.Length)];
    }
}
