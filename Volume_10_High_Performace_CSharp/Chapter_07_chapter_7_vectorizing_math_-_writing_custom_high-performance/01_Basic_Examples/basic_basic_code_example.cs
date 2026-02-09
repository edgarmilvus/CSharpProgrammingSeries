
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

# Source File: basic_basic_code_example.cs
# Description: Basic Code Example
# ==========================================

using System;
using System.Numerics; // Required for Vector<T>
using System.Runtime.CompilerServices; // For MethodImplOptions.AggressiveInlining

public class VectorScalingDemo
{
    public static void Main()
    {
        // 1. Define the input data (e.g., raw token scores).
        // We use a multiple of the typical Vector<float>.Count (which is 4 on AVX, 8 on AVX512).
        // For this "Hello World" example, we stick to 16 elements for clarity.
        float[] tokenScores = { 1.5f, 2.0f, 0.5f, -1.0f, 3.0f, 4.0f, 0.1f, 0.9f, 
                                1.5f, 2.0f, 0.5f, -1.0f, 3.0f, 4.0f, 0.1f, 0.9f };
        
        // The scaling factor (e.g., a learning rate or temperature parameter).
        float scaleFactor = 2.0f;

        Console.WriteLine("--- Scalar Processing (Naive Loop) ---");
        float[] scalarResult = ScaleScalar((float[])tokenScores.Clone(), scaleFactor);
        PrintArray(scalarResult);

        Console.WriteLine("\n--- Vector Processing (SIMD) ---");
        float[] vectorResult = ScaleVector((float[])tokenScores.Clone(), scaleFactor);
        PrintArray(vectorResult);
    }

    /// <summary>
    /// Standard scalar processing: processes one element at a time.
    /// </summary>
    private static float[] ScaleScalar(float[] data, float scale)
    {
        for (int i = 0; i < data.Length; i++)
        {
            data[i] *= scale; // Single multiplication per iteration
        }
        return data;
    }

    /// <summary>
    /// High-performance SIMD processing: processes multiple elements simultaneously.
    /// </summary>
    private static float[] ScaleVector(float[] data, float scale)
    {
        // Convert the scalar 'scale' to a Vector<T> where every lane contains the same value.
        // Example: if Vector<float>.Count is 4, this creates [2.0, 2.0, 2.0, 2.0].
        Vector<float> scaleVector = new Vector<float>(scale);

        // Calculate the number of elements we can process in full vector chunks.
        int i = 0;
        int lastSIMDIndex = data.Length - Vector<float>.Count;

        // Loop through the array in steps of the vector width.
        for (; i <= lastSIMDIndex; i += Vector<float>.Count)
        {
            // Load a chunk of data from memory into a CPU register.
            // This loads 4 floats (if AVX) into a single 128-bit or 256-bit register.
            Vector<float> dataChunk = new Vector<float>(data, i);

            // Perform the multiplication on all lanes simultaneously.
            // This compiles down to a single SIMD instruction (e.g., vmulps on x86).
            Vector<float> resultChunk = dataChunk * scaleVector;

            // Store the result back into the array.
            // This writes the 4 calculated values back to memory in one operation.
            resultChunk.CopyTo(data, i);
        }

        // Handle the "tail" of the array (remaining elements not divisible by Vector.Count).
        // Since Vector<T>.Count is dynamic (hardware dependent), we must handle leftovers scalar-wise.
        for (; i < data.Length; i++)
        {
            data[i] *= scale;
        }

        return data;
    }

    private static void PrintArray(float[] arr)
    {
        Console.WriteLine(string.Join(", ", arr));
    }
}
