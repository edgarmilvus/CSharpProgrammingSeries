
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

# Source File: solution_exercise_23.cs
# Description: Solution for Exercise 23
# ==========================================

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

async Task RunVersionControlAsync()
{
    var builder = Kernel.CreateBuilder();
    builder.AddOpenAIChatCompletion("gpt-4o", "api-key-here");
    var kernel = builder.Build();

    var agent = new ChatCompletionAgent(kernel, "You are a design agent.");
    
    var currentHistory = new ChatHistory();
    var historyStack = new Stack<ChatHistory>();

    // Simulation Loop
    while (true)
    {
        Console.Write("\nUser (type 'exit', 'undo', or message): ");
        var input = Console.ReadLine() ?? "";

        if (input == "exit") break;

        if (input == "undo")
        {
            if (historyStack.Count > 0)
            {
                currentHistory = historyStack.Pop();
                Console.WriteLine("[State Reverted]");
            }
            else
            {
                Console.WriteLine("[No history to undo]");
            }
            continue;
        }

        // Push current state before modifying
        var snapshot = new ChatHistory();
        foreach(var msg in currentHistory) snapshot.AddMessage(msg.Role, msg.Content);
        historyStack.Push(snapshot);

        // Add user input and get response
        currentHistory.AddUserMessage(input);
        var response = await agent.InvokeAsync(currentHistory);
        currentHistory.AddAssistantMessage(response.ToString());

        Console.WriteLine($"Agent: {response}");
    }
}
