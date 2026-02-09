
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

namespace DataStructuresDeepDive
{
    class Program
    {
        static void Main(string[] args)
        {
            // ---------------------------------------------------------
            // SCENARIO: Building a vocabulary for a simple tokenizer.
            // ---------------------------------------------------------

            // 1. The "Slow" Way: Using a List (Linear Search)
            // ---------------------------------------------------------
            List<string> vocabularyList = new List<string>();
            
            // Simulating adding words to our vocabulary
            vocabularyList.Add("hello");
            vocabularyList.Add("world");
            vocabularyList.Add("data");
            vocabularyList.Add("structures");
            
            // We need to find the index (ID) of a specific word.
            string wordToFind = "structures";
            
            // MANUAL SEARCH ALGORITHM (No LINQ shortcuts allowed!)
            // We iterate through the list until we find a match.
            int foundIndex = -1;
            for (int i = 0; i < vocabularyList.Count; i++)
            {
                if (vocabularyList[i] == wordToFind)
                {
                    foundIndex = i;
                    break; // Found it, stop searching.
                }
            }

            Console.WriteLine($"[List] Word '{wordToFind}' found at index: {foundIndex}");
            // Complexity: O(N) - In the worst case, we check every single item.


            // 2. The "Fast" Way: Using a HashSet (Hashing)
            // ---------------------------------------------------------
            // In a real tokenizer, we map string -> int, but for this 
            // "Hello World" example, we will use HashSet<string> to focus 
            // purely on the lookup mechanism without value overhead.
            HashSet<string> vocabularySet = new HashSet<string>();
            
            // Adding words. Internally, this calculates a hash code 
            // and places the string in a specific "bucket".
            vocabularySet.Add("hello");
            vocabularySet.Add("world");
            vocabularySet.Add("data");
            vocabularySet.Add("structures");

            // The Lookup Operation
            // The HashSet doesn't scan. It computes the hash of "structures"
            // and jumps directly to the bucket where it should be.
            bool exists = vocabularySet.Contains(wordToFind);

            Console.WriteLine($"[HashSet] Does '{wordToFind}' exist? {exists}");
            // Complexity: O(1) - Average case. Constant time regardless of list size.


            // 3. The Dictionary Way: Key-Value Mapping (Tokenizers use this)
            // ---------------------------------------------------------
            Dictionary<string, int> tokenToIdMap = new Dictionary<string, int>();
            
            // Populate with IDs
            tokenToIdMap["hello"] = 0;
            tokenToIdMap["world"] = 1;
            tokenToIdMap["data"] = 2;
            tokenToIdMap["structures"] = 3;

            // Retrieve the ID
            // Again, this uses the hash of the key to find the bucket instantly.
            if (tokenToIdMap.TryGetValue("structures", out int tokenId))
            {
                Console.WriteLine($"[Dictionary] Token 'structures' maps to ID: {tokenId}");
            }
        }
    }
}
