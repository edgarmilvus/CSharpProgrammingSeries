
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

# Source File: theory_theoretical_foundations_part1.cs
# Description: Theoretical Foundations
# ==========================================

using System;
using System.Collections.Generic;

public class TokenizerVocabulary
{
    // A dictionary mapping tokens (strings) to their integer IDs
    private Dictionary<string, int> _vocab;

    public TokenizerVocabulary()
    {
        // Initialize with a reasonable capacity to minimize resizing
        _vocab = new Dictionary<string, int>(capacity: 10000);
    }

    public void AddToken(string token, int id)
    {
        // O(1) average, O(n) worst case (if resizing or many collisions)
        _vocab[token] = id;
    }

    public int GetTokenId(string token)
    {
        // O(1) average lookup
        if (_vocab.TryGetValue(token, out int id))
        {
            return id;
        }
        // Return unknown token ID (standard in NLP)
        return -1; 
    }
}
