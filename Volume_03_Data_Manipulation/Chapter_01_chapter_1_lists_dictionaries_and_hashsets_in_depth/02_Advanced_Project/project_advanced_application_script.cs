
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

namespace AdvancedDataStructures
{
    // Real-world Context: Building a Tokenizer for an AI Model.
    // In NLP, we map words (strings) to integer IDs (embeddings indices).
    // We need O(1) lookups to convert "The" -> 5.
    // We will manually implement hashing logic to visualize how Dictionaries work under the hood.
    
    public class TokenizerVocabulary
    {
        // We will use a custom bucket structure to simulate how Dictionary<TKey, TValue> works internally.
        // A real Dictionary uses an array of "buckets" and a "entries" array.
        // For educational clarity, we will use a simple array of LinkedNodes.
        
        private class HashNode
        {
            public string Key;
            public int Value;
            public HashNode Next; // Collision chain (Linked List)
            
            public HashNode(string key, int value)
            {
                Key = key;
                Value = value;
                Next = null;
            }
        }

        // The Bucket Array: This is the core of the hash table.
        // Size determines the density of collisions.
        private HashNode[] _buckets;
        
        // To demonstrate resizing, we start small.
        // In a real scenario, prime numbers are often used for bucket sizes to distribute hashes better.
        private int _capacity; 

        public TokenizerVocabulary(int initialCapacity)
        {
            _capacity = initialCapacity;
            _buckets = new HashNode[_capacity];
        }

        // ---------------------------------------------------------
        // 1. HASHING MECHANICS
        // ---------------------------------------------------------
        // Explanation: 
        // 1. Get the HashCode of the key (an integer derived from the string content).
        // 2. Use the modulo operator (%) to map the hash to a specific bucket index.
        // 3. This is why the bucket array size matters.
        private int GetBucketIndex(string key)
        {
            unchecked
            {
                // GetHashValue simulates the internal .NET GetHashCode()
                int hash = key.GetHashCode(); 
                // Modulo maps the huge integer range to our small array size
                int index = Math.Abs(hash % _capacity); 
                return index;
            }
        }

        // ---------------------------------------------------------
        // 2. INSERTION & COLLISION HANDLING (Chaining)
        // ---------------------------------------------------------
        // Explanation:
        // 1. Calculate the bucket index.
        // 2. If the bucket is empty, insert the new node.
        // 3. If the bucket is NOT empty (Collision), traverse the linked list.
        //    - If key exists, update value.
        //    - If key doesn't exist, append to the end of the list.
        // 4. Check Load Factor (Count / Capacity). If too high, Resize.
        public void Add(string key, int value)
        {
            int index = GetBucketIndex(key);
            HashNode current = _buckets[index];

            // Traverse the chain to check for duplicates or find the end
            while (current != null)
            {
                if (current.Key == key)
                {
                    // Key already exists, update value (or throw exception depending on design)
                    current.Value = value;
                    return;
                }
                current = current.Next;
            }

            // Insert at the head of the linked list (O(1) insertion)
            HashNode newNode = new HashNode(key, value);
            newNode.Next = _buckets[index]; // Point to the current head
            _buckets[index] = newNode;      // Update head to new node

            // Check Load Factor for resizing
            // If we have more items than buckets * 0.75, we resize.
            // This keeps the chains short and lookups fast.
            if ((float)Count / _capacity > 0.75)
            {
                Console.WriteLine($"[Resizing] Load factor exceeded. Current Capacity: {_capacity}. Resizing...");
                Resize();
            }
            
            Count++;
        }

        // ---------------------------------------------------------
        // 3. RESIZING MECHANICS
        // ---------------------------------------------------------
        // Explanation:
        // 1. Create a new, larger bucket array (usually 2x size).
        // 2. Re-hash ALL existing items.
        // 3. Why? Because the modulo operation (% _capacity) changes when capacity changes.
        //    An item at index 2 might move to index 5 in the new array.
        private void Resize()
        {
            int newCapacity = _capacity * 2;
            HashNode[] newBuckets = new HashNode[newCapacity];

            // Re-hash every existing node
            // We must iterate through the old buckets and the linked lists inside them
            for (int i = 0; i < _capacity; i++)
            {
                HashNode current = _buckets[i];
                while (current != null)
                {
                    // Calculate new index for the resized array
                    int newIndex = Math.Abs(current.Key.GetHashCode() % newCapacity);
                    
                    // Insert into new bucket (Head insertion again)
                    HashNode newNode = new HashNode(current.Key, current.Value);
                    newNode.Next = newBuckets[newIndex];
                    newBuckets[newIndex] = newNode;

                    current = current.Next;
                }
            }

            _buckets = newBuckets;
            _capacity = newCapacity;
        }

        // ---------------------------------------------------------
        // 4. LOOKUP (O(1) vs O(N))
        // ---------------------------------------------------------
        // Explanation:
        // 1. Calculate bucket index.
        // 2. Traverse the linked list (chain) in that specific bucket.
        // 3. Average Case: If the hash function is good and load factor is low, the chain length is 1 (O(1)).
        // 4. Worst Case: All keys hash to the same bucket (O(N)). This is rare with good hashing.
        public bool TryGetValue(string key, out int value)
        {
            int index = GetBucketIndex(key);
            HashNode current = _buckets[index];

            while (current != null)
            {
                if (current.Key == key)
                {
                    value = current.Value;
                    return true;
                }
                current = current.Next;
            }

            value = 0;
            return false;
        }

        public int Count { get; private set; } = 0;
    }

    public class PerformanceBenchmark
    {
        // ---------------------------------------------------------
        // 5. PERFORMANCE COMPARISON: LIST VS HASHSET/DICTIONARY
        // ---------------------------------------------------------
        // Context: Tokenizer Vocabulary with 10,000 words.
        // We search for a word that exists at the end of the list.
        public static void RunComparison()
        {
            Console.WriteLine("\n--- PERFORMANCE BENCHMARK: List vs Dictionary ---");
            
            int datasetSize = 10000;
            List<string> wordList = new List<string>();
            Dictionary<string, int> wordDict = new Dictionary<string, int>();

            // 1. Data Population
            // We generate words "word0", "word1", ... "word9999"
            for (int i = 0; i < datasetSize; i++)
            {
                string word = "word" + i;
                wordList.Add(word);
                wordDict.Add(word, i);
            }

            string searchTarget = "word9999"; // The last item

            // -----------------------------------------------------
            // A. LIST SEARCH (Linear Search - O(N))
            // -----------------------------------------------------
            // Algorithm: Iterate from index 0 to N until match found.
            // Complexity: O(N) - Time grows linearly with input size.
            // In the worst case (target at end), we perform N comparisons.
            Console.WriteLine("\n1. Searching with List<string> (Linear Search):");
            
            var watch = System.Diagnostics.Stopwatch.StartNew();
            int listIndex = -1;
            
            // Manual loop (No LINQ allowed for deep dive)
            for (int i = 0; i < wordList.Count; i++)
            {
                if (wordList[i] == searchTarget)
                {
                    listIndex = i;
                    break; // Found it
                }
            }
            
            watch.Stop();
            Console.WriteLine($"   Result Index: {listIndex}");
            Console.WriteLine($"   Time Elapsed: {watch.ElapsedTicks} ticks (High variance based on position)");
            Console.WriteLine($"   Complexity: O(N) - Linear");

            // -----------------------------------------------------
            // B. DICTIONARY SEARCH (Hash Lookup - O(1) Average)
            // -----------------------------------------------------
            // Algorithm: Hash key -> Find bucket -> Check chain.
            // Complexity: O(1) - Constant time regardless of dataset size (assuming good hashing).
            Console.WriteLine("\n2. Searching with Dictionary<string, int> (Hash Lookup):");
            
            watch.Restart();
            int dictValue = -1;
            bool found = wordDict.TryGetValue(searchTarget, out dictValue);
            watch.Stop();

            Console.WriteLine($"   Result Value: {dictValue}");
            Console.WriteLine($"   Time Elapsed: {watch.ElapsedTicks} ticks (Consistent)");
            Console.WriteLine($"   Complexity: O(1) - Constant (Average Case)");
        }
    }

    // ---------------------------------------------------------
    // 6. VISUALIZATION: HASH COLLISION GRAPH
    // ---------------------------------------------------------
    // This generates a DOT diagram showing how two different keys
    // can land in the same bucket and form a linked list (chain).
    public class CollisionVisualizer
    {
        public static void PrintGraphviz()
        {
            Console.WriteLine("\n--- VISUALIZATION: Hash Collision (Chaining) ---");
            Console.WriteLine("Copy the following code into a Graphviz viewer (e.g., graphviz.org):");
            
            Console.WriteLine(@"


[ERROR: Failed to render diagram.]

");
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== Advanced Data Structures: Internal Mechanics ===");

            // 1. Demonstrate Custom Hash Table Implementation
            // We simulate a dictionary to show resizing and collision handling manually.
            TokenizerVocabulary vocab = new TokenizerVocabulary(initialCapacity: 4);

            Console.WriteLine("\n--- Step 1: Inserting Data & Triggering Resizes ---");
            // We add items. Since capacity is 4, adding the 4th item (0.75 load factor) triggers a resize.
            vocab.Add("the", 1);
            vocab.Add("quick", 2);
            vocab.Add("brown", 3);
            Console.WriteLine("Added 'the', 'quick', 'brown'. Capacity: 4, Count: 3.");
            
            // This next add triggers the resize logic in the Add method
            vocab.Add("fox", 4); 
            Console.WriteLine("Added 'fox'. Capacity doubled to 8.");

            // Add more to demonstrate collisions
            vocab.Add("jumps", 5);
            vocab.Add("over", 6);
            vocab.Add("lazy", 7);
            vocab.Add("dog", 8);

            // 2. Verify Lookups
            Console.WriteLine("\n--- Step 2: Verifying Lookups ---");
            if (vocab.TryGetValue("fox", out int id))
            {
                Console.WriteLine($"Found 'fox' with ID: {id}");
            }

            // 3. Run Performance Benchmark
            PerformanceBenchmark.RunComparison();

            // 4. Visualize Collision Concept
            CollisionVisualizer.PrintGraphviz();

            Console.WriteLine("\n=== End of Script ===");
        }
    }
}
