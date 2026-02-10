
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

// Source File: basic_basic_code_example.cs
// Description: Basic Code Example
// ==========================================

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.AI.ChatCompletion;
using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization;

// 1. Define the data structure for the plan output.
// We enforce structured output to make validation easier and to reduce hallucination risks.
public class ShoppingPlan
{
    [JsonPropertyName("items")]
    public List<string> Items { get; set; } = new();

    [JsonPropertyName("total_price")]
    public decimal TotalPrice { get; set; }

    [JsonPropertyName("currency")]
    public string Currency { get; set; } = "USD";
}

// 2. Define a plugin with a validation function.
// This simulates a business rule check (e.g., budget constraint).
public class BudgetPlugin
{
    [KernelFunction("validate_budget")]
    [Description("Checks if the total price of the shopping list is within the budget.")]
    public bool ValidateBudget(
        [Description("The total price of the items")] decimal totalPrice,
        [Description("The maximum budget allowed")] decimal budgetLimit)
    {
        return totalPrice <= budgetLimit;
    }
}

// 3. The Main Application Logic
class Program
{
    static async Task Main(string[] args)
    {
        // Initialize the Kernel (mocked for this example to be self-contained).
        // In a real scenario, you would use KernelBuilder.CreateBuilder() with Azure OpenAI or OpenAI services.
        var kernel = new KernelBuilder().Build();
        
        // Register the budget plugin
        kernel.ImportPluginFromObject(new BudgetPlugin(), "Budget");

        // Mock the AI Chat Completion Service
        // We simulate an AI that might hallucinate a high price or invalid format.
        var mockChatCompletion = new MockChatCompletionService();
        kernel.Plugins.AddFromObject(new MockAIService(mockChatCompletion));

        // Define the budget constraint
        decimal budgetLimit = 50.00m;

        Console.WriteLine($"Starting shopping plan generation with budget: ${budgetLimit}");

        // 4. The Validation Loop
        // We attempt to generate a plan and validate it. If it fails, we retry.
        int maxRetries = 3;
        int attempt = 0;
        bool isValid = false;
        ShoppingPlan? finalPlan = null;

        while (attempt < maxRetries && !isValid)
        {
            attempt++;
            Console.WriteLine($"\n--- Attempt {attempt} ---");

            try
            {
                // Step A: Generate the plan (Simulated AI Call)
                // The AI generates a JSON string representing the shopping list.
                string planJson = await mockChatCompletion.GeneratePlanAsync();
                Console.WriteLine($"AI Generated Plan: {planJson}");

                // Step B: Parse the output
                // We use System.Text.Json to strictly parse the expected structure.
                var plan = JsonSerializer.Deserialize<ShoppingPlan>(planJson);
                
                if (plan == null)
                {
                    Console.WriteLine("Error: Failed to parse plan structure.");
                    continue; // Retry
                }

                // Step C: Validate the plan using the Kernel Function
                // We invoke the registered plugin to check business logic (budget).
                var result = await kernel.InvokeAsync<bool>("Budget", "validate_budget", new()
                {
                    ["totalPrice"] = plan.TotalPrice,
                    ["budgetLimit"] = budgetLimit
                });

                if (result)
                {
                    isValid = true;
                    finalPlan = plan;
                    Console.WriteLine("Success: Plan is valid and within budget.");
                }
                else
                {
                    Console.WriteLine($"Validation Error: Total price ${plan.TotalPrice} exceeds budget ${budgetLimit}.");
                    // In a real loop, you might pass this error back to the AI to correct its next attempt.
                }
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"Parsing Error: AI output was not valid JSON. Details: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected Error: {ex.Message}");
            }
        }

        if (finalPlan != null)
        {
            Console.WriteLine($"\nFinal Approved Plan: {string.Join(", ", finalPlan.Items)} for ${finalPlan.TotalPrice}");
        }
        else
        {
            Console.WriteLine("\nFailed to generate a valid plan after max retries.");
        }
    }
}

// --- Mock Services for Demonstration Purposes ---
// These classes simulate the AI behavior without requiring external API keys.

public class MockChatCompletionService
{
    private int _callCount = 0;

    public async Task<string> GeneratePlanAsync()
    {
        await Task.Delay(100); // Simulate network latency
        _callCount++;

        // Simulating Hallucinations:
        // 1. First attempt: Hallucinates a price way over budget.
        // 2. Second attempt: Hallucinates a malformed JSON.
        // 3. Third attempt: Returns a valid, correct plan.
        
        if (_callCount == 1)
        {
            return """
            {
                "items": ["Laptop", "Mouse", "Keyboard"],
                "total_price": 999.99,
                "currency": "USD"
            }
            """;
        }
        else if (_callCount == 2)
        {
            return "This is not JSON, this is a hallucination."; // Hallucination: Wrong format
        }
        else
        {
            return """
            {
                "items": ["Notebook", "Pen"],
                "total_price": 45.00,
                "currency": "USD"
            }
            """;
        }
    }
}

public class MockAIService(MockChatCompletionService service)
{
    [KernelFunction("generate_shopping_plan")]
    [Description("Generates a shopping list based on user intent.")]
    public async Task<string> GeneratePlan()
    {
        return await service.GeneratePlanAsync();
    }
}
