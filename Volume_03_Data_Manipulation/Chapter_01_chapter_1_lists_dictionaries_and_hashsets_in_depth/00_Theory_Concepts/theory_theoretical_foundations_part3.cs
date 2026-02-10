
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

// Source File: theory_theoretical_foundations_part3.cs
// Description: Theoretical Foundations
// ==========================================

using System;
using System.Collections;
using System.Collections.Generic;

// A simplified conceptual model of a hash table bucket
public class SimpleBucket<T>
{
    public T Item;
    public SimpleBucket<T> Next;
}

public class SimpleHashTable<T> : IEnumerable<T>
{
    private SimpleBucket<T>[] _buckets = new SimpleBucket<T>[8];

    public void Add(T item)
    {
        int index = Math.Abs(item.GetHashCode()) % _buckets.Length;
        
        // Add to the front of the linked list (Chaining)
        var newBucket = new SimpleBucket<T> 
        { 
            Item = item, 
            Next = _buckets[index] 
        };
        _buckets[index] = newBucket;
    }

    // Using 'yield return' to traverse the sparse array efficiently
    // without creating a separate list in memory.
    public IEnumerator<T> GetEnumerator()
    {
        for (int i = 0; i < _buckets.Length; i++)
        {
            var current = _buckets[i];
            while (current != null)
            {
                yield return current.Item;
                current = current.Next;
            }
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
