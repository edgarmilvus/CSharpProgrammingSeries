
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

# Source File: solution_exercise_1.cs
# Description: Solution for Exercise 1
# ==========================================

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using System;
using System.Linq;
using System.Threading.Tasks;

public class CodeRefiner
{
    public static async Task RunAsync()
    {
        // 1. Initialize Kernel (Assuming builder pattern or similar configuration)
        var builder = Kernel.CreateBuilder();
        builder.AddOpenAIChatCompletion("gpt-4o-mini", Environment.GetEnvironmentVariable("OPENAI_API_KEY"));
        var kernel = builder.Build();

        // 2. Create the Agent
        var agent = new ChatCompletionAgent(kernel)
        {
            Name = "CodeRefiner",
            Instructions = "You are an expert C# developer. Generate code snippets based on user requests. " +
                           "Ensure code follows best practices, specifically including async/await and exception handling."
        };

        // 3. Initialize Chat History
        var chatHistory = new ChatHistory();
        
        // Initial User Request
        string userRequest = "Write a C# method to download a file from a URL.";
        chatHistory.AddUserMessage(userRequest);
        
        Console.WriteLine($"User: {userRequest}\n");

        int iterations = 0;
        bool isCodeValid = false;
        string finalResponse = "";

        // 4. The Autonomous Loop
        while (iterations < 5 && !isCodeValid)
        {
            iterations++;
            Console.WriteLine($"--- Iteration {iterations} ---");

            // Get response from agent
            var agentResponse = await agent.InvokeAsync(chatHistory);
            string responseContent = agentResponse.Message.Content;
            
            // Store the agent's response in history
            chatHistory.AddAssistantMessage(responseContent);
            finalResponse = responseContent;

            Console.WriteLine($"Agent: {responseContent}\n");

            // Check for explicit validity statement
            if (responseContent.Contains("Code is valid"))
            {
                isCodeValid = true;
                break;
            }

            // 5. Feedback Mechanism (Simulated Self-Evaluation)
            // We append the feedback prompt to the history to guide the next iteration
            string feedbackPrompt = "Review the following code. Does it strictly adhere to C# best practices regarding exception handling? If not, explain why and provide a corrected version.";
            chatHistory.AddUserMessage(feedbackPrompt);
        }

        // 6. Output Results
        Console.WriteLine("--- Final Result ---");
        if (isCodeValid)
        {
            Console.WriteLine("Status: Code validated successfully.");
        }
        else
        {
            Console.WriteLine("Status: Max iterations reached. Returning best attempt.");
        }
        Console.WriteLine($"Total Iterations: {iterations}");
        Console.WriteLine("Final Code Snippet:");
        Console.WriteLine(finalResponse);
    }
}
