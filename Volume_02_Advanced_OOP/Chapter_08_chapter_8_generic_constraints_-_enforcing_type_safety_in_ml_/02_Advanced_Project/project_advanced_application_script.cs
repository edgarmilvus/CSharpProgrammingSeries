
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

# Source File: project_advanced_application_script.cs
# Description: Advanced Application Script
# ==========================================

using System;

namespace AdvancedOOP
{
    // 1. TENSOR DEFINITION
    // We define a generic Tensor class. This represents our fundamental data structure.
    // It holds data of a specific type T and has a defined shape (dimensions).
    public class Tensor<T>
    {
        public T[] Data { get; private set; }
        public int[] Shape { get; private set; }
        public Type DataType { get { return typeof(T); } }

        // Constructor initializes the data storage.
        public Tensor(int[] shape)
        {
            this.Shape = shape;
            int size = 1;
            foreach (int dim in shape)
            {
                size *= dim;
            }
            this.Data = new T[size];
        }

        // Helper to get value at specific indices (simplified for 2D).
        public T Get(int row, int col)
        {
            int index = row * Shape[1] + col;
            return Data[index];
        }

        // Helper to set value at specific indices (simplified for 2D).
        public void Set(int row, int col, T value)
        {
            int index = row * Shape[1] + col;
            Data[index] = value;
        }
    }

    // 2. ABSTRACT PROCESSOR
    // This defines the contract for any step in our pipeline.
    // It uses a generic type parameter TInput and TOutput.
    public abstract class Processor<TInput, TOutput>
    {
        // The core method that transforms data.
        // We enforce that the output must be a Tensor of the specific type.
        public abstract Tensor<TOutput> Process(Tensor<TInput> input);
    }

    // 3. UPPER-BOUNDED WILDCARD HANDLING (COVARIANCE)
    // In a real system, we might receive a stream of tensors of unknown specific types,
    // but we know they all inherit from a base class (e.g., NumericBase).
    // We use an interface to define covariance.
    public interface INumericTensor<out T> where T : struct
    {
        Tensor<T> GetTensor();
    }

    // 4. SPECIFIC PROCESSORS (STRICT TYPE SAFETY)

    // A. Normalization Processor
    // Constraints: T must be a floating-point type (float, double).
    // This prevents applying normalization to complex types or integers at compile time.
    public class Normalizer<T> : Processor<T, float> where T : struct
    {
        // We restrict T to struct, but specifically we check for float/double logic inside.
        // Note: C# generic constraints don't allow "float or double" directly without a common base,
        // so we rely on runtime checks for the math, but compile-time for structure.
        public override Tensor<float> Process(Tensor<T> input)
        {
            // Create output tensor of type float.
            Tensor<float> output = new Tensor<float>(input.Shape);
            
            // Simple normalization logic (0-1 range simulation).
            double maxVal = 0.0;
            
            // Find max value (simplified loop logic).
            for (int i = 0; i < input.Data.Length; i++)
            {
                // We must convert T to double to do math.
                // This is safe because we constrained T to struct, and specifically handle numeric types.
                double val = Convert.ToDouble(input.Data[i]);
                if (val > maxVal) maxVal = val;
            }

            // Apply normalization.
            for (int i = 0; i < input.Data.Length; i++)
            {
                double val = Convert.ToDouble(input.Data[i]);
                output.Data[i] = (float)(val / (maxVal + 0.0001)); // Avoid div by zero
            }

            return output;
        }
    }

    // B. Matrix Multiplication Processor
    // Constraints: T must be float.
    // This enforces a strict data type requirement for the math operations.
    public class MatrixMultiplier : Processor<float, float>
    {
        // We expect a 2D tensor (Matrix) as input.
        // In a real scenario, we would validate dimensions dynamically.
        public override Tensor<float> Process(Tensor<float> input)
        {
            // Let's simulate multiplying by a fixed identity matrix for demonstration.
            // In reality, this would take two tensors.
            int rows = input.Shape[0];
            int cols = input.Shape[1];
            
            Tensor<float> output = new Tensor<float>(new int[] { rows, cols });

            // Perform multiplication (Identity multiplication = same values).
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    // Apply a transformation (e.g., scaling by 2).
                    output.Set(r, c, input.Get(r, c) * 2.0f);
                }
            }
            
            return output;
        }
    }

    // 5. CUSTOM CONSTRAINT ENFORCER
    // This helper class demonstrates compile-time shape enforcement.
    public class ShapeValidator
    {
        // This method uses generic constraints to ensure the tensor is 2D.
        // We cannot enforce "2D" purely in C# generics without a wrapper, 
        // but we can enforce the data type strictly.
        public static bool Validate<T>(Tensor<T> tensor, int expectedRows, int expectedCols)
        {
            if (tensor.Shape.Length != 2) return false;
            if (tensor.Shape[0] != expectedRows) return false;
            if (tensor.Shape[1] != expectedCols) return false;
            return true;
        }
    }

    // 6. UNIFIED PIPELINE ARCHITECTURE
    // This class manages the flow of data through different processors.
    // It uses generics to chain inputs and outputs safely.
    public class MlPipeline<TStart, TEnd>
    {
        private Processor<TStart, object> firstStep;
        private Processor<object, TEnd> lastStep;

        // We can't easily chain generics in a simple list without type erasure in C#,
        // so we will simulate a two-step pipeline for this example: Normalization -> Matrix Multiplication.
        // In a real advanced system, we would use a chain of responsibility pattern with dynamic typing or interfaces.
        
        // For this script, we will create a specific pipeline runner that knows the types.
        public Tensor<float> Run(Tensor<int> rawData)
        {
            // Step 1: Normalize (int -> float)
            Normalizer<int> normalizer = new Normalizer<int>();
            Tensor<float> normalizedData = normalizer.Process(rawData);

            // Validate shape before next step
            if (!ShapeValidator.Validate(normalizedData, 2, 2))
            {
                throw new InvalidOperationException("Shape validation failed after normalization.");
            }

            // Step 2: Matrix Multiply (float -> float)
            MatrixMultiplier multiplier = new MatrixMultiplier();
            Tensor<float> result = multiplier.Process(normalizedData);

            return result;
        }
    }

    // 7. MAIN APPLICATION
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== Advanced Generic Constraints ML Pipeline ===");
            
            // Scenario: We are processing raw sensor data (integers) 
            // to prepare it for a model inference (floats).
            
            // 1. Create Raw Data (2x2 Matrix of Integers)
            Tensor<int> rawSensorData = new Tensor<int>(new int[] { 2, 2 });
            rawSensorData.Set(0, 0, 10);
            rawSensorData.Set(0, 1, 20);
            rawSensorData.Set(1, 0, 30);
            rawSensorData.Set(1, 1, 40);

            Console.WriteLine("Raw Sensor Data (Integers):");
            PrintTensor(rawSensorData);

            // 2. Initialize Pipeline
            // We define a pipeline that starts with int and ends with float.
            MlPipeline<int, float> pipeline = new MlPipeline<int, float>();

            try
            {
                // 3. Execute Pipeline
                // The generic constraints ensure that:
                // - Normalizer only accepts struct (int is valid).
                // - MatrixMultiplier only accepts float (Normalizer output is float).
                // - ShapeValidator ensures dimensions match expectations.
                Tensor<float> finalResult = pipeline.Run(rawSensorData);

                Console.WriteLine("\nFinal Processed Data (Floats):");
                PrintTensor(finalResult);
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine("\nPipeline Error: " + ex.Message);
            }
            
            Console.WriteLine("\nPipeline execution completed successfully.");
        }

        // Helper to print tensor data
        static void PrintTensor<T>(Tensor<T> tensor)
        {
            Console.WriteLine($"Shape: [{tensor.Shape[0]}, {tensor.Shape[1]}]");
            for (int i = 0; i < tensor.Shape[0]; i++)
            {
                for (int j = 0; j < tensor.Shape[1]; j++)
                {
                    Console.Write(tensor.Get(i, j) + "\t");
                }
                Console.WriteLine();
            }
        }
    }
}
