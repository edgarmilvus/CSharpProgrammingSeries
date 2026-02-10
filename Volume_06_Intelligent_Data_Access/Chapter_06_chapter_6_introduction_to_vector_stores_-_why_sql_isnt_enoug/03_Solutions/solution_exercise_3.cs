
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

// Project: VectorMath
// File: VectorMathExtensions.cs

using System;
using System.Numerics; // Requires .NET 8 for simplified Vector<T> usage
using System.Runtime.CompilerServices;

namespace VectorMath
{
    public static class VectorMathExtensions
    {
        /// <summary>
        /// Calculates Cosine Similarity between two vectors using SIMD intrinsics for performance.
        /// Formula: (A . B) / (||A|| * ||B||)
        /// </summary>
        public static float CosineSimilarity(this ReadOnlySpan<float> vectorA, ReadOnlySpan<float> vectorB)
        {
            if (vectorA.Length != vectorB.Length)
                throw new ArgumentException("Vectors must be of the same length.");

            if (vectorA.Length == 0)
                throw new ArgumentException("Vectors cannot be empty.");

            // Use SIMD if the hardware supports it and the vector length is sufficient
            if (Vector.IsHardwareAccelerated && vectorA.Length >= Vector<float>.Count)
            {
                return CalculateSimd(vectorA, vectorB);
            }

            // Fallback to scalar calculation
            return CalculateScalar(vectorA, vectorB);
        }

        private static float CalculateScalar(ReadOnlySpan<float> a, ReadOnlySpan<float> b)
        {
            float dotProduct = 0f;
            float magnitudeA = 0f;
            float magnitudeB = 0f;

            for (int i = 0; i < a.Length; i++)
            {
                dotProduct += a[i] * b[i];
                magnitudeA += a[i] * a[i];
                magnitudeB += b[i] * b[i];
            }

            if (magnitudeA == 0f || magnitudeB == 0f)
                return 0f; // Avoid division by zero

            return dotProduct / (MathF.Sqrt(magnitudeA) * MathF.Sqrt(magnitudeB));
        }

        private static float CalculateSimd(ReadOnlySpan<float> a, ReadOnlySpan<float> b)
        {
            var dotProductVec = Vector<float>.Zero;
            var magnitudeAVec = Vector<float>.Zero;
            var magnitudeBVec = Vector<float>.Zero;

            int i = 0;
            int vectorWidth = Vector<float>.Count;

            // Process in chunks using SIMD
            for (; i <= a.Length - vectorWidth; i += vectorWidth)
            {
                var va = new Vector<float>(a.Slice(i, vectorWidth));
                var vb = new Vector<float>(b.Slice(i, vectorWidth));

                dotProductVec += va * vb;
                magnitudeAVec += va * va;
                magnitudeBVec += vb * vb;
            }

            // Horizontal reduction to get scalar values from Vectors
            float dotProduct = 0f;
            float magnitudeA = 0f;
            float magnitudeB = 0f;

            for (int j = 0; j < vectorWidth; j++)
            {
                dotProduct += dotProductVec[j];
                magnitudeA += magnitudeAVec[j];
                magnitudeB += magnitudeBVec[j];
            }

            // Process remaining elements
            for (; i < a.Length; i++)
            {
                dotProduct += a[i] * b[i];
                magnitudeA += a[i] * a[i];
                magnitudeB += b[i] * b[i];
            }

            if (magnitudeA == 0f || magnitudeB == 0f)
                return 0f;

            return dotProduct / (MathF.Sqrt(magnitudeA) * MathF.Sqrt(magnitudeB));
        }
    }
}

// Project: VectorMath.Tests (xUnit)
// File: VectorMathTests.cs

using Xunit;
using System;

namespace VectorMath.Tests
{
    public class VectorMathExtensionsTests
    {
        [Fact]
        public void CosineSimilarity_IdenticalVectors_ReturnsOne()
        {
            var v = new float[] { 1.0f, 2.0f, 3.0f };
            var similarity = v.CosineSimilarity(v);
            Assert.Equal(1.0f, similarity, 5);
        }

        [Fact]
        public void CosineSimilarity_OrthogonalVectors_ReturnsZero()
        {
            var a = new float[] { 1.0f, 0.0f, 0.0f };
            var b = new float[] { 0.0f, 1.0f, 0.0f };
            var similarity = a.CosineSimilarity(b);
            Assert.Equal(0.0f, similarity, 5);
        }

        [Fact]
        public void CosineSimilarity_DifferentLengths_ThrowsException()
        {
            var a = new float[] { 1.0f, 2.0f };
            var b = new float[] { 1.0f };
            Assert.Throws<ArgumentException>(() => a.CosineSimilarity(b));
        }
        
        [Fact]
        public void CosineSimilarity_ZeroVectors_ReturnsZero()
        {
            var a = new float[] { 0.0f, 0.0f };
            var b = new float[] { 0.0f, 0.0f };
            var similarity = a.CosineSimilarity(b);
            Assert.Equal(0.0f, similarity, 5);
        }
    }
}
