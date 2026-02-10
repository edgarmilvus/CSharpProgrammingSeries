
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

// Source File: solution_exercise_2.cs
// Description: Solution for Exercise 2
// ==========================================

using System;
using System.Collections.Generic;
using Microsoft.ML.OnnxRuntimeGenAI;

public class TokenizerDemo
{
    public static void Main(string[] args)
    {
        // Mocking the model and tokenizer instantiation for the exercise context.
        // In a real app, the model must be loaded first.
        // using var model = new OnnxGenAIModel(new ModelOptions { ModelPath = "./models/phi-3" });
        // using var tokenizer = new OnnxGenAITokenizer(model);

        // For demonstration, we will simulate the tokenizer usage.
        // Since we cannot run actual inference without a model file, 
        // this code demonstrates the API structure.
        
        var prompts = new List<string>
        {
            "What is the capital of France?",
            "Explain quantum computing in one sentence.", // Varying length
            "", // Edge case: Empty string
            "Write a poem."
        };

        Console.WriteLine("Preparing batched inputs...");
        
        // NOTE: To run this code, you need a valid loaded model.
        // PrepareBatchedInputs(tokenizer, prompts);
    }

    public static void PrepareBatchedInputs(OnnxGenAITokenizer tokenizer, List<string> prompts)
    {
        try 
        {
            // The Encode method converts strings into a NamedTensors object.
            // It handles batching automatically based on the input list size.
            using NamedTensors inputs = tokenizer.Encode(prompts);

            // The input IDs are typically stored under the key "input_ids" or similar,
            // depending on the specific model configuration.
            // We access the first tensor (assuming standard LLM input structure).
            Tensor<int> inputIds = inputs.Tensors["input_ids"];

            // Print the dimensions (shape) of the tensor.
            // Shape is usually [BatchSize, SequenceLength].
            Console.WriteLine($"Tensor Shape: [{inputIds.Dimensions[0]}, {inputIds.Dimensions[1]}]");
            
            // Analysis of edge cases:
            // 1. Empty string: The tokenizer usually returns a tensor with shape [1, 2] 
            //    (containing Start-of-Sequence and End-of-Sequence tokens).
            // 2. Varying lengths: The tokenizer pads shorter sequences to match the longest 
            //    sequence in the batch, resulting in a rectangular tensor.
        }
        catch (KeyNotFoundException)
        {
            Console.WriteLine("Error: 'input_ids' tensor not found. Check model specific output keys.");
        }
    }
}
