
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

# Source File: solution_exercise_5.cs
# Description: Solution for Exercise 5
# ==========================================

using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace PromptEngineeringExercises;

public class CodeCommenterPlugin
{
    [KernelFunction, Description("Generates insightful XML comments for C# code.")]
    public async Task<string> GenerateComments(
        Kernel kernel,
        [Description("The C# code to comment")] string code)
    {
        // Three-stage CoT prompt: Analysis -> Drafting -> Critique & Refine
        var prompt = """
            You are an expert C# developer. Generate high-quality XML documentation comments.
            
            Follow this three-step process strictly:

            1. ANALYSIS:
            Analyze the provided C# code. Identify the inputs, outputs, and the core algorithm or logic being implemented. Do not write any comments yet.

            2. DRAFTING:
            Based on your analysis, draft XML documentation comments for each method and line of significant logic. Focus on the 'why', not just the 'what'.

            3. CRITIQUE & REFINE:
            Review your draft. Is it too verbose? Is it missing a critical edge case (like the discount logic)? Is it unclear? If you identify any issues, refine the draft to produce the final version.

            Return ONLY the final commented code block.

            Code to comment:
            {{$code}}
            """;

        var result = await kernel.InvokePromptAsync<string>(prompt, new KernelArguments { ["code"] = code });
        return result ?? "// No comments generated.";
    }
}

public static class Program
{
    public static async Task Main(string[] args)
    {
        var builder = Kernel.CreateBuilder();
        // builder.AddAzureOpenAIChatCompletion(...);
        var kernel = builder.Build();

        var commenterPlugin = new CodeCommenterPlugin();

        var sourceCode = """
            public static double CalculateDiscount(decimal price, int customerYears)
            {
                double discount = 0.0;
                if (customerYears > 5)
                {
                    discount = 0.15;
                }
                else if (customerYears > 1)
                {
                    discount = 0.05;
                }
                if (price > 1000)
                {
                    discount += 0.02;
                }
                return discount;
            }
            """;

        Console.WriteLine("--- Code Commenting Result ---");

        try
        {
            var commentedCode = await commenterPlugin.GenerateComments(kernel, sourceCode);
            Console.WriteLine(commentedCode);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}
