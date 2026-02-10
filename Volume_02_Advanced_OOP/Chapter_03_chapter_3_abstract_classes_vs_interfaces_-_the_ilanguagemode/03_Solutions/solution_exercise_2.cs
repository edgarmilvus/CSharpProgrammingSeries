
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

public class TransformerModel : ILanguageModel
{
    // Implementation of the Name property
    public string ModelName => "Transformer-XL";

    public List<string> Tokenize(string input)
    {
        // Simulating a tokenizer by splitting on whitespace
        // StringSplitOptions.RemoveEmptyEntries ensures empty tokens are ignored
        string[] tokens = input.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        return new List<string>(tokens);
    }

    public string Generate(List<string> tokens, int maxTokens)
    {
        // Simulate generation logic specific to Transformers
        string sequence = string.Join(" ", tokens);
        return $"{sequence} [Transformer Generated]";
    }

    public void Train(List<string> corpus)
    {
        // Simulate training process
        Console.WriteLine($"Training {ModelName} on corpus of size {corpus.Count}...");
    }
}

public class RNNModel : ILanguageModel
{
    // Implementation of the Name property
    public string ModelName => "LSTM-RNN";

    public List<string> Tokenize(string input)
    {
        // Reusing the same simple tokenization logic for consistency
        string[] tokens = input.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        return new List<string>(tokens);
    }

    public string Generate(List<string> tokens, int maxTokens)
    {
        // Simulate generation logic specific to RNNs (sequential processing)
        string sequence = string.Join(" ", tokens);
        return $"{sequence} [RNN Generated]";
    }

    public void Train(List<string> corpus)
    {
        // Simulate training process
        Console.WriteLine($"Training {ModelName} on corpus of size {corpus.Count}...");
    }
}
