
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

# Source File: solution_exercise_5.cs
# Description: Solution for Exercise 5
# ==========================================

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;

public class ThreadSafeVocab
{
    // Version A: Immutable (Snapshot)
    // Note: Using ImmutableList for this exercise as per constraints. 
    // In a real scenario requiring O(1) lookups, we would use ImmutableDictionary.
    private ImmutableList<KeyValuePair<string, int>> _immutableList = ImmutableList<KeyValuePair<string, int>>.Empty;
    
    public void AddImmutable(string token, int id)
    {
        // Creates a new list every time. Thread-safe by nature (interlocked exchange).
        // Expensive for frequent writes (O(log N) + memory allocation).
        var newItem = new KeyValuePair<string, int>(token, id);
        
        // Interlocked ensures atomic swap of the reference
        Interlocked.Exchange(ref _immutableList, _immutableList.Add(newItem));
    }

    public int GetImmutable(string token)
    {
        // Reads are lock-free, but linear scan O(N) because we are using ImmutableList (List-based).
        // If we used ImmutableDictionary, this would be O(log N).
        foreach (var kvp in _immutableList)
        {
            if (kvp.Key == token) return kvp.Value;
        }
        throw new KeyNotFoundException();
    }

    // Version B: ConcurrentDictionary
    private readonly ConcurrentDictionary<string, int> _concurrentDict = new ConcurrentDictionary<string, int>();

    public void AddConcurrent(string token, int id)
    {
        // Thread-safe, lock-free reads, fine-grained locking on writes (per bucket).
        // O(1) average case.
        _concurrentDict[token] = id;
    }

    public int GetConcurrent(string token)
    {
        if (_concurrentDict.TryGetValue(token, out int id))
        {
            return id;
        }
        throw new KeyNotFoundException();
    }
}

public class Exercise5Runner
{
    public static void Run()
    {
        Console.WriteLine("\n--- Exercise 5: Thread Safety Trade-offs ---");
        
        var vocab = new ThreadSafeVocab();
        var tasks = new List<Task>();

        // Simulate concurrent writes
        for (int i = 0; i < 10; i++)
        {
            int id = i; // Capture for closure
            tasks.Add(Task.Run(() => 
            {
                vocab.AddImmutable($"token_{id}", id);
                vocab.AddConcurrent($"concurrent_{id}", id);
            }));
        }

        Task.WaitAll(tasks.ToArray());

        Console.WriteLine("Immutable approach: Safe, but memory intensive (snapshots). Good for functional programming.");
        Console.WriteLine("Concurrent approach: Optimized for high concurrency. Good for stateful services.");
        
        // Note: In a real Tokenizer scenario (mostly reads, rare writes), 
        // ImmutableDictionary is excellent. In a dynamic cache (frequent writes), ConcurrentDictionary wins.
    }
}
