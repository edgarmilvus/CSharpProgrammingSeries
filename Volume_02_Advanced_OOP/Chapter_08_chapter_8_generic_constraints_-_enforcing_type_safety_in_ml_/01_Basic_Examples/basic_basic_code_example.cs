
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

# Source File: basic_basic_code_example.cs
# Description: Basic Code Example
# ==========================================

using System;
using System.Collections.Generic;

// 1. Define a generic interface for our data processors.
// This enforces that any processor must implement a 'Process' method.
public interface IProcessor<T>
{
    T Process(T input);
}

// 2. Create a specific processor for Temperature data.
// We use a generic constraint ': IComparable<T>' to ensure we can compare values.
public class TemperatureValidator<T> : IProcessor<T> where T : IComparable<T>
{
    private readonly T _minThreshold;
    private readonly T _maxThreshold;

    public TemperatureValidator(T min, T max)
    {
        _minThreshold = min;
        _maxThreshold = max;
    }

    // The 'where T : IComparable<T>' constraint allows us to use .CompareTo().
    public T Process(T input)
    {
        // Check if input is less than min
        if (input.CompareTo(_minThreshold) < 0)
        {
            Console.WriteLine($"Warning: Value {input} is below minimum. Returning min.");
            return _minThreshold;
        }

        // Check if input is greater than max
        if (input.CompareTo(_maxThreshold) > 0)
        {
            Console.WriteLine($"Warning: Value {input} is above maximum. Returning max.");
            return _maxThreshold;
        }

        return input;
    }
}

// 3. Define a pipeline class to chain operations.
// We use a generic constraint 'where T : struct' to ensure value types (like numbers).
public class Pipeline<T> where T : struct
{
    private readonly List<IProcessor<T>> _processors = new List<IProcessor<T>>();

    public void AddProcessor(IProcessor<T> processor)
    {
        _processors.Add(processor);
    }

    public T Run(T initialValue)
    {
        T currentValue = initialValue;
        
        // Iterate through the list of processors (Allowed: foreach loop)
        foreach (var processor in _processors)
        {
            currentValue = processor.Process(currentValue);
        }
        
        return currentValue;
    }
}

// 4. Main execution
public class Program
{
    public static void Main()
    {
        // Context: We are reading sensor data. 
        // We want to clamp values between 0.0 and 100.0.
        
        // Initialize the pipeline with Double type
        var pipeline = new Pipeline<double>();
        
        // Add a validator (Constraint: T must be IComparable<double>)
        pipeline.AddProcessor(new TemperatureValidator<double>(0.0, 100.0));

        // --- Test Cases ---
        
        Console.WriteLine("--- Processing Sensor Data ---");
        
        // Case A: Normal value
        double safeData = 50.5;
        double resultA = pipeline.Run(safeData);
        Console.WriteLine($"Input: {safeData}, Final Output: {resultA}\n");

        // Case B: Value exceeding max threshold
        double highData = 150.0;
        double resultB = pipeline.Run(highData);
        Console.WriteLine($"Input: {highData}, Final Output: {resultB}\n");

        // Case C: Value below min threshold
        double lowData = -10.0;
        double resultC = pipeline.Run(lowData);
        Console.WriteLine($"Input: {lowData}, Final Output: {resultC}\n");
    }
}
