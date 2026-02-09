
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

# Source File: solution_exercise_5.cs
# Description: Solution for Exercise 5
# ==========================================

using System;
using System.Numerics;

public class AdvancedNormalizer
{
    /// <summary>
    /// Refactored vectorized normalizer.
    /// Separates the sequential accumulator calculation from the parallelizable clamping.
    /// </summary>
    public static void VectorizedNormalize(Span<float> tokens, float decayFactor, float clampValue)
    {
        int vectorWidth = Vector<float>.Count;
        int i = 0;

        // --- PASS 1: Scalar Accumulator Calculation ---
        // The accumulator recurrence (A_i = f(A_{i-1}, x_i)) is inherently sequential 
        // and cannot be vectorized without changing the algorithm (e.g., prefix sums, which 
        // are complex for this specific non-linear recurrence).
        // We calculate the accumulator values into a temporary array.
        
        float[] accumulators = new float[tokens.Length];
        float accumulator = 0.0f;
        
        for (i = 0; i < tokens.Length; i++)
        {
            float val = tokens[i];
            val = (val * decayFactor) + (accumulator * 0.1f);
            accumulator = val;
            accumulators[i] = val;
        }

        // --- PASS 2: Vectorized Clamping & Scaling ---
        // Now that we have the intermediate accumulator values, we can process them 
        // in parallel. The dependency is broken.
        
        i = 0;
        var clampVec = new Vector<float>(clampValue);
        var negClampVec = new Vector<float>(-clampValue);

        // Main SIMD loop
        for (; i <= tokens.Length - vectorWidth; i += vectorWidth)
        {
            // Load the pre-calculated accumulator values
            var valVec = new Vector<float>(accumulators.AsSpan(i, vectorWidth));

            // Apply Clamping: val = min(val, clampValue) and val = max(val, -clampValue)
            // Note: Vector.Clamp exists in .NET 6+.
            // If not, we do: val = Min(val, clamp); val = Max(val, -clamp);
            
            // Using Vector.Clamp (available in .NET 6+)
            valVec = Vector.Clamp(valVec, negClampVec, clampVec);

            // Store back to original tokens span
            valVec.CopyTo(tokens.Slice(i, vectorWidth));
        }

        // Tail elements for clamping
        for (; i < tokens.Length; i++)
        {
            float val = accumulators[i];
            if (val > clampValue) val = clampValue;
            if (val < -clampValue) val = -clampValue;
            tokens[i] = val;
        }
    }
}
