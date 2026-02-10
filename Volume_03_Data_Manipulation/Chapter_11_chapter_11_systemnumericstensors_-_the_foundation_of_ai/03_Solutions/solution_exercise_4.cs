
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

// Source File: solution_exercise_4.cs
// Description: Solution for Exercise 4
// ==========================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics.Tensors;

public static class TensorMath
{
    public static float CalculateFunctionalDotProduct(
        IEnumerable<float> streamA, 
        IEnumerable<float> streamB)
    {
        // 1. ALIGNMENT: Functional handling of variable lengths.
        // We materialize streamA to determine the target length (Length A).
        var listA = streamA.ToList();
        int targetLength = listA.Count;

        // We process streamB:
        // - Take(targetLength): Truncates if B is longer than A.
        // - Concat(Repeat(0f, ...)): Pads with zeros if B is shorter than A.
        var alignedB = streamB
            .Take(targetLength)
            .Concat(Enumerable.Repeat(0.0f, Math.Max(0, targetLength - streamB.Count())))
            .ToArray(); // Materialize to Array for Tensor creation

        // 2. TENSOR CREATION: Convert aligned data to Tensors.
        // We assume a dense memory layout.
        // Note: Tensor.Create is used here to wrap the underlying array memory.
        var tensorA = Tensor.Create(listA.ToArray(), targetLength);
        var tensorB = Tensor.Create(alignedB, targetLength);

        // 3. HARDWARE ACCELERATION:
        // Perform element-wise multiplication. This utilizes SIMD (AVX/AVX2) 
        // or GPU backends if configured (e.g., via providers like oneDNN or CUDA).
        var productTensor = Tensor.Multiply(tensorA, tensorB);

        // 4. REDUCTION: Sum the elements of the resulting tensor.
        float dotProduct = Tensor.Sum(productTensor);

        return dotProduct;
    }
}
