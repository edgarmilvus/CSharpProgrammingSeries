
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

using System;
using System.Collections.Generic;

namespace MLEngine.GenericConstraints
{
    // 1. Base class and subclasses
    public abstract class Prediction { }

    public class BinaryPrediction : Prediction 
    { 
        public bool Result { get; set; } 
    }

    public class RegressionPrediction : Prediction 
    { 
        public double Value { get; set; } 
    }

    // 2. Prediction Aggregator
    public class PredictionAggregator
    {
        // 3. Method accepting Covariant interface (IReadOnlyList<out T>)
        public int ProcessPredictions(IReadOnlyList<Prediction> predictions)
        {
            int count = 0;
            foreach (var p in predictions) 
            {
                count++;
            }
            return count;
        }

        // 4. Overloaded method for specific type
        public int ProcessBinaryPredictions(IReadOnlyList<BinaryPrediction> predictions)
        {
            int trueCount = 0;
            foreach (var p in predictions)
            {
                if (p.Result) trueCount++;
            }
            return trueCount;
        }
    }

    public class Exercise2Runner
    {
        public static void Run()
        {
            var aggregator = new PredictionAggregator();

            var binaryList = new List<BinaryPrediction>
            {
                new BinaryPrediction { Result = true },
                new BinaryPrediction { Result = false }
            };

            // Implicit covariance: List<BinaryPrediction> is assignable to IReadOnlyList<Prediction>
            int totalCount = aggregator.ProcessPredictions(binaryList);
            Console.WriteLine($"Total Predictions: {totalCount}");

            int trueCount = aggregator.ProcessBinaryPredictions(binaryList);
            Console.WriteLine($"True Binary Predictions: {trueCount}");
        }
    }
}
