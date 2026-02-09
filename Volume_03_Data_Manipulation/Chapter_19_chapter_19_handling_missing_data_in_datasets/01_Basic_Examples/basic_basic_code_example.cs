
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
using System.Numerics; // Required for Vector<T> (SIMD)

public class HighPerformanceDataPreprocessor
{
    public static void ProcessSensorData()
    {
        // Real-world context: Processing a stream of sensor readings (e.g., temperature)
        // where some values are missing (represented as -999.0f).
        // We need to replace these with the global average without allocating new arrays.

        // 1. ALLOCATION: Heap allocation for the raw data buffer.
        // In a real AI pipeline, this might be a massive tensor buffer loaded from disk.
        float[] rawData = new float[] { 22.5f, -999.0f, 23.1f, 22.8f, -999.0f, 24.0f };

        // 2. ZERO-ALLOCATION SLICING: Create a Span over the existing array.
        // Span<T> provides a type-safe view into memory without copying data.
        // This allows processing "slices" of large tensors efficiently.
        Span<float> dataSlice = rawData.AsSpan();

        // Calculate the average of valid data (ignoring missing values) for imputation.
        // We use a simple loop here to avoid LINQ allocations.
        float sum = 0;
        int validCount = 0;
        foreach (float val in dataSlice)
        {
            if (val > -999.0f) // Check for missing data marker
            {
                sum += val;
                validCount++;
            }
        }
        float globalAverage = validCount > 0 ? sum / validCount : 0.0f;

        // 3. HARDWARE ACCELERATION (SIMD): Using Vector<T> for batch processing.
        // This processes multiple data points simultaneously (e.g., 4 floats at once on AVX2).
        // Note: For this simple "Hello World", we simulate the logic. 
        // In a real scenario, we would loop with Vector.IsHardwareAccelerated checks.
        
        // 4. IN-PLACE MUTATION: Modifying the Span directly.
        // No new memory is allocated for the result. This is critical for high-throughput AI.
        for (int i = 0; i < dataSlice.Length; i++)
        {
            if (dataSlice[i] < -998.0f) // Detect missing value
            {
                dataSlice[i] = globalAverage; // Impute directly into memory
            }
        }

        // Output results to verify
        Console.WriteLine($"Imputed Average: {globalAverage}");
        Console.WriteLine("Processed Data (Span): " + string.Join(", ", dataSlice.ToArray()));
    }
}
