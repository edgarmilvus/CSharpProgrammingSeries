
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
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ML.OnnxRuntime;

namespace BertEdgeExercises
{
    public sealed class ModelManager : IDisposable
    {
        private static readonly Lazy<ModelManager> _lazy = new Lazy<ModelManager>(() => new ModelManager());
        private InferenceSession _session;
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(2, 2); // Max 2 concurrent requests
        private bool _isWarmedUp = false;
        private readonly object _lock = new object();

        public static ModelManager Instance => _lazy.Value;

        private ModelManager() { }

        public void Initialize(string modelPath)
        {
            lock (_lock)
            {
                if (_session != null) return;

                var options = new SessionOptions();
                options.GraphOptimizationLevel = GraphOptimizationLevel.ORT_ENABLE_ALL;
                options.AppendExecutionProvider_CPU(); // Default to CPU for stability
                
                _session = new InferenceSession(modelPath, options);
                WarmUp();
            }
        }

        private void WarmUp()
        {
            if (_isWarmedUp || _session == null) return;

            Console.WriteLine("Warming up model...");
            
            // Create dummy input (zeros)
            // Assuming input shape [1, 128] for single batch warmup
            long[] dummyIds = new long[128];
            long[] dummyMask = new long[128];
            
            var inputTensors = new List<NamedOnnxValue>
            {
                NamedOnnxValue.CreateFromTensor("input_ids", new DenseTensor<long>(dummyIds, new[] { 1, 128 })),
                NamedOnnxValue.CreateFromTensor("attention_mask", new DenseTensor<long>(dummyMask, new[] { 1, 128 }))
            };

            // Run the warm-up inference
            try 
            {
                using (var results = _session.Run(inputTensors)) 
                {
                    // Force iteration to ensure execution completes
                    foreach(var r in results) { var _ = r.AsTensor<float>(); }
                }
                _isWarmedUp = true;
                Console.WriteLine("Warm-up complete.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warm-up failed: {ex.Message}");
            }
        }

        public async Task<InferenceSession> GetSessionAsync()
        {
            // Ensure initialization is complete before returning session
            if (_session == null) throw new InvalidOperationException("ModelManager not initialized.");
            
            // Simulate async wait if semaphore is full (throttling)
            await _semaphore.WaitAsync();
            return _session;
        }

        public void ReleaseSession()
        {
            _semaphore.Release();
        }

        public void Dispose()
        {
            _semaphore?.Dispose();
            _session?.Dispose();
        }
    }
}
