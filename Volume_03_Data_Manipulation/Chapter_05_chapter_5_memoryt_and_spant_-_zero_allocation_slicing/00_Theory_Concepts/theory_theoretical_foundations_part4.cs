
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

// Source File: theory_theoretical_foundations_part4.cs
// Description: Theoretical Foundations
// ==========================================

using System;
using System.Collections.Generic;

public class TokenizerLookup
{
    // Simulated vocabulary: Array of token strings
    private string[] _vocabulary = new string[] { "hello", "world", "transformer", "attention" };
    
    // Lookup dictionary for O(1) access (Standard approach)
    private Dictionary<string, int> _vocabMap;

    public TokenizerLookup()
    {
        _vocabMap = new Dictionary<string, int>();
        for (int i = 0; i < _vocabulary.Length; i++)
        {
            _vocabMap[_vocabulary[i]] = i;
        }
    }

    // Zero-Allocation Lookup using Span<char>
    // We iterate manually to avoid LINQ (which allocates delegates/enumerators)
    public int? FindTokenId(Span<char> tokenSpan)
    {
        for (int i = 0; i < _vocabulary.Length; i++)
        {
            // Compare the Span<char> with the string
            // This creates a Span<char> from the string and compares memory
            if (tokenSpan.SequenceEqual(_vocabulary[i].AsSpan()))
            {
                return i;
            }
        }
        return null;
    }

    public void ProcessStream(ReadOnlyMemory<char> stream)
    {
        // We can slice the stream (Memory<T>) and pass it to synchronous processing
        ReadOnlySpan<char> view = stream.Span;
        
        // Example: Extracting a word "hello" from the stream without allocation
        // Assuming we know the start and length (e.g., from a previous scan)
        ReadOnlySpan<char> token = view.Slice(0, 5); // "hello"
        
        int? id = FindTokenId(token);
        Console.WriteLine($"Token ID: {id}");
    }
}
