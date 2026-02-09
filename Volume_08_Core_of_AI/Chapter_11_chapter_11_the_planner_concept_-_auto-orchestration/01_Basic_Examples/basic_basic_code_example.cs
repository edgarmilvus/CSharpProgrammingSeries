
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
using Microsoft.SemanticKernel.Skills.Core;
using System;
using System.Threading.Tasks;

// Context: A user wants to plan a simple meal for dinner.
// Problem: The user has a vague goal ("plan a dinner") but needs the AI to orchestrate
// specific skills (fetching a recipe and generating a shopping list) to achieve it.
// Solution: Use the SequentialPlanner to automatically generate a plan using available plugins.

public class Program
{
    public static async Task Main(string[] args)
    {
        // 1. Initialize the Kernel
        var kernel = new KernelBuilder()
            .WithAzureChatCompletionService(
                deploymentName: "gpt-35-turbo", // Replace with your deployment name
                endpoint: "https://your-endpoint.openai.azure.com/", // Replace with your endpoint
                apiKey: "your-api-key") // Replace with your API key
            .Build();

        // 2. Register Native Functions as Skills/Plugins
        // We are using the built-in TextSkill for demonstration, but in a real scenario,
        // these would be custom functions (e.g., GetRecipeFromDatabase, GenerateShoppingList).
        var textSkill = kernel.ImportSkill(new TextSkill(), "text");

        // 3. Define the Goal
        // The planner will interpret this natural language string and map it to the available functions.
        string goal = "Write a haiku about a cat, then capitalize it, and finally reverse the text.";

        try
        {
            // 4. Create the Planner
            // SequentialPlanner creates a step-by-step plan to achieve the goal.
            var planner = new SequentialPlanner(kernel);

            // 5. Generate the Plan
            // The planner queries the LLM to determine which functions to call and in what order.
            var plan = await planner.CreatePlanAsync(goal);

            Console.WriteLine("Generated Plan:");
            Console.WriteLine(plan.ToJson());
            Console.WriteLine("--------------------------------------------------");

            // 6. Execute the Plan
            // The kernel executes the generated plan step-by-step.
            var result = await kernel.RunAsync(plan);

            Console.WriteLine("Execution Result:");
            Console.WriteLine(result.Result);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }
}
