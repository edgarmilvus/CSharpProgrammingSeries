
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

# Source File: solution_exercise_10.cs
# Description: Solution for Exercise 10
# ==========================================

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.Diagnostics;
using System.Text.Json;

async Task RunAsyncParallelAgentsAsync()
{
    var builder = Kernel.CreateBuilder();
    builder.AddOpenAIChatCompletion("gpt-4o", "api-key-here");
    var kernel = builder.Build();

    var sentimentAgent = new ChatCompletionAgent(kernel, "Analyze sentiment of the text: {input}");
    var entityAgent = new ChatCompletionAgent(kernel, "Extract entities from the text: {input}");
    var summaryAgent = new ChatCompletionAgent(kernel, "Summarize the text: {input}");

    string inputText = "Microsoft announced new AI features in Seattle. Satya Nadella was present.";

    // 1. Sequential Baseline
    var sw = Stopwatch.StartNew();
    var s1 = await sentimentAgent.InvokeAsync(inputText);
    var s2 = await entityAgent.InvokeAsync(inputText);
    var s3 = await summaryAgent.InvokeAsync(inputText);
    sw.Stop();
    Console.WriteLine($"Sequential Time: {sw.ElapsedMilliseconds}ms");

    // 2. Parallel Execution
    sw.Restart();
    var task1 = sentimentAgent.InvokeAsync(inputText);
    var task2 = entityAgent.InvokeAsync(inputText);
    var task3 = summaryAgent.InvokeAsync(inputText);

    await Task.WhenAll(task1, task2, task3);
    sw.Stop();
    Console.WriteLine($"Parallel Time: {sw.ElapsedMilliseconds}ms");

    // Aggregation
    var result = new
    {
        Sentiment = task1.Result.ToString(),
        Entities = task2.Result.ToString(),
        Summary = task3.Result.ToString()
    };

    Console.WriteLine("\nAggregated Result:");
    Console.WriteLine(JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true }));
}
