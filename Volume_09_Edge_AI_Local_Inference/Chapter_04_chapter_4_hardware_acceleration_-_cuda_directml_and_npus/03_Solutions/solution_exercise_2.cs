
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
using System.Collections.Generic;
using System.IO;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;

namespace HardwareAccelerationExercises
{
    public class MemoryManager
    {
        public static void RunInferenceWithMemoryManagement(string modelPath, float[] inputData, int[] inputShape)
        {
            // Snapshot 1: Before allocation
            long memBefore = GC.GetTotalMemory(true);
            Console.WriteLine($"[Memory] Managed Memory Before: {memBefore / 1024.0 / 1024.0:F2} MB");

            // Configure Session Options
            using var sessionOptions = new SessionOptions();
            sessionOptions.EnableMemPattern = true; // Optimization for memory pattern
            sessionOptions.GraphOptimizationLevel = GraphOptimizationLevel.ORT_ENABLE_ALL;

            // Determine execution provider (using logic from Ex 1, hardcoded for demo stability)
            string provider = HardwareAcceleratorFactory.GetOptimalExecutionProvider();
            
            // Apply provider to session options
            if (provider == "CUDAExecutionProvider") sessionOptions.AppendExecutionProvider_CUDA(0);
            else if (provider == "DmlExecutionProvider") sessionOptions.AppendExecutionProvider_Dml(0);
            // CPU is default, no specific append needed usually, but can be explicit if required.

            // Ensure the model file exists for the demo
            if (!File.Exists(modelPath))
            {
                Console.WriteLine($"Model file not found at {modelPath}. Skipping inference.");
                return;
            }

            try
            {
                // 1. Resource Scope: InferenceSession
                // 'using' ensures Dispose() is called, releasing the native model memory.
                using var session = new InferenceSession(modelPath, sessionOptions);

                // 2. Prepare Inputs
                // 'using' ensures the tensor memory is tracked and released.
                var inputTensor = new DenseTensor<float>(inputData, inputShape);
                var inputs = new List<NamedOnnxValue>
                {
                    NamedOnnxValue.CreateFromTensor("input", inputTensor)
                };

                // 3. Run Inference
                // The result is an IDisposableReadOnlyCollection.
                using var results = session.Run(inputs);

                // Process results (example)
                foreach (var result in results)
                {
                    Console.WriteLine($"Output: {result.Name}");
                    // Accessing result AsTensor<float>() etc.
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Error] Inference failed: {ex.Message}");
            }

            // Snapshot 2: After disposal
            // GC.Collect() forces cleanup to show the impact of 'using' statements.
            GC.Collect();
            GC.WaitForPendingFinalizers();
            
            long memAfter = GC.GetTotalMemory(true);
            Console.WriteLine($"[Memory] Managed Memory After: {memAfter / 1024.0 / 1024.0:F2} MB");
            Console.WriteLine($"[Memory] Memory Reclaimed: {(memBefore - memAfter) / 1024.0 / 1024.0:F2} MB");
        }
    }
}
