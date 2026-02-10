
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

// Source File: theory_theoretical_foundations_part7.cs
// Description: Theoretical Foundations
// ==========================================

using System;
using System.Collections.Generic;
using System.Linq;

public class DataPipeline
{
    public List<Vector> PrepareForEmbedding(List<string> rawDocuments)
    {
        // 1. Parallel Filtering (Cleaning)
        // 2. Parallel Projection (Normalization)
        // 3. Immediate Execution (Materialization)
        
        return rawDocuments
            .AsParallel()
            .WithDegreeOfParallelism(4) // Reserve CPU for the model inference later
            .Where(doc => !string.IsNullOrWhiteSpace(doc) && doc.Length > 50)
            .Select(doc => 
            {
                // Pure transformation: Input Doc -> Normalized String
                return doc.ToLowerInvariant()
                          .Replace("\n", " ")
                          .Trim();
            })
            .Select(normalized => 
            {
                // Simulate vectorization (usually a heavy external call)
                // In a real scenario, this might be a call to an AI service or local model
                return Embed(normalized); 
            })
            .ToList(); // Trigger the parallel execution
    }

    private Vector Embed(string text)
    {
        // Placeholder for vector generation
        return new Vector(); 
    }
}

public class Vector { /* ... */ }
