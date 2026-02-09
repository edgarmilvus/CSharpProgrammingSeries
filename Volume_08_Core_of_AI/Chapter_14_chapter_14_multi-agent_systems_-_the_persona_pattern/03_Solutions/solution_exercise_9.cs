
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

# Source File: solution_exercise_9.cs
# Description: Solution for Exercise 9
# ==========================================

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

async Task RunSwarmIntelligenceAsync()
{
    var builder = Kernel.CreateBuilder();
    builder.AddOpenAIChatCompletion("gpt-4o", "api-key-here");
    var kernel = builder.Build();

    var history = new ChatHistory();
    history.AddSystemMessage("You are in a group chat deciding on lunch. Read history. If location violates your constraint, say 'VETO: [Reason]'. If acceptable, say 'APPROVE: [Location]'.");

    // Constraints via specific prompts
    var agentA = new ChatCompletionAgent(kernel, "Constraint: You hate spicy food. Veto Indian or Thai.");
    var agentB = new ChatCompletionAgent(kernel, "Constraint: You are vegetarian. Veto Steakhouses.");
    var agentC = new ChatCompletionAgent(kernel, "Constraint: You have a low budget. Veto expensive places.");

    var locationsToTry = new[] { "Expensive Steakhouse", "Spicy Thai Place", "Cheap Veggie Diner" };

    foreach (var location in locationsToTry)
    {
        Console.WriteLine($"\nProposing: {location}");
        history.AddUserMessage($"Let's go to {location}");

        bool vetoed = false;
        var agents = new[] { agentA, agentB, agentC };

        foreach (var agent in agents)
        {
            var response = await agent.InvokeAsync(history);
            Console.WriteLine($"{agent.Name}: {response}");
            history.AddAssistantMessage(response.ToString());

            if (response.ToString().StartsWith("VETO", StringComparison.OrdinalIgnoreCase))
            {
                vetoed = true;
            }
        }

        if (!vetoed)
        {
            Console.WriteLine($"\n[Consensus Reached]: {location}");
            break;
        }
        else
        {
            Console.WriteLine("Consensus failed. Trying next location...");
        }
    }
}
