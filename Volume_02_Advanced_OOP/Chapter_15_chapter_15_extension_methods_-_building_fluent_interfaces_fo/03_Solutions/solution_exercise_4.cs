
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
using System.Diagnostics;
using System.Linq;

// --- 1. Updated Tensor Class (Simulating Legacy + New Features) ---
namespace LegacyTensorLib
{
    public class Tensor
    {
        public double[] Data { get; private set; }
        public int Rows { get; }
        public int Cols { get; }

        public Tensor(double[] data, int rows, int cols)
        {
            Data = data;
            Rows = rows;
            Cols = cols;
        }

        // Simulating Softmax
        public Tensor Softmax()
        {
            var expData = Data.Select(x => Math.Exp(x)).ToArray();
            double sum = expData.Sum();
            var result = expData.Select(x => x / sum).ToArray();
            return new Tensor(result, Rows, Cols);
        }

        // New Validation Method
        public void Validate()
        {
            if (Data.Any(double.IsNaN))
            {
                throw new InvalidOperationException("Tensor contains NaN values.");
            }
            Console.WriteLine("Validation Passed.");
        }
    }
}

// --- 2. Tensor Pipeline Class ---
public class TensorPipeline
{
    private readonly List<Func<LegacyTensorLib.Tensor, LegacyTensorLib.Tensor>> _operations;

    public TensorPipeline()
    {
        _operations = new List<Func<LegacyTensorLib.Tensor, LegacyTensorLib.Tensor>>();
    }

    // Internal method to add a raw operation
    public void Add(Func<LegacyTensorLib.Tensor, LegacyTensorLib.Tensor> operation)
    {
        _operations.Add(operation);
    }

    // Execute the chain
    public LegacyTensorLib.Tensor Execute(LegacyTensorLib.Tensor input)
    {
        var current = input;
        foreach (var op in _operations)
        {
            current = op(current);
        }
        return current;
    }
}

// --- 3. Extension Methods for the Pipeline ---
public static class PipelineExtensions
{
    // Adds a step with logging and timing
    public static TensorPipeline AddStep(this TensorPipeline pipeline, Func<LegacyTensorLib.Tensor, LegacyTensorLib.Tensor> step, string stepName)
    {
        pipeline.Add(input =>
        {
            var sw = Stopwatch.StartNew();
            var result = step(input);
            sw.Stop();
            Console.WriteLine($"[Log] Step '{stepName}' executed in {sw.ElapsedMilliseconds}ms.");
            return result;
        });
        return pipeline;
    }

    // Adds a validation step
    public static TensorPipeline AddValidation(this TensorPipeline pipeline)
    {
        pipeline.Add(input =>
        {
            input.Validate(); // This might throw
            return input;
        });
        return pipeline;
    }
}

// --- 4. Tensor Extensions (Reused from Exercise 1) ---
namespace AIChainExtensions
{
    public static class TensorExtensions
    {
        public static LegacyTensorLib.Tensor Normalize(this LegacyTensorLib.Tensor tensor)
        {
            double sum = tensor.Data.Sum();
            if (Math.Abs(sum) < 1e-9) return tensor;
            var result = tensor.Data.Select(x => x / sum).ToArray();
            return new LegacyTensorLib.Tensor(result, tensor.Rows, tensor.Cols);
        }

        public static LegacyTensorLib.Tensor Clip(this LegacyTensorLib.Tensor tensor, double min, double max)
        {
            var result = tensor.Data.Select(x => Math.Max(min, Math.Min(max, x))).ToArray();
            return new LegacyTensorLib.Tensor(result, tensor.Rows, tensor.Cols);
        }
    }
}

// --- 5. Usage ---
public class Program
{
    public static void Main()
    {
        using AIChainExtensions;

        // Input data: Contains negative values and values > 1
        double[] rawData = { 0.5, -1.5, 2.0, 0.0 };
        var tensor = new LegacyTensorLib.Tensor(rawData, 2, 2);

        // Build the Pipeline
        var pipeline = new TensorPipeline();
        
        pipeline
            .AddValidation() // Step 1: Check validity
            .AddStep(t => t.Normalize(), "Normalize") // Step 2: Normalize
            .AddStep(t => t.Clip(0, 1), "Clip")       // Step 3: Clip
            .AddStep(t => t.Softmax(), "Softmax");    // Step 4: Softmax

        try
        {
            var result = pipeline.Execute(tensor);
            Console.WriteLine($"\nFinal Result: [{string.Join(", ", result.Data)}]");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Pipeline failed: {ex.Message}");
        }
    }
}
