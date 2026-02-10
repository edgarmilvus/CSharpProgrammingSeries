
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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text.Json;

public record ChunkVector(string Id, ReadOnlyMemory<float> Embedding);

public class VectorDatabase
{
    private readonly List<ChunkVector> _vectors = new();

    public void Load(string dbPath)
    {
        if (!File.Exists(dbPath)) throw new FileNotFoundException("Vector DB not found", dbPath);

        var lines = File.ReadAllLines(dbPath);
        foreach (var line in lines)
        {
            var doc = JsonSerializer.Deserialize<ChunkVector>(line);
            _vectors.Add(doc);
        }
    }

    public List<(string Id, float Score)> Search(float[] queryEmbedding, int k, float minSimilarity = 0.0f)
    {
        var results = new PriorityQueue<(string Id, float Score), float>();
        
        // Ensure query is normalized for cosine similarity
        float queryMagnitude = CalculateMagnitudeSimd(queryEmbedding);
        if (queryMagnitude == 0) return new List<(string Id, float Score)>();

        foreach (var chunk in _vectors)
        {
            // Skip if embedding is empty/zero
            if (chunk.Embedding.Length == 0) continue;

            // Calculate Cosine Similarity
            float similarity = CalculateCosineSimilaritySimd(chunk.Embedding.Span, queryEmbedding, queryMagnitude);
            
            // Interactive Challenge: Threshold Filter
            if (similarity < minSimilarity) continue;

            // Maintain Top-K using Min-Heap (PriorityQueue in .NET 6+)
            if (results.Count < k)
            {
                results.Enqueue((chunk.Id, similarity), similarity);
            }
            else if (similarity > results.Peek().Score)
            {
                results.EnqueueDequeue((chunk.Id, similarity), similarity);
            }
        }

        return results.UnorderedItems
            .OrderByDescending(x => x.Priority)
            .Select(x => (x.Element.Id, x.Element.Score))
            .ToList();
    }

    // SIMD Optimized Cosine Similarity
    private float CalculateCosineSimilaritySimd(ReadOnlySpan<float> a, ReadOnlySpan<float> b, float bMagnitude)
    {
        // 1. Dot Product
        float dotProduct = DotProductSimd(a, b);

        // 2. Magnitude of A
        float aMagnitude = CalculateMagnitudeSimd(a);

        if (aMagnitude == 0 || bMagnitude == 0) return 0;

        return dotProduct / (aMagnitude * bMagnitude);
    }

    private float DotProductSimd(ReadOnlySpan<float> a, ReadOnlySpan<float> b)
    {
        int length = Math.Min(a.Length, b.Length);
        int i = 0;
        float sum = 0;

        // Use Vector<float> for SIMD (typically 4 floats on x86/x64)
        int vectorSize = Vector<float>.Count;
        var vectorSum = Vector<float>.Zero;

        // Process in vector-sized chunks
        for (; i <= length - vectorSize; i += vectorSize)
        {
            var va = new Vector<float>(a.Slice(i, vectorSize));
            var vb = new Vector<float>(b.Slice(i, vectorSize));
            vectorSum += va * vb;
        }

        // Horizontal sum of the vector
        for (int j = 0; j < vectorSize; j++)
        {
            sum += vectorSum[j];
        }

        // Process remaining elements
        for (; i < length; i++)
        {
            sum += a[i] * b[i];
        }

        return sum;
    }

    private float CalculateMagnitudeSimd(ReadOnlySpan<float> a)
    {
        // Magnitude is sqrt(sum of squares)
        float dot = DotProductSimd(a, a);
        return (float)Math.Sqrt(dot);
    }
}
