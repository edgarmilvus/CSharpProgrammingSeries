
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

# Source File: basic_basic_code_example.cs
# Description: Basic Code Example
# ==========================================

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System;

namespace PersonaPatternDemo
{
    // 1. Define a Plugin for the Engineer to check technical feasibility
    public class TechnicalFeasibilityPlugin
    {
        [KernelFunction("check_technical_feasibility")]
        [Description("Analyzes a feature request for technical complexity and implementation risks.")]
        public string CheckFeasibility(
            [Description("The feature description")] string feature,
            [Description("The current tech stack")] string techStack)
        {
            // Simulating a deterministic logic check (mocking database/API calls)
            if (feature.Contains("Real-time") || feature.Contains("AI"))
            {
                return "HIGH_COMPLEXITY: Requires new infrastructure. Estimated 8 weeks.";
            }
            if (feature.Contains("Dark Mode"))
            {
                return "LOW_COMPLEXITY: CSS changes only. Estimated 1 week.";
            }
            return "MEDIUM_COMPLEXITY: Standard API updates. Estimated 3 weeks.";
        }
    }

    class Program
    {
        static async Task Main(string[] args)
        {
            // --- SETUP ---
            // We will use a single Kernel instance, but define two distinct ChatHistory contexts
            // to simulate separate agents holding their own memory.
            var builder = Kernel.CreateBuilder();
            
            // CONFIG: Replace with your LLM provider (e.g., AzureOpenAI, OpenAI)
            // For this demo to run, you must configure a valid LLM connection here.
            builder.AddAzureOpenAIChatCompletion(
                deploymentName: "gpt-4o", 
                endpoint: "https://your-endpoint.openai.azure.com/", 
                apiKey: "your-api-key");

            var kernel = builder.Build();

            // --- PERSONA DEFINITIONS ---
            
            // Persona A: The Product Manager (The Visionary)
            const string pmSystemPrompt = """
                You are Sarah, a Product Manager. 
                Your goal is to advocate for the user. 
                You prioritize features based on user delight and market trends.
                You must insist on features that sound "cool" and "modern".
                Keep your responses concise and business-focused.
                """;

            // Persona B: The Senior Engineer (The Pragmatist)
            const string engSystemPrompt = """
                You are Alex, a Senior Software Engineer.
                Your goal is to protect system stability and manage technical debt.
                You are cynical about "flashy" features. 
                You have access to a tool to check technical feasibility.
                If a feature is high complexity, you MUST reject it unless provided more time/resources.
                """;

            // --- AGENT CONTEXTS ---
            // We simulate two agents by giving them separate ChatHistory instances.
            // This preserves their individual conversation threads and "memory".
            var pmHistory = new ChatHistory(pmSystemPrompt);
            var engHistory = new ChatHistory(engSystemPrompt);

            // --- THE INTERACTION LOOP ---
            Console.WriteLine("🚀 Starting Product Meeting Simulation...\n");

            // 1. PM Proposes a Feature
            string initialProposal = "I want to add Real-time Voice AI Translation to the chat feature.";
            Console.WriteLine($"[PM (Sarah)]: {initialProposal}");
            
            // Add to PM's memory
            pmHistory.AddUserMessage(initialProposal);

            // 2. Engineer Receives Proposal (Reads PM's message)
            // The engineer sees the proposal as a user message in his own context.
            engHistory.AddUserMessage(initialProposal);

            // Engineer processes the request using the LLM + Tools
            var engKernelFunction = kernel.CreateFunctionFromPrompt(engSystemPrompt);
            var chatCompletion = kernel.GetRequiredService<IChatCompletionService>();

            // Engineer invokes the LLM to generate a response
            // We attach the tool to the execution context
            var toolConfig = new KernelPromptExecutionSettings 
            { 
                ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions 
            };
            
            // Register the plugin on the kernel for the engineer to use
            kernel.Plugins.AddFromType<TechnicalFeasibilityPlugin>("TechCheck");

            // Engineer generates response based on his persona + tools
            var engResponse = await chatCompletion.GetChatMessageContentsAsync(
                engHistory, 
                toolConfig, 
                kernel
            );

            // Extract the text response
            string engResponseText = engResponse[^1].Content; // Get last message
            Console.WriteLine($"[Eng (Alex)]: {engResponseText}");

            // 3. Context Switching: The PM hears the response
            // The PM adds the Engineer's response to her history to formulate a rebuttal.
            pmHistory.AddAssistantMessage(engResponseText); // Simulating the engineer speaking to her

            // PM generates a counter-argument
            var pmResponse = await chatCompletion.GetChatMessageContentsAsync(pmHistory, null, kernel);
            string pmResponseText = pmResponse[^1].Content;
            Console.WriteLine($"[PM (Sarah)]: {pmResponseText}");

            // 4. Engineer hears the rebuttal
            engHistory.AddUserMessage(pmResponseText);
            
            // Engineer processes again
            var finalEngResponse = await chatCompletion.GetChatMessageContentsAsync(engHistory, toolConfig, kernel);
            Console.WriteLine($"[Eng (Alex)]: {finalEngResponse[^1].Content}");

            Console.WriteLine("\n--- Meeting Ended ---");
        }
    }
}
