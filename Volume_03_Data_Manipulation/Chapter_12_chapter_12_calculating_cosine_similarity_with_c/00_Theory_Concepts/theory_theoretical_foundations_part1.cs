
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

// Source File: theory_theoretical_foundations_part1.cs
// Description: Theoretical Foundations
// ==========================================

using System;
using System.Collections.Generic;
using System.Linq;

public class DataPipeline
{
    public static void ProcessDocuments(IEnumerable<string> rawDocs)
    {
        // 1. DEFINING THE QUERY (Deferred Execution)
        // No processing happens here. We are just building a blueprint.
        var validDocsQuery = rawDocs
            .Where(doc => !string.IsNullOrWhiteSpace(doc))
            .Select(doc => doc.Trim().ToLowerInvariant());

        // 2. EXECUTION TRIGGER
        // The pipeline is executed here. If rawDocs changes after this definition
        // but before this loop, the query reflects those changes.
        foreach (var doc in validDocsQuery)
        {
            Console.WriteLine($"Processing: {doc}");
        }

        // 3. IMMEDIATE EXECUTION
        // We force the query to run now and store the results in memory.
        // This creates a snapshot of the data at this specific moment.
        List<string> processedList = validDocsQuery.ToList();
    }
}
