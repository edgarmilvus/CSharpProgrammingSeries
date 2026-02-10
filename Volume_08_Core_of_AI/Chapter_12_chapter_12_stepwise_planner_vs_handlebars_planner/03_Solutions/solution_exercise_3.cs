
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

// Source File: solution_exercise_3.cs
// Description: Solution for Exercise 3
// ==========================================

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Planning;
using Microsoft.SemanticKernel.Planning.Handlebars;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.Diagnostics;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System;

public class DataPipelineComparison
{
    private static readonly List<SalesRecord> _dataset = new()
    {
        new SalesRecord { Region = "North", Revenue = 100 },
        new SalesRecord { Region = "North", Revenue = 200 },
        new SalesRecord { Region = "South", Revenue = 50 }
    };

    public static async Task ExecuteAsync()
    {
        var kernel = Kernel.CreateBuilder()
            .AddAzureOpenAIChatCompletion(
                deploymentName: "gpt-4o-mini",
                endpoint: Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT"),
                apiKey: Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY"))
            .Build();

        // 2. Plugin Definition
        kernel.ImportPluginFromObject(new DataProcessingPlugin(_dataset), "Data");

        var goal = "Filter sales data for 'Region: North', calculate the average revenue, and generate a summary report.";
        
        Console.WriteLine($"Goal: {goal}\n");

        // --- Step 1: Stepwise Planner Analysis ---
        Console.WriteLine("--- Stepwise Planner ---");
        var stepwiseStopwatch = Stopwatch.StartNew();
        
        var stepwisePlanner = new StepwisePlanner(kernel, new StepwisePlannerConfig 
        { 
            MaxIterations = 5,
            // Interactive Challenge: Disable auto-invoke for manual control
            ExecutionSettings = new OpenAIPromptExecutionSettings { FunctionChoiceBehavior = FunctionChoiceBehavior.Auto() } 
        });
        
        // Note: We simulate the manual execution logic here for the retry mechanism
        var stepwisePlan = stepwisePlanner.CreatePlan(goal);
        int retryCount = 0;
        bool success = false;
        
        while (retryCount < 3 && !success)
        {
            try 
            {
                // In a real scenario with AutoInvoke=false, we would iterate through stepwisePlan.Steps
                // and invoke them manually. Here we invoke the plan directly but wrap in try/catch.
                await stepwisePlan.InvokeAsync(kernel);
                success = true;
            }
            catch (Exception ex)
            {
                retryCount++;
                Console.WriteLine($"Stepwise Attempt {retryCount} failed: {ex.Message}");
                // In manual mode, we would identify the failed step here
            }
        }
        
        stepwiseStopwatch.Stop();
        Console.WriteLine($"Stepwise Execution Time: {stepwiseStopwatch.ElapsedMilliseconds}ms");
        Console.WriteLine($"Stepwise Invocations: {stepwisePlan.Steps.Count} (approx)");
        Console.WriteLine($"Retries: {retryCount}");

        // --- Step 2: Handlebars Planner Analysis ---
        Console.WriteLine("\n--- Handlebars Planner ---");
        var handlebarsStopwatch = Stopwatch.StartNew();
        
        var handlebarsPlanner = new HandlebarsPlanner(new HandlebarsPlannerConfig { AllowLoops = true });
        var handlebarsPlan = await handlebarsPlanner.CreatePlanAsync(kernel, goal);
        
        // Execution
        var result = await handlebarsPlan.InvokeAsync(kernel);
        
        handlebarsStopwatch.Stop();
        Console.WriteLine($"Handlebars Execution Time: {handlebarsStopwatch.ElapsedMilliseconds}ms");
        Console.WriteLine($"Handlebars Plan String Length: {handlebarsPlan.RawPlan.Length} chars");
        Console.WriteLine($"Result: {result}");

        // --- Interactive Challenge: Exception Handling ---
        // The CalculateAverage function throws if empty.
        // We observe Stepwise stops immediately (or retries via our loop).
        // Handlebars usually generates a template that might contain error text or fallbacks.
    }
}

public class DataProcessingPlugin
{
    private readonly List<SalesRecord> _data;
    public DataProcessingPlugin(List<SalesRecord> data) => _data = data;

    [Description("Filters data by region.")]
    public List<SalesRecord> FilterData(string criteria)
    {
        // Simple parsing for "Region: X"
        var region = criteria.Replace("Region: ", "").Trim();
        return _data.FindAll(d => d.Region == region);
    }

    [Description("Calculates average revenue.")]
    public double CalculateAverage(List<SalesRecord> data)
    {
        if (data == null || data.Count == 0)
            throw new InvalidOperationException("Cannot calculate average of empty list.");
        
        double sum = 0;
        foreach (var d in data) sum += d.Revenue;
        return sum / data.Count;
    }

    [Description("Generates a report.")]
    public string GenerateReport(double average, string criteria)
    {
        return $"Report for {criteria}: Average Revenue is {average:C}.";
    }
}

public class SalesRecord
{
    public string Region { get; set; } = "";
    public double Revenue { get; set; }
}
