
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

# Source File: solution_exercise_19.cs
# Description: Solution for Exercise 19
# ==========================================

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

async Task RunChainOfDensityAsync()
{
    var builder = Kernel.CreateBuilder();
    builder.AddOpenAIChatCompletion("gpt-4o", "api-key-here");
    var kernel = builder.Build();

    var sourceText = "Apple Inc. announced the iPhone 15 in Cupertino. It features a new A16 chip.";
    
    var generator = new ChatCompletionAgent(kernel, "Generate a verbose summary of the text.");
    var extractor = new ChatCompletionAgent(kernel, "Extract specific entities (People, Places, Orgs) from the source text that are NOT in the current summary.");
    var rewriter = new ChatCompletionAgent(kernel, "Rewrite the summary to include the missing entities, keeping length similar.");

    // Step 1: Initial Summary
    var summary = await generator.InvokeAsync(sourceText);
    Console.WriteLine($"V1 Summary: {summary}");

    // Step 2 & 3: Loop (Chain of Density)
    for (int i = 0; i < 2; i++)
    {
        var missingEntities = await extractor.InvokeAsync($"Source: {sourceText}\nCurrent Summary: {summary}");
        Console.WriteLine($"Missing Entities: {missingEntities}");

        summary = await rewriter.InvokeAsync($"Current Summary: {summary}\nMissing Entities: {missingEntities}");
        Console.WriteLine($"V{i+2} Summary: {summary}");
    }
}
