
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

// Source File: solution_exercise_39.cs
// Description: Solution for Exercise 39
// ==========================================

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

async Task RunPersonalizedNewsAsync()
{
    var builder = Kernel.CreateBuilder();
    builder.AddOpenAIChatCompletion("gpt-4o", "api-key-here");
    var kernel = builder.Build();

    var summarizer = new ChatCompletionAgent(kernel, "Summarize the provided news headlines.");

    // Mock Data
    var userInterests = new[] { "Tech", "Finance" };
    var headlines = new[] {
        "Apple stock rises",
        "Football match result",
        "New AI model released",
        "Weather forecast"
    };

    // Filtering Logic
    var filteredNews = headlines.Where(h => userInterests.Any(i => h.Contains(i, StringComparison.OrdinalIgnoreCase))).ToList();

    Console.WriteLine($"Filtered News: {string.Join(", ", filteredNews)}");

    if (filteredNews.Any())
    {
        var summary = await summarizer.InvokeAsync(string.Join("\n", filteredNews));
        Console.WriteLine($"Personalized Summary: {summary}");
    }
}
