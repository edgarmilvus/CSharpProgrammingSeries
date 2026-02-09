
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

namespace Book3_Chapter5_AdvancedStructures
{
    // REAL-WORLD CONTEXT:
    // We are building a lightweight Tokenizer for a Natural Language Processing (NLP) task.
    // We need to map unique words (strings) to integer IDs (vocabulary indices) efficiently.
    // We will compare the performance of a naive List-based approach versus a Dictionary/HashSet approach
    // by simulating the internal mechanics of hashing and bucketing.

    public class TokenizerSimulation
    {
        public static void Main()
        {
            Console.WriteLine("=== Advanced Application: Internal Mechanics of Lookups ===");
            Console.WriteLine("Context: Building a Vocabulary for NLP Tokenization.");
            Console.WriteLine();

            // 1. The Dataset
            // A small corpus of text to process.
            string[] corpus = {
                "the", "quick", "brown", "fox", "jumps", "over", "the", "lazy", "dog",
                "the", "quick", "brown", "fox", "jumps", "over", "the", "lazy", "dog"
            };

            // 2. Approach A: Naive List-Based Lookup (O(N))
            // We use a List<string> to store the vocabulary.
            // We use a List<int> to store the corresponding IDs.
            // This simulates how a beginner might implement this without knowing Hashing.
            Console.WriteLine("--- Approach A: List-Based Lookup (Linear Search) ---");
            List<string> listVocab = new List<string>();
            List<int> listIds = new List<int>();
            
            int nextId = 0;
            foreach (string word in corpus)
            {
                // To check if a word exists, we must scan the entire list.
                int foundIndex = -1;
                for (int i = 0; i < listVocab.Count; i++)
                {
                    if (listVocab[i] == word)
                    {
                        foundIndex = i;
                        break;
                    }
                }

                if (foundIndex != -1)
                {
                    // Word found: O(N) complexity per lookup.
                    // Console.WriteLine($"Found '{word}' at index {foundIndex}");
                }
                else
                {
                    // Word not found: Add it.
                    listVocab.Add(word);
                    listIds.Add(nextId++);
                    // Console.WriteLine($"Added '{word}' with ID {nextId - 1}");
                }
            }
            
            Console.WriteLine($"Vocabulary Size: {listVocab.Count}");
            Console.WriteLine("Algorithmic Complexity: O(N) for lookups.");
            Console.WriteLine("Impact: As vocabulary grows (e.g., 100,000 words), lookup time increases linearly.");
            Console.WriteLine();

            // 3. Approach B: Dictionary<K,V> and HashSet<T> (O(1) Average)
            // We simulate the internal mechanics of a Hash Table to understand WHY it's faster.
            // In C#, Dictionary is an implementation of a Hash Table.
            Console.WriteLine("--- Approach B: Dictionary-Based Lookup (Hashing & Buckets) ---");
            
            // We will manually implement a simplified "Bucket" structure to visualize the concept.
            // A real Dictionary uses an array of "Buckets". Each bucket holds a Linked List of entries.
            // This handles "Collisions" (when two different strings hash to the same bucket).
            
            // Let's define a fixed number of buckets for simulation (Real Dictionaries resize dynamically).
            int bucketCount = 10; 
            List<(string word, int id)>[] buckets = new List<(string, int)>[bucketCount];

            // Reset ID counter
            nextId = 0;

            foreach (string word in corpus)
            {
                // STEP 1: HASHING
                // Convert the string (variable length) into a fixed-size integer (hash code).
                int hashCode = word.GetHashCode();
                
                // STEP 2: BUCKET INDEXING
                // Map the hash code to a specific bucket index (0 to 9).
                // We use the modulo operator (%).
                int bucketIndex = Math.Abs(hashCode) % bucketCount;

                // Initialize bucket if null
                if (buckets[bucketIndex] == null)
                {
                    buckets[bucketIndex] = new List<(string, int)>();
                }

                // STEP 3: COLLISION RESOLUTION (Linear Probing / Chaining simulation)
                // Check if the word already exists in this specific bucket.
                bool foundInBucket = false;
                foreach (var entry in buckets[bucketIndex])
                {
                    if (entry.word == word)
                    {
                        foundInBucket = true;
                        // Console.WriteLine($"Found '{word}' in bucket {bucketIndex} (Hash collision handled).");
                        break;
                    }
                }

                if (!foundInBucket)
                {
                    // Add to bucket
                    buckets[bucketIndex].Add((word, nextId));
                    // Console.WriteLine($"Added '{word}' (ID: {nextId}) to bucket {bucketIndex}.");
                    nextId++;
                }
            }

            Console.WriteLine($"Simulated Buckets Used: {bucketCount}");
            Console.WriteLine("Algorithmic Complexity: O(1) average for lookups.");
            Console.WriteLine("Impact: Lookup time remains constant regardless of vocabulary size.");
            Console.WriteLine();

            // 4. Real-World C# Implementation (Using actual Dictionary and HashSet)
            // Now we use the optimized built-in types.
            Console.WriteLine("--- Approach C: Optimized C# Implementation ---");
            
            // Dictionary<TKey, TValue>: Maps unique keys to values.
            Dictionary<string, int> vocabDict = new Dictionary<string, int>();
            
            // HashSet<T>: Stores unique elements only. Useful for checking existence without storing extra data.
            HashSet<string> uniqueWords = new HashSet<string>();

            foreach (string word in corpus)
            {
                // Dictionary Check: O(1) average
                if (!vocabDict.ContainsKey(word))
                {
                    vocabDict.Add(word, vocabDict.Count);
                }

                // HashSet Check: O(1) average
                // This is highly optimized for "Does this exist?" queries.
                uniqueWords.Add(word); // Add returns bool, but we ignore it here.
            }

            Console.WriteLine($"Dictionary Count: {vocabDict.Count}");
            Console.WriteLine($"HashSet Count: {uniqueWords.Count}");
            
            // Demonstrate Iterators (yield return) for querying the data without LINQ
            Console.WriteLine("\nIterating via Custom Iterator (yield return):");
            foreach (var pair in GetHighFrequencyWords(vocabDict, 0)) // 0 is threshold
            {
                Console.WriteLine($"  Word: {pair.Key}, ID: {pair.Value}");
            }
        }

        // ALLOWED: Iterators (yield return)
        // Demonstrates how to iterate collections manually without LINQ.
        // This is useful for lazy evaluation in data pipelines.
        public static IEnumerable<KeyValuePair<string, int>> GetHighFrequencyWords(
            Dictionary<string, int> vocab, int threshold)
        {
            // We use the Dictionary's internal enumerator (which is efficient).
            // In a real scenario, we might calculate frequency first.
            foreach (var pair in vocab)
            {
                // Simulating a filter condition
                if (pair.Value >= threshold)
                {
                    // yield return pauses execution, returns the value, 
                    // and resumes when the next item is requested.
                    yield return pair;
                }
            }
        }
    }

    // VISUALIZATION: Internal Structure of a Dictionary Bucket
    /*
    

[ERROR: Failed to render diagram.]


    */
}
