
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

// Source File: theory_theoretical_foundations_part6.cs
// Description: Theoretical Foundations
// ==========================================

// Conceptual SIMD addition using fixed pointers
public unsafe void AddVectors(float[] a, float[] b, float[] result)
{
    // Pin all arrays to ensure stability during the operation
    fixed (float* aPtr = a)
    fixed (float* bPtr = b)
    fixed (float* rPtr = result)
    {
        int i = 0;
        int length = a.Length;
        
        // Process in blocks of Vector<float>.Count (e.g., 8 floats for AVX)
        int vectorLength = Vector<float>.Count;
        int lastBlockIndex = length - (length % vectorLength);
        
        for (; i < lastBlockIndex; i += vectorLength)
        {
            // Load data directly from memory into SIMD registers
            Vector<float> va = Vector.LoadUnsafe(ref aPtr[i]);
            Vector<float> vb = Vector.LoadUnsafe(ref bPtr[i]);
            
            // Perform vector addition
            Vector<float> vres = va + vb;
            
            // Store result back to memory
            vres.StoreUnsafe(ref rPtr[i]);
        }
        
        // Handle remaining elements (tail processing)
        for (; i < length; i++)
        {
            rPtr[i] = aPtr[i] + bPtr[i];
        }
    }
}
