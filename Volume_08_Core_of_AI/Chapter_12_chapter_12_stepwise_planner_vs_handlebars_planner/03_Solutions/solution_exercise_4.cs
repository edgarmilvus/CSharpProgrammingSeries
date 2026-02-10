
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

// Source File: solution_exercise_4.cs
// Description: Solution for Exercise 4
// ==========================================

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Planning;
using Microsoft.SemanticKernel.Planning.Handlebars;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.ComponentModel;
using System.Threading.Tasks;
using System;

public class HybridSupportAgent
{
    private readonly Kernel _kernel;
    private readonly StepwisePlanner _compliancePlanner;
    private readonly HandlebarsPlanner _responsePlanner;

    public HybridSupportAgent()
    {
        // Single Kernel Instance
        _kernel = Kernel.CreateBuilder()
            .AddAzureOpenAIChatCompletion(
                deploymentName: "gpt-4o-mini",
                endpoint: Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT"),
                apiKey: Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY"))
            .Build();

        // Import shared plugins
        _kernel.ImportPluginFromObject(new CompliancePlugin(), "Compliance");
        _kernel.ImportPluginFromObject(new ResponsePlugin(), "Response");

        // Initialize Planners
        _compliancePlanner = new StepwisePlanner(_kernel, new StepwisePlannerConfig
        {
            MaxIterations = 5, // Interactive Challenge: Limit iterations
            ExecutionSettings = new OpenAIPromptExecutionSettings { FunctionChoiceBehavior = FunctionChoiceBehavior.Auto() }
        });

        _responsePlanner = new HandlebarsPlanner(new HandlebarsPlannerConfig
        {
            AllowLoops = true
        });
    }

    public async Task<string> ProcessRequest(string userMessage)
    {
        // 1. Intent Detection (Simple Semantic Function)
        var intent = await DetectIntentAsync(userMessage);
        Console.WriteLine($"Detected Intent: {intent}");

        // 2. Routing Logic
        if (intent.Contains("ComplianceCheck") || intent.Contains("Refund"))
        {
            // Route to Stepwise for Validation
            Console.WriteLine("Routing to Stepwise Planner (Compliance)...");
            
            var plan = _compliancePlanner.CreatePlan(userMessage);
            
            // Interactive Challenge: Plan Executor Middleware (Simulated)
            if (plan.Steps.Count > 5)
            {
                Console.WriteLine("Plan too complex (>5 steps). Aborting Stepwise.");
                // Fallback to Handlebars generic response
                return await GenerateFallbackResponseAsync("Complex compliance check required.");
            }

            try
            {
                var result = await plan.InvokeAsync(_kernel);
                // Check for specific failure logic in result if needed
                if (result.ToString().Contains("Violation"))
                {
                    return "Compliance Violation Detected. Request denied.";
                }
                return $"Compliance Validated. Result: {result}";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Stepwise Execution Error: {ex.Message}");
                return await GenerateFallbackResponseAsync("System error during validation.");
            }
        }
        else
        {
            // Route to Handlebars for Response Generation
            Console.WriteLine("Routing to Handlebars Planner (Response)...");
            
            // We inspect the plan before execution as per requirements
            var plan = await _responsePlanner.CreatePlanAsync(_kernel, userMessage);
            Console.WriteLine($"Generated Template Length: {plan.RawPlan.Length}");
            
            // Execute
            return await plan.InvokeAsync(_kernel);
        }
    }

    private async Task<string> DetectIntentAsync(string input)
    {
        // Simplified intent detection for demo purposes
        if (input.Contains("refund") || input.Contains("policy")) return "ComplianceCheck";
        return "GenerateResponse";
    }

    private async Task<string> GenerateFallbackResponseAsync(string context)
    {
        // Use a simple Handlebars template for fallback
        var prompt = $"Generate a polite response explaining that: {context}";
        var function = _kernel.CreateFunctionFromPrompt(prompt);
        return await _kernel.InvokeAsync<string>(function);
    }
}

// Mock Plugins
public class CompliancePlugin
{
    [Description("Checks eligibility for refund.")]
    public string CheckRefundEligibility(string orderId)
    {
        // Simulate logic
        return "Eligible";
    }
}

public class ResponsePlugin
{
    [Description("Generates empathetic response.")]
    public string GenerateResponse(string context)
    {
        return $"Here is a helpful response regarding: {context}";
    }
}

// Usage Example
public class Program
{
    public static async Task Main()
    {
        var agent = new HybridSupportAgent();
        
        // Test Case 1: Compliance
        var response1 = await agent.ProcessRequest("Can I refund order #123?");
        Console.WriteLine($"Output: {response1}\n");

        // Test Case 2: General Inquiry
        var response2 = await agent.ProcessRequest("Tell me about your services.");
        Console.WriteLine($"Output: {response2}");
    }
}
