
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

public class BpeOptimizer
{
    /// <summary>
    /// Counts occurrences of an adjacent pair [leftToken, rightToken] using SIMD acceleration.
    /// </summary>
    public static int FindPairFrequencySimd(ReadOnlySpan<int> tokens, int leftToken, int rightToken)
    {
        int count = 0;
        int i = 0;

        // Check if SIMD is supported by the hardware.
        if (Vector.IsHardwareAccelerated)
        {
            int vectorSize = Vector<int>.Count;
            // We need to stop early to avoid reading out of bounds when checking i+1.
            int limit = tokens.Length - vectorSize;

            // Create a vector containing the leftToken repeated.
            Vector<int> leftVec = new Vector<int>(leftToken);

            for (; i < limit; i += vectorSize)
            {
                // Load a chunk of tokens into a vector.
                Vector<int> chunk = new Vector<int>(tokens.Slice(i, vectorSize));

                // Compare the chunk with the leftToken vector.
                // This returns a bitmask where bits are set if elements are equal.
                Vector<int> matchLeft = Vector.Equals(chunk, leftVec);

                // We also need to check the NEXT token in the sequence (i+1).
                // Load the next chunk (shifted by 1).
                Vector<int> nextChunk = new Vector<int>(tokens.Slice(i + 1, vectorSize));
                Vector<int> rightVec = new Vector<int>(rightToken);
                Vector<int> matchRight = Vector.Equals(nextChunk, rightVec);

                // Combine the conditions: Both must be true (adjacent match).
                Vector<int> pairMatch = Vector.BitwiseAnd(matchLeft, matchRight);

                // Convert the vector mask to a bitmask integer.
                int mask = Vector.AsVectorInt32(pairMatch).ToScalar();

                // Count set bits in the mask (popcount).
                // This counts how many valid pairs start at indices i through i+vectorSize-1.
                count += BitOperations.PopCount((uint)mask);
            }
        }

        // Fallback / Tail processing: Handle remaining elements with scalar logic.
        // We stop at tokens.Length - 1 to safely check tokens[i+1].
        for (; i < tokens.Length - 1; i++)
        {
            if (tokens[i] == leftToken && tokens[i + 1] == rightToken)
            {
                count++;
            }
        }

        return count;
    }
}
