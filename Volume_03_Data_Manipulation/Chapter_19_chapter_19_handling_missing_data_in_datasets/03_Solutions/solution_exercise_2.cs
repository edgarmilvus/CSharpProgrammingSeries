
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

# Source File: solution_exercise_2.cs
# Description: Solution for Exercise 2
# ==========================================

using System;
using System.Numerics;

public class Imputer
{
    public static void ImputeMissingValuesSimd(float[] data)
    {
        // 1. Calculate Mean (excluding NaNs)
        double sum = 0;
        int validCount = 0;
        
        // First pass: Scalar accumulation for correctness.
        // While we could vectorize this, the logic for handling NaNs 
        // requires careful masking. A scalar loop is often sufficient 
        // for the reduction step unless the dataset is massive.
        for (int i = 0; i < data.Length; i++)
        {
            if (!float.IsNaN(data[i]))
            {
                sum += data[i];
                validCount++;
            }
        }

        if (validCount == 0) return; // Nothing to impute
        
        float mean = (float)(sum / validCount);

        // 2. Impute using SIMD
        int i = 0;
        int vectorSize = Vector<float>.Count;
        
        // Create a vector filled with the mean value
        Vector<float> meanVector = new Vector<float>(mean);

        // Process the bulk of the array in vector-sized chunks
        for (; i <= data.Length - vectorSize; i += vectorSize)
        {
            // Load the data vector from memory
            Vector<float> dataVector = new Vector<float>(data, i);

            // NaN Masking Trick: 
            // In IEEE 754, NaN is not equal to itself. 
            // (x == x) returns false for NaN, true for valid numbers.
            // Vector.Equals returns a Vector<float> where elements are 
            // -1.0f (all bits set) for true, and 0.0f for false.
            Vector<float> isValidMask = Vector.Equals(dataVector, dataVector);

            // ConditionalSelect chooses from the first or second vector 
            // based on the mask. This is a hardware-accelerated blend instruction.
            Vector<float> result = Vector.ConditionalSelect(isValidMask, dataVector, meanVector);
            
            // Store the result back to memory
            result.CopyTo(data, i);
        }

        // Handle the tail (remaining elements that don't fit in a vector)
        for (; i < data.Length; i++)
        {
            if (float.IsNaN(data[i]))
            {
                data[i] = mean;
            }
        }
    }
}
