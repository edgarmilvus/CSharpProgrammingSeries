
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

# Source File: solution_exercise_3.cs
# Description: Solution for Exercise 3
# ==========================================

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Planning;
using System;
using System.Threading.Tasks;

// Primary Skills
public class TravelSkills
{
    [KernelFunction("SearchFlights")]
    public async Task<string> SearchFlightsAsync(string destination, string date)
    {
        // Simulate API failure or empty result
        Console.WriteLine($"[Skill] Searching flights to {destination} on {date}...");
        return "No flights available."; 
    }

    [KernelFunction("BookHotel")]
    public async Task<string> BookHotelAsync(string location, string checkInDate)
    {
        Console.WriteLine($"[Skill] Booking hotel in {location} for {checkInDate}...");
        return $"Hotel booked in {location}.";
    }

    [KernelFunction("CheckWeather")]
    public async Task<string> CheckWeatherAsync(string location)
    {
        Console.WriteLine($"[Skill] Checking weather for {location}...");
        return "Sunny, 25°C";
    }
}

// Fallback Skill
public class FallbackSkill
{
    [KernelFunction("PlanStaycation")]
    public async Task<string> PlanStaycationAsync(string originalDestination)
    {
        Console.WriteLine($"[Fallback] Flight search failed. Generating Staycation plan for {originalDestination}...");
        return $"Plan B: Local staycation in {originalDestination}. Activities: Museum, Fine Dining.";
    }
}

public class ResilientPlannerExample
{
    public static async Task RunAsync()
    {
        var builder = Kernel.CreateBuilder();
        builder.AddOpenAIChatCompletion(
            modelId: "gpt-4o-mini", 
            apiKey: Environment.GetEnvironmentVariable("OPENAI_API_KEY") ?? "fake-key-for-demo"
        );
        var kernel = builder.Build();

        // 1. Register Skills
        var travelSkills = new TravelSkills();
        var fallbackSkill = new FallbackSkill();
        
        kernel.ImportPluginFromObject(travelSkills, "Travel");
        kernel.ImportPluginFromObject(fallbackSkill, "Fallback");

        // 2. Configure Planner with Custom Instructions
        // We instruct the planner to check availability before booking.
        var plannerConfig = new FunctionCallingStepwisePlannerOptions
        {
            MaxTokens = 2000,
            PromptTemplate = @"
                You are a travel planner. 
                1. First, search for flights to the destination.
                2. If flights are available, proceed to book a hotel and check weather.
                3. If flights are NOT available (e.g., the result says 'No flights'), 
                   IMMEDIATELY switch to using the 'PlanStaycation' function in the Fallback plugin.
                4. Do not attempt to book a hotel if no flight is found.
                Goal: {{$input}}
            "
        };

        var planner = new FunctionCallingStepwisePlanner(plannerConfig);

        // 3. Execute the Plan
        string goal = "Plan a weekend trip to Paris";
        Console.WriteLine($"Goal: {goal}\n");

        try 
        {
            var result = await planner.ExecuteAsync(kernel, goal);
            
            Console.WriteLine("\n--- Execution Result ---");
            Console.WriteLine(result.Result);
            
            // Inspect the internal plan steps generated
            if (result.ChatHistory != null)
            {
                Console.WriteLine("\n--- Internal Plan Steps (Simulated from LLM reasoning) ---");
                // The FunctionCallingStepwisePlanner returns a final result, 
                // but internally it decides which functions to call.
                // In this scenario, the LLM sees "No flights" and calls PlanStaycation.
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Execution failed: {ex.Message}");
        }
    }
}

// Entry point for testing
// await ResilientPlannerExample.RunAsync();
