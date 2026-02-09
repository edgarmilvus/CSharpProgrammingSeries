
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

# Source File: solution_exercise_3.cs
# Description: Solution for Exercise 3
# ==========================================

using System;

// BEFORE (The problem):
public interface ITensorOperation
{
    object Add(object other); // Boxing occurs on input and output
}

public class TensorOld : ITensorOperation
{
    private object[] _data; 
    
    public TensorOld(int size) { _data = new object[size]; }
    
    public object Add(object other)
    {
        // Complex logic to check types, cast, add, and box result
        return 0; 
    }
}

// AFTER (The solution):
public interface IOptimizedTensor
{
    double GetScalar(int index);
    void SetScalar(int index, double value);
    void AddInPlace(double[] otherData);
}

public class OptimizedTensor : IOptimizedTensor
{
    // Primitive array: No boxing!
    private double[] _data;

    public OptimizedTensor(int size)
    {
        _data = new double[size];
    }

    public double GetScalar(int index)
    {
        // Direct memory access, no unboxing
        return _data[index];
    }

    public void SetScalar(int index, double value)
    {
        // Direct memory access, no boxing
        _data[index] = value;
    }

    public void AddInPlace(double[] otherData)
    {
        // Direct arithmetic on primitives
        for (int i = 0; i < _data.Length; i++)
        {
            _data[i] += otherData[i];
        }
    }
}

public class RefactorDemo
{
    public static void Main()
    {
        OptimizedTensor t1 = new OptimizedTensor(100);
        OptimizedTensor t2 = new OptimizedTensor(100);

        // Fill data
        for (int i = 0; i < 100; i++)
        {
            t1.SetScalar(i, 1.0);
            t2.SetScalar(i, 2.0);
        }

        // Perform addition
        // We extract the raw array for the operation to avoid passing the object itself
        double[] buffer = new double[100];
        for(int i=0; i<100; i++) buffer[i] = t2.GetScalar(i);

        t1.AddInPlace(buffer);
        
        Console.WriteLine($"Result at index 0: {t1.GetScalar(0)}"); 
    }
}
