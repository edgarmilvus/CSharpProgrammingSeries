
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

// Source File: solution_exercise_7.cs
// Description: Solution for Exercise 7
// ==========================================

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

public class DebateSession
{
    public ChatHistory ChatHistory { get; } = new ChatHistory();
    public int TurnCount { get; set; } = 0;
    public int MaxTurns { get; set; } = 3; // Low for demo
    public bool IsConcluded { get; set; } = false;
}

async Task RunStatefulDebateAsync()
{
    var builder = Kernel.CreateBuilder();
    builder.AddOpenAIChatCompletion("gpt-4o", "api-key-here");
    var kernel = builder.Build();

    var session = new DebateSession();
    session.ChatHistory.AddSystemMessage("Project: Mobile App. Constraints: 2 months, Low Budget.");

    var optimist = new ChatCompletionAgent(kernel, "Advocate for features.");
    var pessimist = new ChatCompletionAgent(kernel, "Advocate for cutting scope.");
    var summarizer = new ChatCompletionAgent(kernel, "Summarize the debate and identify the deadlock.");

    // Simulate User Input to start
    string userRequest = "Plan the v2.0 features.";

    while (!session.IsConcluded)
    {
        if (session.TurnCount >= session.MaxTurns)
        {
            Console.WriteLine("\n[Max Turns Reached - Triggering Summarizer]");
            session.ChatHistory.AddUserMessage("Summarize the current deadlock.");
            var summary = await summarizer.InvokeAsync(session.ChatHistory);
            Console.WriteLine($"Summary: {summary}");
            break;
        }

        // Token Management Simulation (Logic Check)
        int estimatedTokens = session.ChatHistory.Sum(m => m.Content.Length);
        if (estimatedTokens > 200) // Very low threshold for demo
        {
            Console.WriteLine("[Token Threshold Exceeded - Compressing History]");
            // In a real app, we would call the summarizer here and replace history
            // For this demo, we just print a warning.
        }

        // Alternating Turns
        var currentAgent = (session.TurnCount % 2 == 0) ? optimist : pessimist;
        
        Console.WriteLine($"\n--- Turn {session.TurnCount + 1} ({currentAgent.GetType().Name}) ---");
        
        // Check for manual interrupt
        if (session.TurnCount == 1) // Simulate user interrupt on turn 2
        {
             Console.WriteLine("User Input: 'Force Conclusion'");
             session.IsConcluded = true;
             continue;
        }

        var response = await currentAgent.InvokeAsync(session.ChatHistory);
        Console.WriteLine(response);
        session.ChatHistory.AddAssistantMessage(response.ToString());
        
        session.TurnCount++;
    }
}
