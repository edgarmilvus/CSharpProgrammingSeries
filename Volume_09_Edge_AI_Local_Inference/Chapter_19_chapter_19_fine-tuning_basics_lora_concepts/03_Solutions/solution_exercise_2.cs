
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

using Microsoft.ML.OnnxRuntime;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace LoraSessionManagement
{
    public class LoraSessionManager : IDisposable
    {
        // Thread-safe storage for sessions. Key: Adapter Name, Value: InferenceSession
        private readonly ConcurrentDictionary<string, InferenceSession> _sessions;
        private readonly SemaphoreSlim _semaphore;
        private bool _disposed = false;

        public LoraSessionManager()
        {
            _sessions = new ConcurrentDictionary<string, InferenceSession>();
            _semaphore = new SemaphoreSlim(1, 1); // Limit concurrent session creation
        }

        /// <summary>
        /// Loads the base model from a byte array.
        /// </summary>
        public InferenceSession LoadBaseModel(byte[] onnxBytes)
        {
            return LoadAdapterInternal("base", onnxBytes);
        }

        /// <summary>
        /// Loads a LoRA adapter from a byte array.
        /// </summary>
        public InferenceSession LoadAdapter(byte[] loraBytes, string adapterName)
        {
            // In a real scenario, the 'loraBytes' might be a merged ONNX or 
            // we would apply graph surgery here. For this exercise, we treat it as a distinct session.
            return LoadAdapterInternal(adapterName, loraBytes);
        }

        private InferenceSession LoadAdapterInternal(string key, byte[] modelBytes)
        {
            _semaphore.Wait();
            try
            {
                if (_disposed) throw new ObjectDisposedException(nameof(LoraSessionManager));

                // Use OrtEnv and SessionOptions to create session from memory
                var options = new SessionOptions();
                
                // Simulate DirectML/CUDA acceleration if available
                // options.AppendExecutionProviderDirectML(0); 
                
                // Create a ReadOnlyMemory wrapper for the byte array
                ReadOnlyMemory<byte> modelMemory = new ReadOnlyMemory<byte>(modelBytes);
                
                var session = new InferenceSession(modelMemory, options);
                
                _sessions.AddOrUpdate(key, session, (k, old) => {
                    old.Dispose();
                    return session;
                });

                Console.WriteLine($"Session loaded for key: {key}");
                return session;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        /// <summary>
        /// Simulates injecting LoRA weights. In a real dynamic scenario, 
        /// we might use SessionOptions to apply a custom execution provider 
        /// or modify the graph inputs if the ONNX was designed for dynamic adapters.
        /// </summary>
        public void ApplyLoraWeights(InferenceSession session, Dictionary<string, float[]> loraWeights)
        {
            // Since ONNX Runtime doesn't natively support "plugging in" weights 
            // without graph surgery or a custom operator, we simulate the logic:
            // 1. Identify input tensors that match adapter layer names.
            // 2. Bind the weights to specific input nodes (if the model was exported with 'dummy' inputs for weights).
            
            foreach (var layer in loraWeights)
            {
                Console.WriteLine($"Binding LoRA weight '{layer.Key}' to session inputs...");
                // In a full implementation, you would use session.Run() with specific inputs 
                // that represent the adapter weights, or use a custom OrtValue.
            }
        }

        public InferenceSession GetSession(string key)
        {
            if (_sessions.TryGetValue(key, out var session))
            {
                return session;
            }
            throw new KeyNotFoundException($"Session for '{key}' not found.");
        }

        public void Dispose()
        {
            if (_disposed) return;
            
            _semaphore.Wait();
            try
            {
                foreach (var session in _sessions.Values)
                {
                    session.Dispose();
                }
                _sessions.Clear();
                _disposed = true;
            }
            finally
            {
                _semaphore.Release();
                _semaphore.Dispose();
            }
        }
    }

    // Example usage
    class Program
    {
        static void Main(string[] args)
        {
            // Dummy byte arrays (usually loaded from File.ReadAllBytes)
            byte[] baseModelBytes = new byte[1024]; 
            byte[] loraBytes = new byte[512];

            using (var manager = new LoraSessionManager())
            {
                // Load base
                var baseSession = manager.LoadBaseModel(baseModelBytes);
                
                // Load adapter
                var adapterSession = manager.LoadAdapter(loraBytes, "math_adapter");
                
                // Apply weights (Simulation)
                manager.ApplyLoraWeights(adapterSession, new Dictionary<string, float[]> { { "q_proj", new float[10] } });
            }
        }
    }
}
