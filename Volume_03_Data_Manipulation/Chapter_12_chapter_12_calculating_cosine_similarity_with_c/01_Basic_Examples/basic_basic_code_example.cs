
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

// Source File: basic_basic_code_example.cs
// Description: Basic Code Example
// ==========================================

using System;
using System.Collections.Generic;
using System.Linq;

public class DocumentPreprocessor
{
    public static void Main()
    {
        // 1. The Raw Data Source (Simulating a stream of documents)
        var rawDocuments = new List<string>
        {
            "The quick brown fox jumps over the lazy dog.",
            "  A quick brown DOG is a happy pet.  ", // Contains whitespace and casing issues
            "The lazy fox sleeps all day.",
            "Just a random sentence." // Irrelevant data
        };

        // 2. Define the Processing Pipeline (Deferred Execution)
        // We define the steps here, but no processing happens yet.
        // The 'where' and 'Select' lambdas are stored as an expression tree.
        var processingPipeline = rawDocuments
            .Where(doc => doc.Contains("fox")) // Step A: Filter (Clean/Select)
            .Select(doc => doc.Trim().ToLower()) // Step B: Normalize
            .Select(doc => $"[PROCESSED]: {doc}"); // Step C: Format

        Console.WriteLine("Pipeline defined. No processing has occurred yet.\n");

        // 3. Trigger Execution (Immediate Execution)
        // The pipeline is executed only when we iterate (e.g., .ToList()).
        // This is where the data is actually cleaned and transformed.
        List<string> processedResults = processingPipeline.ToList();

        // 4. Output the results
        Console.WriteLine("Execution Triggered. Results:");
        foreach (var result in processedResults)
        {
            Console.WriteLine(result);
        }
    }
}
