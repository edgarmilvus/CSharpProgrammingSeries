
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

# Source File: solution_exercise_8.cs
# Description: Solution for Exercise 8
# ==========================================

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.Text.Json;
using System.Text.Json.Nodes;

async Task RunRecursiveCriticAsync()
{
    var builder = Kernel.CreateBuilder();
    builder.AddOpenAIChatCompletion("gpt-4o", "api-key-here");
    var kernel = builder.Build();

    var generator = new ChatCompletionAgent(kernel, "You are a SQL expert. Write a query to: {input}");
    var critic = new ChatCompletionAgent(
        kernel: kernel,
        instructions: "You are a SQL Linter. Review the query for syntax errors. Return JSON: { 'IsValid': boolean, 'Errors': string[] }.",
        executionSettings: new OpenAIPromptExecutionSettings { ResponseFormat = "json_object" }
    );

    string userRequest = "Select the top 3 customers by total spend in 2023.";
    string currentSql = "";
    string errorFeedback = "";
    int attempts = 0;
    bool isValid = false;

    while (attempts < 3 && !isValid)
    {
        attempts++;
        Console.WriteLine($"\n--- Attempt {attempts} ---");

        // 1. Generate (with feedback if retrying)
        string prompt = errorFeedback == "" 
            ? userRequest 
            : $"Previous SQL: {currentSql}. Errors: {errorFeedback}. Rewrite to fix errors.";
        
        var sqlResponse = await generator.InvokeAsync(prompt);
        currentSql = sqlResponse.ToString();
        Console.WriteLine($"Generated SQL:\n{currentSql}");

        // 2. Critique
        var critiqueJson = await critic.InvokeAsync(currentSql);
        Console.WriteLine($"Critique: {critiqueJson}");

        // 3. Parse
        try
        {
            var json = JsonNode.Parse(critiqueJson.ToString());
            isValid = json["IsValid"]?.GetValue<bool>() ?? false;
            
            if (!isValid)
            {
                var errors = json["Errors"]?.AsArray();
                errorFeedback = string.Join(", ", errors);
            }
        }
        catch { break; }
    }

    if (isValid) Console.WriteLine("\n[Success] Valid SQL Generated.");
    else Console.WriteLine("\n[Failed] Max attempts reached.");
}
