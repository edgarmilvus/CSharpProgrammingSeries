
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

public void AnalyzeWord(ReadOnlySpan<char> word)
{
    // We can inspect the characters without allocating a new string.
    // This method accepts a slice of the original buffer, or the whole buffer.
    if (word.Length > 0 && char.IsUpper(word[0]))
    {
        Console.WriteLine($"Found capitalized word: {word}");
    }
}

public void ProcessDocument()
{
    string massiveDocument = "The quick brown Fox jumps over the lazy Dog.";
    
    // NO ALLOCATION HERE. 
    // We are creating a view, not a copy.
    ReadOnlySpan<char> docSpan = massiveDocument.AsSpan();

    // We can slice without allocation.
    // This creates a new Span pointing to a subset of the original memory.
    ReadOnlySpan<char> foxSlice = docSpan.Slice(10, 3); // Points to "Fox"
    
    AnalyzeWord(foxSlice);
}
