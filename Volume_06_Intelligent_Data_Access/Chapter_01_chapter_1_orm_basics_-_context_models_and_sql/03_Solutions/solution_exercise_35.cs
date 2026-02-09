
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

# Source File: solution_exercise_35.cs
# Description: Solution for Exercise 35
# ==========================================

using System;
using System.Collections.Generic;
using System.Linq;

// 1. Extremely Compressed Vectors (Binary Embeddings)
public class IoTVectorSearch
{
    // IoT devices might only have KBs of RAM.
    // Float arrays are too heavy.
    // Use Binary Embeddings (Bits instead of Floats).
    // 00101010 instead of [0.1, -0.2, 0.5]
    
    public bool[] Binarize(float[] vector)
    {
        // Simple thresholding: > 0 is 1, else 0
        return vector.Select(v => v > 0).ToArray();
    }

    public double HammingDistance(bool[] a, bool[] b)
    {
        // Extremely fast bitwise operation
        int distance = 0;
        for (int i = 0; i < a.Length; i++)
        {
            if (a[i] != b[i]) distance++;
        }
        return distance;
    }

    public void SearchOnIoT(float[] query, List<StoredVector> database)
    {
        var queryBits = Binarize(query);
        
        // Linear scan is acceptable for small N on IoT
        var bestMatch = database
            .Select(v => new { v.Id, Dist = HammingDistance(v.Bits, queryBits) })
            .OrderBy(x => x.Dist)
            .FirstOrDefault();
    }
}

public class StoredVector
{
    public int Id { get; set; }
    public bool[] Bits { get; set; } // Binary representation
}
