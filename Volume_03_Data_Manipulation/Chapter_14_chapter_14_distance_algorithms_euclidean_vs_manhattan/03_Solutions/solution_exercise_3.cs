
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
using System.Collections.Generic;
using System.Linq;

public class MatrixOperations
{
    public static IEnumerable<double> Normalize(IEnumerable<double> vector)
    {
        // Calculate L2 norm (magnitude)
        var norm = Math.Sqrt(vector.Sum(x => x * x));
        
        // Handle zero vector to avoid division by zero
        if (norm == 0) return vector;
        
        return vector.Select(x => x / norm);
    }

    public static double[][] ComputeDistanceMatrix(List<List<double>> dataset)
    {
        // CRITICAL: Materialize the normalized dataset.
        // If we kept this as IEnumerable, the normalization calculation 
        // would re-run every time a vector is accessed in the nested loops below.
        var normalizedData = dataset.Select(v => Normalize(v).ToList()).ToList();

        int n = normalizedData.Count;

        // Use PLINQ for the outer loop (rows) to parallelize computation.
        return normalizedData
            .AsParallel()
            .Select((currentVector, i) =>
                // Inner loop uses standard LINQ to calculate distances against other vectors.
                // We skip the lower triangle (i+1) to avoid redundant calculations (distance(A,B) == distance(B,A)).
                normalizedData
                    .Skip(i + 1)
                    .Select(other => Euclidean(currentVector, other))
                    .ToArray()
            )
            .ToArray();
    }

    // Helper method reused from Exercise 1
    private static double Euclidean(IEnumerable<double> a, IEnumerable<double> b)
    {
        return Math.Sqrt(a.Zip(b, (x, y) => (x - y) * (x - y)).Sum());
    }
}
