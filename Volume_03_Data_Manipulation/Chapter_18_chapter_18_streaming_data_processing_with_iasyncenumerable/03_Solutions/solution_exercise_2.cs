
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
using System.Runtime.CompilerServices;

public static class VectorMath
{
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static void NormalizeL2(Span<float> vector)
    {
        // 1. Calculate Sum of Squares (Magnitude squared) using SIMD
        // Vector<T>.Count depends on hardware (8 for AVX2, 16 for AVX512).
        var sumVector = Vector<float>.Zero;
        int i = 0;
        int vectorWidth = Vector<float>.Count;

        // Main SIMD loop: Process vectorWidth elements at a time
        for (; i <= vector.Length - vectorWidth; i += vectorWidth)
        {
            // Load data directly from Span into a Vector register
            var load = new Vector<float>(vector.Slice(i, vectorWidth));
            
            // Multiply and accumulate. 
            // This executes 'vectorWidth' multiplications in a single CPU instruction.
            sumVector += load * load; 
        }

        // Horizontal sum: Reduce the Vector<T> register to a single float
        float sumSq = 0.0f;
        for (int j = 0; j < vectorWidth; j++)
        {
            sumSq += sumVector[j];
        }

        // 2. Handle the tail (remaining elements that don't fit in a Vector)
        for (; i < vector.Length; i++)
        {
            sumSq += vector[i] * vector[i];
        }

        // 3. Calculate Magnitude and Inverse
        float magnitude = MathF.Sqrt(sumSq);
        float inverseMagnitude = 1.0f / magnitude;

        // 4. Scale in place (SIMD)
        // Create a vector containing the scalar inverseMagnitude
        var scaleVector = new Vector<float>(inverseMagnitude);
        
        i = 0;
        // Vectorized scaling loop
        for (; i <= vector.Length - vectorWidth; i += vectorWidth)
        {
            var data = new Vector<float>(vector.Slice(i, vectorWidth));
            var result = data * scaleVector; // SIMD multiplication
            
            // Copy back to the original span
            result.CopyTo(vector.Slice(i, vectorWidth));
        }

        // Handle tail for scaling
        for (; i < vector.Length; i++)
        {
            vector[i] *= inverseMagnitude;
        }
    }
}
