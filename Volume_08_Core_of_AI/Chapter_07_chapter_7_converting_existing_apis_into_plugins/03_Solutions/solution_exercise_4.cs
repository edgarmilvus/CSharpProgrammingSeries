
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

# Source File: solution_exercise_4.cs
# Description: Solution for Exercise 4
# ==========================================

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Plugins.OpenApi;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// Result Classes
public record Flight(string Airline, DateTime Departure, double Price);
public record Hotel(string Name, string City, double PricePerNight);
public record TravelItinerary(List<Flight> Flights, List<Hotel> Hotels);

public class TravelOrchestrator
{
    private readonly Kernel _kernel;

    public TravelOrchestrator(Kernel kernel)
    {
        _kernel = kernel;
    }

    [KernelFunction("PlanTrip")]
    public async Task<TravelItinerary> PlanTripAsync(string request)
    {
        // 1. Parse Request (Simulated for exercise focus)
        // Assumption: Request format "Find flights from JFK to LHR and hotels near London."
        // We extract: Origin=JFK, Destination=LHR, City=London
        var origin = "JFK"; 
        var destination = "LHR"; 
        var city = "London";

        // 2. Context Management
        var context = new KernelArguments();

        // 3. Sequential Execution with Conditional Logic
        List<Flight> flights = null;
        int dayOffset = 0;
        int attempts = 0;

        // Loop for alternative dates
        while (attempts < 3) // Try current day, +1 day, +2 day
        {
            var searchDate = DateTime.UtcNow.AddDays(dayOffset);
            
            // Call Flight Plugin
            // In a real scenario, we would use _kernel.InvokeAsync<FlightResult>
            // Mocking the result for the solution structure
            var flightResult = await SearchFlights(origin, destination, searchDate);
            
            if (flightResult.Any())
            {
                flights = flightResult;
                break; // Found flights, exit loop
            }
            
            attempts++;
            dayOffset = attempts; // Shift day forward
        }

        // If no flights found after retries, we might throw or return empty. 
        // For this exercise, we proceed with hotels even if flights fail (or return empty list).

        // Store flights in context (or simply use local variable for chaining)
        // context["flights"] = flights; 

        // 4. Call Hotel Plugin (dependent on destination city)
        // Mocking the result
        var hotels = await SearchHotels(city);

        // 5. Aggregate
        return new TravelItinerary(flights ?? new List<Flight>(), hotels);
    }

    // Mocks for Flight API (OpenAPI style)
    private async Task<List<Flight>> SearchFlights(string origin, string dest, DateTime date)
    {
        // Simulate API call
        await Task.Delay(50); 
        // Logic: If date is today, return empty (to trigger retry logic), else return flights
        if (date.Date == DateTime.UtcNow.Date) return new List<Flight>(); 
        
        return new List<Flight> 
        { 
            new Flight("Delta", date.AddHours(10), 450.00),
            new Flight("BA", date.AddHours(12), 500.00)
        };
    }

    // Mocks for Hotel API (Legacy style)
    private async Task<List<Hotel>> SearchHotels(string city)
    {
        await Task.Delay(50);
        return new List<Hotel>
        {
            new Hotel("Grand Plaza", city, 200.00),
            new Hotel("City Inn", city, 120.00)
        };
    }
}

// Usage Example
/*
var kernel = new Kernel();
var orchestrator = new TravelOrchestrator(kernel);
var itinerary = await orchestrator.PlanTripAsync("Find flights from JFK to LHR...");
*/
