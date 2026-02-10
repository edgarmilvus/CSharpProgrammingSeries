
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

using System.Numerics; // Requires System.Numerics package
using System.Runtime.Intrinsics;

public void AddVectorsSimd(Span<float> a, Span<float> b, Span<float> result)
{
    // Determine how many floats fit into a single Vector register
    int vectorSize = Vector<float>.Count;
    
    int i = 0;
    
    // Loop unrolling for SIMD
    for (; i <= a.Length - vectorSize; i += vectorSize)
    {
        // Load chunks of memory into SIMD registers
        var va = new Vector<float>(a.Slice(i, vectorSize));
        var vb = new Vector<float>(b.Slice(i, vectorSize));
        
        // Perform the addition on ALL elements in the register simultaneously
        var vres = va + vb;
        
        // Store the result back into the memory
        vres.CopyTo(result.Slice(i, vectorSize));
    }

    // Handle the "tail" (remaining elements that didn't fit in a full vector)
    for (; i < a.Length; i++)
    {
        result[i] = a[i] + b[i];
    }
}
