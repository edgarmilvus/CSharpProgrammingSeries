
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

// Source File: solution_exercise_2.cs
// Description: Solution for Exercise 2
// ==========================================

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.Text.Json;
using System.Text.Json.Nodes;

public class WritingSession
{
    public ChatHistory ChatHistory { get; } = new ChatHistory();
}

async Task RunReflexiveEditorAsync()
{
    var builder = Kernel.CreateBuilder();
    builder.AddOpenAIChatCompletion("gpt-4o", "api-key-here");
    var kernel = builder.Build();

    var session = new WritingSession();

    // Agents
    var draftingAgent = new ChatCompletionAgent(
        kernel: kernel,
        instructions: "You are a creative copywriter. You write concise, engaging marketing emails. You accept feedback and revise your drafts.",
        executionSettings: new OpenAIPromptExecutionSettings { MaxTokens = 300 }
    );

    var critiqueAgent = new ChatCompletionAgent(
        kernel: kernel,
        instructions: "You are a strict editor. Evaluate drafts based on tone, clarity, and length. Return JSON: { 'Score': int (1-10), 'Critique': string, 'ImprovementSuggestions': string (optional if Score < 8) }.",
        executionSettings: new OpenAIPromptExecutionSettings { MaxTokens = 300, ResponseFormat = "json_object" }
    );

    // Initial Context
    string topic = "New AI Features";
    session.ChatHistory.AddUserMessage($"Write a marketing email about: {topic}");

    int maxIterations = 3;
    int currentIteration = 0;
    bool isApproved = false;

    // Orchestration Loop
    while (currentIteration < maxIterations && !isApproved)
    {
        currentIteration++;
        Console.WriteLine($"\n--- Iteration {currentIteration} ---");

        // 1. Drafting Agent
        var draft = await draftingAgent.InvokeAsync(session.ChatHistory);
        Console.WriteLine($"Draft:\n{draft}");
        session.ChatHistory.AddAssistantMessage(draft.ToString());

        // 2. Critique Agent
        session.ChatHistory.AddUserMessage("Critique this draft and provide a score.");
        var critiqueJson = await critiqueAgent.InvokeAsync(session.ChatHistory);
        Console.WriteLine($"Critique JSON:\n{critiqueJson}");

        // 3. Parse Score
        try
        {
            var jsonNode = JsonNode.Parse(critiqueJson.ToString());
            int score = jsonNode["Score"]?.GetValue<int>() ?? 0;
            
            session.ChatHistory.AddAssistantMessage(critiqueJson.ToString());

            Console.WriteLine($"Score: {score}/10");

            if (score >= 8)
            {
                isApproved = true;
                Console.WriteLine("\n[DRAFT APPROVED]");
            }
            else
            {
                // The loop continues, the LLM context grows with the history
                session.ChatHistory.AddUserMessage("Please rewrite the draft incorporating the critique.");
            }
        }
        catch
        {
            Console.WriteLine("Failed to parse critique JSON.");
            break;
        }
    }

    // Final Output
    Console.WriteLine("\n--- Final Approved Draft ---");
    Console.WriteLine(session.ChatHistory.Last().Content);
}
