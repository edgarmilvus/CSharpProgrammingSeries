
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

// Source File: basic_basic_code_example.cs
// Description: Basic Code Example
// ==========================================

using System;
using System.Collections.Generic;

public class DictionaryVsListLookup
{
    public static void Main()
    {
        // REAL-WORLD CONTEXT:
        // Imagine a system processing a stream of unique tokens (e.g., words in a book or user IDs).
        // We need to check if a specific token has already been processed.
        // This is a critical operation in Tokenizers for AI models or Graph Traversal algorithms.

        // DATA SETUP:
        // We will populate a List and a HashSet with 10,000 integers.
        const int dataSize = 10000;
        List<int> listData = new List<int>(dataSize);
        HashSet<int> hashSetData = new HashSet<int>();

        for (int i = 0; i < dataSize; i++)
        {
            listData.Add(i);
            hashSetData.Add(i);
        }

        int searchTarget = 9999; // The value we are looking for

        // ---------------------------------------------------------
        // 1. LIST LOOKUP (Linear Search)
        // ---------------------------------------------------------
        // Algorithm: Iterate through the list until the item is found.
        // Complexity: O(n) - In the worst case, we check every element.
        bool foundInList = false;
        foreach (int number in listData)
        {
            if (number == searchTarget)
            {
                foundInList = true;
                break; // Found it, stop searching
            }
        }

        // ---------------------------------------------------------
        // 2. HASHSET LOOKUP (Hash Table Lookup)
        // ---------------------------------------------------------
        // Algorithm: 
        // 1. Calculate HashCode of searchTarget.
        // 2. Map HashCode to a specific "Bucket" (index).
        // 3. Check the bucket. If collision exists, verify exact value.
        // Complexity: O(1) - Average case. Time remains constant regardless of size.
        bool foundInHashSet = hashSetData.Contains(searchTarget);

        // OUTPUT RESULTS
        Console.WriteLine($"Searching for value: {searchTarget}");
        Console.WriteLine($"List Lookup Result: {foundInList}");
        Console.WriteLine($"HashSet Lookup Result: {foundInHashSet}");
        
        // PERFORMANCE COMPARISON DEMONSTRATION
        // We will now perform the lookup 1000 times to measure the difference.
        
        System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();

        // Test List Performance
        stopwatch.Start();
        for (int i = 0; i < 1000; i++)
        {
            // We search for a value that is at the end of the list (worst case for List)
            // This forces the loop to iterate almost 10,000 times per check.
            listData.Contains(searchTarget); 
        }
        stopwatch.Stop();
        Console.WriteLine($"\nTime taken for 1000 lookups (List): {stopwatch.ElapsedMilliseconds} ms");

        // Test HashSet Performance
        stopwatch.Restart();
        for (int i = 0; i < 1000; i++)
        {
            // HashSet uses the hash code to jump directly to the bucket.
            // It does not iterate through previous elements.
            hashSetData.Contains(searchTarget);
        }
        stopwatch.Stop();
        Console.WriteLine($"Time taken for 1000 lookups (HashSet): {stopwatch.ElapsedMilliseconds} ms");
    }
}
