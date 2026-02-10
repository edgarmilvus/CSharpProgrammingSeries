
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

// Source File: solution_exercise_4.cs
// Description: Solution for Exercise 4
// ==========================================

using System;
using System.Diagnostics;
using System.Threading;

namespace LlamaSharpExercises
{
    public class ModelBenchmark
    {
        private readonly string _modelPath;

        public ModelBenchmark(string modelPath)
        {
            _modelPath = modelPath;
        }

        public void RunFullBenchmark()
        {
            Console.WriteLine("=== Starting Model Benchmark ===");

            // 1. CPU Benchmark (GpuLayers = 0)
            Console.WriteLine("\n--- Backend: CPU ---");
            RunConfiguration(gpuLayers: 0, batchSize: 1);
            RunConfiguration(gpuLayers: 0, batchSize: 8);
            RunConfiguration(gpuLayers: 0, batchSize: 32);

            // 2. GPU Benchmark (GpuLayers = 99) - Simulated
            Console.WriteLine("\n--- Backend: GPU (Simulated) ---");
            // Note: In a real environment, this requires CUDA/Vulkan libraries
            RunConfiguration(gpuLayers: 99, batchSize: 1);
            RunConfiguration(gpuLayers: 99, batchSize: 8);
            RunConfiguration(gpuLayers: 99, batchSize: 32);
        }

        private void RunConfiguration(int gpuLayers, int batchSize)
        {
            long memBefore = GC.GetTotalMemory(true);
            var sw = Stopwatch.StartNew();

            // Simulate Model Loading
            var modelParams = new LlamaCppLib.LlamaModelParams 
            { 
                Threads = (uint)Environment.ProcessorCount, 
                GpuLayers = gpuLayers 
            };
            
            // Using the mock class from Ex 1 for demonstration
            using var model = new LlamaCppLib.LlamaModel(_modelPath, modelParams);
            
            sw.Stop();
            long memAfter = GC.GetTotalMemory(false);
            
            long memUsed = memAfter - memBefore;
            double initTime = sw.ElapsedMilliseconds;

            // Simulate Inference Speed
            double tps = MeasureTPS(model, batchSize);

            Console.WriteLine($"Config: GPU Layers={gpuLayers}, Batch={batchSize} | " +
                              $"Init: {initTime}ms | Mem: {memUsed/1024/1024:F2} MB | TPS: {tps:F2}");
        }

        private double MeasureTPS(LlamaCppLib.LlamaModel model, int batchSize)
        {
            // Simulate generating 100 tokens
            int totalTokens = 100;
            var ctxParams = new LlamaCppLib.LlamaContextParams { ContextSize = 2048, BatchSize = batchSize };
            using var ctx = model.CreateContext(ctxParams);

            var sw = Stopwatch.StartNew();
            
            // Simulate processing time based on batch size (Mock logic)
            // Real logic: ctx.Eval(tokens) inside a loop
            Thread.Sleep((int)(totalTokens * (10.0 / batchSize) + 50)); // Mock delay

            sw.Stop();
            return totalTokens / sw.Elapsed.TotalSeconds;
        }
    }

    public class DynamicAdaptationSystem
    {
        private bool _useGpu;
        private int _batchSize;

        public void RunSimulation()
        {
            Console.WriteLine("\n=== Dynamic Adaptation Simulation ===");
            
            // Initial profiling (Simulated)
            Console.WriteLine("Profiling Hardware...");
            Console.WriteLine("GPU Detected: Yes (Assumed for simulation)");
            
            while (true)
            {
                Console.Write("\nEnter prompt (or 'exit'): ");
                string? input = Console.ReadLine();
                if (input?.ToLower() == "exit") break;

                // 1. Check Battery (Simulated)
                int batteryLevel = new Random().Next(0, 100);
                Console.WriteLine($"[Battery Level: {batteryLevel}%]");

                // 2. Adapt Configuration
                AdaptToBattery(batteryLevel);

                // 3. Run Inference (Simulated)
                Console.WriteLine($"Running Inference with Config: GPU={_useGpu}, Batch={_batchSize}");
                SimulatePowerConsumption();
            }
        }

        private void AdaptToBattery(int batteryLevel)
        {
            if (batteryLevel < 20)
            {
                // Battery Saver Mode
                _useGpu = false; // Force CPU
                _batchSize = 1;  // Minimize parallelism to save power
                Console.WriteLine("[Mode] Battery Saver Active: CPU Backend, Batch Size 1");
            }
            else
            {
                // Performance Mode
                _useGpu = true;
                _batchSize = 32;
                Console.WriteLine("[Mode] Performance Active: GPU Backend, Batch Size 32");
            }
        }

        private void SimulatePowerConsumption()
        {
            // Arbitrary calculation based on config
            int powerUnits = _useGpu ? 50 : 10; // GPU consumes more
            powerUnits *= _batchSize; // Higher batch = more power
            
            // Simulate processing delay
            Thread.Sleep(200);
            
            Console.WriteLine($"[Power Report] Estimated consumption: {powerUnits} units");
        }
    }
}
