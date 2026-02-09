
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

# Source File: solution_exercise_5.cs
# Description: Solution for Exercise 5
# ==========================================

using System;

// 1. Define the struct
public readonly struct TensorShape
{
    public readonly int D; // Depth
    public readonly int H; // Height
    public readonly int W; // Width

    public TensorShape(int d, int h, int w)
    {
        D = d;
        H = h;
        W = w;
    }

    // Managed method for standard usage
    public readonly int ElementCount() => D * H * W;
}

public static class TensorOps
{
    // 3. Unsafe method to access struct via pointer
    public static unsafe int GetElementCountFromPointer(TensorShape* shapePtr)
    {
        // 4. Access fields using the -> operator
        // We dereference the pointer to access the struct members
        return shapePtr->D * shapePtr->H * shapePtr->W;
    }
}

// 6. Memory Layout Visualization
/*
   Assuming a 32-bit (4-byte) integer for each field and no padding (common for structs with sequential layout):
   
   Address Offset | Field
   ----------------|-------
   0x00           | D (int)
   0x04           | H (int)
   0x08           | W (int)
   
   Total Size: 12 bytes (0x0C)
   
   The struct is value-type and contains only primitive value types (ints). 
   It has no references to the managed heap.
*/
