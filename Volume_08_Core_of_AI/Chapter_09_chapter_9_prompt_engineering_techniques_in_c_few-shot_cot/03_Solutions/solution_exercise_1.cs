
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

// Source File: solution_exercise_1.cs
// Description: Solution for Exercise 1
// ==========================================

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System.ComponentModel;

namespace PromptEngineeringExercises;

public class SentimentAnalyzerPlugin
{
    [KernelFunction, Description("Analyzes the sentiment of the given text.")]
    public async Task<string> AnalyzeSentiment(
        Kernel kernel, 
        [Description("The text to analyze")] string text)
    {
        // The prompt uses Few-Shot Prompting by providing explicit examples
        // before asking the AI to classify the new input.
        var prompt = """
            You are a sentiment analysis AI. Classify the input text into one of four categories: 
            Positive, Negative, Sarcastic, or Mixed.

            Examples:
            1. Input: "Oh, fantastic. A meeting that could have been an email." -> Output: Sarcastic
            2. Input: "The food was delicious, but the service was painfully slow." -> Output: Mixed
            3. Input: "I'm just thrilled to be stuck in traffic." -> Output: Negative

            Analyze the following text and return only the category name:
            Text: {{$text}}
            """;

        var result = await kernel.InvokePromptAsync<string>(prompt, new KernelArguments { ["text"] = text });
        return result ?? "Unknown";
    }
}

// Console Application Integration
public static class Program
{
    public static async Task Main(string[] args)
    {
        // 1. Kernel Setup
        var builder = Kernel.CreateBuilder();
        
        // NOTE: Replace with your actual LLM provider configuration (e.g., AzureOpenAI or OpenAI)
        // builder.AddAzureOpenAIChatCompletion(...); 
        // builder.AddOpenAIChatCompletion(...);
        
        // For demonstration purposes, we will simulate the kernel if no LLM is configured.
        // In a real scenario, you must configure the chat completion service.
        var kernel = builder.Build();

        // 2. Plugin Creation
        var sentimentPlugin = new SentimentAnalyzerPlugin();
        
        // 3. Integration
        var testCases = new[]
        {
            "This is the best day of my life!",
            "The code compiles, which is a start.",
            "Another bug fix, just what I needed."
        };

        Console.WriteLine("--- Sentiment Analysis Results ---");

        foreach (var testCase in testCases)
        {
            try 
            {
                // Note: If no LLM is configured, this will throw an exception.
                // We wrap it in a try-catch to allow the code to compile and run 
                // even without valid API keys for this example.
                var result = await sentimentPlugin.AnalyzeSentiment(kernel, testCase);
                Console.WriteLine($"Input: \"{testCase}\"");
                Console.WriteLine($"Result: {result}\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Input: \"{testCase}\"");
                Console.WriteLine($"Error: LLM service not configured. (Details: {ex.Message})\n");
            }
        }
    }
}
