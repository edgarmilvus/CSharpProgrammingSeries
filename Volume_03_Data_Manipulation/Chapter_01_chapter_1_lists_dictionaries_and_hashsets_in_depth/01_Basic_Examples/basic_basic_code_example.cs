
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
using System.Diagnostics;

namespace Book3_Chapter1_BasicExample
{
    class Program
    {
        static void Main(string[] args)
        {
            // CONTEXT: In Natural Language Processing (NLP), a Tokenizer breaks text into words (tokens).
            // Each token is mapped to a unique integer ID. We need to check if a token exists in our vocabulary.
            // We will compare three data structures for this lookup task.

            int vocabularySize = 100000; // 100k tokens
            List<string> tokenVocabulary = GenerateVocabulary(vocabularySize);
            List<string> testTokens = new List<string> { "token_50000", "token_99999", "unknown_token" };

            Console.WriteLine($"Vocabulary Size: {vocabularySize} tokens");
            Console.WriteLine("--------------------------------------------------");

            // 1. Using List<T> (Linear Search)
            // Complexity: O(N) - Time increases linearly with vocabulary size.
            // Memory: Low overhead, stores only data.
            Console.WriteLine("1. Testing List<string> (Linear Search):");
            SearchWithList(tokenVocabulary, testTokens);

            Console.WriteLine("--------------------------------------------------");

            // 2. Using HashSet<T> (Hash-based Lookup)
            // Complexity: O(1) - Constant time on average.
            // Memory: Higher overhead due to internal buckets and hash storage.
            Console.WriteLine("2. Testing HashSet<string> (Hash-based Lookup):");
            SearchWithHashSet(tokenVocabulary, testTokens);

            Console.WriteLine("--------------------------------------------------");

            // 3. Using Dictionary<TKey, TValue> (Key-Value Lookup)
            // Complexity: O(1) - Constant time on average.
            // Used when we need to retrieve an ID (Value) given a Token (Key).
            Console.WriteLine("3. Testing Dictionary<string, int> (Key-Value Lookup):");
            SearchWithDictionary(tokenVocabulary, testTokens);
        }

        // Generates a list of tokens: "token_0", "token_1", ...
        static List<string> GenerateVocabulary(int size)
        {
            List<string> vocab = new List<string>(size);
            for (int i = 0; i < size; i++)
            {
                vocab.Add($"token_{i}");
            }
            return vocab;
        }

        // --- LIST IMPLEMENTATION ---
        static void SearchWithList(List<string> vocab, List<string> queries)
        {
            // ALGORITHM: Linear Search
            // We iterate through the list from start to finish until we find a match.
            // If the item is at the end or not present, we scan the entire list.
            foreach (string query in queries)
            {
                bool found = false;
                // Manual iteration (No LINQ shortcuts allowed for deep dive)
                for (int i = 0; i < vocab.Count; i++)
                {
                    if (vocab[i] == query)
                    {
                        found = true;
                        break; // Found it, stop searching
                    }
                }

                Console.WriteLine($"  Query '{query}': {(found ? "Found" : "Not Found")}");
            }

            // PERFORMANCE NOTE:
            // If we searched for "token_99999", the loop runs 100,000 times.
            // If we searched for "token_0", the loop runs 1 time.
            // This inconsistency is why Lists are slow for lookups.
        }

        // --- HASHSET IMPLEMENTATION ---
        static void SearchWithHashSet(List<string> vocab, List<string> queries)
        {
            // OPTIMIZATION: Convert List to HashSet once.
            // This operation takes O(N) time, but subsequent lookups are O(1).
            HashSet<string> hashSet = new HashSet<string>(vocab);

            foreach (string query in queries)
            {
                // ALGORITHM: Hash Lookup
                // 1. Calculate HashCode of the string.
                // 2. Map HashCode to a specific "Bucket" index.
                // 3. Check only the items in that bucket.
                bool found = hashSet.Contains(query);

                Console.WriteLine($"  Query '{query}': {(found ? "Found" : "Not Found")}");
            }

            // PERFORMANCE NOTE:
            // Whether we search for "token_99999" or "token_0", the time taken is roughly the same.
            // This is O(1) average case.
        }

        // --- DICTIONARY IMPLEMENTATION ---
        static void SearchWithDictionary(List<string> vocab, List<string> queries)
        {
            // REAL-WORLD CONTEXT: We usually need the ID of the token, not just a boolean.
            // Dictionary maps Key (Token) -> Value (ID).
            Dictionary<string, int> dictionary = new Dictionary<string, int>();

            // Building the dictionary: O(N)
            for (int i = 0; i < vocab.Count; i++)
            {
                dictionary.Add(vocab[i], i);
            }

            foreach (string query in queries)
            {
                // ALGORITHM: Hash Lookup with Value Retrieval
                if (dictionary.TryGetValue(query, out int tokenId))
                {
                    Console.WriteLine($"  Query '{query}': Found ID {tokenId}");
                }
                else
                {
                    Console.WriteLine($"  Query '{query}': Not Found");
                }
            }
        }
    }
}
