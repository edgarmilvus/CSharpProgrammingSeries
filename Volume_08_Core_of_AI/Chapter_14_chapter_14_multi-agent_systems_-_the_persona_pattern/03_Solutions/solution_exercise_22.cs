
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

# Source File: solution_exercise_22.cs
# Description: Solution for Exercise 22
# ==========================================

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

async Task RunTranslatorBridgeAsync()
{
    var builder = Kernel.CreateBuilder();
    builder.AddOpenAIChatCompletion("gpt-4o", "api-key-here");
    var kernel = builder.Build();

    var specAgent = new ChatCompletionAgent(kernel, "Write feature specs in English.");
    var translatorAgent = new ChatCompletionAgent(kernel, "Translate text to Japanese or Spanish.");
    var devAgent = new ChatCompletionAgent(kernel, "You read Japanese. Output implementation plan in Japanese.");
    var qaAgent = new ChatCompletionAgent(kernel, "You read Spanish. Output test cases in Spanish.");

    // 1. Spec in English
    var spec = await specAgent.InvokeAsync("Feature: Dark Mode toggle.");
    Console.WriteLine($"English Spec: {spec}");

    // 2. Translate to Japanese for Dev
    var japaneseSpec = await translatorAgent.InvokeAsync($"Translate to Japanese: {spec}");
    Console.WriteLine($"Japanese Spec: {japaneseSpec}");

    var devPlan = await devAgent.InvokeAsync(japaneseSpec.ToString());
    Console.WriteLine($"Dev Plan (Japanese): {devPlan}");

    // 3. Translate to Spanish for QA
    var spanishSpec = await translatorAgent.InvokeAsync($"Translate to Spanish: {spec}");
    Console.WriteLine($"Spanish Spec: {spanishSpec}");

    var qaCases = await qaAgent.InvokeAsync(spanishSpec.ToString());
    Console.WriteLine($"QA Cases (Spanish): {qaCases}");
}
