
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

# Source File: solution_exercise_3.cs
# Description: Solution for Exercise 3
# ==========================================

using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

// Mocking the previous LocalInferenceEngine for context
public class LocalInferenceEngine : IDisposable
{
    private readonly InferenceSession _session;

    public LocalInferenceEngine(string modelPath)
    {
        // _session = new InferenceSession(modelPath); // Uncomment in production
    }

    public string RunInference(string prompt)
    {
        // Mock inference response for demonstration
        // In reality, this would handle tokenization, input tensors, and output processing
        Console.WriteLine($"[System] Processing prompt: {prompt.Substring(0, Math.Min(50, prompt.Length))}...");
        return "This is a simulated response from the ONNX model.";
    }

    public void Dispose()
    {
        _session?.Dispose();
    }
}

public class ChatSessionManager
{
    private readonly ConcurrentConversationBuffer _buffer = new();
    private readonly LocalInferenceEngine _engine;
    private const int MaxTokens = 500; // Example limit

    public ChatSessionManager(string modelPath)
    {
        _engine = new LocalInferenceEngine(modelPath);
    }

    // Requirement 2: Prompt Templating for Phi-3/ChatML style
    public string BuildPrompt(IEnumerable<ChatMessage> history)
    {
        var sb = new StringBuilder();
        
        foreach (var msg in history)
        {
            switch (msg.Role)
            {
                case ChatMessageRole.User:
                    sb.Append($"user\n{msg.Content} ");
                    break;
                case ChatMessageRole.Assistant:
                    sb.Append($"assistant\n{msg.Content} ");
                    break;
                case ChatMessageRole.System:
                    sb.Append($"system\n{msg.Content} ");
                    break;
            }
        }

        // Ensure the prompt ends with the token that triggers the assistant to generate
        sb.Append("assistant\n");
        return sb.ToString();
    }

    public void ProcessUserInput(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return;

        // Add user message to state
        _buffer.AddMessage(ChatMessageRole.User, input);

        // Get context (filtered by token limit)
        var context = _buffer.GetContext(MaxTokens, TokenCounter.EstimateTokens);
        
        // Build formatted prompt
        string prompt = BuildPrompt(context);

        // Run Inference
        string response = _engine.RunInference(prompt);

        // Persist state: Add assistant response
        _buffer.AddMessage(ChatMessageRole.Assistant, response);

        // Diagnostic Output
        PrintDiagnostics(input, response);
    }

    private void PrintDiagnostics(string input, string response)
    {
        var process = Process.GetCurrentProcess();
        long memoryMB = process.WorkingSet64 / (1024 * 1024);
        int estimatedTokens = TokenCounter.EstimateTokens(input + response);
        
        Console.WriteLine($"--- Diagnostics ---");
        Console.WriteLine($"Est. Tokens: {estimatedTokens}");
        Console.WriteLine($"Memory Usage: {memoryMB} MB");
        Console.WriteLine($"Buffer Count: {_buffer.Count}");
        Console.WriteLine($"-------------------");
    }

    public void ClearSession() => _buffer.Clear();
    
    public void Dispose() => _engine.Dispose();
}

// Main Console Application Entry Point
public class Program
{
    public static void Main(string[] args)
    {
        string modelPath = "path/to/phi-3.onnx"; // Update path
        using var sessionManager = new ChatSessionManager(modelPath);

        Console.WriteLine("Local Chat Session Started. Type /exit to quit, /clear to reset.");

        while (true)
        {
            Console.Write("> ");
            string input = Console.ReadLine();

            if (input.Equals("/exit", StringComparison.OrdinalIgnoreCase))
            {
                break;
            }
            
            if (input.Equals("/clear", StringComparison.OrdinalIgnoreCase))
            {
                sessionManager.ClearSession();
                Console.WriteLine("Session cleared.");
                continue;
            }

            sessionManager.ProcessUserInput(input);
        }
    }
}
