
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

// Source File: solution_exercise_4.cs
// Description: Solution for Exercise 4
// ==========================================

// File: TextProcessingPlugin.cs
using Microsoft.SemanticKernel;

public class TextProcessingPlugin
{
    [KernelFunction("summarize_text")]
    public string SummarizeText(string input)
    {
        // Simple heuristic: Return the first sentence or a shortened string
        if (string.IsNullOrEmpty(input)) return string.Empty;
        var sentences = input.Split('.');
        return sentences.Length > 0 ? sentences[0] + "." : input.Substring(0, Math.Min(50, input.Length));
    }

    [KernelFunction("analyze_sentiment")]
    public string AnalyzeSentiment(string input)
    {
        // Simple heuristic for demo purposes
        if (input.Contains("error") || input.Contains("bad")) return "Negative";
        if (input.Contains("good") || input.Contains("great")) return "Positive";
        return "Neutral";
    }

    // Interactive Challenge: Translation Function
    [KernelFunction("translate_text")]
    public string TranslateText(string targetLanguage, string input)
    {
        // Mock translation logic
        return $"[{targetLanguage.ToUpper()}] {input}";
    }
}

// File: Program.cs
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Planning.Sequential;

var kernel = Kernel.CreateBuilder()
    .AddAzureOpenAIChatCompletion("deployment-name", "https://endpoint", "key")
    .Build();

// Import the plugin
kernel.ImportPluginFromObject(new TextProcessingPlugin(), "textProcessing");

// Configure the Sequential Planner
var planner = new SequentialPlanner(kernel);

// Interactive Challenge: Update prompt for 3 steps (Summarize -> Translate -> Analyze)
var planPrompt = @"
Summarize the following text, translate it to French, and then analyze the sentiment of the translated summary: 
'The quick brown fox jumps over the lazy dog. This is a great example of a sentence.'
";

try
{
    // Generate the plan
    var plan = await planner.CreatePlanAsync(planPrompt);

    Console.WriteLine("Generated Plan Steps:");
    foreach (var step in plan.Steps)
    {
        Console.WriteLine($" - {step.PluginName}.{step.Name}");
    }

    // Execute the plan
    var result = await plan.InvokeAsync(kernel);

    Console.WriteLine("\nExecution Result:");
    Console.WriteLine(result.ToString());
}
catch (KernelException ke)
{
    Console.WriteLine($"Planning failed: {ke.Message}");
}
