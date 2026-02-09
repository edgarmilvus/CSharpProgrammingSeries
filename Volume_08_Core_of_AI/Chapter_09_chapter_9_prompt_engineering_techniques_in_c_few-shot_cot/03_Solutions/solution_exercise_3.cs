
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
using System.ComponentModel;

namespace PromptEngineeringExercises;

public class EmailClassifierPlugin
{
    [KernelFunction, Description("Classifies an email into Urgent, Informational, or Spam.")]
    public async Task<string> ClassifyEmail(
        Kernel kernel,
        [Description("The email content")] string emailContent)
    {
        // Refactored prompt combining CoT and Few-Shot techniques.
        var prompt = """
            You are an expert email classifier. 
            Categories: Urgent, Informational, Spam.

            Step 1 (CoT Analysis): 
            Analyze the email's content, subject line, and sender. 
            Reason about keywords (e.g., "invoice", "deadline", "subscribe") and intent (e.g., request for action, informational broadcast).
            Explicitly state your reasoning before classifying.

            Step 2 (Few-Shot Context): 
            Use these examples to guide your final classification:
            - Input: "URGENT: Server is down." -> Output: Urgent
            - Input: "Weekly newsletter attached." -> Output: Informational
            - Input: "You've won a prize! Click here." -> Output: Spam

            Step 3 (Classification):
            Based on your analysis and examples, classify the email.

            Email to classify:
            {{$emailContent}}
            """;

        var result = await kernel.InvokePromptAsync<string>(prompt, new KernelArguments { ["emailContent"] = emailContent });
        return result ?? "Unclassified";
    }
}

public static class Program
{
    public static async Task Main(string[] args)
    {
        var builder = Kernel.CreateBuilder();
        // builder.AddAzureOpenAIChatCompletion(...);
        var kernel = builder.Build();

        var classifier = new EmailClassifierPlugin();

        // Test case designed to confuse a simple classifier
        var ambiguousEmail = "Subject: URGENT: Newsletter Subscription Confirmation\nBody: Please confirm your subscription to our newsletter by clicking the link. This is an automated message.";

        Console.WriteLine("--- Email Classification Result ---");
        
        try
        {
            var result = await classifier.ClassifyEmail(kernel, ambiguousEmail);
            Console.WriteLine($"Email: {ambiguousEmail.Replace("\n", " ")}");
            Console.WriteLine($"Classification: {result}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}
