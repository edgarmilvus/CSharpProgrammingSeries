
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

// Source File: project_advanced_application_script.cs
// Description: Advanced Application Script
// ==========================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace AgenticCustomerSupport
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // 1. Kernel Initialization
            // The Kernel is the central orchestrator. It manages dependencies and execution.
            // We use the default builder which automatically loads configuration from environment variables
            // or appsettings.json (specifically the 'AzureOpenAI' or 'OpenAI' sections).
            IKernelBuilder kernelBuilder = Kernel.CreateBuilder();
            
            // In a real scenario, we would add the specific AI service here:
            // kernelBuilder.AddAzureOpenAIChatCompletion(...)
            // For this demonstration, we assume the kernel is configured.
            Kernel kernel = kernelBuilder.Build();

            // 2. Service Retrieval
            // We explicitly request the IChatCompletionService from the kernel's service provider.
            // This decouples our logic from the specific AI provider (e.g., Azure OpenAI vs. OpenAI).
            IChatCompletionService chatService = kernel.Services.GetRequiredService<IChatCompletionService>();

            // 3. Conversation State Management
            // We initialize a ChatHistory object. This is a specialized collection that maintains
            // the sequence of messages (User, Assistant, System) required for context-aware responses.
            ChatHistory history = new ChatHistory();
            
            // 4. System Prompt Injection
            // We inject a system message to define the AI's persona and constraints immediately.
            // This sets the tone for all subsequent interactions.
            history.AddSystemMessage("You are a helpful customer support agent for 'TechNova', a fictional electronics retailer. " +
                                     "You must only answer questions related to TechNova products. If asked about unrelated topics, politely decline. " +
                                     "Keep responses concise and professional.");

            Console.WriteLine("TechNova Support Bot Initialized. Type 'exit' to quit.\n");

            // 5. The Interaction Loop
            // A continuous loop simulating a persistent chat session.
            while (true)
            {
                Console.Write("User: ");
                string? userInput = Console.ReadLine();

                // Input validation and exit condition
                if (string.IsNullOrWhiteSpace(userInput)) continue;
                if (userInput.Equals("exit", StringComparison.OrdinalIgnoreCase)) break;

                // 6. Adding User Context
                // The user's input is added to the history object. This is crucial for the model
                // to understand the immediate context and the conversation history.
                history.AddUserMessage(userInput);

                try
                {
                    // 7. Synchronous vs. Asynchronous Execution
                    // We use GetChatMessageContentAsync. In a real app, we might use streaming
                    // (GetStreamingChatMessageContentsAsync) for UI responsiveness, but for a console app,
                    // waiting for the full response is acceptable.
                    // We pass 'null' for executionSettings to use the default configuration.
                    ChatMessageContent response = await chatService.GetChatMessageContentAsync(
                        history, 
                        executionSettings: null, 
                        kernel: kernel
                    );

                    // 8. Output and History Management
                    // We display the response and immediately add it to the history.
                    // If we fail to add the assistant's response, the model will "forget" its own answer
                    // in the next loop iteration.
                    Console.WriteLine($"Assistant: {response.Content}");
                    history.AddAssistantMessage(response.Content ?? string.Empty);
                }
                catch (Exception ex)
                {
                    // 9. Error Handling
                    // Robust applications must handle API failures (timeouts, rate limits, network issues).
                    // We log the error but do not add it to the history to prevent corrupting the model's context.
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"[Error]: {ex.Message}. Please try again.");
                    Console.ResetColor();
                }
            }

            Console.WriteLine("\nSession ended. Goodbye!");
        }
    }
}
