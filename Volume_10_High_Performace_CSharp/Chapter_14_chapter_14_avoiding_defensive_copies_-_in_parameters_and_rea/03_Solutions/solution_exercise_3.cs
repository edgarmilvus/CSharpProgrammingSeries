
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

using System;

namespace Exercise3
{
    public struct TokenInfo
    {
        public int TokenID;
        public int Position;
        public int Score; // Mutable field
    }

    public static class TokenNormalizer
    {
        // Requirement 2 & 3: Iterate over Span efficiently
        public static void NormalizeScores(Span<TokenInfo> tokens, float decayFactor)
        {
            // Using foreach on Span<T> is allocation-free in modern C#
            for (int i = 0; i < tokens.Length; i++)
            {
                // Requirement 4: Pass by ref to allow mutation
                ApplyDecay(ref tokens[i], decayFactor);
            }
        }

        // Requirement 4: Helper method using ref
        public static void ApplyDecay(ref TokenInfo token, float factor)
        {
            // Modifying the struct in place via the reference
            token.Score = (int)(token.Score * factor);
        }

        // Requirement 5: ReadOnly version
        public static void LogScores(ReadOnlySpan<TokenInfo> tokens)
        {
            foreach (var token in tokens)
            {
                // Cannot call ApplyDecay(ref token, ...) here because token is a copy
                // and modifying it would be useless. 
                // Also, ApplyDecay expects 'ref', but ReadOnlySpan gives 'in' semantics (read-only).
                Console.WriteLine(token.Score);
            }
        }
    }

    class Program
    {
        static void Main()
        {
            // Stackalloc or stack-based array for demonstration
            Span<TokenInfo> tokenStream = stackalloc TokenInfo[10];
            for (int i = 0; i < tokenStream.Length; i++)
            {
                tokenStream[i] = new TokenInfo { TokenID = i, Position = i, Score = 100 };
            }

            TokenNormalizer.NormalizeScores(tokenStream, 0.9f);

            // Requirement 5: Implications of ReadOnlySpan
            ReadOnlySpan<TokenInfo> readOnlyStream = tokenStream;
            TokenNormalizer.LogScores(readOnlyStream);
        }
    }
}
