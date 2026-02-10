
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

// Source File: solution_exercise_1.cs
// Description: Solution for Exercise 1
// ==========================================

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.Text.Json;
using System.Text.Json.Nodes;

// Assuming Kernel and ChatCompletionService setup is handled in a shared context
// For this exercise, we simulate the environment variables.

async Task RunBugTriageAsync()
{
    // 1. Kernel Configuration
    var builder = Kernel.CreateBuilder();
    // Replace with your actual LLM configuration (Azure OpenAI or OpenAI)
    builder.AddOpenAIChatCompletion("gpt-4o", "api-key-here"); 
    var kernel = builder.Build();

    // 2. Agent 1: Bug Reporter
    var bugReporterPrompt = """
        You are a frustrated, non-technical user. 
        You are submitting a bug report. 
        Do NOT use technical jargon. 
        Focus on symptoms and how it makes you feel. 
        Be vague and emotional.
        """;

    var bugReporterAgent = new ChatCompletionAgent(
        kernel: kernel,
        instructions: bugReporterPrompt,
        executionSettings: new OpenAIPromptExecutionSettings { MaxTokens = 200 }
    );

    // 3. Agent 2: Triage Lead
    var triageLeadPrompt = """
        You are a calm, analytical senior developer.
        You receive raw bug reports and must extract structured data.
        Output ONLY valid JSON matching this schema:
        {
          "Severity": "Low | Medium | High | Critical",
          "Category": "UI | Backend | Database | Network",
          "Description": "A concise summary of the issue."
        }
        Do not add any text before or after the JSON.
        """;

    var triageLeadAgent = new ChatCompletionAgent(
        kernel: kernel,
        instructions: triageLeadPrompt,
        executionSettings: new OpenAIPromptExecutionSettings { MaxTokens = 300, ResponseFormat = "json_object" }
    );

    // 4. Orchestration
    var seedTopic = "Login button is unresponsive";
    
    // Bug Reporter generates complaint
    var chatHistory = new ChatHistory();
    chatHistory.AddUserMessage($"Complain about this issue: {seedTopic}");
    
    var rawComplaint = await bugReporterAgent.InvokeAsync(chatHistory);
    Console.WriteLine($"[Bug Reporter Output]:\n{rawComplaint}");

    // Pass to Triage Lead
    chatHistory.AddAssistantMessage(rawComplaint);
    chatHistory.AddUserMessage("Please triage this report into JSON.");

    var jsonResponse = await triageLeadAgent.InvokeAsync(chatHistory);
    Console.WriteLine($"\n[Triage Lead Raw Output]:\n{jsonResponse}");

    // 5. Output & Parsing
    try 
    {
        var parsedJson = JsonNode.Parse(jsonResponse.ToString());
        var severity = parsedJson["Severity"]?.ToString();
        var category = parsedJson["Category"]?.ToString();
        
        Console.WriteLine($"\n[Parsed JSON Object]:");
        Console.WriteLine($"Severity: {severity}");
        Console.WriteLine($"Category: {category}");
        Console.WriteLine($"Description: {parsedJson["Description"]}");
    }
    catch (JsonException)
    {
        Console.WriteLine("Failed to parse JSON output.");
    }
}
