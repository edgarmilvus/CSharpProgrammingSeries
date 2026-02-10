
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
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

public static class CharacterCategorizer
{
    public static (int wordChars, int punctuation, int whitespace) CategorizeCharacters(ReadOnlySpan<char> text)
    {
        int wordCount = 0;
        int punctCount = 0;
        int whiteCount = 0;

        // Use unsafe context to work with pointers for maximum performance
        unsafe
        {
            fixed (char* ptr = text)
            {
                char* current = ptr;
                int length = text.Length;
                int i = 0;

                // SIMD Processing (Vectorized)
                // We process 128-bit vectors (Vector<ushort> on most hardware).
                // char is 2 bytes.
                if (Vector.IsHardwareAccelerated)
                {
                    int vectorSize = Vector<ushort>.Count;
                    
                    // Loop unrolling or processing in chunks
                    while (i <= length - vectorSize)
                    {
                        // Load vector from memory
                        var vector = Vector.LoadUnsafe(ref Unsafe.As<char, ushort>(ref *current));
                        
                        // Process vector elements
                        for (int j = 0; j < vectorSize; j++)
                        {
                            char c = (char)vector[j];
                            Categorize(c, ref wordCount, ref punctCount, ref whiteCount);
                        }

                        current += vectorSize;
                        i += vectorSize;
                    }
                }

                // Scalar Fallback for remaining elements
                for (; i < length; i++)
                {
                    Categorize(*current, ref wordCount, ref punctCount, ref whiteCount);
                    current++;
                }
            }
        }

        return (wordCount, punctCount, whiteCount);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Categorize(char c, ref int word, ref int punct, ref int white)
    {
        if (char.IsLetterOrDigit(c)) word++;
        else if (char.IsPunctuation(c)) punct++;
        else if (char.IsWhiteSpace(c)) white++;
        // Note: Other characters (symbols, control) are ignored in this specific requirement
    }
}
