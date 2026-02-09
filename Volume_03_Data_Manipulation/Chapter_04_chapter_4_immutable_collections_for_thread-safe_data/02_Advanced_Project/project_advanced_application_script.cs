
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

# Source File: project_advanced_application_script.cs
# Description: Advanced Application Script
# ==========================================

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace DataStructuresDeepDive
{
    // A custom implementation of a Hash Table to demonstrate internal mechanics.
    // This mimics how Dictionary<TKey, TValue> works under the hood.
    public class CustomHashTable
    {
        // Internal storage: An array of "Buckets".
        // In a real Dictionary, this is an array of Entry structs.
        private string[] _buckets;
        private int _size;

        public CustomHashTable(int initialCapacity)
        {
            _buckets = new string[initialCapacity];
            _size = 0;
        }

        // The Hash Function: Maps a key to an array index.
        // 1. Get HashCode: Converts string to integer.
        // 2. Modulo: Ensures the index fits within the array bounds.
        private int GetBucketIndex(string key)
        {
            int hashCode = key.GetHashCode();
            // Math.Abs handles negative hash codes
            int index = Math.Abs(hashCode) % _buckets.Length;
            return index;
        }

        // INSERT OPERATION: O(1) average, O(N) worst case (collision chain)
        public void Add(string key)
        {
            // Check for resizing (Load Factor logic)
            if (_size >= _buckets.Length * 0.7)
            {
                Resize();
            }

            int index = GetBucketIndex(key);

            // Handle Collisions:
            // If the bucket is already occupied, we use Linear Probing
            // (moving to the next available slot).
            while (_buckets[index] != null)
            {
                // If key already exists, do not add duplicate
                if (_buckets[index] == key) return;

                // Move to next index (wrap around if at end)
                index = (index + 1) % _buckets.Length;
            }

            _buckets[index] = key;
            _size++;
        }

        // LOOKUP OPERATION: O(1) average
        public bool Contains(string key)
        {
            int index = GetBucketIndex(key);
            int startIndex = index;

            // Probe until we find the key or an empty slot
            while (_buckets[index] != null)
            {
                if (_buckets[index] == key)
                {
                    return true; // Found!
                }

                // Move to next index
                index = (index + 1) % _buckets.Length;

                // Optimization: If we loop back to start, the item isn't here
                if (index == startIndex) break;
            }

            return false;
        }

        private void Resize()
        {
            // Create a new, larger array (usually double the size)
            string[] newBuckets = new string[_buckets.Length * 2];

            // Rehash all existing items into the new array
            // This is an O(N) operation, but happens rarely
            foreach (var item in _buckets)
            {
                if (item != null)
                {
                    int newIndex = Math.Abs(item.GetHashCode()) % newBuckets.Length;
                    // Simplified probing for resize demo
                    while (newBuckets[newIndex] != null)
                    {
                        newIndex = (newIndex + 1) % newBuckets.Length;
                    }
                    newBuckets[newIndex] = item;
                }
            }

            _buckets = newBuckets;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== Tokenizer Vocabulary Builder Simulation ===");
            Console.WriteLine("Scenario: Processing 10,000 unique words to build a vocabulary ID map.\n");

            // 1. SETUP: Generate dummy data (corpus of words)
            List<string> corpus = new List<string>();
            for (int i = 0; i < 10000; i++)
            {
                corpus.Add($"token_{i}");
            }

            // --- APPROACH 1: NAIVE LIST (BAD FOR LOOKUPS) ---
            Console.WriteLine("1. Testing Naive List Approach...");
            List<string> vocabularyList = new List<string>();
            Stopwatch swList = Stopwatch.StartNew();

            foreach (string word in corpus)
            {
                // CRITICAL MECHANIC: List.Contains scans the array linearly.
                // Complexity: O(N) where N is current list size.
                // As the list grows, every check gets slower.
                if (!vocabularyList.Contains(word))
                {
                    vocabularyList.Add(word);
                }
            }
            swList.Stop();
            Console.WriteLine($"   Time taken: {swList.ElapsedMilliseconds} ms");
            Console.WriteLine($"   Complexity: O(N^2) total for N items.\n");

            // --- APPROACH 2: CUSTOM HASH TABLE (EFFICIENT) ---
            Console.WriteLine("2. Testing Custom Hash Table (Dictionary-like)...");
            // Start with a small capacity to demonstrate resizing
            CustomHashTable vocabularyHash = new CustomHashTable(16); 
            Stopwatch swHash = Stopwatch.StartNew();

            foreach (string word in corpus)
            {
                // CRITICAL MECHANIC: Hash Table probes directly to a bucket index.
                // Complexity: O(1) average.
                // Performance remains constant regardless of size.
                vocabularyHash.Add(word);
            }
            swHash.Stop();
            Console.WriteLine($"   Time taken: {swHash.ElapsedMilliseconds} ms");
            Console.WriteLine($"   Complexity: O(N) total for N items.\n");

            // --- APPROACH 3: BUILT-IN DICTIONARY (PRODUCTION STANDARD) ---
            Console.WriteLine("3. Testing Built-in Dictionary<TKey, TValue>...");
            Dictionary<string, int> vocabularyDict = new Dictionary<string, int>();
            Stopwatch swDict = Stopwatch.StartNew();

            int idCounter = 0;
            foreach (string word in corpus)
            {
                // Built-in Dictionary uses optimized hashing and buckets.
                // It also handles memory management and collision resolution efficiently.
                if (!vocabularyDict.ContainsKey(word))
                {
                    vocabularyDict.Add(word, idCounter++);
                }
            }
            swDict.Stop();
            Console.WriteLine($"   Time taken: {swDict.ElapsedMilliseconds} ms");

            // Visualizing the performance difference
            Console.WriteLine("\n=== Performance Analysis ===");
            Console.WriteLine($"List Approach:     {swList.ElapsedMilliseconds} ms (Linear Growth)");
            Console.WriteLine($"Custom Hash:       {swHash.ElapsedMilliseconds} ms (Constant Time)");
            Console.WriteLine($"Built-in Dict:     {swDict.ElapsedMilliseconds} ms (Optimized Constant Time)");

            if (swList.ElapsedMilliseconds > swHash.ElapsedMilliseconds * 10)
            {
                Console.WriteLine("\nConclusion: Dictionary lookups are exponentially faster for large datasets.");
            }

            // --- ITERATOR DEMO: YIELD RETURN ---
            Console.WriteLine("\n=== Memory Efficient Iteration (Iterators) ===");
            Console.WriteLine("Generating a sequence of IDs using 'yield return'...");
            
            // Using an iterator to generate IDs on demand without creating a full list in memory
            foreach (int id in GetIdsAboveThreshold(vocabularyDict, 5000))
            {
                // Only processing items as needed
                if (id % 10000 == 0) // Just printing a few to verify
                {
                    Console.WriteLine($"   Found ID: {id}");
                }
            }
        }

        // Custom Iterator using 'yield return'
        // This allows us to iterate over a collection without storing the entire result set in memory.
        static IEnumerable<int> GetIdsAboveThreshold(Dictionary<string, int> dict, int threshold)
        {
            foreach (var pair in dict)
            {
                if (pair.Value > threshold)
                {
                    // 'yield return' pauses execution here, returns the value,
                    // and waits for the next MoveNext() call from the foreach loop.
                    yield return pair.Value;
                }
            }
        }
    }
}
