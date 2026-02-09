
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

# Source File: basic_basic_code_example.cs
# Description: Basic Code Example
# ==========================================

using System;
using System.Numerics; // Required for Vector<T> (SIMD)

namespace HighPerformanceAI
{
    // Represents a vector of floats for AI embeddings.
    // We mark it as 'readonly struct' to guarantee immutability.
    // This allows the compiler to safely pass references to this struct
    // instead of copying it, provided we also use 'in' parameters.
    public readonly struct EmbeddingVector
    {
        private readonly float[] _values;

        public EmbeddingVector(float[] values)
        {
            // Defensive copy is necessary here during construction, 
            // but we want to avoid copies during processing.
            _values = values;
        }

        public int Length => _values.Length;

        // Indexer to access elements.
        // 'readonly' modifier ensures this method does not modify struct state.
        public readonly float this[int index] => _values[index];

        // Calculates the dot product with another vector.
        // 'in' parameter avoids copying the 'other' struct.
        // 'readonly' modifier on the method ensures we don't modify 'this'.
        public readonly float DotProduct(in EmbeddingVector other)
        {
            if (Length != other.Length)
                throw new ArgumentException("Vectors must be of the same length.");

            float sum = 0f;
            
            // In a real high-performance scenario, we would use Vector<T> (SIMD) here.
            // For this basic example, we use a simple loop to demonstrate the mechanics.
            for (int i = 0; i < Length; i++)
            {
                sum += this[i] * other[i];
            }

            return sum;
        }
    }

    class Program
    {
        static void Main()
        {
            // 1. Setup data
            float[] dataA = new float[1000];
            float[] dataB = new float[1000];
            
            // Fill with dummy data
            var random = new Random();
            for(int i = 0; i < 1000; i++)
            {
                dataA[i] = (float)random.NextDouble();
                dataB[i] = (float)random.NextDouble();
            }

            // 2. Create struct instances
            // Note: Structs are value types. These are allocated on the stack (if local)
            // or inline inside the containing type.
            var vectorA = new EmbeddingVector(dataA);
            var vectorB = new EmbeddingVector(dataB);

            // 3. Perform calculation
            // Without 'in', 'vectorB' would be copied entirely into the DotProduct method.
            // With 'in', we pass a reference (effectively a pointer) to vectorB.
            float similarity = vectorA.DotProduct(in vectorB);

            Console.WriteLine($"Dot Product Result: {similarity:F4}");
        }
    }
}
