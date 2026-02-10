
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

// Source File: solution_exercise_2.cs
// Description: Solution for Exercise 2
// ==========================================

using System;
using System.Collections.Generic;
using System.Text;

public class CustomDictionary<TKey, TValue>
{
    // Array of Linked Lists (Buckets)
    private readonly LinkedList<KeyValuePair<TKey, TValue>>[] _buckets;
    private readonly int _capacity;

    public CustomDictionary(int capacity)
    {
        _capacity = capacity;
        _buckets = new LinkedList<KeyValuePair<TKey, TValue>>[capacity];
    }

    private int GetBucketIndex(TKey key)
    {
        if (key == null) throw new ArgumentNullException(nameof(key));
        
        // 1. Get HashCode
        int hash = key.GetHashCode();
        
        // 2. Handle negative hashes (Abs) and Modulo to fit in array
        return Math.Abs(hash) % _capacity;
    }

    public void Add(TKey key, TValue value)
    {
        int index = GetBucketIndex(key);

        // Initialize bucket if null
        if (_buckets[index] == null)
        {
            _buckets[index] = new LinkedList<KeyValuePair<TKey, TValue>>();
        }

        // Check for existing key in the chain (update logic)
        var current = _buckets[index].First;
        while (current != null)
        {
            if (current.Value.Key.Equals(key))
            {
                current.Value = new KeyValuePair<TKey, TValue>(key, value);
                return;
            }
            current = current.Next;
        }

        // Collision handling: Add to the end of the linked list
        _buckets[index].AddLast(new KeyValuePair<TKey, TValue>(key, value));
    }

    public TValue Get(TKey key)
    {
        int index = GetBucketIndex(key);
        var bucket = _buckets[index];

        if (bucket == null) throw new KeyNotFoundException();

        // Linear scan within the specific bucket (Chain)
        var current = bucket.First;
        while (current != null)
        {
            if (current.Value.Key.Equals(key))
            {
                return current.Value.Value;
            }
            current = current.Next;
        }

        throw new KeyNotFoundException();
    }

    // Helper for visualization
    public string GetVisualizationData()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("digraph G {");
        sb.AppendLine("  rankdir=TB;");
        sb.AppendLine("  node [shape=record];");

        for (int i = 0; i < _capacity; i++)
        {
            if (_buckets[i] != null && _buckets[i].Count > 0)
            {
                sb.AppendLine($"  Bucket_{i} [label=\"Bucket {i}\"];");
                int nodeCount = 0;
                foreach (var kvp in _buckets[i])
                {
                    sb.AppendLine($"  Node_{i}_{nodeCount} [label=\"{kvp.Key} | {kvp.Value}\"];");
                    sb.AppendLine($"  Bucket_{i} -> Node_{i}_{nodeCount};");
                    if (nodeCount > 0)
                    {
                        sb.AppendLine($"  Node_{i}_{nodeCount - 1} -> Node_{i}_{nodeCount} [dir=back];");
                    }
                    nodeCount++;
                }
            }
        }
        sb.AppendLine("}");
        return sb.ToString();
    }
}

public class Exercise2Runner
{
    public static void Run()
    {
        Console.WriteLine("\n--- Exercise 2: Custom Dictionary with Chaining ---");
        
        // Small capacity (4) forces collisions for demonstration
        var dict = new CustomDictionary<string, int>(4); 
        
        dict.Add("apple", 1);   
        dict.Add("banana", 2);  
        dict.Add("cherry", 3);
        dict.Add("date", 4);

        Console.WriteLine($"Get 'cherry': {dict.Get("cherry")}");
        
        Console.WriteLine("\nVisualizing Bucket Structure (Graphviz DOT):");
        Console.WriteLine(dict.GetVisualizationData());
    }
}
