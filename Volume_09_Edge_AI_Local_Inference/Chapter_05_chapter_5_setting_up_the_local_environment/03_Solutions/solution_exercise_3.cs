
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
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ML.OnnxRuntime;

namespace LocalAI.Inference
{
    // 1. Define the configuration record
    public record ModelConfig(string ModelPath, string ExecutionProvider, bool EnableGraphOptimization);

    public static class ModelLoader
    {
        // 2. Public async method to load the model
        public static async Task<InferenceSession> LoadAsync(ModelConfig config)
        {
            // 3. Initialize SessionOptions
            using var options = new SessionOptions();

            if (config.EnableGraphOptimization)
            {
                options.GraphOptimizationLevel = GraphOptimizationLevel.ORT_ENABLE_ALL;
            }

            // 4. Determine available providers
            var availableProviders = OrtEnv.Instance.SessionOptions.GetAvailableProviders();
            string providerToUse = config.ExecutionProvider;

            // 5. Fallback Mechanism using Pattern Matching
            if (!availableProviders.Contains(config.ExecutionProvider))
            {
                Console.WriteLine($"[WARNING] Requested provider '{config.ExecutionProvider}' not available. " +
                                  $"Available: {string.Join(", ", availableProviders)}. " +
                                  $"Falling back to CPU.");

                providerToUse = "CPUExecutionProvider";
            }

            // Map friendly names to ONNX Runtime provider strings if necessary
            // (Assuming config uses exact strings like "CUDAExecutionProvider")
            options.AppendExecutionProvider(providerToUse);

            // 6. Return the Task<InferenceSession> (Async instantiation)
            // Note: InferenceSession constructor is synchronous, but returning Task allows for future async file I/O
            return await Task.Run(() => new InferenceSession(config.ModelPath, options));
        }
    }
}
