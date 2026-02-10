
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

// Source File: solution_exercise_26.cs
// Description: Solution for Exercise 26
// ==========================================

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

async Task RunParallelDebateAsync()
{
    var builder = Kernel.CreateBuilder();
    builder.AddOpenAIChatCompletion("gpt-4o", "api-key-here");
    var kernel = builder.Build();

    // Fan-out Agents
    var parisAgent = new ChatCompletionAgent(kernel, "Research attractions in Paris.");
    var romeAgent = new ChatCompletionAgent(kernel, "Research attractions in Rome.");
    var berlinAgent = new ChatCompletionAgent(kernel, "Research attractions in Berlin.");

    // Aggregator Agent
    var aggregatorAgent = new ChatCompletionAgent(kernel, "Combine these lists into a single itinerary, sorted by geography.");

    // 1. Fan-out (Parallel)
    var taskParis = parisAgent.InvokeAsync("Paris");
    var taskRome = romeAgent.InvokeAsync("Rome");
    var taskBerlin = berlinAgent.InvokeAsync("Berlin");

    await Task.WhenAll(taskParis, taskRome, taskBerlin);

    // 2. Fan-in (Aggregation)
    var parisResult = taskParis.Result.ToString();
    var romeResult = taskRome.Result.ToString();
    var berlinResult = taskBerlin.Result.ToString();

    Console.WriteLine($"Paris: {parisResult}");
    Console.WriteLine($"Rome: {romeResult}");
    Console.WriteLine($"Berlin: {berlinResult}");

    // 3. Aggregate
    var combinedContext = $"Paris: {parisResult}\nRome: {romeResult}\nBerlin: {berlinResult}";
    var itinerary = await aggregatorAgent.InvokeAsync(combinedContext);

    Console.WriteLine($"\nFinal Itinerary:\n{itinerary}");
}
