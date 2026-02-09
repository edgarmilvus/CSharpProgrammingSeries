
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

# Source File: solution_exercise_5.cs
# Description: Solution for Exercise 5
# ==========================================

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Planning;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

public class AsyncDataFetcher
{
    [KernelFunction("FetchUserPreferences")]
    public async Task<string> FetchUserPreferencesAsync(string userId)
    {
        Console.WriteLine($"[Skill] Fetching User Preferences (Start)...");
        await Task.Delay(1000); // Simulate network delay
        Console.WriteLine($"[Skill] Fetching User Preferences (Done)");
        return "Dark Mode, Metric Units";
    }

    [KernelFunction("FetchGlobalConfig")]
    public async Task<string> FetchGlobalConfigAsync()
    {
        Console.WriteLine($"[Skill] Fetching Global Config (Start)...");
        await Task.Delay(1000); // Simulate network delay
        Console.WriteLine($"[Skill] Fetching Global Config (Done)");
        return "Config v2.1";
    }

    [KernelFunction("FetchWeatherForecast")]
    public async Task<string> FetchWeatherForecastAsync(string location)
    {
        Console.WriteLine($"[Skill] Fetching Weather (Start)...");
        await Task.Delay(1000); // Simulate network delay
        Console.WriteLine($"[Skill] Fetching Weather (Done)");
        return "Sunny, 22°C";
    }
}

public class ParallelExecutionExample
{
    public static async Task RunAsync()
    {
        var builder = Kernel.CreateBuilder();
        builder.AddOpenAIChatCompletion(
            modelId: "gpt-4o-mini", 
            apiKey: Environment.GetEnvironmentVariable("OPENAI_API_KEY") ?? "fake-key-for-demo"
        );
        var kernel = builder.Build();

        var fetcher = new AsyncDataFetcher();
        kernel.ImportPluginFromObject(fetcher, "Data");

        // 1. Generate a Sequential Plan
        var planner = new SequentialPlanner();
        string goal = "Get user preferences, global config, and weather forecast for New York.";
        
        var plan = await planner.CreatePlanAsync(kernel, goal);

        Console.WriteLine("Generated Sequential Plan:");
        foreach (var step in plan.Steps)
        {
            Console.WriteLine($"- {step.Name}");
        }

        // 2. Analyze Dependencies for Parallel Execution
        // In a real scenario, we would parse the Plan's Input/Output variables.
        // For this exercise, we assume the planner generated independent steps.
        // We will manually group them for the demonstration.
        
        Console.WriteLine("\n--- Executing in Parallel ---");
        var sw = Stopwatch.StartNew();

        // Group 1: Independent steps (No shared inputs/outputs based on goal analysis)
        var task1 = kernel.InvokeAsync(fetcher, "FetchUserPreferences", new KernelArguments("user123"));
        var task2 = kernel.InvokeAsync(fetcher, "FetchGlobalConfig");
        var task3 = kernel.InvokeAsync(fetcher, "FetchWeatherForecast", new KernelArguments("New York"));

        // Execute concurrently
        await Task.WhenAll(task1, task2, task3);

        sw.Stop();
        Console.WriteLine($"\nParallel Execution Time: {sw.ElapsedMilliseconds}ms (Expected ~1000ms for concurrent tasks)");

        // 3. Compare with Sequential Execution (Simulated)
        Console.WriteLine("\n--- Executing Sequentially (Comparison) ---");
        sw.Restart();
        
        await kernel.InvokeAsync(fetcher, "FetchUserPreferences", new KernelArguments("user123"));
        await kernel.InvokeAsync(fetcher, "FetchGlobalConfig");
        await kernel.InvokeAsync(fetcher, "FetchWeatherForecast", new KernelArguments("New York"));

        sw.Stop();
        Console.WriteLine($"\nSequential Execution Time: {sw.ElapsedMilliseconds}ms (Expected ~3000ms)");
    }
}

// Entry point for testing
// await ParallelExecutionExample.RunAsync();
