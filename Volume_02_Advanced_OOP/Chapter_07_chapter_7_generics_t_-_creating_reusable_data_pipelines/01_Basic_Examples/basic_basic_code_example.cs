
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

// A generic interface representing a component that transforms data.
// <TInput>: The type of data entering the transformer.
// <TOutput>: The type of data leaving the transformer.
public interface ITransformer<TInput, TOutput>
{
    TOutput Transform(TInput input);
}

// A concrete implementation of the transformer.
// We use 'where' constraints to ensure TInput is a value type (struct) 
// and TOutput is a class, ensuring memory safety.
public class NumericNormalizer<TInput, TOutput> : ITransformer<TInput, TOutput>
    where TInput : struct // Constraint: TInput must be a value type (e.g., int, double, float)
    where TOutput : class // Constraint: TOutput must be a reference type (e.g., string, byte[])
{
    private readonly TOutput _placeholderValue;

    // Constructor to initialize the transformer with a default output value.
    public NumericNormalizer(TOutput placeholderValue)
    {
        _placeholderValue = placeholderValue;
    }

    public TOutput Transform(TInput input)
    {
        // In a real AI scenario, this would perform complex normalization math.
        // Here, we simply check if the input is a number and return the placeholder.
        // We use runtime type checking because we cannot perform arithmetic 
        // on generic types 'TInput' directly without constraints like 'where TInput : INumber'.
        
        if (typeof(TInput) == typeof(int))
        {
            int val = (int)(object)input;
            Console.WriteLine($"Normalizing integer: {val}");
            // Return the placeholder cast to TOutput (simulating a processed byte array or string)
            return _placeholderValue;
        }
        
        throw new NotSupportedException("This example only supports integers for simplicity.");
    }
}

// Main application to demonstrate the pipeline.
public class Program
{
    public static void Main()
    {
        // 1. Create a transformer that takes an 'int' and outputs a 'string' (representing a processed feature).
        // This mimics converting raw sensor data (int) into a normalized string descriptor.
        ITransformer<int, string> sensorProcessor = new NumericNormalizer<int, string>("Normalized_Feature_Vector");

        // 2. Process a raw data point.
        int rawSensorValue = 42;
        string processedFeature = sensorProcessor.Transform(rawSensorValue);

        Console.WriteLine($"Input: {rawSensorValue}, Output: {processedFeature}");

        // 3. Attempting to use an incompatible type will cause a compile-time error.
        // Uncommenting the line below will fail because 'sensorProcessor' expects an 'int', not a 'double'.
        // sensorProcessor.Transform(3.14); 
    }
}
