
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

// Source File: theory_theoretical_foundations_part5.cs
// Description: Theoretical Foundations
// ==========================================

using System;
using System.Numerics; // Requires System.Runtime.Intrinsics
using System.Runtime.Intrinsics.X86; // For hardware checks

public float CalculateSimilaritySimd(float[] embeddingA, float[] embeddingB)
{
    int length = embeddingA.Length;
    int vectorSize = Vector<float>.Count; // Depends on CPU (e.g., 8 for AVX2)
    
    float sum = 0;
    int i = 0;

    // Process in chunks using SIMD registers
    for (; i <= length - vectorSize; i += vectorSize)
    {
        var aVec = new Vector<float>(embeddingA, i);
        var bVec = new Vector<float>(embeddingB, i);
        
        // Multiply elements and accumulate
        // This compiles to hardware instructions like VFMADDPS (Fused Multiply-Add)
        sum += Vector.Dot(aVec, bVec);
    }

    // Process remaining elements (tail) scalarly
    for (; i < length; i++)
    {
        sum += embeddingA[i] * embeddingB[i];
    }

    return sum;
}
