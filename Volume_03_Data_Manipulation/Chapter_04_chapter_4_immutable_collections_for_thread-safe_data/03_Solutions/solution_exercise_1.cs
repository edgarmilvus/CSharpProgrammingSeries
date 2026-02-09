
#
# These sources are part of the "C# Programming Series" by Edgar Milvus, 
# you can find it on stores: 
# 
# https://www.amazon.com/dp/B0GKJ3NYL6 or https://tinyurl.com/CSharpProgrammingBooks or 
# https://leanpub.com/u/edgarmilvus (quantity discounts)
# 
# New books info: https://linktr.ee/edgarmilvus 
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

# Source File: solution_exercise_1.cs
# Description: Solution for Exercise 1
# ==========================================

using System;
using System.Collections.Generic;
using System.Diagnostics;

public class TokenizerVocabulary
{
    private readonly Dictionary<string, int> _vocab;

    public TokenizerVocabulary()
    {
        _vocab = new Dictionary<string, int>();
    }

    public void Add(string token, int id)
    {
        // O(1) average case.
        // Internal Mechanism: Computes hash of 'token', maps to a bucket index.
        // If collision occurs (rare with good hashing), it handles it via chaining or probing.
        _vocab[token] = id;
    }

    public int GetId(string token)
    {
        // O(1) average case.
        // Internal Mechanism: Hashes 'token', finds bucket, checks equality.
        if (_vocab.TryGetValue(token, out int id))
        {
            return id;
        }
        throw new KeyNotFoundException($"Token '{token}' not found.");
    }
}

public class SlowTokenizerVocabulary
{
    private readonly List<KeyValuePair<string, int>> _vocabList;

    public SlowTokenizerVocabulary()
    {
        _vocabList = new List<KeyValuePair<string, int>>();
    }

    public void Add(string token, int id)
    {
        // O(1) amortized (dynamic array resizing).
        _vocabList.Add(new KeyValuePair<string, int>(token, id));
    }

    public int GetId(string token)
    {
        // O(N) - Linear Search.
        // Internal Mechanism: Iterates from index 0 to N-1, comparing strings.
        // String comparison is O(K) where K is length, but the loop dominates.
        for (int i = 0; i < _vocabList.Count; i++)
        {
            if (_vocabList[i].Key == token)
            {
                return _vocabList[i].Value;
            }
        }
        throw new KeyNotFoundException($"Token '{token}' not found.");
    }
}

public class Exercise1Runner
{
    public static void Run()
    {
        Console.WriteLine("--- Exercise 1: Tokenizer Lookup Efficiency ---");
        
        int size = 100000;
        var dictVocab = new TokenizerVocabulary();
        var listVocab = new SlowTokenizerVocabulary();

        // Populate
        for (int i = 0; i < size; i++)
        {
            string token = $"token_{i}";
            dictVocab.Add(token, i);
            listVocab.Add(token, i);
        }

        // Benchmark Dictionary
        var sw = Stopwatch.StartNew();
        int id = dictVocab.GetId($"token_{size - 1}"); // Last item
        sw.Stop();
        Console.WriteLine($"Dictionary Lookup (O(1)): {sw.ElapsedTicks} ticks. Result: {id}");

        // Benchmark List
        sw.Restart();
        id = listVocab.GetId($"token_{size - 1}"); // Last item (worst case for List)
        sw.Stop();
        Console.WriteLine($"List Lookup (O(N)): {sw.ElapsedTicks} ticks. Result: {id}");
        
        Console.WriteLine("Explanation: Dictionary uses a hash function to compute an index in an array (bucket).");
        Console.WriteLine("List requires iterating through every element until a match is found.");
    }
}
