
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

// Source File: theory_theoretical_foundations_part5.cs
// Description: Theoretical Foundations
// ==========================================

public static IEnumerable<Vector> NormalizeDataset(IEnumerable<Vector> dataset)
{
    // Materialize to calculate statistics (Immediate)
    var matrix = dataset.ToList();
    
    // Calculate Mean for each dimension
    int dimensions = matrix.First().Dimension;
    var means = Enumerable.Range(0, dimensions)
                          .Select(dim => matrix.Average(v => v.Coordinates[dim]))
                          .ToArray();

    // Calculate Standard Deviation for each dimension
    var stdDevs = Enumerable.Range(0, dimensions)
                            .Select(dim => 
                            {
                                var dimValues = matrix.Select(v => v.Coordinates[dim]).ToArray();
                                var mean = means[dim];
                                var sumSquares = dimValues.Sum(val => Math.Pow(val - mean, 2));
                                return Math.Sqrt(sumSquares / dimValues.Length);
                            })
                            .ToArray();

    // Apply transformation (Deferred until enumeration)
    return matrix.Select(v => 
    {
        var normalizedCoords = new double[v.Dimension];
        for (int i = 0; i < v.Dimension; i++)
        {
            // Avoid division by zero
            normalizedCoords[i] = stdDevs[i] == 0 ? 0 : (v.Coordinates[i] - means[i]) / stdDevs[i];
        }
        return new Vector(normalizedCoords);
    });
}
