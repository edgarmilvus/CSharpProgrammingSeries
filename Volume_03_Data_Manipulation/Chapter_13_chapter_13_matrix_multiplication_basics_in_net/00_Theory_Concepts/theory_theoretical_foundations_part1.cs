
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

# Source File: theory_theoretical_foundations_part1.cs
# Description: Theoretical Foundations
# ==========================================

using System;
using System.Collections.Generic;
using System.Linq;

public class MatrixOperations
{
    // Calculates C = A * B
    public static IEnumerable<IEnumerable<double>> Multiply(
        IEnumerable<IEnumerable<double>> matrixA, 
        IEnumerable<IEnumerable<double>> matrixB)
    {
        // Convert B to a list for efficient column access (Immediate Execution on B)
        // This is a performance optimization: B is accessed multiple times.
        var bList = matrixB.Select(row => row.ToList()).ToList();
        
        int rowsA = matrixA.Count();
        int colsA = matrixA.First().Count();
        int colsB = bList.First().Count();

        // Validate dimensions
        if (matrixA.First().Count() != bList.Count)
            throw new ArgumentException("Inner matrix dimensions must agree.");

        // The outer Select projects each row of A into a new row of C
        return matrixA.Select((rowA, indexA) => 
        {
            // For each row in A, calculate the dot product with every column in B
            // We use a range to access columns by index
            return Enumerable.Range(0, colsB).Select(colIndex =>
            {
                // Access the specific column of B
                // In a purely functional approach without side effects, 
                // we map B's column index to the values.
                var columnB = bList.Select(rowB => rowB[colIndex]);

                // Calculate Dot Product: Sum of (A_row[i] * B_col[i])
                return rowA.Zip(columnB, (a, b) => a * b).Sum();
            });
        });
    }
}
