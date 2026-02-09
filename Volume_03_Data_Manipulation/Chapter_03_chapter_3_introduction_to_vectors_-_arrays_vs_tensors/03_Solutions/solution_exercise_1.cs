
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

public class CollisionSimulator
{
    // We use an array of Linked Lists to handle collisions (Chaining).
    private LinkedList<KeyValuePair<int, string>>[] _buckets;
    private int _bucketCount;

    public CollisionSimulator(int bucketCount)
    {
        _bucketCount = bucketCount;
        _buckets = new LinkedList<KeyValuePair<int, string>>[bucketCount];
    }

    // A "weak" hash function to demonstrate collisions.
    // In a real scenario, we'd use key.GetHashCode() % _bucketCount.
    // Here, we force clustering to show the mechanics.
    private int GetBucketIndex(int key)
    {
        // Artificially restrict keys to the first 3 buckets to force collisions
        return key % 3; 
    }

    public void Add(int key, string value)
    {
        int index = GetBucketIndex(key);
        
        // Initialize bucket if null
        if (_buckets[index] == null)
        {
            _buckets[index] = new LinkedList<KeyValuePair<int, string>>();
        }

        // Check for existing key (Update vs Add)
        var bucket = _buckets[index];
        foreach (var pair in bucket)
        {
            if (pair.Key == key)
            {
                // Key exists, update value
                // Note: In a real dictionary, we might update the node value directly
                // but for this simulation, we remove and re-add for simplicity.
                bucket.Remove(pair);
                break;
            }
        }

        bucket.AddLast(new KeyValuePair<int, string>(key, value));
    }

    public string Get(int key)
    {
        int index = GetBucketIndex(key);
        var bucket = _buckets[index];

        if (bucket == null) return null;

        // Linear scan within the bucket (Linked List traversal)
        foreach (var pair in bucket)
        {
            if (pair.Key == key)
            {
                return pair.Value;
            }
        }

        return null; // Key not found
    }

    public void PrintStats()
    {
        int maxDepth = 0;
        int emptyBuckets = 0;
        
        for (int i = 0; i < _bucketCount; i++)
        {
            if (_buckets[i] == null)
            {
                emptyBuckets++;
                continue;
            }
            int depth = 0;
            foreach(var node in _buckets[i]) depth++;
            if (depth > maxDepth) maxDepth = depth;
        }

        Console.WriteLine($"Total Buckets: {_bucketCount}");
        Console.WriteLine($"Empty Buckets: {emptyBuckets}");
        Console.WriteLine($"Max Chain Length (Depth): {maxDepth}");
        Console.WriteLine($"Worst-Case Lookup Complexity: O({maxDepth})"); // Linear scan
    }
}

// Usage Example for the Exercise
public class Exercise1Runner
{
    public static void Run()
    {
        var sim = new CollisionSimulator(10); // 10 buckets
        
        // Intentionally adding items that will collide (keys 1, 4, 7, 10...)
        for (int i = 1; i <= 20; i++) 
        {
            sim.Add(i, $"Token_{i}");
        }

        sim.PrintStats();
        
        // Demonstrate Lookup
        Console.WriteLine($"Lookup Key 4: {sim.Get(4)}");
        Console.WriteLine($"Lookup Key 5: {sim.Get(5)}"); // Might be in a different bucket
    }
}
