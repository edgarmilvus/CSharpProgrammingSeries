
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

// Source File: solution_exercise_4.cs
// Description: Solution for Exercise 4
// ==========================================

using System;
using System.Buffers; // For ArrayPool

public class MedianCalculator
{
    public static float[] CalculateMediansPooled(float[][] dataset)
    {
        if (dataset == null || dataset.Length == 0) return Array.Empty<float>();

        int featureCount = dataset.Length;
        float[] medians = new float[featureCount];

        for (int i = 0; i < featureCount; i++)
        {
            float[] featureData = dataset[i];
            
            // Step 1: Count valid entries to determine buffer size
            int validCount = 0;
            foreach (var val in featureData)
            {
                if (!float.IsNaN(val)) validCount++;
            }

            if (validCount == 0)
            {
                medians[i] = float.NaN;
                continue;
            }

            // Step 2: Rent a buffer from the shared pool
            // Note: The pool may return an array larger than requested.
            float[] buffer = ArrayPool<float>.Shared.Rent(validCount);

            try
            {
                // Step 3: Fill the buffer using Span
                // We create a slice of the rented array to match the exact validCount.
                // This prevents reading uninitialized data in the rented array's tail.
                Span<float> validSpan = buffer.AsSpan(0, validCount);
                int index = 0;
                
                // Manual copy loop
                foreach (var val in featureData)
                {
                    if (!float.IsNaN(val))
                    {
                        validSpan[index++] = val;
                    }
                }

                // Step 4: Sort the Span in-place
                // Array.Sort has an overload that accepts Span<T>
                Array.Sort(validSpan);

                // Step 5: Calculate Median
                int mid = validCount / 2;
                if (validCount % 2 == 0)
                {
                    medians[i] = (validSpan[mid - 1] + validSpan[mid]) / 2.0f;
                }
                else
                {
                    medians[i] = validSpan[mid];
                }
            }
            finally
            {
                // Step 6: Return the buffer to the pool
                // The finally block guarantees execution, even if an exception 
                // occurs during sorting or calculation.
                ArrayPool<float>.Shared.Return(buffer);
            }
        }

        return medians;
    }
}
