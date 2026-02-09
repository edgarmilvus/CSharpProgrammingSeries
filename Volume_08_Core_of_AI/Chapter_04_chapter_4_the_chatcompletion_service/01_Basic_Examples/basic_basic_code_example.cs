
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
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System;
using System.Threading.Tasks;

// This example simulates a customer service chatbot for a bookstore.
// The bot maintains conversation history to provide context-aware responses.
public class Program
{
    public static async Task Main(string[] args)
    {
        // 1. Setup the Kernel
        // In a production environment, this configuration usually comes from appsettings.json.
        // We use OpenAI's GPT-4o-mini for this example.
        var builder = Kernel.CreateBuilder();
        
        // CRITICAL: Replace with your actual API key or use environment variables.
        builder.AddOpenAIChatCompletion(
            modelId: "gpt-4o-mini", 
            apiKey: "YOUR_API_KEY_HERE");
        
        Kernel kernel = builder.Build();

        // 2. Retrieve the ChatCompletion Service
        // The kernel automatically registers the IChatCompletionService when adding OpenAI/Azure AI.
        var chatService = kernel.GetRequiredService<IChatCompletionService>();

        // 3. Initialize Conversation History
        // ChatCompletion relies on a sequence of messages, not a single string prompt.
        var chatHistory = new ChatHistory();

        // 4. Define Execution Settings (Optional but recommended)
        // We can control model behavior like temperature (randomness) here.
        var executionSettings = new OpenAIPromptExecutionSettings
        {
            Temperature = 0.7f,
            MaxTokens = 500
        };

        // 5. The Interaction Loop
        string userPrompt = "I'm looking for a mystery novel set in London.";
        
        Console.WriteLine($"User: {userPrompt}");
        
        // A. Add user input to history
        chatHistory.AddUserMessage(userPrompt);

        // B. Get the streaming response
        // We use streaming for a better user experience (typing effect).
        // Alternatively, use GetChatMessageContentAsync for a single block response.
        Console.Write("Assistant: ");
        
        await foreach (var content in chatService.GetStreamingChatMessageContentsAsync(
            chatHistory, 
            executionSettings, 
            kernel))
        {
            // Stream output to console immediately
            Console.Write(content.Content);
        }
        
        Console.WriteLine("\n"); // New line after streaming finishes

        // C. Capture the full response and add it back to history
        // Note: In streaming scenarios, you usually reconstruct the full message 
        // from the chunks before adding it to history, or use a helper method.
        // For simplicity here, we will fetch the non-streaming version to ensure history is correct.
        var fullResponse = await chatService.GetChatMessageContentAsync(
            chatHistory, 
            executionSettings, 
            kernel);
        
        chatHistory.AddAssistantMessage(fullResponse.Content);

        // 6. Second Interaction (Demonstrating Context Retention)
        string followUp = "Does it have a hardboiled detective?";
        Console.WriteLine($"\nUser: {followUp}");
        
        chatHistory.AddUserMessage(followUp);

        Console.Write("Assistant: ");
        await foreach (var content in chatService.GetStreamingChatMessageContentsAsync(
            chatHistory, 
            executionSettings, 
            kernel))
        {
            Console.Write(content.Content);
        }
        Console.WriteLine();
    }
}
