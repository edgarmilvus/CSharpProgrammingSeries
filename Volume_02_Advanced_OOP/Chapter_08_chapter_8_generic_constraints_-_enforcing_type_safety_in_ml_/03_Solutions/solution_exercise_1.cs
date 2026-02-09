
#
# These sources are part of the "C# Programming Series" by Edgar Milvus, 
# you can find it on stores: 
# 
# https://www.amazon.com/dp/B0GKJ3NYL6 or https://tinyurl.com/CSharpProgrammingBooks or 
# https://leanpub.com/u/edgarmilvus (quantity discounts)
# 
# New books info: https://linktr.ee/edgarmilvus 
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

# Source File: solution_exercise_1.cs
# Description: Solution for Exercise 1
# ==========================================

using System;
using System.Collections.Generic;

namespace MLEngine.GenericConstraints
{
    // 1. Define the custom constraint interface
    public interface INumber
    {
        // Method to convert the underlying type to double for calculation
        double ToDouble();
    }

    // Wrapper for int to satisfy INumber
    public struct IntWrapper : INumber
    {
        public int Value;
        public IntWrapper(int val) { Value = val; }
        public double ToDouble() => (double)Value;
    }

    // Wrapper for double to satisfy INumber
    public struct DoubleWrapper : INumber
    {
        public double Value;
        public DoubleWrapper(double val) { Value = val; }
        public double ToDouble() => Value;
    }

    // 2. Generic Processor with Constraint
    public class DataStreamProcessor<T> where T : INumber
    {
        private readonly List<T> _dataStream;

        public DataStreamProcessor()
        {
            _dataStream = new List<T>();
        }

        public void AddData(T dataPoint)
        {
            _dataStream.Add(dataPoint);
        }

        // 3. Calculate Statistics returning a tuple
        public (double Sum, double Average) CalculateStatistics()
        {
            if (_dataStream.Count == 0)
                return (0.0, 0.0);

            double sum = 0.0;
            
            // Iterating without LINQ or Lambdas
            for (int i = 0; i < _dataStream.Count; i++)
            {
                // Safe conversion via the interface
                sum += _dataStream[i].ToDouble();
            }

            return (sum, sum / _dataStream.Count);
        }
    }

    public class Exercise1Runner
    {
        public static void Run()
        {
            // 4. Demonstrate usage with int
            var intProcessor = new DataStreamProcessor<IntWrapper>();
            intProcessor.AddData(new IntWrapper(10));
            intProcessor.AddData(new IntWrapper(20));
            var intStats = intProcessor.CalculateStatistics();
            Console.WriteLine($"Int Stats - Sum: {intStats.Sum}, Avg: {intStats.Average}");

            // 4. Demonstrate usage with double
            var doubleProcessor = new DataStreamProcessor<DoubleWrapper>();
            doubleProcessor.AddData(new DoubleWrapper(5.5));
            doubleProcessor.AddData(new DoubleWrapper(10.5));
            var doubleStats = doubleProcessor.CalculateStatistics();
            Console.WriteLine($"Double Stats - Sum: {doubleStats.Sum}, Avg: {doubleStats.Average}");

            // 4. Compilation failure for string (Uncomment to test)
            // var stringProcessor = new DataStreamProcessor<string>();
            // Error: The type 'string' must be convertible to 'INumber'.
        }
    }
}
