
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

// Source File: solution_exercise_1.cs
// Description: Solution for Exercise 1
// ==========================================

using System;

public class DataSlicer
{
    // Simulates a large buffer of data (e.g., 1000 batches of 100 time steps)
    private float[] _sensorData;
    private const int SequenceLength = 100;

    public DataSlicer(int totalBatches)
    {
        _sensorData = new float[totalBatches * SequenceLength];
        // Initialize with dummy data
        Random.Shared.NextSingle(_sensorData);
    }

    /// <summary>
    /// Extracts a window of data for a specific batch index.
    /// </summary>
    /// <param name="batchIndex">The index of the batch (0-based).</param>
    /// <returns>A Span<float> representing the window. Returns empty if out of bounds.</returns>
    public Span<float> GetSensorWindow(int batchIndex)
    {
        // Calculate the starting index of the batch in the flat array
        int startIndex = batchIndex * SequenceLength;

        // Perform manual boundary checks to prevent AccessViolationException
        if (startIndex < 0 || startIndex >= _sensorData.Length)
        {
            return Span<float>.Empty;
        }

        // Create a Span over the entire array and slice the specific window.
        // This operation is zero-allocation; it simply wraps the pointer 
        // with a new offset and length.
        return _sensorData.AsSpan().Slice(startIndex, SequenceLength);
    }
}

// Extension for Random to fill array
public static class RandomExtensions
{
    public static void NextSingle(this Random random, float[] buffer)
    {
        for (int i = 0; i < buffer.Length; i++)
        {
            buffer[i] = (float)random.NextDouble();
        }
    }
}
