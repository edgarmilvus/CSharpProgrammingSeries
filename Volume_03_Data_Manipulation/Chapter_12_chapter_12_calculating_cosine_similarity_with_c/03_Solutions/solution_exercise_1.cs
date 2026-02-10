
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

using System;
using System.Collections.Generic;
using System.Linq;

public class TextPipeline
{
    public static void Main()
    {
        // 1. Source data
        var documents = new List<string>
        {
            "Hello, World! This is C#.",
            "LINQ is powerful."
        };

        // 2. Define the deferred query
        // We are constructing the pipeline, not executing it yet.
        var tokenizedQuery = documents
            .Select(doc => doc.ToLower())
            .Select(doc => new string(doc.Where(c => !char.IsPunctuation(c)).ToArray()))
            .SelectMany(doc => doc.Split(' ', StringSplitOptions.RemoveEmptyEntries));

        // 3. Modify the source data
        // This is allowed because the query hasn't been executed.
        documents.Add("Deferred execution is fascinating.");

        Console.WriteLine("--- Processing Pipeline ---");

        // 4. Immediate Execution (Iteration)
        // The query is now executed. The new document is processed.
        foreach (var token in tokenizedQuery)
        {
            Console.WriteLine(token);
        }
        
        // 5. Proof of Deferred Execution
        // If we had called .ToList() before adding the document, 
        // the new document would not appear in the results.
    }
}
