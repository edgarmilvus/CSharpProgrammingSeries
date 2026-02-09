
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

# Source File: theory_theoretical_foundations_part9.cs
# Description: Theoretical Foundations
# ==========================================

using System;

class Program
{
    static void Main()
    {
        // A 2x3 grid (2 rows, 3 columns)
        int[,] grid = {
            { 1, 2, 3 },
            { 4, 5, 6 }
        };

        // Outer loop: Iterates over the rows
        // In a 2D array, the first item is the 'row' dimension.
        // However, 'foreach' doesn't care about dimensions, it just grabs chunks.
        
        // To be strict with allowed concepts, we treat the 2D array as a sequence of rows.
        // But to visualize it, we usually use nested loops.
        
        // Let's look at the rows first.
        // We need to know how many rows exist. We use the .GetLength(0) method (allowed in Ch 11).
        int rows = grid.GetLength(0);
        int cols = grid.GetLength(1);

        // We will use a standard 'for' loop for the rows because 'foreach' on a 2D array
        // returns 1D arrays (which might be confusing for a beginner right now).
        // But let's stick to the pure 'foreach' concept where possible.

        // Actually, strictly speaking, 'foreach' on a multi-dimensional array iterates linearly.
        // But to make it understandable, let's simulate the grid traversal logic using allowed concepts.
        
        Console.WriteLine("--- Reading the Grid ---");
        
        // We know the size of the first dimension (rows)
        for (int i = 0; i < grid.GetLength(0); i++)
        {
            // We know the size of the second dimension (cols)
            for (int j = 0; j < grid.GetLength(1); j++)
            {
                // Accessing specific index
                int value = grid[i, j];
                Console.Write(value + " ");
            }
            Console.WriteLine(); // New line after every row
        }
    }
}
