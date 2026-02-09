
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

# Source File: basic_basic_code_example.cs
# Description: Basic Code Example
# ==========================================

using System;
using System.Collections.Generic;

namespace DataStructuresBasics
{
    // A simplified custom HashTable to demonstrate internal mechanics.
    // In production, use System.Collections.Generic.Dictionary<TKey, TValue>.
    public class CustomHashTable<TKey, TValue>
    {
        // Internal bucket structure to handle collisions via chaining (linked lists).
        private class Bucket
        {
            public TKey Key;
            public TValue Value;
            public Bucket Next; // Pointer to the next item in this bucket (collision chain)

            public Bucket(TKey key, TValue value)
            {
                Key = key;
                Value = value;
                Next = null;
            }
        }

        private Bucket[] _buckets;
        private int _capacity;
        private int _count;

        public CustomHashTable(int initialCapacity = 16)
        {
            _capacity = initialCapacity;
            _buckets = new Bucket[_capacity];
            _count = 0;
        }

        // 1. Hashing: Converts a key into an array index.
        // Complexity: O(1) for calculation.
        private int GetBucketIndex(TKey key)
        {
            // Get hash code from the key.
            int hashCode = key.GetHashCode();
            // Ensure non-negative index using modulo operator.
            return Math.Abs(hashCode) % _capacity;
        }

        // 2. Insertion: Adds a key-value pair.
        // Complexity: Average O(1), Worst O(N) if resizing is needed or many collisions occur.
        public void Insert(TKey key, TValue value)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));

            int index = GetBucketIndex(key);
            Bucket current = _buckets[index];

            // Check for existing key (Update scenario)
            while (current != null)
            {
                if (current.Key.Equals(key))
                {
                    current.Value = value; // Update existing value
                    return;
                }
                current = current.Next;
            }

            // Insert new node at the head of the linked list (bucket)
            // This handles collisions: multiple keys hash to the same index.
            Bucket newBucket = new Bucket(key, value);
            newBucket.Next = _buckets[index]; // Point to the old head
            _buckets[index] = newBucket;      // Update head to new node
            _count++;

            // 3. Resizing: If load factor is high, resize array to maintain O(1) performance.
            // Load Factor = Count / Capacity. Threshold usually ~0.75.
            if (_count > _capacity * 0.75)
            {
                Resize();
            }
        }

        // 4. Lookup: Retrieves a value by key.
        // Complexity: Average O(1), Worst O(N) (if all keys collide into one bucket).
        public TValue Get(TKey key)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));

            int index = GetBucketIndex(key);
            Bucket current = _buckets[index];

            // Traverse the linked list in the bucket
            while (current != null)
            {
                if (current.Key.Equals(key))
                {
                    return current.Value;
                }
                current = current.Next;
            }

            throw new KeyNotFoundException($"Key '{key}' not found.");
        }

        // 5. Resizing Mechanism: Doubles capacity and rehashes all items.
        // This is the most expensive operation (O(N)), but amortized over many inserts, it remains O(1).
        private void Resize()
        {
            int newCapacity = _capacity * 2;
            Bucket[] newBuckets = new Bucket[newCapacity];

            // Rehash all existing items into the new array
            for (int i = 0; i < _capacity; i++)
            {
                Bucket current = _buckets[i];
                while (current != null)
                {
                    // Recalculate index based on new capacity
                    int newIndex = Math.Abs(current.Key.GetHashCode()) % newCapacity;
                    
                    // Insert into new bucket (chaining)
                    Bucket newNode = new Bucket(current.Key, current.Value);
                    newNode.Next = newBuckets[newIndex];
                    newBuckets[newIndex] = newNode;

                    current = current.Next;
                }
            }

            _buckets = newBuckets;
            _capacity = newCapacity;
            Console.WriteLine($"DEBUG: Resized to capacity {_capacity}");
        }

        // 6. Iteration: Using 'yield return' to expose items without exposing internal structure.
        public IEnumerable<KeyValuePair<TKey, TValue>> Iterate()
        {
            for (int i = 0; i < _capacity; i++)
            {
                Bucket current = _buckets[i];
                while (current != null)
                {
                    yield return new KeyValuePair<TKey, TValue>(current.Key, current.Value);
                    current = current.Next;
                }
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            // Real-world context: A simple Tokenizer Vocabulary used in NLP.
            // We map unique words (string) to their integer IDs (int).
            // Efficient lookups are critical here (O(1) vs O(N) for Lists).
            var vocab = new CustomHashTable<string, int>();

            Console.WriteLine("--- Inserting Tokens ---");
            vocab.Insert("the", 1);
            vocab.Insert("quick", 2);
            vocab.Insert("brown", 3);
            vocab.Insert("fox", 4);
            
            // Demonstrate Collision: Force a collision if possible (depends on hash codes).
            // In a real scenario, this happens naturally with large datasets.
            vocab.Insert("jumps", 5);
            vocab.Insert("over", 6);
            vocab.Insert("lazy", 7);
            vocab.Insert("dog", 8);

            Console.WriteLine("\n--- Retrieving Tokens ---");
            Console.WriteLine($"ID of 'fox': {vocab.Get("fox")}");
            Console.WriteLine($"ID of 'dog': {vocab.Get("dog")}");

            Console.WriteLine("\n--- Iterating via Yield Return ---");
            foreach (var pair in vocab.Iterate())
            {
                Console.WriteLine($"Token: {pair.Key,-10} ID: {pair.Value}");
            }

            // Performance Comparison Context
            Console.WriteLine("\n--- Complexity Comparison ---");
            Console.WriteLine("List.Contains: O(N) - Must scan every element.");
            Console.WriteLine("HashSet/Dictionary.Contains: O(1) - Direct index access via hashing.");
        }
    }
}
