
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
using System.Linq;

public class FunctionalMatrixExercise
{
    public static void Run()
    {
        // Sample Data (3x3)
        double[,] matrixA = { { 1, 2, 3 }, { 4, 5, 6 }, { 7, 8, 9 } };
        double[,] matrixB = { { 9, 8, 7 }, { 6, 5, 4 }, { 3, 2, 1 } };
        double[] bias = { 1, 1, 1 };

        int rowsA = matrixA.GetLength(0);
        int colsA = matrixA.GetLength(1);
        int colsB = matrixB.GetLength(1);

        // --- FUNCTIONAL IMPLEMENTATION ---
        
        // Outer Loop: Iterate over rows of A (i)
        var resultMatrix = Enumerable.Range(0, rowsA)
            .Select(i => 
            {
                // Middle Loop: Iterate over columns of B (j)
                return Enumerable.Range(0, colsB)
                    .Select(j => 
                    {
                        // Inner Loop: Dot Product (Reduction via Sum)
                        // k represents the shared dimension (cols of A / rows of B)
                        double dotProduct = Enumerable.Range(0, colsA)
                            .Sum(k => matrixA[i, k] * matrixB[k, j]);

                        // Transformation: Add Bias
                        double biased = dotProduct + bias[j];

                        // Transformation: Activation Function (ReLU)
                        return Math.Max(0, biased);
                    })
                    .ToArray(); // Materialize the row into an array
            })
            .ToArray(); // Materialize the matrix into a jagged array

        // Output Result
        Console.WriteLine("Functional Matrix Result:");
        for (int i = 0; i < resultMatrix.Length; i++)
        {
            Console.WriteLine($"Row {i}: {string.Join(", ", resultMatrix[i].Select(x => x.ToString("F2")))}");
        }
    }
}
