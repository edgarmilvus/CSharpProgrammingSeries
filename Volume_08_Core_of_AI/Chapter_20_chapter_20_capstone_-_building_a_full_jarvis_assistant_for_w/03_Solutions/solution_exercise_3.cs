
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

// Project: Jarvis.OrchestratorTest.csproj
// Dependencies: Microsoft.SemanticKernel, Microsoft.SemanticKernel.Planners.Core

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Planners;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Jarvis.OrchestratorTest
{
    public class FilePlugin
    {
        [KernelFunction("list_files")]
        [Description("Lists files in a specific directory.")]
        public List<string> ListFiles([Description("The path to the directory")] string directoryPath)
        {
            try
            {
                return Directory.GetFiles(directoryPath).Select(Path.GetFileName).ToList();
            }
            catch (Exception ex)
            {
                return new List<string> { $"Error: {ex.Message}" };
            }
        }
    }

    public class AnalysisPlugin
    {
        [KernelFunction("analyze_text")]
        [Description("Analyzes text and provides a summary or word count.")]
        public string AnalyzeText([Description("The text to analyze")] string text)
        {
            // Simulation of analysis
            int wordCount = text.Split(new[] { ' ', '\t', '\n' }, StringSplitOptions.RemoveEmptyEntries).Length;
            return $"Analysis Result: The text contains {wordCount} words. Summary: This is a simulated analysis of the content.";
        }
    }

    class Program
    {
        static async Task Main(string[] args)
        {
            // 1. Setup Kernel with LLM (Configure your AI endpoint here)
            var builder = Kernel.CreateBuilder();
            // builder.AddAzureOpenAIChatCompletion(...) 
            var kernel = builder.Build();

            // 2. Import Plugins
            kernel.ImportPluginFromObject(new FilePlugin(), "Files");
            kernel.ImportPluginFromObject(new AnalysisPlugin(), "Analyzer");

            // 3. Create Planner
            var planner = new StepwisePlanner(kernel);

            // 4. Define Goal
            string goal = "Find all text files in the C:\\Temp directory and provide a summary of the first file's content.";

            Console.WriteLine($"Goal: {goal}");

            // 5. Execute Plan
            // Note: StepwisePlanner requires an LLM to generate the plan steps.
            // If no LLM is configured, this will throw or return an empty plan.
            try
            {
                var plan = await planner.CreatePlanAsync(goal);
                
                Console.WriteLine("\n--- Generated Plan Steps ---");
                foreach (var step in plan.Steps)
                {
                    Console.WriteLine($"- {step.Name} ({step.PluginName})");
                }

                Console.WriteLine("\n--- Executing Plan ---");
                // Execute the plan
                KernelResult result = await plan.InvokeAsync(kernel);

                Console.WriteLine($"\nFinal Result: {result}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Planning/Execution failed: {ex.Message}");
                Console.WriteLine("Note: Ensure an LLM (e.g., Azure OpenAI) is configured for the planner to work.");
            }
        }
    }
}
