
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
using System.Collections.Generic;
using System.Linq;
using Microsoft.ML.OnnxRuntime;

namespace HardwareAccelerationExercises
{
    public class HybridExecutor
    {
        public void RunHybridInference(string modelPath)
        {
            // 1. Refactored Factory: Get a prioritized list of providers
            var executionProviders = GetHybridExecutionProviders();
            
            Console.WriteLine($"Configuring Hybrid Session with Providers: {string.Join(", ", executionProviders)}");

            using var sessionOptions = new SessionOptions();
            
            // 2. Modify SessionOptions: Append providers in priority order
            // Note: The CPU provider is implicitly available, but adding it explicitly 
            // ensures it's in the allowed list for fallback.
            foreach (var provider in executionProviders)
            {
                try
                {
                    if (provider == "CUDAExecutionProvider")
                        sessionOptions.AppendExecutionProvider_CUDA(0);
                    else if (provider == "DmlExecutionProvider")
                        sessionOptions.AppendExecutionProvider_Dml(0);
                    else if (provider == "CPUExecutionProvider")
                        sessionOptions.AppendExecutionProvider_CPU(); 
                    
                    Console.WriteLine($"✅ Added {provider} to session configuration.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ Failed to add {provider}: {ex.Message}");
                }
            }

            // 3. Validation: Create the session
            // If the model has operators not supported by the GPU, ONNX Runtime will 
            // automatically fall back to the CPU provider defined in the session options.
            try
            {
                using var session = new InferenceSession(modelPath, sessionOptions);
                Console.WriteLine("✅ Hybrid Session created successfully.");
                
                // In a real test, we would run inference here. 
                // The Graphviz visualization (below) illustrates the flow.
                Console.WriteLine("Simulating inference with hybrid operators...");
                // session.Run(...);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Session creation failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Updated Factory to return a prioritized list for hybrid execution.
        /// </summary>
        private List<string> GetHybridExecutionProviders()
        {
            var providers = new List<string>();

            // Priority 1: GPU
            if (HardwareAcceleratorFactory.GetOptimalExecutionProvider() is string provider && 
                (provider == "CUDAExecutionProvider" || provider == "DmlExecutionProvider"))
            {
                providers.Add(provider);
            }

            // Priority 2: CPU (Always added as fallback)
            // In hybrid execution, the CPU is essential for operators the GPU cannot handle.
            providers.Add("CPUExecutionProvider");

            return providers;
        }
    }

    /* 
     * VISUALIZATION (Graphviz DOT Format)
     * 
     * This diagram illustrates the data flow in the Hybrid Execution configuration.
     * 
     * digraph HybridExecution {
     *     rankdir=LR;
     *     node [shape=box, style=rounded];
     *     
     *     Input [label="Input Tensor"];
     *     Split [label="Graph Partitioner"];
     *     GPU [label="GPU Execution Provider\n(Matrix Ops, Conv)"];
     *     CPU [label="CPU Execution Provider\n(Control Flow, Custom Ops)"];
     *     Agg [label="Result Aggregation"];
     *     Output [label="Final Output"];
     *     
     *     Input -> Split;
     *     Split -> GPU [label="Supported Ops"];
     *     Split -> CPU [label="Unsupported Ops"];
     *     GPU -> Agg;
     *     CPU -> Agg;
     *     Agg -> Output;
     * }
     */
}
