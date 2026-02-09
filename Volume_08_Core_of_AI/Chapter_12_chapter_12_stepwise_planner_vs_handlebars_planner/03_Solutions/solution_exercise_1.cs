
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

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Planning;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.ComponentModel;
using System.Threading.Tasks;
using System;

public class SmartHomeAutomation
{
    public static async Task ExecuteAsync()
    {
        // 1. Kernel Setup
        var kernel = Kernel.CreateBuilder()
            // Using AzureOpenAIChatCompletion for realistic planning
            .AddAzureOpenAIChatCompletion(
                deploymentName: "gpt-4o-mini", 
                endpoint: Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT"),
                apiKey: Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY"))
            .Build();

        // 2. Function Definition (Native C# Plugins)
        // Note: Using Description attributes helps the planner understand function purpose
        kernel.ImportPluginFromObject(new HomeAutomationPlugin(), "home");

        // 3. Planner Configuration
        var plannerConfig = new StepwisePlannerConfig
        {
            MaxIterations = 10,
            MinIterationTimeMs = 1000,
            ExecutionSettings = new OpenAIPromptExecutionSettings
            {
                // Enable capturing intermediate function results
                FunctionChoiceBehavior = FunctionChoiceBehavior.Auto() 
            }
        };

        var planner = new StepwisePlanner(kernel, plannerConfig);

        // 4. Execution
        var goal = "Prepare the living room for movie night. Dim the lights to 10%, set the thermostat to 20 degrees Celsius in 'Cool' mode, and start the projector on HDMI 1.";
        
        Console.WriteLine($"Planning Goal: {goal}\n");

        try 
        {
            var plan = planner.CreatePlan(goal);
            
            // 5. Analysis: Print generated plan steps
            Console.WriteLine("--- Generated Plan Steps ---");
            foreach (var step in plan.Steps)
            {
                Console.WriteLine($"- {step.PluginName}.{step.Name}({string.Join(", ", step.Parameters)})");
            }

            Console.WriteLine("\n--- Executing Plan ---");
            var result = await plan.InvokeAsync(kernel);

            Console.WriteLine($"\nFinal Execution Result: {result}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Execution failed: {ex.Message}");
        }

        // --- Interactive Challenge: Safety Constraint ---
        Console.WriteLine("\n\n--- Interactive Challenge: Safety Mode ---");
        
        // Modify the plugin to enforce safety mode
        // (In a real scenario, we'd update the plugin code. Here we simulate the logic)
        var safeGoal = "Dim lights to 5% with safety mode enabled.";
        
        // We simulate the planner reacting to a constraint violation
        // by catching the KernelException during execution.
        try
        {
            // Re-initialize kernel with the updated plugin logic (simulated)
            // For this exercise, we will manually simulate the failure logic
            // because updating the plugin dynamically in one snippet is complex.
            
            Console.WriteLine($"Attempting: {safeGoal}");
            
            // Simulating the failure logic of DimLights(5%, isSafeMode: true)
            // The planner would ideally try to call the function, get an error, 
            // and attempt to adjust. We catch the exception here.
            
            throw new KernelException("Safety Constraint Violation: Brightness 5% is below the safe limit of 20%.");
        }
        catch (KernelException ex)
        {
            Console.WriteLine($"[Fallback Mechanism Triggered]: {ex.Message}");
            Console.WriteLine("System Action: Logging failure and requesting user clarification or defaulting to 20%.");
        }
    }
}

// Plugin Definition
public class HomeAutomationPlugin
{
    [Description("Dims the lights to a specific brightness level.")]
    public string DimLights(int brightnessLevel, bool isSafeMode = false)
    {
        if (isSafeMode && brightnessLevel < 20)
        {
            throw new ArgumentException("Brightness cannot be below 20% in Safe Mode.");
        }
        return $"Lights dimmed to {brightnessLevel}%.";
    }

    [Description("Sets the thermostat temperature and mode.")]
    public string SetThermostat(double temperature, string mode)
    {
        return $"Thermostat set to {temperature}°C in {mode} mode.";
    }

    [Description("Starts the projector on a specific input source.")]
    public string StartProjector(string inputSource)
    {
        return $"Projector started on {inputSource}.";
    }
}
