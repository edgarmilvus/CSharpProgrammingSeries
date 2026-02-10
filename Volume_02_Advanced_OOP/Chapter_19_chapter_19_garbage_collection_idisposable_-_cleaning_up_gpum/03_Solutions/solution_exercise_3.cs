
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

public class GpuTensorBuffer : IDisposable
{
    private IntPtr _handle;
    public string Name { get; }

    public GpuTensorBuffer(string name, int size)
    {
        Name = name;
        _handle = new IntPtr(12345); // Dummy handle simulation
        Console.WriteLine($"[ALLOC] {Name}");
    }

    public void Dispose()
    {
        if (_handle != IntPtr.Zero)
        {
            Console.WriteLine($"[FREE]  {Name}");
            _handle = IntPtr.Zero;
        }
    }
}

public class NeuralNetworkSimulator
{
    public void RunTrainingStep_Nested()
    {
        Console.WriteLine("\n--- Starting Nested Using Test ---");
        try
        {
            using (var input = new GpuTensorBuffer("Input", 1024))
            {
                using (var weights = new GpuTensorBuffer("Weights", 4096))
                {
                    Console.WriteLine("Calculating forward pass...");
                    // Simulate an operation that fails
                    throw new InvalidOperationException("Calculation error: Gradient exploded!");
                    
                    // This line is never reached, so 'output' is never created
                    using (var output = new GpuTensorBuffer("Output", 1024))
                    {
                        // ...
                    }
                } // weights.Dispose() called here during stack unwinding
            } // input.Dispose() called here
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Caught Exception: {ex.Message}");
        }
    }

    public void RunTrainingStep_Clean()
    {
        Console.WriteLine("\n--- Starting Clean Using Test (C# 8.0+) ---");
        try
        {
            // Single using statement with multiple declarations
            // Resources are disposed in reverse order of declaration (weights, then input)
            using (var input = new GpuTensorBuffer("Input", 1024),
                        weights = new GpuTensorBuffer("Weights", 4096))
            {
                Console.WriteLine("Calculating forward pass...");
                throw new InvalidOperationException("Calculation error: Gradient exploded!");
            } 
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Caught Exception: {ex.Message}");
        }
    }
}

public class Program
{
    public static void Main()
    {
        var sim = new NeuralNetworkSimulator();
        
        // Test 1: Nested (Old Style)
        // The exception triggers unwinding, disposing 'weights', then 'input'.
        sim.RunTrainingStep_Nested();

        // Test 2: Clean (New Style)
        // Both 'input' and 'weights' are disposed even though an exception occurs.
        sim.RunTrainingStep_Clean();
    }
}
