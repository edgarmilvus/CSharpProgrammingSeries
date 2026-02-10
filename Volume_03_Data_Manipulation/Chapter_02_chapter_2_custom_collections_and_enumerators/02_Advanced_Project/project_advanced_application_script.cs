
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

// Source File: project_advanced_application_script.cs
// Description: Advanced Application Script
// ==========================================

using System;
using System.Collections;
using System.Collections.Generic;

namespace Book3_Chapter2_AdvancedApp
{
    // 1. LINEAR VOCABULARY (List-based)
    // ---------------------------------------------------------
    // Internal Mechanics: Uses a contiguous array (List<T>).
    // Lookup: Linear Search (O(N)).
    // Insertion: O(1) amortized (until resize).
    // Memory: Compact, but search is slow for large datasets.
    public class LinearTokenVocabulary : IEnumerable<string>
    {
        private List<string> _tokens;

        public LinearTokenVocabulary()
        {
            _tokens = new List<string>();
        }

        // Adds a token to the end of the list.
        // Complexity: O(1) amortized.
        public void Add(string token)
        {
            // We do not check for duplicates here to demonstrate 
            // the inefficiency of linear search later.
            _tokens.Add(token);
        }

        // Finds the index of a token.
        // Complexity: O(N) - Linear Time.
        // Why? The CPU must check 'token' against every element in memory sequentially.
        public int GetId(string token)
        {
            // Manual loop (No LINQ allowed per constraints).
            for (int i = 0; i < _tokens.Count; i++)
            {
                // String equality check is expensive if called many times.
                if (_tokens[i] == token)
                {
                    return i;
                }
            }
            return -1; // Not found
        }

        // IEnumerator implementation for iteration.
        public IEnumerator<string> GetEnumerator()
        {
            return _tokens.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    // 2. HASHED VOCABULARY (Dictionary-based)
    // ---------------------------------------------------------
    // Internal Mechanics: Uses Buckets and Hash Codes.
    // Lookup: O(1) average, O(N) worst case (collisions).
    // Insertion: O(1) average.
    // Memory: Higher overhead (stores buckets + linked lists).
    public class HashedTokenVocabulary : IEnumerable<KeyValuePair<string, int>>
    {
        // We use the built-in Dictionary for the backing store to ensure stability,
        // but we will explain the logic as if we wrote it from scratch.
        private Dictionary<string, int> _map;
        
        // In a raw implementation, we would need:
        // private LinkedList<string>[] _buckets; 
        // private int _count;

        public HashedTokenVocabulary()
        {
            // The .NET Dictionary handles resizing automatically.
            _map = new Dictionary<string, int>();
        }

        // Adds a token and assigns a unique ID.
        // Complexity: O(1) average.
        public void Add(string token)
        {
            if (!_map.ContainsKey(token))
            {
                _map[token] = _map.Count;
            }
        }

        // Finds the ID of a token.
        // Complexity: O(1) average.
        // Why? 
        // 1. Compute HashCode of 'token' (fast).
        // 2. Map HashCode to Bucket Index (modulo operation).
        // 3. Check only the items in that bucket (usually 1 item).
        public int GetId(string token)
        {
            if (_map.TryGetValue(token, out int id))
            {
                return id;
            }
            return -1;
        }

        public IEnumerator<KeyValuePair<string, int>> GetEnumerator()
        {
            return _map.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("--- Tokenizer Performance Test ---\n");

            // DATA SETUP
            // We will populate both collections with the same data.
            // Dataset size: 10,000 tokens.
            int datasetSize = 10000;
            string[] testData = new string[datasetSize];

            // Generate dummy tokens (e.g., "word0", "word1"...)
            // We use a standard loop here.
            for (int i = 0; i < datasetSize; i++)
            {
                testData[i] = "word" + i;
            }

            // 1. INITIALIZE COLLECTIONS
            LinearTokenVocabulary linearVocab = new LinearTokenVocabulary();
            HashedTokenVocabulary hashedVocab = new HashedTokenVocabulary();

            Console.WriteLine($"Populating collections with {datasetSize} items...");
            
            // Populate Linear
            foreach (string word in testData)
            {
                linearVocab.Add(word);
            }

            // Populate Hashed
            foreach (string word in testData)
            {
                hashedVocab.Add(word);
            }

            Console.WriteLine("Populated.\n");

            // 2. PERFORMANCE BENCHMARK
            // We will search for a token that exists at the very end of the list.
            // This represents the "Worst Case" for the Linear search.
            string targetToken = "word" + (datasetSize - 1);

            Console.WriteLine($"Searching for target token: '{targetToken}'");
            Console.WriteLine("--------------------------------------------------");

            // --- TEST A: LINEAR SEARCH ---
            Console.WriteLine("1. Testing LinearTokenVocabulary (List-based)...");
            
            long ticksStart = DateTime.Now.Ticks;
            int linearId = linearVocab.GetId(targetToken);
            long ticksEnd = DateTime.Now.Ticks;
            long linearDuration = ticksEnd - ticksStart;

            Console.WriteLine($"   Result ID: {linearId}");
            Console.WriteLine($"   Time taken (Ticks): {linearDuration}");
            Console.WriteLine($"   Complexity: O(N) - The CPU scanned {datasetSize} items.");
            Console.WriteLine();

            // --- TEST B: HASHED SEARCH ---
            Console.WriteLine("2. Testing HashedTokenVocabulary (Dictionary-based)...");

            ticksStart = DateTime.Now.Ticks;
            int hashedId = hashedVocab.GetId(targetToken);
            ticksEnd = DateTime.Now.Ticks;
            long hashedDuration = ticksEnd - ticksStart;

            Console.WriteLine($"   Result ID: {hashedId}");
            Console.WriteLine($"   Time taken (Ticks): {hashedDuration}");
            Console.WriteLine($"   Complexity: O(1) - The CPU jumped directly to the bucket.");
            Console.WriteLine();

            // 3. COMPARISON & EXPLANATION
            Console.WriteLine("--------------------------------------------------");
            Console.WriteLine("ANALYSIS:");
            
            if (linearDuration > hashedDuration)
            {
                Console.WriteLine($"The Hashed approach was significantly faster by a factor of {linearDuration / (hashedDuration + 1)}x.");
            }
            else
            {
                Console.WriteLine("Note: For very small datasets, List overhead might be lower, but Hashing scales better.");
            }

            Console.WriteLine("\nWhy the difference?");
            Console.WriteLine("1. List (Linear): Checks 'word0', 'word1', ... until 'word9999'.");
            Console.WriteLine("2. Dictionary (Hashed): Calculates hash of 'word9999', maps to a bucket, and retrieves the value immediately.");
            
            Console.WriteLine("\n--- Iteration Test (Yield Return) ---");
            Console.WriteLine("Iterating over Hashed Vocabulary (First 5 items):");

            // Using 'yield return' logic implicitly via foreach
            int count = 0;
            foreach (var pair in hashedVocab)
            {
                Console.WriteLine($"   Token: {pair.Key}, ID: {pair.Value}");
                count++;
                if (count >= 5) break;
            }
        }
    }
}
