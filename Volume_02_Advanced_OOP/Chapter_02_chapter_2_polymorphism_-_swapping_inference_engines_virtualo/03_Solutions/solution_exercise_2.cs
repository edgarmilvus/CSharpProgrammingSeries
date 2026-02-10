
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
using System.Linq; // Allowed for simplicity, but logic can be manual loops

// Base Class
public class TensorOperation
{
    // 1. Virtual method with base implementation (Sum)
    public virtual double Compute(double[] data)
    {
        double sum = 0;
        foreach (double val in data)
        {
            sum += val;
        }
        return sum;
    }
}

// 2. Derived Class: DenseTensor
public class DenseTensor : TensorOperation
{
    // Override to calculate Average
    public override double Compute(double[] data)
    {
        double sum = 0;
        foreach (double val in data)
        {
            sum += val;
        }
        return sum / data.Length;
    }
}

// 3. Derived Class: SparseTensor
public class SparseTensor : TensorOperation
{
    // Override to calculate Max
    public override double Compute(double[] data)
    {
        if (data.Length == 0) return 0;
        
        double max = data[0];
        foreach (double val in data)
        {
            if (val > max) max = val;
        }
        return max;
    }
}

// Challenge: Benchmark Method
public class TensorBenchmark
{
    public void Benchmark(TensorOperation op, double[] data)
    {
        double result = op.Compute(data);
        Console.WriteLine($"Result: {result}");
    }
}

// Main Program
public class Program
{
    public static void Main()
    {
        double[] testData = { 1.0, 2.0, 3.0, 4.0 };
        
        TensorBenchmark bench = new TensorBenchmark();

        // Test Base Class
        Console.Write("Base (Sum): ");
        bench.Benchmark(new TensorOperation(), testData);

        // Test DenseTensor (Average)
        Console.Write("Dense (Avg): ");
        bench.Benchmark(new DenseTensor(), testData);

        // Test SparseTensor (Max)
        Console.Write("Sparse (Max): ");
        bench.Benchmark(new SparseTensor(), testData);
        
        // --- DEMONSTRATION OF 'new' KEYWORD ISSUE ---
        Console.WriteLine("\n--- Testing 'new' keyword issue ---");
        DenseTensorNew dtNew = new DenseTensorNew();
        // Casting to base class reference
        TensorOperation opRef = dtNew;
        
        Console.Write("Using 'new' (Base Ref): ");
        // This will call TensorOperation.Compute (Sum), NOT the DenseTensorNew logic!
        bench.Benchmark(opRef, testData); 
    }
}

// Class used to demonstrate the 'new' keyword issue
public class DenseTensorNew : TensorOperation
{
    // Hiding the base method, NOT overriding it
    public new double Compute(double[] data)
    {
        double sum = 0;
        foreach (double val in data) sum += val;
        return sum / data.Length;
    }
}
