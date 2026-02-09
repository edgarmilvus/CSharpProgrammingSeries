
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

# Source File: solution_exercise_32.cs
# Description: Solution for Exercise 32
# ==========================================

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

async Task RunGameMasterAsync()
{
    var builder = Kernel.CreateBuilder();
    builder.AddOpenAIChatCompletion("gpt-4o", "api-key-here");
    var kernel = builder.Build();

    var gameMaster = new ChatCompletionAgent(kernel, "Route user actions to: 'Barkeep', 'Guard', or 'Thief'. Output only the NPC name.");
    
    var barkeep = new ChatCompletionAgent(kernel, "You are a grumpy barkeep. Give quests.");
    var guard = new ChatCompletionAgent(kernel, "You are a suspicious guard. Ask for ID.");
    var thief = new ChatCompletionAgent(kernel, "You are a sneaky thief. Try to pickpocket.");

    string userAction = "I walk up to the counter.";

    // 1. Game Master Routing
    var npcName = await gameMaster.InvokeAsync(userAction);
    Console.WriteLine($"Game Master routed to: {npcName}");

    // 2. NPC Response
    string response = "";
    if (npcName.ToString().Contains("Barkeep")) response = (await barkeep.InvokeAsync(userAction)).ToString();
    else if (npcName.ToString().Contains("Guard")) response = (await guard.InvokeAsync(userAction)).ToString();
    else if (npcName.ToString().Contains("Thief")) response = (await thief.InvokeAsync(userAction)).ToString();

    Console.WriteLine($"NPC: {response}");
}
