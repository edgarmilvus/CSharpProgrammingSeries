
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

public class VectorizedVocabulary
{
    private LinkedList<KeyValuePair<int, float[]>>[] _buckets;
    private int _bucketCount;

    public VectorizedVocabulary(int bucketCount)
    {
        _bucketCount = bucketCount;
        _buckets = new LinkedList<KeyValuePair<int, float[]>>[bucketCount];
    }

    private int GetBucketIndex(int id)
    {
        return id % _bucketCount;
    }

    public void Add(int id, float[] vector)
    {
        int index = GetBucketIndex(id);
        if (_buckets[index] == null) _buckets[index] = new LinkedList<KeyValuePair<int, float[]>>();

        var bucket = _buckets[index];
        // Update if exists
        foreach (var pair in bucket)
        {
            if (pair.Key == id)
            {
                bucket.Remove(pair);
                break;
            }
        }
        bucket.AddLast(new KeyValuePair<int, float[]>(id, vector));
    }

    public float[] GetVector(int id)
    {
        int index = GetBucketIndex(id);
        var bucket = _buckets[index];
        if (bucket == null) return null;

        foreach (var pair in bucket)
        {
            if (pair.Key == id) return pair.Value;
        }
        return null;
    }

    // Custom Iterator using yield return
    public IEnumerable<KeyValuePair<int, float[]>> GetAllTokens()
    {
        for (int i = 0; i < _bucketCount; i++)
        {
            if (_buckets[i] == null) continue;

            foreach (var node in _buckets[i])
            {
                // Yield return allows streaming one item at a time
                yield return node;
            }
        }
    }

    public float[] AddVectors(int id1, int id2)
    {
        float[] v1 = GetVector(id1);
        float[] v2 = GetVector(id2);

        if (v1 == null || v2 == null) throw new KeyNotFoundException("One or both IDs not found.");
        if (v1.Length != v2.Length) throw new InvalidOperationException("Vector dimensions mismatch.");

        float[] result = new float[v1.Length];
        
        // Manual loop - NO LINQ
        for (int i = 0; i < v1.Length; i++)
        {
            result[i] = v1[i] + v2[i];
        }

        return result;
    }
}

public class Exercise4Runner
{
    public static void Run()
    {
        var vocab = new VectorizedVocabulary(5);
        
        // Add dummy embeddings (3-dimensional vectors)
        vocab.Add(10, new float[] { 0.1f, 0.2f, 0.3f });
        vocab.Add(20, new float[] { 0.5f, 0.5f, 0.5f });
        vocab.Add(11, new float[] { 1.0f, 0.0f, 0.0f }); // Forces collision with 10 (if bucket size is 5, 10%5=0, 11%5=1... wait, let's check logic)

        // Note: My GetBucketIndex uses modulo. 10 % 5 = 0. 11 % 5 = 1. 
        // To force collision in this small set, let's add 15 (15 % 5 = 0).
        vocab.Add(15, new float[] { 2.0f, 2.0f, 2.0f });

        Console.WriteLine("--- Iterating via Yield Return ---");
        foreach (var token in vocab.GetAllTokens())
        {
            Console.WriteLine($"ID: {token.Key}, Vector: [{string.Join(", ", token.Value)}]");
        }

        Console.WriteLine("\n--- Vector Addition ---");
        float[] sum = vocab.AddVectors(10, 15);
        Console.WriteLine($"Sum of ID 10 and 15: [{string.Join(", ", sum)}]");
    }
}
