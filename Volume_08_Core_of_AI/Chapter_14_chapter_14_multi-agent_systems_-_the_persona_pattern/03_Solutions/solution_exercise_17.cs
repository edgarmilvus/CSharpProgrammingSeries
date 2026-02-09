
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

# Source File: solution_exercise_17.cs
# Description: Solution for Exercise 17
# ==========================================

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

async Task RunCostOptimizationRouterAsync()
{
    // Setup multiple kernels (simulated by different execution settings or real endpoints)
    var builder = Kernel.CreateBuilder();
    builder.AddOpenAIChatCompletion("gpt-4o", "api-key-here"); // Expensive
    var kernelExpensive = builder.Build();

    var classifier = new ChatCompletionAgent(kernelExpensive, "Classify complexity of query: 'High', 'Medium', or 'Low'.");
    
    // Mock Agents for different models
    var expensiveAgent = new ChatCompletionAgent(kernelExpensive, "You are GPT-4o. Provide detailed reasoning.");
    
    // Cost tracking
    double totalCost = 0;
    double cost4o = 0.03; // per 1k tokens (mock)
    double cost35 = 0.001; // per 1k tokens (mock)

    string[] queries = { "Write a detailed essay on quantum physics", "Is Paris in France?" };

    foreach (var query in queries)
    {
        Console.WriteLine($"\nQuery: {query}");
        
        // 1. Classify
        var complexity = await classifier.InvokeAsync(query);
        Console.WriteLine($"Complexity: {complexity}");

        string response = "";
        double estimatedTokens = query.Length / 4.0; // Rough estimate

        // 2. Route & Cost
        if (complexity.ToString().Contains("High"))
        {
            response = (await expensiveAgent.InvokeAsync(query)).ToString();
            totalCost += estimatedTokens * cost4o / 1000;
            Console.WriteLine("Model: GPT-4o");
        }
        else
        {
            // Simulate cheap model invocation
            response = "Simple answer generated.";
            totalCost += estimatedTokens * cost35 / 1000;
            Console.WriteLine("Model: GPT-3.5-Turbo");
        }
        
        Console.WriteLine($"Est. Cost: ${estimatedTokens * cost4o / 1000:F6}");
    }

    Console.WriteLine($"\nTotal Estimated Cost: ${totalCost:F6}");
}
