
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

# Source File: solution_exercise_21.cs
# Description: Solution for Exercise 21
# ==========================================

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

public class UserSession
{
    public string UserPreference { get; set; } = "Try a creative style.";
}

async Task RunFeedbackLoopAsync()
{
    var builder = Kernel.CreateBuilder();
    builder.AddOpenAIChatCompletion("gpt-4o", "api-key-here");
    var kernel = builder.Build();

    var session = new UserSession();

    // Poem Generator Agent (Prompt is dynamic)
    var poemAgent = new ChatCompletionAgent(kernel, "You write poems. " + session.UserPreference);

    // 1. First Poem
    var poem1 = await poemAgent.InvokeAsync("Write a poem about the ocean.");
    Console.WriteLine($"Poem 1:\n{poem1}");

    // 2. User Feedback
    Console.Write("Rate this poem (1-5): ");
    int rating = int.Parse(Console.ReadLine() ?? "3");

    // 3. Update State
    if (rating < 3)
    {
        session.UserPreference = "The user dislikes this style. Try a strict haiku format.";
    }
    else
    {
        session.UserPreference = "The user likes this style. Continue.";
    }

    Console.WriteLine($"Updated Preference: {session.UserPreference}");

    // 4. Next Invocation (Re-creating agent to simulate prompt update)
    // In a real app, you might pass this as KernelArguments or update the agent's execution settings.
    var poemAgentV2 = new ChatCompletionAgent(kernel, "You write poems. " + session.UserPreference);
    
    var poem2 = await poemAgentV2.InvokeAsync("Write another poem about the ocean.");
    Console.WriteLine($"Poem 2:\n{poem2}");
}
