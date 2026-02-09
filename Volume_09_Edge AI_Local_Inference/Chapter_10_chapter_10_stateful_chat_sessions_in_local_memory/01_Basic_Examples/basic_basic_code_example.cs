
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;

// ---------------------------------------------------------
// REAL-WORLD CONTEXT
// ---------------------------------------------------------
// Imagine you are building a local, offline customer support chatbot
// for a smart appliance (e.g., a washing machine). The user asks:
// 1. "How do I clean the filter?" (Context: Washing Machine)
// 2. "What temperature should I use?" (Context: Washing Machine)
//
// Without state management, the AI treats question #2 as a generic query.
// With state management, the AI remembers we are talking about washing machines.
//
// This code demonstrates a "Hello World" of stateful chat:
// 1. It maintains a conversation history in memory.
// 2. It automatically truncates history to fit hardware constraints (token limits).
// 3. It prepares the state for an ONNX Runtime inference session.
// ---------------------------------------------------------

public class StatefulChatSession
{
    // ---------------------------------------------------------
    // CONFIGURATION & STATE DEFINITIONS
    // ---------------------------------------------------------
    
    // In a real ONNX model (like Phi-3 or Llama), the context window is limited.
    // For this example, we simulate a hard limit of 20 tokens to demonstrate truncation logic.
    private const int MaxContextTokens = 20;

    // A simple struct to represent a chat message.
    // In production, this would include roles (User, Assistant, System) and metadata.
    public struct ChatMessage
    {
        public string Role { get; set; } // "user" or "assistant"
        public string Content { get; set; }
        public int TokenCount { get; set; } // Estimated token count

        public override string ToString() => $"{Role}: {Content}";
    }

    // The in-memory buffer holding the conversation history.
    // We use a List for dynamic resizing.
    private readonly List<ChatMessage> _conversationHistory = new List<ChatMessage>();

    // ---------------------------------------------------------
    // CORE LOGIC: ADDING MESSAGES & MANAGING TOKENS
    // ---------------------------------------------------------

    /// <summary>
    /// Adds a message to the session and ensures the total token count
    /// stays within the hardware constraints (MaxContextTokens).
    /// </summary>
    public void AddMessage(string role, string content)
    {
        // 1. Estimate tokens (Simplified: 1 word ~= 1 token for demo purposes)
        int estimatedTokens = EstimateTokenCount(content);

        var message = new ChatMessage
        {
            Role = role,
            Content = content,
            TokenCount = estimatedTokens
        };

        // 2. Add to history
        _conversationHistory.Add(message);

        // 3. Enforce Token Limit (Truncation Strategy)
        // We remove oldest messages until we fit within the limit.
        // This is a "Sliding Window" approach common in Edge AI.
        EnforceTokenLimit();
    }

    /// <summary>
    /// Simulates a tokenizer. In real scenarios, use the specific model's tokenizer (e.g., Tiktoken).
    /// </summary>
    private int EstimateTokenCount(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return 0;
        // Naive estimation: Split by spaces and punctuation
        return text.Split(new[] { ' ', '.', ',', '!', '?' }, StringSplitOptions.RemoveEmptyEntries).Length;
    }

    /// <summary>
    /// Removes the oldest messages if the total token count exceeds MaxContextTokens.
    /// </summary>
    private void EnforceTokenLimit()
    {
        int totalTokens = _conversationHistory.Sum(m => m.TokenCount);

        // Iterate from the start (oldest messages) to remove excess
        while (totalTokens > MaxContextTokens && _conversationHistory.Count > 0)
        {
            var removed = _conversationHistory[0];
            _conversationHistory.RemoveAt(0);
            totalTokens -= removed.TokenCount;
            
            Console.WriteLine($"[System] Truncated history. Removed: '{removed.Content}'");
        }
    }

    // ---------------------------------------------------------
    // ONNX RUNTIME INTEGRATION PREPARATION
    // ---------------------------------------------------------

    /// <summary>
    /// Prepares the stateful prompt for the ONNX model.
    /// This method formats the history into a single string (prompt engineering).
    /// </summary>
    public string GetFormattedPrompt(string newQuery)
    {
        var sb = new StringBuilder();

        // 1. System Instruction (Context Anchor)
        sb.AppendLine("System: You are a helpful assistant for a washing machine.");
        
        // 2. Append Conversation History
        foreach (var msg in _conversationHistory)
        {
            sb.AppendLine($"{msg.Role}: {msg.Content}");
        }

        // 3. Append the new user query
        sb.AppendLine($"user: {newQuery}");

        return sb.ToString();
    }

    /// <summary>
    /// Mocks the creation of ONNX Runtime input tensors using the session state.
    /// In a real app, this converts the text string into Integer IDs (InputIDs).
    /// </summary>
    public void RunMockInference(string userQuery)
    {
        Console.WriteLine($"\n--- Processing Query: '{userQuery}' ---");

        // 1. Prepare the prompt with history
        string fullPrompt = GetFormattedPrompt(userQuery);
        Console.WriteLine($"[Prompt Prepared]\n{fullPrompt}");

        // 2. Convert to Input Tensor (Mock Logic)
        // Real ONNX models expect 'input_ids' (LongTensor) and 'attention_mask'.
        // Here we simulate the tokenization process.
        var inputIds = TokenizeToIds(fullPrompt);
        
        // 3. Create Tensor (Simulated ONNX Input)
        // Dimensions: [BatchSize (1), SequenceLength (variable)]
        var tensor = new DenseTensor<long>(inputIds, new[] { 1, inputIds.Length });
        
        Console.WriteLine($"[Tensor Created] Shape: [1, {inputIds.Length}], Tokens: {inputIds.Length}");

        // 4. Inference (Simulated)
        // In a real app: using var session = new InferenceSession("model.onnx");
        // var outputs = session.Run(new List<NamedOnnxValue> { ... });
        Console.WriteLine("[Inference] Simulated ONNX execution complete.");
        
        // 5. Update State (Simulated Assistant Response)
        // In a real app, the model generates this response.
        // We add it to history to maintain context for the NEXT turn.
        string mockResponse = "Based on our previous conversation, I recommend checking the manual.";
        AddMessage("assistant", mockResponse);
        Console.WriteLine($"[State Updated] Assistant response added to memory.");
    }

    private long[] TokenizeToIds(string text)
    {
        // Extremely simplified tokenizer for demonstration
        // Maps characters to long IDs just to show tensor creation.
        return text.Select(c => (long)c).ToArray();
    }

    // ---------------------------------------------------------
    // MAIN EXECUTION FLOW
    // ---------------------------------------------------------
    public static void Main()
    {
        Console.WriteLine("=== Local Edge AI: Stateful Chat Demo ===\n");

        var session = new StatefulChatSession();

        // SCENARIO 1: Initial Context
        // User asks about the washing machine.
        session.RunMockInference("How do I clean the filter?");
        
        // SCENARIO 2: Context Retention
        // User asks a follow-up. The system MUST remember the washing machine context.
        // If we didn't manage state, the AI would lose the topic.
        session.RunMockInference("What temperature should I use?");

        // SCENARIO 3: Token Limit Enforcement
        // We will flood the chat with long messages to trigger the truncation logic.
        Console.WriteLine("\n=== Testing Token Limit Enforcement ===");
        string longText = "This is a very long sentence designed to exceed the token limit we set. " +
                          "We are testing the sliding window mechanism.";
        
        // Add multiple long messages to fill the buffer
        for (int i = 0; i < 5; i++)
        {
            session.AddMessage("user", $"Message {i}: {longText}");
        }

        Console.WriteLine("\n=== Final History State ===");
        // Note: Only the most recent messages that fit in the 20-token window remain.
    }
}
