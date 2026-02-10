
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

public class CustomHashSet<T>
{
    // The core mechanism: A Dictionary provides O(1) lookups.
    // We use object as the value because we don't need to store data, just existence.
    private Dictionary<T, object> _dictionary;

    public CustomHashSet()
    {
        _dictionary = new Dictionary<T, object>();
    }

    public void Add(T item)
    {
        // Dictionary.Add throws if key exists, indexer overwrites.
        // For a Set, we just want to ensure existence.
        _dictionary[item] = null; 
    }

    public bool Contains(T item)
    {
        // O(1) average case lookup
        return _dictionary.ContainsKey(item);
    }

    // Modifies the current set to contain only elements present in both.
    public void IntersectWith(IEnumerable<T> other)
    {
        // We need a list to store items that will be removed,
        // because we cannot modify the dictionary while iterating over 'other'.
        List<T> itemsToRemove = new List<T>();

        // Strategy: Identify items in the current set that are NOT in 'other'.
        // 1. Mark all current items for removal.
        // 2. Iterate 'other'. If an item is found, unmark it.
        // 3. Remove marked items.
        
        // Optimization: Instead of marking all, we can iterate 'other' 
        // and check if it exists in current set. If it does, we keep it.
        // But we need a temporary storage for the result or items to remove.
        
        // Let's use the "Remove" strategy:
        // Iterate over a copy of keys or use a list of keys to remove.
        var keys = new List<T>(_dictionary.Keys);
        
        foreach (var key in keys)
        {
            bool foundInOther = false;
            foreach (var otherItem in other)
            {
                if (EqualityComparer<T>.Default.Equals(key, otherItem))
                {
                    foundInOther = true;
                    break; // Found it, so we keep 'key'. Move to next key.
                }
            }

            if (!foundInOther)
            {
                itemsToRemove.Add(key);
            }
        }

        // Perform removal
        foreach (var item in itemsToRemove)
        {
            _dictionary.Remove(item);
        }
    }

    public void PrintSet()
    {
        Console.Write("Set: { ");
        foreach (var key in _dictionary.Keys)
        {
            Console.Write($"{key} ");
        }
        Console.WriteLine("}");
    }
}

// Comparison Logic
public class Exercise2Runner
{
    public static void Run()
    {
        var mySet = new CustomHashSet<int>();
        mySet.Add(1); mySet.Add(2); mySet.Add(3); mySet.Add(4);
        
        List<int> otherList = new List<int> { 3, 4, 5, 6 };

        Console.WriteLine("Before Intersect:");
        mySet.PrintSet();

        // Manual Intersect implementation
        mySet.IntersectWith(otherList);

        Console.WriteLine("After Intersect (should be 3, 4):");
        mySet.PrintSet();

        // Complexity Note:
        // My implementation: O(N * M) where N is size of set, M is size of otherList.
        // Standard HashSet.IntersectWith: O(N + M) assuming hashing is O(1).
        // This exercise highlights why built-in collections are optimized internally.
    }
}
