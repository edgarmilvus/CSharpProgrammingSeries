
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

using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using System;
using System.Linq;

public class SessionOptionsFactory
{
    public SessionOptions Create()
    {
        // 3. Configure SessionOptions with highest priority EP
        var options = new SessionOptions();

        // We try to append the highest priority EP.
        // Note: AppendExecutionProvider methods throw if the EP is not available.
        
        // 1. Check for NVIDIA GPU (Windows/Linux)
        try
        {
            // Note: CUDA version must match the DLL available in the environment
            options.AppendExecutionProvider_CUDA(0); 
            Console.WriteLine("Selected CUDA Execution Provider.");
            return options;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"CUDA not available: {ex.Message}");
        }

        // 2. Check for Windows GPU via DirectML
        try
        {
            options.AppendExecutionProvider_DML(0);
            Console.WriteLine("Selected DirectML Execution Provider.");
            return options;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"DirectML not available: {ex.Message}");
        }

        // 3. Check for Apple Silicon via CoreML
        try
        {
            options.AppendExecutionProvider_CoreML(0); // 0 usually defaults to Any
            Console.WriteLine("Selected CoreML Execution Provider.");
            return options;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"CoreML not available: {ex.Message}");
        }

        // 4. Fallback to CPU (Default behavior if no Append is called, but explicit is better)
        Console.WriteLine("Falling back to CPU Execution Provider.");
        return options; 
    }

    // 5. Edge Case: Fallback mechanism for unsupported operators
    public InferenceSession CreateSessionWithFallback(string modelPath)
    {
        SessionOptions options = Create();
        
        try
        {
            return new InferenceSession(modelPath, options);
        }
        catch (OnnxRuntimeException ex) when (ex.Message.Contains("operator") || ex.Message.Contains("kernel"))
        {
            // Log warning
            Console.WriteLine($"Warning: Model contains operators unsupported by selected EP. Retrying with CPU.");
            
            // Retry with CPU only
            var cpuOptions = new SessionOptions(); 
            return new InferenceSession(modelPath, cpuOptions);
        }
    }
}

// 4. Performance Analysis Benchmark
public class InferenceBenchmark
{
    public async Task RunBenchmark(string modelPath, string input)
    {
        var factory = new SessionOptionsFactory();
        
        // Warm up
        Console.WriteLine("Warming up...");
        
        // Benchmark CPU
        Console.WriteLine("\n--- CPU Benchmark ---");
        await MeasurePerformance(new SessionOptions(), modelPath, input);

        // Benchmark Selected Hardware
        Console.WriteLine("\n--- Hardware EP Benchmark ---");
        var hwOptions = factory.Create();
        await MeasurePerformance(hwOptions, modelPath, input);
    }

    private async Task MeasurePerformance(SessionOptions options, string modelPath, string input)
    {
        using var session = new InferenceSession(modelPath, options);
        
        // Prepare inputs (simplified)
        var inputTensor = new DenseTensor<float>(new float[128], [1, 128]); // Mock shape
        var inputs = new List<NamedOnnxValue> { NamedOnnxValue.CreateFromTensor("input", inputTensor) };
        
        var sw = System.Diagnostics.Stopwatch.StartNew();
        
        // Measure Time to First Token (TTFT) - simplified as inference time here
        using var results = session.Run(inputs);
        sw.Stop();
        var ttft = sw.ElapsedMilliseconds;

        // Measure Tokens Per Second (TPS) - simplified as throughput
        // In reality, this requires iterating generation steps
        var tps = 1000.0 / ttft; // Mock calculation

        Console.WriteLine($"EP: {options.ExecutionProviderOptions.FirstOrDefault().Key}");
        Console.WriteLine($"TTFT: {ttft}ms");
        Console.WriteLine($"TPS (Mock): {tps:F2} tokens/sec");
    }
}
