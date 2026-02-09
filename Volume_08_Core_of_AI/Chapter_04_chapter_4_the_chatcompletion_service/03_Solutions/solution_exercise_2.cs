
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

# Source File: solution_exercise_2.cs
# Description: Solution for Exercise 2
# ==========================================

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

public class ChatSession
{
    private readonly IChatCompletionService _chatService;
    private ChatHistory _chatHistory;
    private readonly int _maxHistoryTokens; // Simulated token limit

    public ChatSession(IChatCompletionService chatService, string systemInstruction = "You are a helpful assistant.")
    {
        _chatService = chatService;
        _chatHistory = new ChatHistory(systemInstruction);
        _maxHistoryTokens = 1000; // Arbitrary limit for this exercise
    }

    public async Task<string> SendMessageAsync(string userMessage)
    {
        // 1. Append user message
        _chatHistory.AddUserMessage(userMessage);

        // 2. Call AI
        var response = await _chatService.GetChatMessageContentAsync(_chatHistory);

        // 3. Append AI response
        _chatHistory.AddAssistantMessage(response.Content);

        // 4. Manage Memory (Sliding Window Strategy)
        ManageHistorySize();

        return response.Content;
    }

    public void ClearHistory()
    {
        // Keeps the system message, clears user/assistant messages
        var systemMsg = _chatHistory.FirstOrDefault(m => m.Role == AuthorRole.System);
        _chatHistory = new ChatHistory();
        if (systemMsg != null)
        {
            _chatHistory.AddMessage(systemMsg.Role, systemMsg.Content);
        }
    }

    public void UpdateSystemInstruction(string newInstruction)
    {
        // Remove existing system message
        var existingSystem = _chatHistory.FirstOrDefault(m => m.Role == AuthorRole.System);
        if (existingSystem != null)
        {
            _chatHistory.Remove(existingSystem);
        }
        
        // Add new system message at the beginning
        _chatHistory.Insert(0, new ChatMessageContent(AuthorRole.System, newInstruction));
    }

    private void ManageHistorySize()
    {
        // Strategy: Sliding Window. 
        // If history gets too long, remove the oldest non-system messages.
        // In a real scenario, we would count actual tokens using a tokenizer.
        
        // Estimate: Assuming ~4 chars per token. 
        string fullHistoryText = string.Join(" ", _chatHistory.Select(m => m.Content));
        
        if (fullHistoryText.Length > _maxHistoryTokens * 4)
        {
            // Remove oldest user/assistant pairs until under limit
            // Note: We must preserve the system message (index 0)
            while (_chatHistory.Count > 1 && fullHistoryText.Length > _maxHistoryTokens * 4)
            {
                // Remove the message after the system message (oldest user message)
                _chatHistory.RemoveAt(1); 
                // Remove the immediate assistant response if it exists
                if (_chatHistory.Count > 1 && _chatHistory[1].Role == AuthorRole.Assistant)
                {
                    _chatHistory.RemoveAt(1);
                }
                
                // Recalculate length (simplified)
                fullHistoryText = string.Join(" ", _chatHistory.Select(m => m.Content));
            }
        }
    }
}

// Main Program for Interactive Challenge
class Program
{
    static async Task Main(string[] args)
    {
        // Setup Kernel (Mocking service for demonstration, replace with real config)
        var builder = Kernel.CreateBuilder();
        builder.AddOpenAIChatCompletion("gpt-3.5-turbo", "sk-demo", "https://api.openai.com/v1");
        Kernel kernel = builder.Build();
        var chatService = kernel.GetRequiredService<IChatCompletionService>();

        var session = new ChatSession(chatService);

        Console.WriteLine("--- Interactive Chat Session ---");
        
        // 1. Formal Interaction
        session.UpdateSystemInstruction("You are a formal business assistant. Use professional language.");
        var response1 = await session.SendMessageAsync("Hello, I would like to schedule a meeting.");
        Console.WriteLine($"Formal AI: {response1}");

        // 2. Update Instruction Mid-Conversation
        Console.WriteLine("\n[System]: Updating instructions to casual tone...");
        session.UpdateSystemInstruction("You are a casual friend. Use slang and be relaxed.");
        
        // 3. Follow-up (History is maintained, but tone changes)
        var response2 = await session.SendMessageAsync("What did I just ask you about?");
        Console.WriteLine($"Casual AI: {response2}");
    }
}
