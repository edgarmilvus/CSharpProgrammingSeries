
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

# Source File: solution_exercise_14.cs
# Description: Solution for Exercise 14
# ==========================================

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

async Task RunCompetitiveAgentsAsync()
{
    var builder = Kernel.CreateBuilder();
    builder.AddOpenAIChatCompletion("gpt-4o", "api-key-here");
    var kernel = builder.Build();

    var creativeAgent = new ChatCompletionAgent(kernel, "Generate a catchy slogan for a coffee brand.");
    var criticalAgent = new ChatCompletionAgent(kernel, "Critique the slogan. Be harsh on clarity and memorability.");
    var refinerAgent = new ChatCompletionAgent(kernel, "Rewrite the slogan to address the critique.");

    // Step 1: Creative generates V1
    var sloganV1 = await creativeAgent.InvokeAsync("Coffee");
    Console.WriteLine($"V1 Slogan: {sloganV1}");

    // Step 2: Critical critiques V1
    var critique = await criticalAgent.InvokeAsync(sloganV1.ToString());
    Console.WriteLine($"Critique: {critique}");

    // Step 3: Refiner generates V2
    var refinementPrompt = $"Original: {sloganV1}\nCritique: {critique}\nRewrite:";
    var sloganV2 = await refinerAgent.InvokeAsync(refinementPrompt);
    Console.WriteLine($"V2 Slogan: {sloganV2}");
}
