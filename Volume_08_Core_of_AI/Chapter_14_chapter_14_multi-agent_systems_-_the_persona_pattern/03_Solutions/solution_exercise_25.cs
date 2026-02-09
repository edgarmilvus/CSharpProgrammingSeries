
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

# Source File: solution_exercise_25.cs
# Description: Solution for Exercise 25
# ==========================================

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.Text.Json;

async Task RunSelfHealingWorkflowAsync()
{
    var builder = Kernel.CreateBuilder();
    builder.AddOpenAIChatCompletion("gpt-4o", "api-key-here");
    var kernel = builder.Build();

    var generator = new ChatCompletionAgent(kernel, "Write Python code to call a weather API.");
    var validator = new ChatCompletionAgent(
        kernel: kernel,
        instructions: "Check the code for syntax errors. Return JSON: { 'IsValid': boolean, 'Error': string }.",
        executionSettings: new OpenAIPromptExecutionSettings { ResponseFormat = "json_object" }
    );

    string code = "";
    int attempts = 0;
    bool isValid = false;

    while (attempts < 3 && !isValid)
    {
        attempts++;
        Console.WriteLine($"\n--- Attempt {attempts} ---");

        // Generate (or Regenerate)
        string prompt = attempts == 1 ? "Write Python code." : $"Fix this code: {code}. Error: {lastError}";
        code = (await generator.InvokeAsync(prompt)).ToString();
        Console.WriteLine($"Code:\n{code}");

        // Validate
        var validationJson = await validator.InvokeAsync(code);
        var json = JsonNode.Parse(validationJson.ToString());
        
        isValid = json["IsValid"]?.GetValue<bool>() ?? false;
        string lastError = json["Error"]?.ToString() ?? "";

        if (!isValid) Console.WriteLine($"Validation Failed: {lastError}");
    }

    if (isValid) Console.WriteLine("\n[Healed Successfully]");
    else Console.WriteLine("\n[Healing Failed]");
}
