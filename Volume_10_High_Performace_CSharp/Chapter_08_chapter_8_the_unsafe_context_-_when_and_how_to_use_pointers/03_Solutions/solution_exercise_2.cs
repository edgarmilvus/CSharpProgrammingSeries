
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

public static class MemoryUtils
{
    public static unsafe int FastMemCmp(byte[] a, byte[] b)
    {
        // Handle null references
        if (a == null || b == null)
        {
            throw new ArgumentNullException("Input arrays cannot be null.");
        }

        // Handle length differences immediately
        if (a.Length != b.Length)
        {
            return a.Length < b.Length ? -1 : 1;
        }

        // If arrays are empty or identical references, return 0
        if (a.Length == 0 || ReferenceEquals(a, b))
        {
            return 0;
        }

        unsafe
        {
            fixed (byte* ptrA = &a[0])
            fixed (byte* ptrB = &b[0])
            {
                byte* pA = ptrA;
                byte* pB = ptrB;
                int length = a.Length;

                // Loop using pointers
                for (int i = 0; i < length; i++)
                {
                    // Dereference pointers to get byte values
                    if (*pA != *pB)
                    {
                        // Return the difference (standard memcmp behavior)
                        return *pA - *pB;
                    }
                    
                    // Increment pointers to next byte
                    pA++;
                    pB++;
                }
            }
        }

        return 0;
    }
}
