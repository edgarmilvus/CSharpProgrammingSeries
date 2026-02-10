
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

// Source File: solution_exercise_2.cs
// Description: Solution for Exercise 2
// ==========================================

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public static class Normalizer
{
    public static IEnumerable<PreprocessedText> NormalizeScores(IEnumerable<PreprocessedText> data)
    {
        // Step 1: Materialize data for statistical analysis.
        // We use Immediate Execution here (.ToList()) to ensure we have a fixed dataset
        // for calculating Min/Max. This prevents re-enumeration of the source.
        var materializedData = data.ToList();

        if (!materializedData.Any())
            return Enumerable.Empty<PreprocessedText>();

        // Calculate statistics (Immediate Execution)
        // We extract the specific property 'NormalizedScore' for aggregation.
        float minScore = materializedData.Min(d => d.NormalizedScore);
        float maxScore = materializedData.Max(d => d.NormalizedScore);
        
        // Calculate range, handling the edge case where all values are identical.
        float range = maxScore - minScore;
        if (range == 0) range = 1.0f;

        // Step 2: Parallel Transformation.
        // We use AsParallel() to distribute the CPU-bound calculation across cores.
        return materializedData
            .AsParallel()
            // Using 'with' expression to create a new instance (Immutability).
            .Select(d => d with 
            { 
                NormalizedScore = (d.NormalizedScore - minScore) / range 
            });
    }
}
