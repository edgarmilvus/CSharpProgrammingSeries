
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
using Microsoft.SemanticKernel.Planning;
using System;
using System.Linq;
using System.Threading.Tasks;

public class FinancialSkills
{
    [KernelFunction("GetStockPrice")]
    public async Task<double> GetStockPriceAsync(string symbol)
    {
        Console.WriteLine($"[Skill] Fetching price for {symbol}...");
        return 350.50;
    }

    [KernelFunction("CalculateMovingAverage")]
    public async Task<double> CalculateMovingAverageAsync(string symbol, int period)
    {
        Console.WriteLine($"[Skill] Calculating MA({period}) for {symbol}...");
        return 345.00;
    }

    [KernelFunction("GenerateReport")]
    public async Task<string> GenerateReportAsync(string data)
    {
        Console.WriteLine($"[Skill] Generating report...");
        return $"Report: {data}";
    }
}

public class FinancialPlannerExample
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
        var financialSkills = new FinancialSkills();
        kernel.ImportPluginFromObject(financialSkills, "Finance");

        // 2. Define Custom Prompt
        string customPrompt = @"
            You are a financial analyst assistant. 
            STRICT RULES:
            1. You must retrieve stock data using 'GetStockPrice' before generating any report.
            2. You may calculate 'CalculateMovingAverage' optionally.
            3. Never call 'GenerateReport' without data input.
            4. Always verify data integrity by ensuring price > 0.
            
            Goal: {{$input}}
        ";

        // 3. Configure Planner
        var plannerConfig = new FunctionCallingStepwisePlannerOptions
        {
            MaxTokens = 2000, // Limit response size
            MinIterationTime = TimeSpan.FromMilliseconds(500), // Throttle execution
            PromptTemplate = customPrompt
        };

        var planner = new FunctionCallingStepwisePlanner(plannerConfig);
        string goal = "Analyze Microsoft stock (MSFT) for the last 30 days and generate a summary report.";

        // 4. Execute with Validation
        // Note: FunctionCallingStepwisePlanner handles the loop internally.
        // To implement "Validation before execution" as requested, we would typically 
        // generate a plan structure first (using SequentialPlanner) or inspect the LLM's intent.
        // However, FCStepwisePlanner is an agentic loop. 
        // Here we demonstrate the configuration and a manual validation check on a hypothetical plan structure.
        
        Console.WriteLine($"Goal: {goal}");
        Console.WriteLine("Config: MaxTokens={plannerConfig.MaxTokens}, CustomPrompt injected.");

        try 
        {
            // Execute
            var result = await planner.ExecuteAsync(kernel, goal);
            Console.WriteLine($"\nResult: {result.Result}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}

// Helper for manual validation (Conceptual)
public static class PlanValidator
{
    public static bool Validate(Plan plan)
    {
        // Check if GenerateReport is called without prior data retrieval
        var generateReportStep = plan.Steps.FirstOrDefault(s => s.Name == "GenerateReport");
        if (generateReportStep != null)
        {
            var previousSteps = plan.Steps.TakeWhile(s => s != generateReportStep);
            bool hasDataSource = previousSteps.Any(s => s.Name == "GetStockPrice" || s.Name == "CalculateMovingAverage");
            
            if (!hasDataSource)
            {
                Console.WriteLine("Validation Failed: GenerateReport called without data source.");
                return false;
            }
        }
        return true;
    }
}

// Entry point for testing
// await FinancialPlannerExample.RunAsync();
