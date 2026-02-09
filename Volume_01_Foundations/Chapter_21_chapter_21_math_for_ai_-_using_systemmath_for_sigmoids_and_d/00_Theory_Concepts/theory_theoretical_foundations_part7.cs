
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

# Source File: theory_theoretical_foundations_part7.cs
# Description: Theoretical Foundations
# ==========================================

using System;

public class Normalizer
{
    public static double[] Normalize(double[] data)
    {
        // 1. Find Min and Max
        double min = data[0];
        double max = data[0];

        // Loop through the array to find the actual min and max
        for (int i = 1; i < data.Length; i++)
        {
            // We can use Math.Min and Math.Max here
            min = Math.Min(min, data[i]);
            max = Math.Max(max, data[i]);
        }

        // 2. Calculate the range
        double range = max - min;

        // 3. Create a new array to hold normalized values
        double[] normalizedData = new double[data.Length];

        // 4. Apply the formula to each element
        for (int i = 0; i < data.Length; i++)
        {
            // (x - min) / range
            normalizedData[i] = (data[i] - min) / range;
        }

        return normalizedData;
    }
}
