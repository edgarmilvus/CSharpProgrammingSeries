
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

# Source File: solution_exercise_40.cs
# Description: Solution for Exercise 40
# ==========================================

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

async Task RunDebuggingAssistantAsync()
{
    var builder = Kernel.CreateBuilder();
    builder.AddOpenAIChatCompletion("gpt-4o", "api-key-here");
    var kernel = builder.Build();

    var diagnosticAgent = new ChatCompletionAgent(kernel, "You are a debugging assistant. Do not solve immediately. Ask one clarifying question about the code structure or error message.");

    // Simulated Interaction
    string userBugReport = "My loop is infinite.";
    
    // Turn 1: Ask Question
    var question = await diagnosticAgent.InvokeAsync(userBugReport);
    Console.WriteLine($"Agent: {question}");

    // Simulate User Answer
    string userAnswer = "The termination condition is i < 10.";
    Console.WriteLine($"User: {userAnswer}");

    // Turn 2: Analyze and Suggest Fix
    // We append the conversation history manually for this simple simulation
    var analysisPrompt = $"Bug: {userBugReport}\nQuestion asked: {question}\nUser Answer: {userAnswer}\nIdentify the bug and suggest a fix.";
    var diagnosis = await diagnosticAgent.InvokeAsync(analysisPrompt);
    
    Console.WriteLine($"Agent Diagnosis: {diagnosis}");
}
