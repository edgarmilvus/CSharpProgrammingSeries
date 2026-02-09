
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

# Source File: solution_exercise_1.cs
# Description: Solution for Exercise 1
# ==========================================

using System;

public class TokenNormalizer
{
    /// <summary>
    /// Normalizes a span of floats in-place using Z-score normalization.
    /// </summary>
    /// <param name="tokens">The span to modify. Must be writable.</param>
    public static void NormalizeTokens(Span<float> tokens)
    {
        if (tokens.IsEmpty)
            return;

        // 1. Calculate Mean
        // We iterate manually to avoid LINQ allocations.
        float sum = 0;
        foreach (float val in tokens)
        {
            sum += val;
        }
        float mean = sum / tokens.Length;

        // 2. Calculate Standard Deviation
        // We use a two-pass approach for better numerical stability.
        // While two passes seem slower, it prevents accumulation errors in large datasets.
        float sumOfSquaredDiffs = 0;
        foreach (float val in tokens)
        {
            float diff = val - mean;
            sumOfSquaredDiffs += diff * diff;
        }

        // Handle edge case where variance is zero to avoid division by zero
        float stdDev = MathF.Sqrt(sumOfSquaredDiffs / tokens.Length);
        
        // 3. Normalize In-Place
        const float epsilon = 1e-8f; // Small constant to prevent division by zero
        
        for (int i = 0; i < tokens.Length; i++)
        {
            tokens[i] = (tokens[i] - mean) / (stdDev + epsilon);
        }
    }

    public static void Main()
    {
        // Simulating a buffer of token embeddings (e.g., from a BERT tokenizer)
        // stackalloc allocates on the stack, avoiding heap pressure entirely.
        Span<float> tokenBuffer = stackalloc float[] { 10.5f, 20.2f, 15.8f, 5.1f, 30.5f };

        Console.WriteLine("Before Normalization:");
        PrintSpan(tokenBuffer);

        NormalizeTokens(tokenBuffer);

        Console.WriteLine("\nAfter Normalization:");
        PrintSpan(tokenBuffer);
    }

    private static void PrintSpan(Span<float> span)
    {
        foreach (var val in span)
        {
            Console.Write($"{val:F4} ");
        }
        Console.WriteLine();
    }
}
