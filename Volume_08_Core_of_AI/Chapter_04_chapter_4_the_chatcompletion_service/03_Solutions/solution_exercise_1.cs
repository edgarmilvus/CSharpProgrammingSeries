
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
using Microsoft.SemanticKernel.ChatCompletion;
using System.Diagnostics;

// Setup configuration (replace with your actual keys/endpoints)
const string modelId = "gpt-4"; // or "gpt-3.5-turbo"
const string apiKey = "YOUR_API_KEY"; 
const string endpoint = "YOUR_AZURE_ENDPOINT"; // If using Azure OpenAI

class Program
{
    static async Task Main(string[] args)
    {
        // 1. Manually instantiate the kernel (no DI)
        var builder = Kernel.CreateBuilder();
        
        // Add Chat Completion service (Choose one based on your provider)
        // For OpenAI:
        // builder.AddOpenAIChatCompletion(modelId, apiKey);
        
        // For Azure OpenAI:
        builder.AddAzureOpenAIChatCompletion(modelId, endpoint, apiKey);

        Kernel kernel = builder.Build();
        var chatService = kernel.GetRequiredService<IChatCompletionService>();

        string sampleText = "The Semantic Kernel is a lightweight, open-source development kit that lets you easily build AI agents and integrate the latest AI models into your C#, Python, or Java code.";

        Console.WriteLine("--- Starting Synchronous vs Asynchronous Demo ---");

        // 2. Measure Synchronous Execution
        var stopwatch = Stopwatch.StartNew();
        var syncResult = SummarizeSynchronously(chatService, sampleText);
        stopwatch.Stop();
        Console.WriteLine($"Synchronous Execution Time: {stopwatch.ElapsedMilliseconds}ms");
        Console.WriteLine($"Result: {syncResult}\n");

        // 3. Measure Asynchronous Execution
        stopwatch.Restart();
        var asyncResult = await SummarizeAsynchronously(chatService, sampleText);
        stopwatch.Stop();
        Console.WriteLine($"Asynchronous Execution Time: {stopwatch.ElapsedMilliseconds}ms");
        Console.WriteLine($"Result: {asyncResult}\n");

        // 4. Edge Case: Mixed Context
        Console.WriteLine("--- Testing Mixed Context (Deadlock Risk) ---");
        try 
        {
            // Note: In a UI context (like WPF/WinForms), this would likely deadlock.
            // In a Console app, it often "works" but blocks the thread unnecessarily.
            var mixedResult = SummarizeMixed(chatService, sampleText); 
            Console.WriteLine("Mixed call completed (but blocked the thread).");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in mixed context: {ex.Message}");
        }

        // 5. Interactive Challenge: Concurrent Execution
        Console.WriteLine("\n--- Testing Concurrent Execution ---");
        string text1 = "Artificial Intelligence is evolving rapidly.";
        string text2 = "Machine Learning models require vast amounts of data.";
        
        stopwatch.Restart();
        var task1 = SummarizeAsynchronously(chatService, text1);
        var task2 = SummarizeAsynchronously(chatService, text2);
        
        await Task.WhenAll(task1, task2);
        stopwatch.Stop();
        
        Console.WriteLine($"Concurrent Execution Time: {stopwatch.ElapsedMilliseconds}ms");
        Console.WriteLine($"Result 1: {task1.Result}");
        Console.WriteLine($"Result 2: {task2.Result}");
    }

    // Synchronous Wrapper
    public static string SummarizeSynchronously(IChatCompletionService chatService, string text)
    {
        // This blocks the thread until the result is returned.
        // It uses .GetAwaiter().GetResult() internally or similar blocking mechanisms.
        // Note: In modern SK, most methods are async. We simulate the sync call here.
        return chatService.GetChatMessageContentAsync(
            $"Summarize the following text in one sentence: {text}").GetAwaiter().GetResult().Content;
    }

    // Asynchronous Implementation
    public static async Task<string> SummarizeAsynchronously(IChatCompletionService chatService, string text)
    {
        // Non-blocking: yields control to the caller while waiting for the AI response.
        var response = await chatService.GetChatMessageContentAsync(
            $"Summarize the following text in one sentence: {text}");
        return response.Content;
    }

    // Edge Case: Calling Sync from Async (Anti-pattern)
    public static string SummarizeMixed(IChatCompletionService chatService, string text)
    {
        // Wrapping async code in a synchronous call (.Result or .GetAwaiter().GetResult())
        // This is dangerous in UI apps (deadlock) and inefficient in server apps (wastes threads).
        return chatService.GetChatMessageContentAsync(
            $"Summarize the following text in one sentence: {text}").GetAwaiter().GetResult().Content;
    }
}
