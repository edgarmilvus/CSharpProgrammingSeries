
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
using Microsoft.SemanticKernel.Functions;
using System.ComponentModel;
using System.Text.Json;
using System.Threading.Tasks;

// Problem Context: 
// Imagine a customer support chatbot that processes user complaints. 
// Before sending a complaint to the backend system, we must ensure:
// 1. No sensitive data (like credit card numbers) is leaked.
// 2. The input is valid (not empty).
// 3. We log the interaction for auditing purposes.
// 
// Instead of cluttering the business logic with validation and logging code,
// we use Semantic Kernel's Function Filters to intercept the execution flow.

// Step 1: Define a Custom Filter
// This class implements IFunctionPreFilter to intercept the function before it runs.
public class InputValidationFilter : IFunctionPreFilter
{
    // This method is called before any kernel function executes.
    public async ValueTask OnFunctionInvocationAsync(FunctionInvocationContext context, CancellationToken cancellationToken)
    {
        // Retrieve the input from the context arguments.
        // We use 'context.Arguments' which is a dictionary of string key-value pairs.
        if (context.Arguments.TryGetValue("complaint", out var complaintObj) && complaintObj is string complaint)
        {
            // Business Logic: Check for empty input.
            if (string.IsNullOrWhiteSpace(complaint))
            {
                // We can modify the result directly to short-circuit the execution.
                // Here, we return a user-friendly error message without calling the actual function.
                context.Result = new FunctionResult(context.Function, "Error: Complaint cannot be empty.");
                return; // Stop execution here.
            }

            // Business Logic: Check for sensitive data (e.g., a pattern looking like a credit card).
            // In a real scenario, use regex or a dedicated PII detection service.
            if (complaint.Contains("1234-5678-9012-3456"))
            {
                // Sanitize the input by replacing the sensitive part.
                complaint = complaint.Replace("1234-5678-9012-3456", "[REDACTED]");
                
                // Update the context arguments so the downstream function receives the clean data.
                context.Arguments["complaint"] = complaint;
            }
        }

        // We must call await context.ProceedAsync() to allow the actual function to run.
        // If we returned early (like in the empty check), we skip this.
        await context.ProceedAsync(cancellationToken);
    }

    // Priority determines the order of execution if multiple filters are registered.
    // Lower numbers run first. We want validation to happen early.
    public int Order => 0;
}

// Step 2: Define a Post-Execution Filter
// This class implements IFunctionPostFilter to intercept the result after the function runs.
public class TelemetryFilter : IFunctionPostFilter
{
    public async ValueTask OnFunctionInvocationAsync(FunctionInvocationContext context, CancellationToken cancellationToken)
    {
        // This runs AFTER the actual function logic has completed.
        // We access the result via context.Result.
        
        // Log the interaction details (simulated with Console.WriteLine).
        Console.WriteLine($"[Telemetry] Function '{context.Function.Name}' executed.");
        Console.WriteLine($"[Telemetry] Result Status: {(context.Result.IsSuccess ? "Success" : "Failure")}");
        
        // We can inspect or modify the result here if needed (e.g., masking output).
        // For this example, we just pass it through.
        await context.ProceedAsync(cancellationToken);
    }

    // We want telemetry to run after validation and the main function.
    public int Order => 100; 
}

// Step 3: Define the Kernel Function (The "Tool" the agent uses)
public class SupportSystem
{
    [KernelFunction("process_complaint")]
    [Description("Processes a customer complaint and logs it to the database.")]
    public string ProcessComplaint(string complaint)
    {
        // This represents the core business logic.
        // It assumes the input is already validated and sanitized.
        return $"Complaint processed successfully: '{complaint}'";
    }
}

// Step 4: Main Execution Block
class Program
{
    static async Task Main(string[] args)
    {
        // Initialize the Kernel builder.
        var builder = Kernel.CreateBuilder();
        
        // Add a simple chat completion service (required by the kernel, though we aren't using AI prompts here).
        // Note: In a real scenario, you would add AzureOpenAI or OpenAI here.
        // For this purely functional example, we rely on direct function calls.
        builder.Services.AddLogging(c => c.AddConsole().SetMinimumLevel(LogLevel.Warning));

        // Build the kernel.
        var kernel = builder.Build();

        // Register our custom filters.
        // Semantic Kernel automatically detects classes implementing the filter interfaces.
        kernel.FunctionInvocationFilters.Add(new InputValidationFilter());
        kernel.FunctionInvocationFilters.Add(new TelemetryFilter());

        // Register the plugin containing our kernel function.
        kernel.Plugins.AddFromType<SupportSystem>("Support");

        // --- SCENARIO 1: Valid Input ---
        Console.WriteLine("--- Test 1: Valid Complaint ---");
        var result1 = await kernel.InvokeAsync("Support", "process_complaint", new KernelArguments { ["complaint"] = "My package arrived late." });
        Console.WriteLine($"Output: {result1}\n");

        // --- SCENARIO 2: Empty Input (Should be caught by Pre-Filter) ---
        Console.WriteLine("--- Test 2: Empty Complaint ---");
        var result2 = await kernel.InvokeAsync("Support", "process_complaint", new KernelArguments { ["complaint"] = "" });
        Console.WriteLine($"Output: {result2}\n");

        // --- SCENARIO 3: Sensitive Data (Should be sanitized by Pre-Filter) ---
        Console.WriteLine("--- Test 3: Complaint with Sensitive Data ---");
        var result3 = await kernel.InvokeAsync("Support", "process_complaint", new KernelArguments { ["complaint"] = "I paid with card 1234-5678-9012-3456 and it failed." });
        Console.WriteLine($"Output: {result3}\n");
    }
}
