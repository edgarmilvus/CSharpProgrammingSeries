
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

# Source File: solution_exercise_3.cs
# Description: Solution for Exercise 3
# ==========================================

using System.Collections.Generic;

namespace Book3_Chapter5_PracticalExercises
{
    public class Exercise3_Solution
    {
        // Naive approach using List
        public class NaiveTokenizer
        {
            private List<string> _vocabulary;

            public NaiveTokenizer()
            {
                _vocabulary = new List<string>();
                // Simulate vocabulary
                for (int i = 0; i < 10000; i++) _vocabulary.Add($"token_{i}");
            }

            // Complexity: O(N) - Linear scan
            // As vocabulary size (N) grows, lookup time grows linearly.
            public int GetTokenId(string token)
            {
                for (int i = 0; i < _vocabulary.Count; i++)
                {
                    if (_vocabulary[i] == token) return i;
                }
                return -1;
            }
        }

        // Optimized approach using Dictionary
        public class OptimizedTokenizer
        {
            private Dictionary<string, int> _vocabularyMap;

            public OptimizedTokenizer()
            {
                _vocabularyMap = new Dictionary<string, int>();
                // Simulate vocabulary
                for (int i = 0; i < 10000; i++) _vocabularyMap.Add($"token_{i}", i);
            }

            // Complexity: O(1) - Hash lookup
            // Lookup time remains constant regardless of vocabulary size (assuming good hashing).
            public int GetTokenId(string token)
            {
                if (_vocabularyMap.TryGetValue(token, out int id))
                {
                    return id;
                }
                return -1;
            }
        }
    }
}
