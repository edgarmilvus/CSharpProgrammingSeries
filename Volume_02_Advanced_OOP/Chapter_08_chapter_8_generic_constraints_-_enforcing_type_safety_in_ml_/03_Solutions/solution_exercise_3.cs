
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

// Source File: solution_exercise_3.cs
// Description: Solution for Exercise 3
// ==========================================

using System;

namespace MLEngine.GenericConstraints
{
    // 1. Interfaces defining tensor shapes
    public interface I1DTensor
    {
        int Length { get; }
    }

    public interface I2DTensor
    {
        int Rows { get; }
        int Cols { get; }
    }

    // 2. Distinct Struct Implementations
    // We use separate structs to enforce shape strictly.
    public struct Vector<T> : I1DTensor where T : struct
    {
        public int Length { get; }
        public Vector(int len) { Length = len; }
    }

    public struct Matrix<T> : I1DTensor, I2DTensor where T : struct
    {
        public int Rows { get; }
        public int Cols { get; }
        public int Length => Rows * Cols; // Explicit implementation or property

        public Matrix(int r, int c) { Rows = r; Cols = c; }
    }

    // 3. Tensor Processor with Constraint
    public class TensorProcessor
    {
        // Constraint: T must implement I2DTensor
        // Note: We constrain the generic type T of the struct, but here we actually want to constrain the struct itself.
        // The cleanest way in C# is to accept the specific Matrix<T> type or an interface.
        // To strictly enforce "Only Matrices", we accept the Matrix<T> struct directly or an interface.
        
        public void ProcessMatrix<T>(Matrix<T> tensor) where T : struct
        {
            Console.WriteLine($"Processing Matrix: {tensor.Rows}x{tensor.Cols}");
        }
    }

    public class Exercise3Runner
    {
        public static void Run()
        {
            var processor = new TensorProcessor();

            // Valid: 2D Tensor (Matrix)
            var matrix = new Matrix<double>(10, 20);
            processor.ProcessMatrix(matrix); 

            // 4. Compile-time error demonstration (Uncomment to test)
            // var vector = new Vector<float>(100);
            // processor.ProcessMatrix(vector); 
            // Error: Argument 1: cannot convert from 'Vector<float>' to 'Matrix<float>'
        }
    }
}
