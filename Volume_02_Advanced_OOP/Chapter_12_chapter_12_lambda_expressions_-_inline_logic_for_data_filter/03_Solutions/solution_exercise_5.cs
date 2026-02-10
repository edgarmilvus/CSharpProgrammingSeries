
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

// Source File: solution_exercise_5.cs
// Description: Solution for Exercise 5
// ==========================================

using System;
using System.Collections.Generic;
using System.Linq;

public class ClosureDemo
{
    public static void Main()
    {
        // Initial noise floor
        float noiseFloor = 5.0f;

        // Create a filter function that CAPTURES the current noiseFloor value
        Func<List<List<float>>, List<float>> filterFunc = CreateDynamicFilter(noiseFloor);

        // Data
        var tensor = new List<List<float>>
        {
            new List<float> { 1.0f, 6.0f, 10.0f },
            new List<float> { 2.0f, 7.0f, 11.0f }
        };

        // Execute the filter
        // Even if we change noiseFloor here, the lambda inside filterFunc 
        // has already closed over the value 5.0f.
        noiseFloor = 20.0f; 

        var result = filterFunc(tensor);

        Console.WriteLine($"Results (filtered with captured threshold 5.0):");
        foreach (var val in result)
        {
            Console.WriteLine(val);
        }
        
        // Note: The result will contain 6, 10, 7, 11. 
        // It will NOT contain 1, 2 (filtered out) and will NOT filter out 6, 7 (which are < 20).
        // This proves the closure captured the value at definition time.
    }

    public static Func<List<List<float>>, List<float>> CreateDynamicFilter(float threshold)
    {
        // The lambda inside the return statement captures 'threshold'
        // This creates a closure.
        return tensor => tensor
            .SelectMany(row => row)
            .Where(val => val > threshold) // 'threshold' is captured here
            .ToList();
    }
}
