
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

# Source File: solution_exercise_4.cs
# Description: Solution for Exercise 4
# ==========================================

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.ML.OnnxRuntimeGenAI;

public class BenchmarkRunner
{
    public static void Main(string[] args)
    {
        string modelPath = "./models/phi-3-mini";
        var prompts = new List<string>
        {
            "What is AI?",
            "Explain the theory of relativity briefly."
        };

        try
        {
            using var model = new OnnxGenAIModel(new ModelOptions { ModelPath = modelPath });
            using var tokenizer = new OnnxGenAITokenizer(model);

            RunBenchmark(prompts, model, tokenizer);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Benchmark failed: {ex.Message}");
        }
    }

    public static void RunBenchmark(List<string> prompts, OnnxGenAIModel model, OnnxGenAITokenizer tokenizer)
    {
        Console.WriteLine("| Prompt | Tokenization (ms) | Generation (ms) | Tokens Generated | TPS |");
        Console.WriteLine("|--------|-------------------|-----------------|------------------|-----|");

        foreach (var prompt in prompts)
        {
            // 1. Benchmark Tokenization
            var swToken = Stopwatch.StartNew();
            using var inputTensors = tokenizer.Encode(new List<string> { prompt });
            swToken.Stop();
            
            // 2. Benchmark Generation
            var swGen = Stopwatch.StartNew();
            using var sequences = new OnnxGenAISequences(inputTensors);
            
            // We use the high-level Generate method here for a complete benchmark of the batch
            // In a real scenario, you might benchmark step-by-step generation.
            model.Generate(sequences); 
            swGen.Stop();

            // 3. Calculate Metrics
            // Note: Getting token count depends on the library API. 
            // Here we estimate based on the input length or the output sequence length.
            // Assuming we can get the output sequence length:
            int outputTokenCount = sequences.Sequences[0].Length; // Simplified access
            
            // Calculate TPS (Tokens Per Second)
            double genMs = swGen.ElapsedMilliseconds;
            double tps = outputTokenCount / (genMs / 1000.0);

            // 4. Print Results
            Console.WriteLine($"| {prompt.Substring(0, Math.Min(15, prompt.Length))}... | {swToken.ElapsedMilliseconds,17} | {genMs,15} | {outputTokenCount,16} | {tps:F2} |");
        }
    }
}
