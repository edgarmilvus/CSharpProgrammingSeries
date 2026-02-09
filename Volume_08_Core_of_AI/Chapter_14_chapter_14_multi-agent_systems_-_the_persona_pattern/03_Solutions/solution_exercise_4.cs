
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

# Source File: solution_exercise_4.cs
# Description: Solution for Exercise 4
# ==========================================

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.Text.Json;
using System.Text.Json.Nodes;

async Task RunDynamicPlannerAsync()
{
    var builder = Kernel.CreateBuilder();
    builder.AddOpenAIChatCompletion("gpt-4o", "api-key-here");
    var kernel = builder.Build();

    // Specialist Agents
    var codeAgent = new ChatCompletionAgent(kernel, "You write C# code.");
    var docsAgent = new ChatCompletionAgent(kernel, "You write XML documentation.");
    var testAgent = new ChatCompletionAgent(kernel, "You write xUnit unit tests.");

    // Planner Agent
    var plannerAgent = new ChatCompletionAgent(
        kernel: kernel,
        instructions: "You are a Task Planner. Analyze user requests and generate a JSON execution plan. The plan must be an array of objects with 'AgentName' and 'TaskDescription'. Available agents: 'CodeGenerator', 'DocumentationWriter', 'TestWriter'.",
        executionSettings: new OpenAIPromptExecutionSettings { ResponseFormat = "json_object" }
    );

    // Interactive Input
    Console.Write("Enter request: ");
    string userRequest = Console.ReadLine() ?? "I need a helper class for date manipulation and the tests for it.";

    // 1. Planning Phase
    var planResponse = await plannerAgent.InvokeAsync(userRequest);
    Console.WriteLine($"Planner Output:\n{planResponse}");

    try
    {
        var planArray = JsonNode.Parse(planResponse.ToString())?.AsArray();
        
        if (planArray != null)
        {
            // 2. Execution Phase
            foreach (var step in planArray)
            {
                string agentName = step["AgentName"]?.ToString() ?? "";
                string taskDesc = step["TaskDescription"]?.ToString() ?? "";

                Console.WriteLine($"\nExecuting Step: {agentName} - {taskDesc}");

                string result = "";
                switch (agentName)
                {
                    case "CodeGenerator":
                        result = (await codeAgent.InvokeAsync(taskDesc)).ToString();
                        break;
                    case "DocumentationWriter":
                        result = (await docsAgent.InvokeAsync(taskDesc)).ToString();
                        break;
                    case "TestWriter":
                        result = (await testAgent.InvokeAsync(taskDesc)).ToString();
                        break;
                    default:
                        Console.WriteLine($"Error: Unknown agent '{agentName}'");
                        continue;
                }
                Console.WriteLine($"Result:\n{result}");
            }
        }
    }
    catch (JsonException)
    {
        Console.WriteLine("Error: Planner returned invalid JSON. Please retry.");
    }
}
