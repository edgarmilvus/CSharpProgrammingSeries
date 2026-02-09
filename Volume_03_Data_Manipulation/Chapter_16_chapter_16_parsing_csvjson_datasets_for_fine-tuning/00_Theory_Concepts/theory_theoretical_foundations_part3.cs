
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

# Source File: theory_theoretical_foundations_part3.cs
# Description: Theoretical Foundations
# ==========================================

using System.Numerics;
using System.Runtime.Intrinsics.X86; // For hardware checks

public class VectorMath
{
    // Calculates dot product using SIMD
    public static float DotProductSimd(ReadOnlySpan<float> a, ReadOnlySpan<float> b)
    {
        int length = a.Length;
        int vectorSize = Vector<float>.Count; // Hardware dependent (4 or 8 usually)
        int i = 0;
        
        Vector<float> sum = Vector<float>.Zero;

        // Process in vector-sized chunks
        for (; i <= length - vectorSize; i += vectorSize)
        {
            var va = new Vector<float>(a.Slice(i, vectorSize));
            var vb = new Vector<float>(b.Slice(i, vectorSize));
            sum += va * vb; // Hardware accelerated multiplication and addition
        }

        // Horizontal sum (add remaining elements)
        float result = 0f;
        for (; i < length; i++)
        {
            result += a[i] * b[i];
        }
        
        // Add the vector accumulator to the scalar result
        for (int j = 0; j < vectorSize; j++)
        {
            result += sum[j];
        }

        return result;
    }
}
