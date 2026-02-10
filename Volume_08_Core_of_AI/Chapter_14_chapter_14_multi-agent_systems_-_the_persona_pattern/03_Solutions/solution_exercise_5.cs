
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

// Source File: solution_exercise_5.cs
// Description: Solution for Exercise 5
// ==========================================

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

public class PersonaProfile
{
    public string Mood { get; set; } = "Calm";
    public string TechnicalLevel { get; set; } = "Novice";
    public int Patience { get; set; } = 5;
}

async Task RunPersonaDrivenApiAsync()
{
    var builder = Kernel.CreateBuilder();
    builder.AddOpenAIChatCompletion("gpt-4o", "api-key-here");
    var kernel = builder.Build();

    var profile = new PersonaProfile { Mood = "Furious", TechnicalLevel = "Novice" };
    var history = new ChatHistory();
    history.AddSystemMessage("You are a customer support agent. De-escalate issues and provide solutions.");

    var customerAgent = new ChatCompletionAgent(kernel, ""); // Instructions injected dynamically
    var supportAgent = new ChatCompletionAgent(kernel, "You are a helpful, calm support agent.");

    // Simulation Loop (4 turns)
    for (int i = 0; i < 4; i++)
    {
        // 1. Update Customer Persona Prompt
        string personaPrompt = $"You are a customer. Mood: {profile.Mood}. Technical Level: {profile.TechnicalLevel}. ";
        if (i == 0) personaPrompt += "Start the conversation by complaining your internet is down.";
        // (In a real scenario, we would update the agent's instructions or pass as KernelArguments)

        // Note: ChatCompletionAgent instructions are immutable in standard SK, 
        // so we simulate persona by injecting it into the user message for this exercise.
        
        string customerInput = "";
        if (i == 0) customerInput = "My internet is down! This is ridiculous!";
        else 
        {
            // Get last support message to react to
            var lastSupportMsg = history.Last().Content;
            // We simulate the LLM generating a response based on persona
            // For this exercise, we manually simulate the persona logic for brevity,
            // but in production, we'd update the Agent's system prompt or KernelArguments.
            customerInput = profile.Mood == "Furious" ? "I don't care, fix it now!" : "Okay, thanks.";
        }

        Console.WriteLine($"[Customer - {profile.Mood}]: {customerInput}");
        history.AddUserMessage(customerInput);

        // 2. Support Agent Responds
        var supportResponse = await supportAgent.InvokeAsync(history);
        Console.WriteLine($"[Support]: {supportResponse}");
        history.AddAssistantMessage(supportResponse.ToString());

        // 3. Update Persona Logic
        if (supportResponse.ToString().Contains("reset") || supportResponse.ToString().Contains("fixed"))
        {
            profile.Mood = "Neutral";
            profile.Patience = 8;
            Console.WriteLine($"[Persona Update: Mood changed from Furious to Neutral]");
        }
    }
}
