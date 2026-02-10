
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

// Source File: solution_exercise_36.cs
// Description: Solution for Exercise 36
// ==========================================

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

async Task RunMeetingSchedulerAsync()
{
    var builder = Kernel.CreateBuilder();
    builder.AddOpenAIChatCompletion("gpt-4o", "api-key-here");
    var kernel = builder.Build();

    var solverAgent = new ChatCompletionAgent(kernel, "Find a 30-minute slot where Alice (9-12), Bob (13-17), and Charlie (10-11) are all available.");

    // Hardcoded logic is usually better for scheduling, but this demonstrates the LLM's reasoning capability.
    // In a real app, you would pass the schedules as JSON data.
    
    string schedules = "Alice: 9:00-12:00, Bob: 13:00-17:00, Charlie: 10:00-11:00";
    var request = $"Find a 30-min overlap for: {schedules}";

    var result = await solverAgent.InvokeAsync(request);
    Console.WriteLine($"Proposed Slot: {result}");
}
