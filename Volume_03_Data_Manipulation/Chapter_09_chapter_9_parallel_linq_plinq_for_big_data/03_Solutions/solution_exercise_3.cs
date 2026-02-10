
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

public class DataVector
{
    public int Id { get; set; }
    public double NormalizedValue { get; set; }
}

public class DataPreprocessor
{
    public static List<DataVector> ProcessLogs(List<string> logs)
    {
        return logs
            // 1. Parallelize the processing of the raw list
            .AsParallel()
            // 2. Transformation: Parse string into an anonymous type
            // This lambda is pure: it takes input and returns output without side effects.
            .Select(log => 
            {
                var parts = log.Split(':');
                return new 
                { 
                    Id = int.Parse(parts[0]), 
                    Val = double.Parse(parts[1]) 
                };
            })
            // 3. Filtering: Keep only non-negative values
            .Where(x => x.Val >= 0)
            // 4. Normalization: Create the final DataVector object
            .Select(x => new DataVector
            {
                Id = x.Id,
                NormalizedValue = x.Val / 100.0
            })
            // 5. Materialization: Convert to List
            .ToList();
    }
}
