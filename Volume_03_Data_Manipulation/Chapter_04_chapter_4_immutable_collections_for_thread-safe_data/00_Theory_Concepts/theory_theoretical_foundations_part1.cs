
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

// Source File: theory_theoretical_foundations_part1.cs
// Description: Theoretical Foundations
// ==========================================

using System;
using System.Collections.Generic;
using System.Collections.Immutable;

public class VocabularyBuilderExample
{
    public static void BuildVocabulary()
    {
        // 1. Create a mutable builder. This is O(1) allocation.
        var builder = ImmutableDictionary.CreateBuilder<string, int>();

        // 2. Perform batch updates. This is efficient as it uses standard
        //    mutable resizing logic internally (similar to List<T>).
        builder.Add("the", 1);
        builder.Add("quick", 2);
        builder.Add("brown", 3);
        
        // Simulating a large batch load
        foreach (var token in new[] { "fox", "jumps", "over" })
        {
            builder[token] = builder.Count + 1;
        }

        // 3. Freeze the collection. This creates the immutable tree structure
        //    and discards the mutable builder.
        ImmutableDictionary<string, int> vocabulary = builder.ToImmutable();

        Console.WriteLine($"Vocabulary size: {vocabulary.Count}");
        // vocabulary.Add("new", 100); // Compile error: ImmutableDictionary has no 'Add' method.
    }
}
