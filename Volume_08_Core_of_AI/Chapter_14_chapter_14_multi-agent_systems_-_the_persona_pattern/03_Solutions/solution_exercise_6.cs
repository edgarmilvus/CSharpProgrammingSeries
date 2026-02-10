
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

// Source File: solution_exercise_6.cs
// Description: Solution for Exercise 6
// ==========================================

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

// Graphviz Diagram (Conceptual)
/*
digraph G {
    Start -> Intake;
    Intake -> Technical [label="Code/Error"];
    Intake -> Billing [label="Invoice/Payment"];
    Intake -> General [label="Other"];
    Intake -> Escalation [label="Ambiguous"];
}
*/

async Task RunGraphBasedWorkflowAsync()
{
    var builder = Kernel.CreateBuilder();
    builder.AddOpenAIChatCompletion("gpt-4o", "api-key-here");
    var kernel = builder.Build();

    // Agents
    var intakeAgent = new ChatCompletionAgent(kernel, "Analyze the request. If it contains code/logs, output 'Technical'. If invoice/payment, output 'Billing'. If ambiguous, output 'Unknown'.");
    var techAgent = new ChatCompletionAgent(kernel, "You are Technical Support. Solve code issues.");
    var billingAgent = new ChatCompletionAgent(kernel, "You are Billing Support. Handle invoices.");
    var generalAgent = new ChatCompletionAgent(kernel, "You are General Inquiry. Answer questions.");
    var escalationAgent = new ChatCompletionAgent(kernel, "You are Human Escalation. Ask for clarification.");

    // Routing Dictionary
    var routingMap = new Dictionary<string, Func<Kernel, string, Task<string>>>
    {
        { "Technical", async (k, input) => (await techAgent.InvokeAsync(input)).ToString() },
        { "Billing", async (k, input) => (await billingAgent.InvokeAsync(input)).ToString() },
        { "General", async (k, input) => (await generalAgent.InvokeAsync(input)).ToString() },
        { "Unknown", async (k, input) => (await escalationAgent.InvokeAsync(input)).ToString() }
    };

    // Test Inputs
    string[] testInputs = {
        "I have an error in my C# loop.",
        "I haven't received my invoice for March.",
        "What are your opening hours?",
        "My thingy isn't working properly." // Ambiguous
    };

    foreach (var input in testInputs)
    {
        Console.WriteLine($"\nInput: {input}");
        
        // 1. Intake Analysis
        var classification = (await intakeAgent.InvokeAsync(input)).ToString().Trim();
        Console.WriteLine($"Routing Decision: {classification}");

        // 2. Routing Logic
        if (routingMap.TryGetValue(classification, out var handler))
        {
            var result = await handler(kernel, input);
            Console.WriteLine($"Response: {result}");
        }
        else
        {
            Console.WriteLine("Critical Routing Error: No handler found.");
        }
    }
}
