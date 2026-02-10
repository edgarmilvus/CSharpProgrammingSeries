
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

// Source File: solution_exercise_29.cs
// Description: Solution for Exercise 29
// ==========================================

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.Text.Json;

async Task RunBatchProcessorAsync()
{
    var builder = Kernel.CreateBuilder();
    builder.AddOpenAIChatCompletion("gpt-4o", "api-key-here");
    var kernel = builder.Build();

    var moderator = new ChatCompletionAgent(
        kernel: kernel,
        instructions: "Review the following comments. Return a JSON array where each element is the status ('Safe' or 'Unsafe') for the corresponding comment.",
        executionSettings: new OpenAIPromptExecutionSettings { ResponseFormat = "json_object" }
    );

    var comments = new List<string> {
        "Hello, nice post!",
        "This is spam!",
        "Great information.",
        "Buy cheap meds here.",
        "I disagree respectfully."
    };

    // Batch size 5 (sending all in one go for this example)
    int batchSize = 5;
    var batch = comments.Take(batchSize).ToList();

    // Construct prompt with all comments
    string prompt = "Comments to review:\n";
    for (int i = 0; i < batch.Count; i++)
    {
        prompt += $"{i + 1}. {batch[i]}\n";
    }

    var result = await moderator.InvokeAsync(prompt);
    Console.WriteLine($"Batch Result:\n{result}");

    // Parse JSON to verify
    try 
    {
        var json = JsonNode.Parse(result.ToString());
        Console.WriteLine("Parsed successfully.");
    }
    catch { Console.WriteLine("Failed to parse JSON."); }
}
