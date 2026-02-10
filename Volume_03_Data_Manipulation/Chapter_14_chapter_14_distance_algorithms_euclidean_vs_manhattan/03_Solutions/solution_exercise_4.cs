
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

public class KNN
{
    public record DataPoint(int Id, string Label, List<double> Features);

    public static string Predict(
        List<double> query, 
        IEnumerable<DataPoint> dataset, 
        int k)
    {
        // 1. Data Cleaning: Filter out points containing NaN in any feature.
        // Using 'All' ensures strict validation of the vector.
        var cleanDataset = dataset.Where(dp => dp.Features.All(f => !double.IsNaN(f)));

        // 2. Distance Calculation & Projection:
        // We project to an anonymous type containing the Label and the calculated Distance.
        // This separates the distance calculation from the sorting logic.
        var distances = cleanDataset
            .Select(dp => new 
            { 
                Label = dp.Label, 
                // Local function or inline lambda for Euclidean distance
                Distance = Math.Sqrt(query.Zip(dp.Features, (q, f) => Math.Pow(q - f, 2)).Sum())
            });

        // 3. Neighbor Selection:
        // Order by distance and take the top k. 
        // ToList() is immediate execution here to materialize the neighbors for the vote.
        var nearestNeighbors = distances
            .OrderBy(d => d.Distance)
            .Take(k)
            .ToList();

        // 4. Majority Vote:
        // Group by label, order by group size descending, select the key.
        var prediction = nearestNeighbors
            .GroupBy(n => n.Label)
            .OrderByDescending(g => g.Count())
            .Select(g => g.Key)
            .FirstOrDefault();

        return prediction ?? "Unknown"; // Handle empty dataset or k=0
    }
}
