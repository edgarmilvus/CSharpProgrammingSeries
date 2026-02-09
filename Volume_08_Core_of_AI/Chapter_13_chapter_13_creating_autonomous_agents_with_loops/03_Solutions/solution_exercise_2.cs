
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

# Source File: solution_exercise_2.cs
# Description: Solution for Exercise 2
# ==========================================

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Planning;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class DynamicOrchestrator
{
    // Mock Database State
    private static bool isDatabaseAvailable = false; // Toggle this to test logic

    public static async Task RunAsync()
    {
        var builder = Kernel.CreateBuilder();
        builder.AddOpenAIChatCompletion("gpt-4o-mini", Environment.GetEnvironmentVariable("OPENAI_API_KEY"));
        
        // Add a native function for Mock Data generation
        builder.Plugins.AddFromType<ReportTools>();
        
        var kernel = builder.Build();

        // 1. Generate Plan
        var planner = new FunctionCallingStepwisePlanner(new FunctionCallingStepwisePlannerOptions { 
            MaxIterations = 5, 
            AllowLooping = false 
        });

        string request = "Prepare a report on recent sales data.";
        Console.WriteLine($"Generating plan for: '{request}'...");

        // Note: FunctionCallingStepwisePlanner returns a result containing the plan
        var planResult = await planner.ExecuteAsync(kernel, request);
        
        // 2. Inspect Plan
        // In a real scenario, we might parse the Plan object. Here we inspect the steps text.
        // We will simulate the "Inspection" by checking the steps provided in the result.
        var steps = planResult.Steps; 
        
        Console.WriteLine("\n--- Generated Plan Steps ---");
        foreach (var step in steps)
        {
            Console.WriteLine($"- {step.Description}");
        }

        // 3. Conditional Logic & Execution Loop
        Console.WriteLine("\n--- Executing Plan ---");
        var executionResults = new List<string>();

        foreach (var step in steps)
        {
            try
            {
                string stepName = step.Description.ToLower();
                bool isFetchStep = stepName.Contains("fetch") || stepName.Contains("database") || stepName.Contains("sales");

                if (isFetchStep)
                {
                    if (isDatabaseAvailable)
                    {
                        Console.WriteLine("Status: Database is available. Executing data fetch...");
                        // We execute the actual step
                        await step.InvokeAsync(kernel); 
                        executionResults.Add($"[Success] {step.Description}");
                    }
                    else
                    {
                        Console.WriteLine("Status: Database NOT available. Skipping fetch and using mock data...");
                        // 4. Dynamic Modification: Replace fetch with mock function
                        var mockResult = await kernel.InvokeAsync<string>("ReportTools", "GenerateMockData");
                        executionResults.Add($"[Modified] {step.Description} -> Mock Data: {mockResult}");
                    }
                }
                else
                {
                    // Normal execution for non-fetch steps
                    await step.InvokeAsync(kernel);
                    executionResults.Add($"[Success] {step.Description}");
                }
            }
            catch (Exception ex)
            {
                // 5. Error Handling
                Console.WriteLine($"Error in step '{step.Description}': {ex.Message}");
                executionResults.Add($"[Failed] {step.Description}");
            }
        }

        // Output
        Console.WriteLine("\n--- Final Aggregated Status ---");
        foreach (var res in executionResults)
        {
            Console.WriteLine(res);
        }
    }
}

// Helper Class for Tools
public class ReportTools
{
    [KernelFunction]
    public string GenerateMockData()
    {
        return "Q1 Sales: $10,000 (Simulated)";
    }
}
