
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
using System.Threading.Tasks;

public class DataProcessor
{
    [KernelFunction("FilterData")]
    public async Task<List<string>> FilterDataAsync(string criteria)
    {
        Console.WriteLine($"[Skill] Filtering data with criteria: {criteria}");
        // Simulate returning data
        return new List<string> { "ID:101", "ID:102" };
    }

    [KernelFunction("ProcessItem")]
    public async Task<string> ProcessItemAsync(string id)
    {
        Console.WriteLine($"[Skill] Processing item: {id}");
        return $"Processed {id} successfully.";
    }
}

public class PlanAdaptationExample
{
    public static async Task RunAsync()
    {
        var builder = Kernel.CreateBuilder();
        builder.AddOpenAIChatCompletion(
            modelId: "gpt-4o-mini", 
            apiKey: Environment.GetEnvironmentVariable("OPENAI_API_KEY") ?? "fake-key-for-demo"
        );
        var kernel = builder.Build();

        // 1. Register Native Skills
        var dataProcessor = new DataProcessor();
        kernel.ImportPluginFromObject(dataProcessor, "Data");

        // 2. Create a Semantic Skill (Prompt Template) for Analysis
        // This function takes data and decides if processing is needed.
        string analyzePrompt = @"
            Analyze the following data list. 
            If the list contains items, return 'PROCESS' followed by the items separated by commas.
            If the list is empty, return 'SKIP'.
            Data: {{$data}}
        ";
        
        var analyzeFunction = kernel.CreateFunctionFromPrompt(
            analyzePrompt, 
            new OpenAIPromptExecutionSettings { MaxTokens = 50 },
            "AnalyzeResults"
        );

        // 3. Define the Goal
        string goal = "Filter data for 'active users' and process the resulting items.";

        // 4. Generate Initial Plan
        // We use a SequentialPlanner to generate the base structure.
        var planner = new SequentialPlanner();
        var plan = await planner.CreatePlanAsync(kernel, goal);

        Console.WriteLine("Initial Plan:");
        Console.WriteLine(plan.ToJson());
        Console.WriteLine();

        // 5. Dynamic Adaptation Logic
        // In a real scenario, we might execute the first step, analyze, and then modify the plan.
        // Here, we simulate the adaptation by executing the Filter step manually, 
        // then asking the LLM to generate a new plan based on that context.
        
        // Execute Filter Step
        var filterResult = await kernel.InvokeAsync(dataProcessor, "FilterData", new KernelArguments("active users"));
        Console.WriteLine($"Filter Result: {string.Join(", ", filterResult)}");

        // Analyze Result
        var analysis = await kernel.InvokeAsync(analyzeFunction, new KernelArguments { ["data"] = string.Join(", ", filterResult) });
        Console.WriteLine($"Analysis Result: {analysis}");

        // Adapt Plan based on Analysis
        if (analysis.ToString().Contains("PROCESS"))
        {
            Console.WriteLine("Adaptation: Items found. Appending processing steps.");
            
            // Create a new plan specifically for processing the filtered items
            // Note: In a full implementation, we would modify the existing Plan object's Steps collection.
            // Here, we demonstrate generating a sub-plan for the identified task.
            string adaptedGoal = $"Process items: {string.Join(", ", filterResult)}";
            var adaptedPlan = await planner.CreatePlanAsync(kernel, adaptedGoal);
            
            Console.WriteLine("Adapted Plan Steps:");
            foreach (var step in adaptedPlan.Steps)
            {
                Console.WriteLine($"- {step.Name}");
            }

            await adaptedPlan.InvokeAsync(kernel);
        }
        else
        {
            Console.WriteLine("Adaptation: No items found. Skipping processing.");
        }
    }
}

// Entry point for testing
// await PlanAdaptationExample.RunAsync();
