
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

# Source File: solution_exercise_3.cs
# Description: Solution for Exercise 3
# ==========================================

using System;
using System.Collections.Generic;

public class CustomDictionary<K, V>
{
    // We use a List of Entries to simulate a Linked List inside a bucket.
    // In a real Dictionary, this is often a struct array with 'next' pointers.
    private struct Entry
    {
        public K Key;
        public V Value;
        public int HashCode;
    }

    private List<Entry>[] buckets;
    private int count;
    private const double LoadFactorThreshold = 0.75;

    public CustomDictionary(int initialCapacity = 16)
    {
        buckets = new List<Entry>[initialCapacity];
        count = 0;
    }

    private int GetBucketIndex(K key)
    {
        int hashCode = key.GetHashCode();
        // Handle negative hash codes by taking absolute value
        int absHash = Math.Abs(hashCode);
        return absHash % buckets.Length;
    }

    public void Add(K key, V value)
    {
        // 1. Check Load Factor and Resize if necessary
        if ((double)count / buckets.Length > LoadFactorThreshold)
        {
            Resize();
        }

        int index = GetBucketIndex(key);
        
        // Initialize bucket if null
        if (buckets[index] == null)
        {
            buckets[index] = new List<Entry>();
        }

        // 2. Collision Handling: Linear scan of the bucket (Linked List)
        foreach (var entry in buckets[index])
        {
            if (entry.Key.Equals(key))
            {
                throw new ArgumentException("Key already exists.");
            }
        }

        // 3. Add to bucket (Append to Linked List)
        buckets[index].Add(new Entry { Key = key, Value = value, HashCode = key.GetHashCode() });
        count++;
    }

    public V Get(K key)
    {
        int index = GetBucketIndex(key);
        var bucket = buckets[index];

        if (bucket != null)
        {
            foreach (var entry in bucket)
            {
                if (entry.Key.Equals(key))
                {
                    return entry.Value;
                }
            }
        }
        
        throw new KeyNotFoundException();
    }

    private void Resize()
    {
        int newCapacity = buckets.Length * 2;
        var oldBuckets = buckets;
        
        buckets = new List<Entry>[newCapacity];
        count = 0; 

        // Rehash all existing items into new buckets
        foreach (var bucket in oldBuckets)
        {
            if (bucket != null)
            {
                foreach (var entry in bucket)
                {
                    // Recursive call to Add handles rehashing index calculation
                    Add(entry.Key, entry.Value);
                }
            }
        }
    }
}
