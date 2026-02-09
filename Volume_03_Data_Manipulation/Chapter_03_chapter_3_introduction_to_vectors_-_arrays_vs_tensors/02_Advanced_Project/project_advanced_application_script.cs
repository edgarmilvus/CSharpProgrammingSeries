
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
using System.Collections;
using System.Collections.Generic;

namespace VectorFundamentals
{
    // PROBLEM CONTEXT:
    // We are building the core data structure for a "Tokenizer" used in an AI language model.
    // In AI, we map words (strings) to unique integer IDs (embeddings) to process text numerically.
    // We need a vocabulary lookup that supports:
    // 1. Extremely fast lookups (O(1) average time) to convert "apple" -> ID 5.
    // 2. Collision handling (what if "apple" and "apply" hash to the same bucket?).
    // 3. Dynamic resizing (the vocabulary grows as we train).
    //
    // We will implement a simplified `Dictionary<string, int>` from scratch to understand the internal mechanics.
    // We will then compare its performance against a `List` to demonstrate Big O complexity.

    public class TokenizerVocabulary
    {
        // INTERNAL MECHANICS: Hashing & Buckets
        // We use an array of "Buckets". Each bucket is a Linked List of entries.
        // This is "Chaining" - a standard way to handle hash collisions.
        // If two strings hash to the same index, they form a chain in that bucket.
        private class Entry
        {
            public int HashCode; // Cached hash of the key
            public string Key;   // The word (e.g., "vector")
            public int Value;    // The ID (e.g., 42)
            public Entry Next;   // Pointer to the next entry in the bucket (for collisions)
        }

        // We start with a small array size. In real systems, this is a prime number to reduce collisions.
        private Entry[] _buckets;
        private int _count = 0;

        public TokenizerVocabulary(int initialCapacity = 16)
        {
            _buckets = new Entry[initialCapacity];
        }

        // ALGORITHM: Hashing and Insertion
        // Time Complexity: O(1) average, O(n) worst case (if all keys collide).
        public void Add(string key, int value)
        {
            // 1. Calculate Hash Code
            // We get a numeric representation of the string.
            int hashCode = key.GetHashCode();
            
            // 2. Determine Bucket Index
            // We use bitwise AND (&) with (_buckets.Length - 1) to map the hash to an index.
            // This only works if _buckets.Length is a power of 2 (or we use modulo %).
            int bucketIndex = hashCode & (_buckets.Length - 1);

            // 3. Check for Existing Key (Collision Handling)
            Entry current = _buckets[bucketIndex];
            while (current != null)
            {
                // Optimization: Check hashcode first (fast integer comparison)
                if (current.HashCode == hashCode && current.Key == key)
                {
                    throw new Exception($"Key '{key}' already exists in vocabulary.");
                }
                current = current.Next;
            }

            // 4. Create New Entry and Insert at Head of Chain
            // We insert at the head of the linked list for O(1) insertion time.
            Entry newEntry = new Entry
            {
                HashCode = hashCode,
                Key = key,
                Value = value,
                Next = _buckets[bucketIndex] // Point to previous head
            };
            _buckets[bucketIndex] = newEntry; // Update bucket head
            _count++;

            // 5. Resize Check (Dynamic Resizing)
            // If the load factor (count / capacity) is too high, we resize to maintain O(1) performance.
            // Load factor > 0.75 is a common threshold.
            if (_count > _buckets.Length * 0.75)
            {
                Resize();
            }
        }

        // ALGORITHM: Lookup
        // Time Complexity: O(1) average.
        public int GetId(string key)
        {
            int hashCode = key.GetHashCode();
            int bucketIndex = hashCode & (_buckets.Length - 1);

            Entry current = _buckets[bucketIndex];
            while (current != null)
            {
                if (current.HashCode == hashCode && current.Key == key)
                {
                    return current.Value; // Found it!
                }
                current = current.Next; // Traverse the chain
            }

            throw new KeyNotFoundException($"Key '{key}' not found.");
        }

        // ALGORITHM: Resizing (Rehashing)
        // This is an expensive operation (O(n)), but happens rarely (amortized O(1)).
        private void Resize()
        {
            int newCapacity = _buckets.Length * 2;
            Entry[] newBuckets = new Entry[newCapacity];

            // Iterate over all existing entries
            for (int i = 0; i < _buckets.Length; i++)
            {
                Entry current = _buckets[i];
                while (current != null)
                {
                    // REHASHING: We must recalculate the bucket index because the array size changed.
                    int newBucketIndex = current.HashCode & (newCapacity - 1);

                    // Insert into new bucket (Head insertion again)
                    Entry nextInChain = current.Next; // Save next before overwriting
                    current.Next = newBuckets[newBucketIndex];
                    newBuckets[newBucketIndex] = current;

                    current = nextInChain;
                }
            }

            _buckets = newBuckets;
        }
    }

    // VISUALIZATION: The Data Structure Layout
    // Imagine _buckets is an array of size 8.
    // Indices: [0] -> null
    //          [1] -> Entry("apple", 5) -> Entry("ape", 6)  (Collision at index 1)
    //          [2] -> null
    //          [3] -> Entry("banana", 10)
    //          ...
    //
    // 

::: {style="text-align: center"}
![In this diagram, a C# program interacts with an AI model by sending a user prompt as input, which the model processes to generate a relevant textual response.](images/b3_c3_s3_diag1.png){width=80% caption="In this diagram, a C# program interacts with an AI model by sending a user prompt as input, which the model processes to generate a relevant textual response."}
:::



    public class Program
    {
        // COMPARISON: List vs HashSet/Dictionary
        // We will demonstrate why we use Hash-based structures for AI vocabularies.
        static void Main(string[] args)
        {
            Console.WriteLine("--- PART 1: Custom Dictionary Implementation ---");
            
            // Initialize our custom tokenizer vocabulary
            TokenizerVocabulary vocab = new TokenizerVocabulary(initialCapacity: 4);

            // Adding words (Simulating vocabulary building)
            // Note: We are manually handling the logic. No LINQ used.
            vocab.Add("tensor", 1);
            vocab.Add("vector", 2);
            vocab.Add("matrix", 3);
            vocab.Add("embedding", 4); // This will trigger a resize internally

            // Lookup
            int id = vocab.GetId("vector");
            Console.WriteLine($"Lookup 'vector': ID {id}");

            Console.WriteLine("\n--- PART 2: Performance Comparison (Big O) ---");

            // Setup: Large dataset
            int datasetSize = 10000;
            List<string> listVocab = new List<string>();
            HashSet<string> hashVocab = new HashSet<string>();

            // Populate data (Simulating 10k words)
            for (int i = 0; i < datasetSize; i++)
            {
                string word = $"word_{i}";
                listVocab.Add(word);
                hashVocab.Add(word);
            }

            string targetWord = $"word_{9999}"; // The last word

            // ---------------------------------------------------------
            // SCENARIO A: List.Contains (Linear Search - O(n))
            // ---------------------------------------------------------
            // How it works: The computer checks index 0, then 1, then 2... until it finds a match.
            // In the worst case (last item), it checks all 10,000 items.
            bool foundInList = false;
            
            // Manual loop (No LINQ .Contains)
            for (int i = 0; i < listVocab.Count; i++)
            {
                if (listVocab[i] == targetWord)
                {
                    foundInList = true;
                    break; // Found it, stop searching
                }
            }
            
            Console.WriteLine($"List Lookup Success: {foundInList}");
            Console.WriteLine("Complexity: O(n) - Time increases linearly with vocabulary size.");
            Console.WriteLine("Impact: In AI with millions of tokens, this is too slow for real-time inference.");

            // ---------------------------------------------------------
            // SCENARIO B: HashSet.Contains (Hash Lookup - O(1) average)
            // ---------------------------------------------------------
            // How it works: 
            // 1. Calculate HashCode of targetWord.
            // 2. Map directly to a bucket index (e.g., index 5).
            // 3. Check only the items in bucket 5.
            // It doesn't matter if we have 10k or 10 million items; the steps remain constant.
            bool foundInHash = hashVocab.Contains(targetWord);

            Console.WriteLine($"\nHashSet Lookup Success: {foundInHash}");
            Console.WriteLine("Complexity: O(1) - Constant time lookup.");
            Console.WriteLine("Impact: Essential for Tokenizers processing text at high speeds.");

            // ---------------------------------------------------------
            // SCENARIO C: Queue (FIFO) for Graph Traversal (AI Context)
            // ---------------------------------------------------------
            // In AI, we often traverse knowledge graphs. A Queue ensures we process nodes layer-by-layer.
            Console.WriteLine("\n--- PART 3: Queue Mechanics (Breadth-First Search) ---");
            
            Queue<string> traversalQueue = new Queue<string>();
            traversalQueue.Enqueue("Root_Node");
            traversalQueue.Enqueue("Child_A");
            traversalQueue.Enqueue("Child_B");

            Console.WriteLine("Traversing Graph using Queue (FIFO):");
            
            // Manual Dequeue simulation
            while (traversalQueue.Count > 0)
            {
                // In a real graph, we would fetch neighbors here
                string node = traversalQueue.Dequeue();
                Console.WriteLine($"Visiting: {node}");
                
                // Simulate finding neighbors
                if (node == "Root_Node")
                {
                    traversalQueue.Enqueue("Grandchild_1");
                    traversalQueue.Enqueue("Grandchild_2");
                }
            }
        }
    }
}
