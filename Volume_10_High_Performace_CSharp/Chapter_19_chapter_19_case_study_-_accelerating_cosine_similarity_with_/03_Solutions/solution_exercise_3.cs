
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

// Source File: solution_exercise_3.cs
// Description: Solution for Exercise 3
// ==========================================

using System;
using System.Numerics;
using System.Runtime.CompilerServices;

public static class CosineSimilarityCalculator
{
    public static float CosineSimilarityBlock(ReadOnlySpan<float> a, ReadOnlySpan<float> b)
    {
        if (a.Length != b.Length)
            throw new ArgumentException("Vectors must be the same length.");
        if (a.Length == 0)
            return 0; // Or throw, depending on definition of empty similarity

        int count = Vector<float>.Count;
        int i = 0;
        
        // Use double for accumulation to maintain precision during summation
        double dotProduct = 0;
        double magA = 0;
        double magB = 0;

        // Calculate the aligned block limit
        // We iterate until we cannot fit a full vector
        int limit = a.Length - count;
        
        if (limit >= 0)
        {
            Vector<float> vecDot = Vector<float>.Zero;
            Vector<float> vecMagA = Vector<float>.Zero;
            Vector<float> vecMagB = Vector<float>.Zero;

            for (i = 0; i <= limit; i += count)
            {
                var va = new Vector<float>(a.Slice(i, count));
                var vb = new Vector<float>(b.Slice(i, count));

                vecDot += va * vb;
                vecMagA += va * va;
                vecMagB += vb * vb;
            }

            dotProduct += Vector.Sum(vecDot);
            magA += Vector.Sum(vecMagA);
            magB += Vector.Sum(vecMagB);
        }

        // Tail loop for remaining elements
        for (; i < a.Length; i++)
        {
            float valA = a[i];
            float valB = b[i];
            dotProduct += valA * valB;
            magA += valA * valA;
            magB += valB * valB;
        }

        // Safety check for zero vectors to avoid division by zero
        if (magA == 0 || magB == 0)
            return 0;

        // Calculate final cosine similarity
        return (float)(dotProduct / (Math.Sqrt(magA) * Math.Sqrt(magB)));
    }
}
