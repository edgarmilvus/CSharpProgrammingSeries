
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
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

public class PipelineBenchmark
{
    private readonly InferenceSession _session;
    private readonly ImagePreprocessor _preprocessor;
    private readonly NmsProcessor _nmsProcessor;
    private readonly string _modelPath;

    public PipelineBenchmark(string modelPath)
    {
        _modelPath = modelPath;
        // Initialize session once
        _session = new InferenceSession(modelPath);
        _preprocessor = new ImagePreprocessor();
        _nmsProcessor = new NmsProcessor();
    }

    public void RunBenchmark(int warmupIterations, int testIterations, string imagePath)
    {
        // Load test image
        using var testImage = Image.Load<Rgba32>(imagePath);
        
        Console.WriteLine($"Starting Benchmark: {testIterations} iterations...");
        
        // 1. Warm-up
        Console.WriteLine("Warming up...");
        for (int i = 0; i < warmupIterations; i++)
        {
            ProcessSingleFrame(testImage);
        }

        // 2. Measurement
        var sw = new Stopwatch();
        var prepTimes = new List<long>();
        var infTimes = new List<long>();
        var postTimes = new List<long>();
        var totalTimes = new List<long>();

        Console.WriteLine("Running measurements...");
        for (int i = 0; i < testIterations; i++)
        {
            sw.Restart();
            
            // Phase 1: Preprocessing
            var prepStart = Stopwatch.GetTimestamp();
            var inputTensor = _preprocessor.PreprocessImage(testImage, 640, 640);
            var prepEnd = Stopwatch.GetTimestamp();
            
            // Phase 2: Inference
            using var inputs = new List<NamedOnnxValue> { NamedOnnxValue.CreateFromTensor("images", inputTensor) };
            var infStart = Stopwatch.GetTimestamp();
            using var results = _session.Run(inputs);
            var infEnd = Stopwatch.GetTimestamp();

            // Phase 3: Post-processing
            var postStart = Stopwatch.GetTimestamp();
            var outputTensor = results.First().AsTensor<float>();
            _nmsProcessor.Process(outputTensor);
            var postEnd = Stopwatch.GetTimestamp();
            
            sw.Stop();

            // Record raw ticks (high resolution)
            prepTimes.Add(prepEnd - prepStart);
            infTimes.Add(infEnd - infStart);
            postTimes.Add(postEnd - postStart);
            totalTimes.Add(sw.ElapsedTicks);

            // Cleanup
            inputTensor.Dispose();
        }

        // 3. Report Generation
        PrintReport("Preprocessing", prepTimes);
        PrintReport("Inference", infTimes);
        PrintReport("Post-processing", postTimes);
        PrintReport("Total Latency", totalTimes);
        
        // Calculate FPS
        double avgTotalMs = totalTimes.Average() / (double)Stopwatch.Frequency * 1000;
        Console.WriteLine($"\nEstimated FPS: {1000.0 / avgTotalMs:F2}");
    }

    private void ProcessSingleFrame(Image<Rgba32> img)
    {
        var tensor = _preprocessor.PreprocessImage(img, 640, 640);
        using var inputs = new List<NamedOnnxValue> { NamedOnnxValue.CreateFromTensor("images", tensor) };
        using var res = _session.Run(inputs);
        var outTensor = res.First().AsTensor<float>();
        _nmsProcessor.Process(outTensor);
        tensor.Dispose();
    }

    private void PrintReport(string phase, List<long> ticks)
    {
        double avgMs = ticks.Average() / (double)Stopwatch.Frequency * 1000;
        double minMs = ticks.Min() / (double)Stopwatch.Frequency * 1000;
        double maxMs = ticks.Max() / (double)Stopwatch.Frequency * 1000;
        
        // Calculate P95 (95th percentile)
        var sorted = ticks.OrderBy(x => x).ToList();
        int p95Index = (int)(sorted.Count * 0.95);
        double p95Ms = sorted[p95Index] / (double)Stopwatch.Frequency * 1000;

        Console.WriteLine($"\n--- {phase} ---");
        Console.WriteLine($"Avg: {avgMs:F2} ms | Min: {minMs:F2} ms | Max: {maxMs:F2} ms | P95: {p95Ms:F2} ms");
    }
}
