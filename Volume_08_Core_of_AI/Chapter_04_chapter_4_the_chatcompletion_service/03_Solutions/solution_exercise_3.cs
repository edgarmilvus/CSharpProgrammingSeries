
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

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI; // For OpenAIPromptExecutionSettings

public class CreativeWriter
{
    private readonly IChatCompletionService _chatService;

    public CreativeWriter(IChatCompletionService chatService)
    {
        _chatService = chatService;
    }

    public async Task<string> GenerateStory(string prompt, double temperature, int maxTokens, string[] stopSequences)
    {
        // 1. Edge Case: Validation
        if (temperature < 0.0 || temperature > 2.0)
            throw new ArgumentOutOfRangeException(nameof(temperature), "Temperature must be between 0.0 and 2.0");

        if (maxTokens > 4096)
            throw new ArgumentOutOfRangeException(nameof(maxTokens), "MaxTokens cannot exceed 4096 for this model.");

        // 2. Configure PromptExecutionSettings
        var executionSettings = new OpenAIPromptExecutionSettings
        {
            Temperature = temperature,
            MaxTokens = maxTokens,
            StopSequences = stopSequences,
            // SystemPrompt = "You are a creative storyteller." // Optional
        };

        // 3. Generate
        // Note: We create a prompt history for the call
        var chatHistory = new ChatHistory();
        chatHistory.AddUserMessage(prompt);

        var response = await _chatService.GetChatMessageContentAsync(chatHistory, executionSettings);
        return response.Content;
    }
}

class Program
{
    static async Task Main(string[] args)
    {
        // Setup
        var builder = Kernel.CreateBuilder();
        builder.AddOpenAIChatCompletion("gpt-3.5-turbo", "sk-demo", "https://api.openai.com/v1");
        Kernel kernel = builder.Build();
        var chatService = kernel.GetRequiredService<IChatCompletionService>();
        
        var writer = new CreativeWriter(chatService);
        string basePrompt = "Write a short story about a robot discovering music.";

        Console.WriteLine("--- Interactive Challenge: Temperature Impact ---");

        // 4. Interactive Challenge Loop
        // Increments: 0.1, 0.55, 1.0, 1.45, 1.9
        double currentTemp = 0.1;
        for (int i = 0; i < 5; i++)
        {
            Console.WriteLine($"\n--- Temperature: {currentTemp:F2} ---");
            try
            {
                var story = await writer.GenerateStory(basePrompt, currentTemp, 200, null);
                Console.WriteLine(story.Substring(0, Math.Min(story.Length, 150)) + "...");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            currentTemp += 0.45;
        }
    }
}
