
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

// 1. Generic DataPoint class
public class DataPoint<T>
{
    public T Value { get; set; }

    public DataPoint(T value)
    {
        this.Value = value;
    }

    public string Describe()
    {
        return $"DataPoint of type {typeof(T).Name}: {Value}";
    }
}

// 3. Subclass with specific type constraint
// NumericalDataPoint inherits from DataPoint<double>, locking the generic type to double
public class NumericalDataPoint : DataPoint<double>
{
    public NumericalDataPoint(double value) : base(value) { }

    public double Normalize()
    {
        // Simple min-max normalization (assuming range 0-100)
        double max = 100.0;
        if (Value > max) return 1.0;
        if (Value < 0) return 0.0;
        return Value / max;
    }
}

// Usage Example
public class Exercise1Runner
{
    public static void Run()
    {
        // Generic usage with a string type
        DataPoint<string> textPoint = new DataPoint<string>("High Risk");
        Console.WriteLine(textPoint.Describe());

        // Specific subclass usage
        NumericalDataPoint numPoint = new NumericalDataPoint(75.5);
        Console.WriteLine(numPoint.Describe());
        Console.WriteLine($"Normalized Value: {numPoint.Normalize()}");
    }
}
