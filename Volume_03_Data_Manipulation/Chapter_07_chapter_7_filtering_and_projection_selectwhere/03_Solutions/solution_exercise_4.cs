
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

using System;
using System.Collections.Generic;
using System.Linq;

public class Embedding
{
    public string Label { get; set; }
    public double[] Vector { get; set; }
}

public class Exercise4
{
    // Pure helper function for Euclidean norm (Magnitude)
    private static double CalculateMagnitude(double[] vector)
    {
        if (vector == null || vector.Length == 0) return 0;
        // Sum of squares
        return Math.Sqrt(vector.Sum(x => x * x));
    }

    public static void Run()
    {
        var embeddings = new List<Embedding>
        {
            new Embedding { Label = "A", Vector = new[] { 1.0, 2.0, 3.0 } }, // Mag ~3.74
            new Embedding { Label = "B", Vector = new[] { 0.0, 0.0, 0.0 } }, // Mag 0 (Filter out)
            new Embedding { Label = "C", Vector = new[] { -1.0, -1.0 } }     // Mag ~1.41
        };

        // The Query
        var normalizedEmbeddings = embeddings
            .Where(e => 
            {
                // Filter: Ensure magnitude is non-zero to avoid division by zero
                var mag = CalculateMagnitude(e.Vector);
                // Use epsilon for floating point safety
                return mag > 0.0001; 
            })
            .Select(e => 
            {
                // Projection: Calculate normalized vector
                var mag = CalculateMagnitude(e.Vector);
                // Pure transformation: Input Vector -> Output Normalized Array
                var normalized = e.Vector.Select(v => v / mag).ToArray();
                
                return new 
                { 
                    Label = e.Label, 
                    NormalizedVector = normalized 
                };
            });

        // Materialize
        var results = normalizedEmbeddings.ToList();

        Console.WriteLine("Normalized Embeddings:");
        foreach (var emb in results)
        {
            Console.WriteLine($"Label: {emb.Label}, Vector: [{string.Join(", ", emb.NormalizedVector)}]");
        }
    }
}
