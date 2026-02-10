
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

// Source File: theory_theoretical_foundations_part4.cs
// Description: Theoretical Foundations
// ==========================================

public static IEnumerable<(int IdA, int IdB, double Distance)> ComputeDistanceMatrix(
    IEnumerable<Vector> dataset, 
    Func<Vector, Vector, double> metric)
{
    // Materialize the dataset to avoid multiple enumeration
    // This is an IMMEDIATE execution step
    var dataPoints = dataset.Select((v, index) => new { Index = index, Vector = v })
                            .ToList();

    // This is a DEFERRED execution query
    // It generates a query plan but does not run it yet
    var query = 
        from a in dataPoints
        from b in dataPoints
        where a.Index < b.Index // Avoid duplicates and self-comparison
        select (a.Index, b.Index, metric(a.Vector, b.Vector));

    return query;
}
