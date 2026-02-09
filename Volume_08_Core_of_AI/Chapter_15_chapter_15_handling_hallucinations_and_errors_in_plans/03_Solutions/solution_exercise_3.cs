
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
using System.Text.Json;
using System.Text.Json.Serialization;

// 1. Travel Plan Classes
public class TravelPlan
{
    public List<PlanStep> Steps { get; set; } = new();
}

public class PlanStep
{
    public string Action { get; set; } = string.Empty; // e.g., "Book Flight"
    public string Resource { get; set; } = string.Empty; // e.g., "FlightID123"
    public string Dependency { get; set; } = string.Empty; // e.g., "DestinationSet"
}

// Custom Exception
public class PlanVerificationException : Exception
{
    public string Feedback { get; }
    public PlanVerificationException(string message, string feedback) : base(message)
    {
        Feedback = feedback;
    }
}

// Reflection Result DTO
public class ReflectionResult
{
    public bool IsSafe { get; set; }
    public string Feedback { get; set; } = string.Empty;
}

// 2. Plan Reflector
public class PlanReflector
{
    private readonly Kernel _kernel;

    public PlanReflector(Kernel kernel)
    {
        _kernel = kernel;
    }

    public async Task<ReflectionResult> ReflectAsync(TravelPlan plan)
    {
        // Serialize plan
        string planJson = JsonSerializer.Serialize(plan);
        
        // Construct prompt for LLM reflection
        string prompt = $"""
            Analyze the following travel plan for safety and logical flow.
            Plan: {planJson}

            Rules:
            1. Ensure no step references a resource that doesn't exist (e.g., 'Book Hotel' requires a 'Destination').
            2. Check for prohibited actions like "Delete User Profile".
            3. Ensure logical order (e.g., Destination must be set before booking flight).

            Return JSON: {{ "isSafe": boolean, "feedback": string }}
            """;

        var result = await _kernel.InvokePromptAsync<ReflectionResult>(prompt);
        
        // Fallback if LLM returns null
        return result ?? new ReflectionResult { IsSafe = false, Feedback = "Reflection failed to return structured output." };
    }
}

// 3. Execution Flow with Regeneration Loop
public class TravelAgent
{
    private readonly Kernel _kernel;
    private readonly PlanReflector _reflector;

    public TravelAgent(Kernel kernel)
    {
        _kernel = kernel;
        _reflector = new PlanReflector(kernel);
    }

    public async Task ExecutePlanAsync()
    {
        int maxAttempts = 3;
        int attempts = 0;
        TravelPlan? currentPlan = null;

        while (attempts < maxAttempts)
        {
            attempts++;
            Console.WriteLine($"--- Generation Attempt {attempts} ---");

            // Step 1: Generate Plan (Simulated)
            currentPlan = GenerateMockPlan(attempts); 
            
            // Step 2: Reflect
            try
            {
                Console.WriteLine("Reflecting on plan...");
                var reflection = await _reflector.ReflectAsync(currentPlan);

                if (!reflection.IsSafe)
                {
                    Console.WriteLine($"Verification Failed: {reflection.Feedback}");
                    // Interactive Challenge: Inject feedback into context for next generation
                    // In a real scenario, we would pass this feedback to the planner's prompt
                    Console.WriteLine($"Injecting feedback into planner context: '{reflection.Feedback}'");
                    continue; // Regenerate
                }

                // Step 3: Execute
                Console.WriteLine("Plan Verified! Executing...");
                await ExecuteSteps(currentPlan);
                return;
            }
            catch (PlanVerificationException ex)
            {
                Console.WriteLine($"Caught exception: {ex.Feedback}");
                // Loop continues
            }
        }
        
        Console.WriteLine("Max attempts reached. Plan could not be verified.");
    }

    private TravelPlan GenerateMockPlan(int attempt)
    {
        // Simulate a planner generating a plan. 
        // Attempt 1 generates a bad plan, Attempt 2 generates a good one.
        if (attempt == 1)
        {
            return new TravelPlan
            {
                Steps = new List<PlanStep>
                {
                    new PlanStep { Action = "Delete User Profile", Resource = "User123" }, // Prohibited
                    new PlanStep { Action = "Book Flight", Resource = "FlightA", Dependency = "Destination" } // Logical error (no destination set yet)
                }
            };
        }
        return new TravelPlan
        {
            Steps = new List<PlanStep>
            {
                new PlanStep { Action = "Set Destination", Resource = "Paris" },
                new PlanStep { Action = "Book Flight", Resource = "FlightA", Dependency = "Destination" }
            }
        };
    }

    private async Task ExecuteSteps(TravelPlan plan)
    {
        foreach (var step in plan.Steps)
        {
            Console.WriteLine($"Executing: {step.Action} on {step.Resource}");
            await Task.Delay(100);
        }
    }
}

// Usage
public class Program
{
    public static async Task Main()
    {
        var kernel = Kernel.CreateBuilder().Build();
        var agent = new TravelAgent(kernel);
        await agent.ExecutePlanAsync();
    }
}
