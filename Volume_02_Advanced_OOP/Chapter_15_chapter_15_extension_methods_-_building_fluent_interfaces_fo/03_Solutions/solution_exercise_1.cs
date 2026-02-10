
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
using System.Linq;

// 1. Simulated External Library (Immutable)
namespace LegacyTensorLib
{
    public class Tensor
    {
        public double[] Data { get; private set; }
        public int Rows { get; }
        public int Cols { get; }

        public Tensor(double[] data, int rows, int cols)
        {
            Data = data;
            Rows = rows;
            Cols = cols;
        }

        public Tensor Add(Tensor other)
        {
            var result = new double[Data.Length];
            for (int i = 0; i < Data.Length; i++)
                result[i] = Data[i] + other.Data[i];
            return new Tensor(result, Rows, Cols);
        }

        public Tensor Multiply(double scalar)
        {
            var result = new double[Data.Length];
            for (int i = 0; i < Data.Length; i++)
                result[i] = Data[i] * scalar;
            return new Tensor(result, Rows, Cols);
        }

        public override string ToString()
        {
            return $"Tensor [{Rows}x{Cols}]: [{string.Join(", ")}]";
        }
    }
}

// 2. Extension Namespace
namespace AIChainExtensions
{
    public static class TensorExtensions
    {
        // Normalizes the tensor so that all elements sum to 1.0
        public static LegacyTensorLib.Tensor Normalize(this LegacyTensorLib.Tensor tensor)
        {
            // Using LINQ Sum() for brevity
            double sum = tensor.Data.Sum();

            // Edge case: Avoid division by zero
            if (Math.Abs(sum) < 1e-9) return tensor;

            var result = new double[tensor.Data.Length];
            for (int i = 0; i < tensor.Data.Length; i++)
            {
                result[i] = tensor.Data[i] / sum;
            }

            return new LegacyTensorLib.Tensor(result, tensor.Rows, tensor.Cols);
        }

        // Clips values between min and max
        public static LegacyTensorLib.Tensor Clip(this LegacyTensorLib.Tensor tensor, double min, double max)
        {
            var result = new double[tensor.Data.Length];
            for (int i = 0; i < tensor.Data.Length; i++)
            {
                double val = tensor.Data[i];
                if (val < min) result[i] = min;
                else if (val > max) result[i] = max;
                else result[i] = val;
            }
            return new LegacyTensorLib.Tensor(result, tensor.Rows, tensor.Cols);
        }

        // Flattens the 2D structure into a 1xN tensor
        public static LegacyTensorLib.Tensor Flatten(this LegacyTensorLib.Tensor tensor)
        {
            // We simply copy the data but change the dimensions
            return new LegacyTensorLib.Tensor(tensor.Data, 1, tensor.Data.Length);
        }
    }
}

// 3. Usage
public class Program
{
    public static void Main()
    {
        // Import extensions
        using AIChainExtensions;

        // Create initial data: 2x2 matrix
        double[] rawData = { 1.0, 2.0, 3.0, 4.0 }; 
        var tensor = new LegacyTensorLib.Tensor(rawData, 2, 2);

        // Apply Fluent Chain
        // 1. Multiply: { 2, 4, 6, 8 }
        // 2. Clip: { 2, 4, 5, 5 } (6 and 8 clipped to 5)
        // 3. Normalize: Sum is 16. 2/16=0.125, 4/16=0.25, 5/16=0.3125
        // 4. Flatten: Rows=1, Cols=4
        var processedTensor = tensor
            .Multiply(2.0)       
            .Clip(0, 5)          
            .Normalize()         
            .Flatten();          

        Console.WriteLine("Original: " + tensor);
        Console.WriteLine("Processed: " + processedTensor);
        Console.WriteLine($"Final Dimensions: {processedTensor.Rows}x{processedTensor.Cols}");
    }
}
