
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

public class TextEmbedding
{
    public string Text { get; set; }
    public float[] Vector { get; set; }
}

public class VectorNormalizer
{
    public static void RunExercise()
    {
        var rawData = new List<TextEmbedding>
        {
            new TextEmbedding { Text = "Valid Vector", Vector = new float[] { 1f, 2f, 2f } }, // Length = 3
            new TextEmbedding { Text = "Null Vector", Vector = null },
            new TextEmbedding { Text = "Empty Vector", Vector = new float[] { } },
            new TextEmbedding { Text = "NaN Vector", Vector = new float[] { 1f, float.NaN, 3f } },
            new TextEmbedding { Text = "Infinite Vector", Vector = new float[] { 1f, 2f, float.PositiveInfinity } }
        };

        // The Functional Pipeline
        var normalizedQuery = rawData
            .Where(e => e.Vector != null && e.Vector.Length > 0)
            .Where(e => !e.Vector.Any(v => float.IsNaN(v) || float.IsInfinity(v)))
            .Select(e => 
            {
                // Calculate Euclidean Length (L2 Norm)
                float sumSquares = e.Vector.Sum(component => component * component);
                float length = (float)Math.Sqrt(sumSquares);
                
                // Normalization: Divide each component by length
                var normalizedVector = e.Vector.Select(v => v / length).ToArray();

                return new 
                {
                    Text = e.Text,
                    NormalizedVector = normalizedVector
                };
            });

        // Immediate Execution
        var results = normalizedQuery.ToList();

        foreach (var item in results)
        {
            Console.WriteLine($"Text: {item.Text}, Vector: [{string.Join(", ", item.NormalizedVector)}]");
        }
    }
}
