
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

// Source File: solution_exercise_2.cs
// Description: Solution for Exercise 2
// ==========================================

using System;
using System.Numerics;

public static class SpanMath
{
    // Generic constraints allow usage of operators and ensure value types
    public static T DotProductSpan<T>(this ReadOnlySpan<T> left, ReadOnlySpan<T> right)
        where T : struct, IIncrementOperators<T>, IMultiplyOperators<T, T, T>, IAdditionOperators<T, T, T>
    {
        if (left.Length != right.Length)
            throw new ArgumentException("Spans must be of equal length.");

        // In .NET 8+, Vector<T> is hardware accelerated on supported platforms.
        // However, generic math with Vector<T> is complex. 
        // For this exercise, we implement a safe scalar fallback that utilizes 
        // the generic math operators defined in the constraints.
        
        T sum = default; // Initializes to 0 for numeric types

        // Note: In a production .NET 8+ environment, we would typically use 
        // Vector<T> if T is float or double specifically, or use 
        // System.Runtime.Intrinsics if targeting specific SIMD sets.
        // Since the constraint is generic, we iterate using the operators.
        
        for (int i = 0; i < left.Length; i++)
        {
            // Uses the IMultiplyOperators constraint
            T product = left[i] * right[i];
            // Uses the IAdditionOperators constraint
            sum += product;
        }

        return sum;
    }
}
