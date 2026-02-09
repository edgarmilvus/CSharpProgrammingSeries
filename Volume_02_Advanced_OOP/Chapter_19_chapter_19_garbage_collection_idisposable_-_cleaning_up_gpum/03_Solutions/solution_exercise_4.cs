
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

# Source File: solution_exercise_4.cs
# Description: Solution for Exercise 4
# ==========================================

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

// --- 1. The Unmanaged Buffer Wrapper ---
public class GpuTensorBuffer : IDisposable
{
    private IntPtr _devicePointer;
    private bool _disposed = false;

    public GpuTensorBuffer(int sizeInBytes)
    {
        _devicePointer = Marshal.AllocHGlobal(sizeInBytes);
        Console.WriteLine($"[GPU] Allocated buffer at {_devicePointer}");
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected void Dispose(bool disposing)
    {
        if (_disposed) return;
        if (_devicePointer != IntPtr.Zero)
        {
            Marshal.FreeHGlobal(_devicePointer);
            Console.WriteLine($"[GPU] Freed buffer at {_devicePointer}");
            _devicePointer = IntPtr.Zero;
        }
        _disposed = true;
    }

    ~GpuTensorBuffer() { Dispose(false); }
}

// --- 2. The Managed Tensor with Reference Counting ---
public class Tensor : IDisposable
{
    public string Name { get; }
    private GpuTensorBuffer _buffer;
    private int _refCount = 0;
    private bool _disposed = false;

    public Tensor(string name, int size)
    {
        Name = name;
        _buffer = new GpuTensorBuffer(size);
        // Start at 0, waiting for first usage to increment
    }

    // Called when a node starts using this tensor
    public void IncrementRefCount()
    {
        _refCount++;
        Console.WriteLine($"[TENSOR {Name}] RefCount incremented to {_refCount}");
    }

    // Called when a node is disposed or stops using this tensor
    public void DecrementRefCount()
    {
        _refCount--;
        Console.WriteLine($"[TENSOR {Name}] RefCount decremented to {_refCount}");
        
        // If ref count hits 0 and we haven't disposed yet, clean up.
        if (_refCount <= 0 && !_disposed)
        {
            Console.WriteLine($"[TENSOR {Name}] RefCount 0. Disposing underlying buffer.");
            Dispose();
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        
        // Dispose the unmanaged buffer
        _buffer?.Dispose();
        _disposed = true;
    }
}

// --- 3. The Node in the Graph ---
public class TensorNode : IDisposable
{
    public string Name { get; }
    private List<Tensor> _inputs = new List<Tensor>();
    private Tensor _output;

    public TensorNode(string name)
    {
        Name = name;
    }

    public void AddInput(Tensor input)
    {
        _inputs.Add(input);
        input.IncrementRefCount();
    }

    public void SetOutput(Tensor output)
    {
        _output = output;
        // Output is created by this node, so we increment its count
        output.IncrementRefCount();
    }

    // The core cleanup logic for the graph
    public void Dispose()
    {
        Console.WriteLine($"\n[DISPOSING NODE] {Name}");
        
        // 1. Decrement ref counts for inputs
        // If an input is used by no other node, it will dispose itself here.
        foreach (var input in _inputs)
        {
            input.DecrementRefCount();
        }

        // 2. Dispose the output tensor associated with this node
        if (_output != null)
        {
            _output.DecrementRefCount(); 
        }
    }
}

// --- 4. Usage Scenario ---
public class Program
{
    public static void Main()
    {
        Console.WriteLine("=== Building Computational Graph ===");
        
        // Create Shared Input (e.g., an image batch)
        var inputTensor = new Tensor("InputImage", 1024);

        // Create Nodes
        var convNode = new TensorNode("ConvolutionLayer");
        var reluNode = new TensorNode("ReLULayer");
        var addNode = new TensorNode("AddLayer (Skip Connection)");

        // Build Graph Connections
        
        // 1. Convolution takes Input
        convNode.AddInput(inputTensor); 
        
        // Output of Conv is input to ReLU
        var convOutput = new Tensor("ConvOutput", 2048);
        convNode.SetOutput(convOutput);
        reluNode.AddInput(convOutput); // ReLU owns the output of Conv

        // 2. Add Layer takes Input (Skip) AND ReLU Output
        var reluOutput = new Tensor("ReLUOutput", 2048);
        reluNode.SetOutput(reluOutput);
        
        addNode.AddInput(inputTensor); // Shared reference!
        addNode.AddInput(reluOutput);

        var finalOutput = new Tensor("FinalOutput", 2048);
        addNode.SetOutput(finalOutput);

        Console.WriteLine("\n=== Graph Construction Complete ===");
        // Input: 1 (Conv) + 1 (Add) = 2
        // ConvOutput: 1 (ReLU)
        // ReLUOutput: 1 (Add)

        Console.WriteLine("\n=== Simulating Training End & Cleanup ===");
        
        // Cleanup happens in reverse order of creation usually, 
        // or triggered by disposing the "Root" or "Loss" node.
        
        // Let's dispose the AddNode first.
        addNode.Dispose();
        // Input RefCount: 2 -> 1 (Still alive)
        // ReLUOutput RefCount: 1 -> 0 (Disposed)

        Console.WriteLine("\n--- Disposing ReLU Node ---");
        reluNode.Dispose();
        // ConvOutput RefCount: 1 -> 0 (Disposed)

        Console.WriteLine("\n--- Disposing Conv Node ---");
        convNode.Dispose();
        // Input RefCount: 1 -> 0 (Disposed)
        
        Console.WriteLine("\n=== All Resources Cleaned Up ===");
    }
}
