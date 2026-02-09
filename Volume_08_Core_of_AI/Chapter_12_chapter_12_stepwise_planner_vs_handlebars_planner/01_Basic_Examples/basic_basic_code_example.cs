
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

# Source File: basic_basic_code_example.cs
# Description: Basic Code Example
# ==========================================

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Planning;
using Microsoft.SemanticKernel.Planning.Handlebars;
using Microsoft.SemanticKernel.Planning.Stepwise;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.ComponentModel;
using System.Text.Json;

// 1. SETUP: Define a simple plugin with mathematical capabilities.
public class MathPlugin
{
    [KernelFunction, Description("Adds two numbers")]
    public double Add(double number1, double number2) => number1 + number2;

    [KernelFunction, Description("Multiplies two numbers")]
    public double Multiply(double number1, double number2) => number1 * number2;
}

// 2. MAIN PROGRAM: Comparing the two planners
class Program
{
    static async Task Main(string[] args)
    {
        // Configuration: Using a local model or a mock for demonstration.
        // In production, replace with AzureOpenAIConfig or OpenAIConfig.
        var kernel = Kernel.CreateBuilder()
            .AddOpenAIChatCompletion(
                modelId: "gpt-3.5-turbo", // Or any model supporting function calling
                apiKey: "fake-key-for-demo") // Placeholder for dependency injection
            .Build();

        // Register the Math Plugin
        var mathPlugin = new MathPlugin();
        kernel.ImportPluginFromObject(mathPlugin, "Math");

        // The user request
        string request = "Calculate the square of the sum of 10 and 5";

        Console.WriteLine($"--- Request: {request} ---\n");

        // ============================================================
        // STRATEGY 1: HANDLEBARS PLANNER
        // ============================================================
        Console.WriteLine("=== HANDLEBARS PLANNER ===");
        
        var handlebarsConfig = new HandlebarsPlannerOptions
        {
            // Handlebars is strict; we limit iterations to prevent infinite loops in simple demos
            MaxIterations = 10 
        };

        var handlebarsPlanner = new HandlebarsPlanner(handlebarsConfig);

        try 
        {
            // Generate the plan (the Handlebars template)
            HandlebarsPlan handlebarsPlan = await handlebarsPlanner.CreatePlanAsync(kernel, request);
            
            Console.WriteLine("Generated Handlebars Template:");
            Console.WriteLine(handlebarsPlan.ToString()); // Prints the template string
            
            Console.WriteLine("\nExecuting Handlebars Plan...");
            
            // Execute the plan. Handlebars requires input variables if the template expects them.
            // For this simple math problem, the plan usually embeds the numbers or infers them.
            var handlebarsResult = await handlebarsPlan.InvokeAsync(kernel, new KernelArguments());
            
            Console.WriteLine($"Result: {handlebarsResult}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Handlebars Plan Failed: {ex.Message}");
            Console.WriteLine("Note: Handlebars often requires specific model support for generating valid templates.");
        }

        Console.WriteLine(new string('-', 40));

        // ============================================================
        // STRATEGY 2: STEPWISE PLANNER
        // ============================================================
        Console.WriteLine("\n=== STEPWISE PLANNER ===");

        var stepwiseConfig = new StepwisePlannerOptions
        {
            MaxIterations = 5,
            MaxTokens = 2000
        };

        var stepwisePlanner = new StepwisePlanner(kernel, stepwiseConfig);

        try
        {
            // Generate the plan (a sequence of reasoning steps)
            var stepwisePlan = stepwisePlanner.CreatePlan(request);
            
            Console.WriteLine("Stepwise Plan Created. Executing...");

            // Execute the plan
            var stepwiseResult = await stepwisePlan.InvokeAsync(kernel, new KernelArguments());

            Console.WriteLine($"Final Result: {stepwiseResult.Result}");
            
            // Stepwise Planner provides rich metadata about the steps taken
            Console.WriteLine("\n--- Steps Taken ---");
            if (stepwiseResult.StepsTaken != null)
            {
                foreach (var step in stepwiseResult.StepsTaken)
                {
                    Console.WriteLine($"- {step.Thought}");
                    if (!string.IsNullOrEmpty(step.Action))
                        Console.WriteLine($"  Action: {step.Action}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Stepwise Plan Failed: {ex.Message}");
        }
    }
}
