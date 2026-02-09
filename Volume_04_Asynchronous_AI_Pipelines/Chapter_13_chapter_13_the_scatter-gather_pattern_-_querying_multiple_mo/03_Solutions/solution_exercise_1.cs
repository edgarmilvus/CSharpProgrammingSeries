
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

# Source File: solution_exercise_1.cs
# Description: Solution for Exercise 1
# ==========================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

// 1. Define Data Models
public class WeatherForecast
{
    public string Condition { get; set; }
    public int TemperatureC { get; set; }
}

public class FlightAvailability
{
    public string FlightNumber { get; set; }
    public bool IsAvailable { get; set; }
}

public class TravelReport
{
    public WeatherForecast Weather { get; set; }
    public List<string> Attractions { get; set; }
    public FlightAvailability Flights { get; set; }
    public string Warnings { get; set; }
}

// 2. Mock Service Class
public class TravelService
{
    private static readonly Random _rng = new Random();

    // Simulates Weather Service (Reliable)
    public async Task<WeatherForecast> GetWeatherForecastAsync(string destination)
    {
        await Task.Delay(100); // Simulate network latency
        return new WeatherForecast { Condition = "Sunny", TemperatureC = 22 };
    }

    // Simulates Attraction Service (Reliable)
    public async Task<List<string>> GetTopAttractionsAsync(string destination)
    {
        await Task.Delay(150); 
        return new List<string> { "Central Park", "Museum of Art", "River Walk" };
    }

    // Simulates Flight Service (Unstable)
    public async Task<FlightAvailability> CheckFlightAvailabilityAsync(string destination)
    {
        await Task.Delay(200);
        // Simulate failure 50% of the time
        if (_rng.Next(0, 2) == 0)
        {
            throw new HttpRequestException("Flight service unavailable due to timeout.");
        }
        return new FlightAvailability { FlightNumber = "FL123", IsAvailable = true };
    }
}

public class TravelAssistant
{
    private readonly TravelService _service = new TravelService();

    public async Task<TravelReport> GenerateReportAsync(string destination)
    {
        // 3. Start all tasks (Scatter)
        // We store them in variables to access them individually later if needed
        var weatherTask = _service.GetWeatherForecastAsync(destination);
        var attractionsTask = _service.GetTopAttractionsAsync(destination);
        var flightTask = _service.CheckFlightAvailabilityAsync(destination);

        // 4. Wait for all to complete (Gather)
        // Even if one faults, WhenAll will throw an AggregateException. 
        // To handle faults individually, we must wrap tasks or await them differently.
        // However, the requirement asks to use Task.WhenAll. 
        // To prevent a single fault from crashing the whole process before aggregation,
        // we can use Task.WhenAll on tasks that are already handling exceptions, 
        // or we can await them individually in a safe way.
        
        // Strategy: Let WhenAll run. If it throws, we catch and inspect.
        // But a cleaner way for "Best Effort" is to await the specific tasks 
        // after WhenAll completes (or faults), checking their status.
        
        Task[] tasks = { weatherTask, attractionsTask, flightTask };
        
        try 
        {
            await Task.WhenAll(tasks);
        }
        catch 
        {
            // We catch the aggregate exception here to prevent the method from crashing.
            // We will inspect individual tasks below to see which one failed.
        }

        // 5. Handle results safely
        WeatherForecast weather = null;
        List<string> attractions = null;
        FlightAvailability flights = null;
        string warnings = string.Empty;

        // Helper to safely get result or log error
        async Task<T> GetResultAsync<T>(Task<T> task, string serviceName) where T : class
        {
            if (task.IsFaulted)
            {
                warnings += $"{serviceName} failed. ";
                return null;
            }
            return await task;
        }

        weather = await GetResultAsync(weatherTask, "Weather");
        attractions = await GetResultAsync(attractionsTask, "Attractions");
        flights = await GetResultAsync(flightTask, "Flight");

        // 6. Aggregate into concrete class
        return new TravelReport
        {
            Weather = weather,
            Attractions = attractions,
            Flights = flights,
            Warnings = warnings
        };
    }
}

// Example Usage
public class Program
{
    public static async Task Main()
    {
        var assistant = new TravelAssistant();
        var report = await assistant.GenerateReportAsync("Paris");
        
        Console.WriteLine($"Weather: {report.Weather?.Condition}");
        Console.WriteLine($"Attractions: {string.Join(", ", report.Attractions ?? new List<string>())}");
        Console.WriteLine($"Flights: {report.Flights?.FlightNumber ?? "None"}");
        Console.WriteLine($"Warnings: {report.Warnings}");
    }
}
