
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

using System.Buffers;

public class TokenEncoder
{
    public void EncodeBatch(string[] inputs)
    {
        // Rent an array from the shared pool.
        // This is a zero-allocation operation if one is available in the pool.
        int[] rentedBuffer = ArrayPool<int>.Shared.Rent(1024);

        try
        {
            // Use the buffer for heavy computation (e.g., converting tokens to IDs)
            for (int i = 0; i < inputs.Length; i++)
            {
                // Process input into rentedBuffer
                // ...
            }
        }
        finally
        {
            // CRITICAL: Return the array to the pool.
            // If we forget this, we cause a memory leak (buffer remains in use).
            // If we clear it, we prevent data leakage between uses.
            ArrayPool<int>.Shared.Return(rentedBuffer, clearArray: true);
        }
    }
}
