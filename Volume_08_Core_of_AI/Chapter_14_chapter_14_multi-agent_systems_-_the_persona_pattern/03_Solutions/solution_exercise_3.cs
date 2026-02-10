
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

// Source File: solution_exercise_3.cs
// Description: Solution for Exercise 3
// ==========================================

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

async Task RunConflictResolutionAsync()
{
    var builder = Kernel.CreateBuilder();
    builder.AddOpenAIChatCompletion("gpt-4o", "api-key-here");
    var kernel = builder.Build();

    var history = new ChatHistory();
    // Context Prompt
    history.AddSystemMessage("Project: Mobile App v2.0. Constraints: Launch in 2 months. Budget: Low.");

    // Agents
    var optimist = new ChatCompletionAgent(kernel, 
        "You are a Product Manager focused on user value. Advocate for adding new features. Prioritize functionality over stability.");
    
    var pessimist = new ChatCompletionAgent(kernel, 
        "You are a Lead Engineer focused on technical debt. Advocate for cutting scope. Prioritize stability and speed.");

    var moderator = new ChatCompletionAgent(kernel, 
        "You are the VP of Engineering. Choose between 'Scope Expansion' or 'Scope Reduction'. Justify based on constraints. Output: Decision: [Option] | Reason: [Text]");

    // Round 1: Optimist
    history.AddUserMessage("Propose features for Mobile App v2.0.");
    var optResponse = await optimist.InvokeAsync(history);
    Console.WriteLine($"[Optimist]: {optResponse}");
    history.AddAssistantMessage(optResponse.ToString());

    // Round 2: Pessimist
    history.AddUserMessage("Respond to the Optimist's proposal.");
    var pessResponse = await pessimist.InvokeAsync(history);
    Console.WriteLine($"[Pessimist]: {pessResponse}");
    history.AddAssistantMessage(pessResponse.ToString());

    // Round 3: Moderator
    history.AddUserMessage("Review the discussion and make a final decision.");
    var modResponse = await moderator.InvokeAsync(history);
    Console.WriteLine($"\n[Moderator Final Decision]:\n{modResponse}");
}
