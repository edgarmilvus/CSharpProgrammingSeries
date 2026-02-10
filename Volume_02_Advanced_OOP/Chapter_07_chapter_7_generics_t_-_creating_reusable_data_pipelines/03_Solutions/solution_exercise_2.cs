
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

// 1. Generic Interface
public interface Transformer<I, O>
{
    O Transform(I input);
}

// 2. Concrete Implementation
public class ScalingTransformer : Transformer<double, double>
{
    private double _factor;

    public ScalingTransformer(double factor)
    {
        _factor = factor;
    }

    public double Transform(double input)
    {
        return input * _factor;
    }
}

// Helper DataPoint class (reused from Exercise 1)
public class DataPoint<T>
{
    public T Value { get; set; }
    public DataPoint(T value) { Value = value; }
}

public class Exercise2Runner
{
    // 3. Wildcard Challenge Method
    // In C#, we simulate 'List<? extends DataPoint<Double>>' by accepting List<DataPoint<double>>.
    // Since List<T> is invariant in C#, we cannot pass a List<NumericalDataPoint> directly 
    // unless we cast it or use IEnumerable<T> (which is covariant). 
    // For this exercise, we stick to the concrete List<DataPoint<double>> to ensure we can modify if needed.
    
    public static void ProcessBatch(List<DataPoint<double>> dataPoints, Transformer<double, double> transformer)
    {
        Console.WriteLine("--- Processing Batch ---");
        // Standard loop as per constraints (no LINQ/Streams)
        for (int i = 0; i < dataPoints.Count; i++)
        {
            double original = dataPoints[i].Value;
            double transformed = transformer.Transform(original);
            Console.WriteLine($"Original: {original} -> Transformed: {transformed}");
        }
    }

    public static void Run()
    {
        var transformer = new ScalingTransformer(0.5);
        
        // Create a list of DataPoint<double>
        var dataset = new List<DataPoint<double>>
        {
            new DataPoint<double>(10.0),
            new DataPoint<double>(20.0),
            new DataPoint<double>(30.0)
        };

        ProcessBatch(dataset, transformer);
    }
}
