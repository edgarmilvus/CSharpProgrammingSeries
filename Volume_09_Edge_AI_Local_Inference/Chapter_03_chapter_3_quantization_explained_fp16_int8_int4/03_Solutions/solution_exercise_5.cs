
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

// Source File: solution_exercise_5.cs
// Description: Solution for Exercise 5
// ==========================================

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.ML.OnnxRuntime;

namespace GraphOptimizationTuning
{
    class Program
    {
        static void Main(string[] args)
        {
            string modelPath = "model_fp32.onnx"; // Assuming a float model for fusion testing
            var inputTensor = new float[1 * 128]; 
            var inputName = "input";

            // 1. Test Different Optimization Levels
            Console.WriteLine("Benchmarking Optimization Levels...");
            
            // Baseline: Basic
            RunBenchmark(modelPath, GraphOptimizationLevel.ORT_ENABLE_BASIC, inputName, inputTensor);
            
            // Optimized: All
            RunBenchmark(modelPath, GraphOptimizationLevel.ORT_ENABLE_ALL, inputName, inputTensor);

            // 2. Disable Specific Patterns (Simulated)
            // Note: C# API allows setting specific disabled optimizations via SessionOptions.ConfigEntry
            // However, pattern disabling is often done via C++ API or environment variables.
            // We will simulate the logic here.
            Console.WriteLine("\nTesting with Specific Pattern Disabled...");
            RunBenchmarkWithDisabledPattern(modelPath, inputName, inputTensor);
        }

        static void RunBenchmark(string modelPath, GraphOptimizationLevel level, string inputName, float[] inputData)
        {
            using var options = new SessionOptions();
            options.GraphOptimizationLevel = level;
            options.AppendExecutionProvider_CPU();

            try
            {
                using var session = new InferenceSession(modelPath, options);
                var inputs = new List<NamedOnnxValue> 
                { 
                    NamedOnnxValue.CreateFromTensor(inputName, 
                        new DenseTensor<float>(inputData, new[] { 1, 128 })) 
                };

                // Warmup
                for (int i = 0; i < 10; i++) session.Run(inputs);

                // Measure
                var sw = Stopwatch.StartNew();
                for (int i = 0; i < 100; i++) session.Run(inputs);
                sw.Stop();

                Console.WriteLine($"Level {level}: {sw.ElapsedMilliseconds}ms (Avg per 100 runs)");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error with level {level}: {ex.Message}");
            }
        }

        static void RunBenchmarkWithDisabledPattern(string modelPath, string inputName, float[] inputData)
        {
            using var options = new SessionOptions();
            options.GraphOptimizationLevel = GraphOptimizationLevel.ORT_ENABLE_ALL;
            
            // 3. Disable specific pattern (Example: ConvActivationFusion)
            // This is a specific config key in ONNX Runtime.
            // Note: Keys might vary by version. 
            options.AddSessionConfigEntry("session.disable_pattern", "ConvActivationFusion");

            options.AppendExecutionProvider_CPU();

            try
            {
                using var session = new InferenceSession(modelPath, options);
                var inputs = new List<NamedOnnxValue> 
                { 
                    NamedOnnxValue.CreateFromTensor(inputName, 
                        new DenseTensor<float>(inputData, new[] { 1, 128 })) 
                };

                var sw = Stopwatch.StartNew();
                for (int i = 0; i < 100; i++) session.Run(inputs);
                sw.Stop();

                Console.WriteLine($"ORT_ENABLE_ALL (No Fusion): {sw.ElapsedMilliseconds}ms");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error disabling pattern: {ex.Message}");
            }
        }
    }
}
