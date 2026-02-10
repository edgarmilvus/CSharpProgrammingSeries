
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

// Source File: solution_exercise_3.cs
// Description: Solution for Exercise 3
// ==========================================

using System;
using System.Collections.Generic;
using System.Numerics;

public class TokenFilter
{
    public static List<int> FilterTokens(ReadOnlySpan<int> tokens, ReadOnlySpan<int> stopTokens)
    {
        // Pre-size the list to avoid frequent reallocations. 
        // We don't know the exact filtered count, but we can guess or use a larger buffer.
        // For this exercise, we'll use a List and let it grow, or use an intermediate array.
        // To minimize allocations, let's use a pre-sized array as a buffer.
        int[] buffer = new int[tokens.Length]; 
        int outputIndex = 0;

        int vectorWidth = Vector<int>.Count;
        int i = 0;

        // Main SIMD Loop
        for (; i <= tokens.Length - vectorWidth; i += vectorWidth)
        {
            var chunk = new Vector<int>(tokens.Slice(i, vectorWidth));
            
            // Accumulate a mask. 0 means valid (no match), non-zero means match.
            // We start with all zeros.
            Vector<int> matchMask = Vector<int>.Zero;

            foreach (int stopToken in stopTokens)
            {
                var stopVec = new Vector<int>(stopToken);
                // Vector.Equals returns a mask of -1 (all bits set) for matches, 0 otherwise.
                matchMask = Vector.BitwiseOr(matchMask, Vector.Equals(chunk, stopVec));
            }

            // Check if the mask is all zeros (no matches found in this chunk)
            // If matchMask is zero, we can copy the whole chunk safely.
            if (Vector<int>.Zero == matchMask)
            {
                // Fast path: No stop tokens in this chunk.
                // We can copy the whole vector to the output.
                chunk.CopyTo(buffer.AsSpan(outputIndex));
                outputIndex += vectorWidth;
            }
            else
            {
                // Slow path: There is at least one stop token in this chunk.
                // We must fall back to scalar processing for this specific chunk 
                // to perform compaction (removing the bad elements).
                for (int j = 0; j < vectorWidth; j++)
                {
                    int val = tokens[i + j];
                    bool isStop = false;
                    foreach (int stop in stopTokens)
                    {
                        if (val == stop)
                        {
                            isStop = true;
                            break;
                        }
                    }
                    if (!isStop)
                    {
                        buffer[outputIndex++] = val;
                    }
                }
            }
        }

        // Process tail elements scalarly
        for (; i < tokens.Length; i++)
        {
            int val = tokens[i];
            bool isStop = false;
            foreach (int stop in stopTokens)
            {
                if (val == stop)
                {
                    isStop = true;
                    break;
                }
            }
            if (!isStop)
            {
                buffer[outputIndex++] = val;
            }
        }

        // Return the final list
        return new List<int>(buffer.AsSpan(0, outputIndex).ToArray());
    }
}
