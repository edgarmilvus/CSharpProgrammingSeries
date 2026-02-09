
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
using System.Linq;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;

namespace LocalAI.Inference
{
    public class LLMWrapper : IDisposable
    {
        private InferenceSession? _session;
        private bool _disposed = false;

        public LLMWrapper(string modelPath)
        {
            // In a real scenario, use the ModelLoader from Ex 3
            _session = new InferenceSession(modelPath);
        }

        // The standard Dispose Pattern
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // Dispose managed resources (the InferenceSession)
                    _session?.Dispose();
                }

                // Clean up unmanaged resources here if any (none in this specific wrapper, 
                // but OrtEnv shutdown logic could go here if managing the singleton lifecycle manually).
                
                _disposed = true;
            }
        }

        // Finalizer (Destructor) - Safety net for unmanaged resources
        ~LLMWrapper()
        {
            // Finalizers should NOT call Dispose(false) in standard .NET Core/5+ 
            // if you only manage managed resources, but it's good practice for the pattern.
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void RunInference(string prompt)
        {
            if (_session == null || _disposed)
                throw new ObjectDisposedException(nameof(LLMWrapper));

            // Mock Tokenization: Convert string to dummy token IDs (integers)
            // In reality, this would be a complex tokenizer step.
            int[] tokenIds = prompt.Split(' ').Select(s => s.Length).ToArray(); 
            // Shape: [1, sequence_length]
            var shape = new long[] { 1, tokenIds.Length };

            // 4. Create OrtValue (Tensor) and ensure disposal
            using var inputTensor = new OrtValue(OrtMemoryInfo.DefaultInstance, tokenIds, shape);
            
            // Prepare input names (usually "input_ids" for ONNX LLMs)
            var inputNames = new List<string> { "input_ids" };
            var inputs = new List<OrtValue> { inputTensor };

            // Run Session
            // Note: Run returns IReadOnlyList<OrtValue>
            using var outputs = _session.Run(inputs, inputNames, new List<string> { "logits" });

            // The 'outputs' list and its contents are disposed when the 'using' block ends.
            // The 'inputTensor' is disposed immediately after the Run call.
            
            Console.WriteLine("Inference completed. Resources cleaned up.");
        }
    }
}
