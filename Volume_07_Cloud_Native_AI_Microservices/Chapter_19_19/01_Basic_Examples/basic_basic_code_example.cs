
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

# Source File: basic_basic_code_example.cs
# Description: Basic Code Example
# ==========================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CloudNativeAiMicroservices.Example
{
    /// <summary>
    /// Represents the core inference engine for our AI agent.
    /// In a real-world scenario, this would wrap a heavy ML library (e.g., PyTorch, TensorFlow, ONNX Runtime).
    /// For this "Hello World" example, we simulate the computational load and memory management.
    /// </summary>
    public class InferenceEngine
    {
        private readonly Random _rng = new Random();
        
        /// <summary>
        /// Simulates a heavy inference operation.
        /// In a containerized GPU environment, this would involve:
        /// 1. Loading input data into GPU VRAM.
        /// 2. Executing matrix multiplications on the GPU.
        /// 3. Retrieving results from VRAM to system RAM.
        /// </summary>
        /// <param name="inputData">The raw input data (e.g., text, image bytes).</param>
        /// <returns>A task representing the inference result with a confidence score.</returns>
        public async Task<InferenceResult> PredictAsync(string inputData)
        {
            // Simulate the latency of GPU computation and data transfer.
            // In a real GPU-bound workload, the duration depends on model size and batch size.
            // Here, we randomize it to mimic variable load.
            int processingTimeMs = _rng.Next(50, 200); 
            await Task.Delay(processingTimeMs);

            // Simulate a result. 
            // In a real scenario, this would be a tensor or structured object.
            double confidence = _rng.NextDouble(); 
            
            return new InferenceResult
            {
                Prediction = $"Processed: {inputData}",
                Confidence = confidence,
                ProcessingTimeMs = processingTimeMs
            };
        }
    }

    /// <summary>
    /// Data Transfer Object (DTO) for the inference result.
    /// </summary>
    public record InferenceResult
    {
        public string Prediction { get; init; } = string.Empty;
        public double Confidence { get; init; }
        public int ProcessingTimeMs { get; init; }
    }

    /// <summary>
    /// Represents the Kubernetes Pod lifecycle and resource management.
    /// In a real deployment, this class would interface with the Kubernetes C# Client 
    /// to report metrics (Prometheus exporter) or handle termination signals.
    /// </summary>
    public class PodContext
    {
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();

        /// <summary>
        /// Simulates the Kubernetes Pod readiness probe.
        /// A Pod is ready only when its internal services (e.g., model loaded into GPU) are initialized.
        /// </summary>
        public bool IsReady { get; private set; } = false;

        public async Task InitializeAsync()
        {
            Console.WriteLine("[PodContext] Initializing model weights from Persistent Volume...");
            // Simulate loading gigabytes of model weights from a mounted PVC (Persistent Volume Claim).
            await Task.Delay(1000); 
            IsReady = true;
            Console.WriteLine("[PodContext] Model loaded. Ready to serve traffic.");
        }

        /// <summary>
        /// Simulates handling the SIGTERM signal sent by Kubernetes during scale-down or rolling updates.
        /// </summary>
        public void RegisterShutdownHandler()
        {
            Console.CancelKeyPress += (s, e) =>
            {
                e.Cancel = true;
                Console.WriteLine("[PodContext] SIGTERM received. Draining connections...");
                _cts.Cancel();
            };
        }

        public CancellationToken GetCancellationToken() => _cts.Token;
    }

    /// <summary>
    /// The main entry point simulating the containerized agent.
    /// </summary>
    class Program
    {
        static async Task Main(string[] args)
        {
            // 1. Setup Infrastructure
            var podContext = new PodContext();
            podContext.RegisterShutdownHandler();
            
            // 2. Initialize Inference Engine (Load Model)
            var engine = new InferenceEngine();
            await podContext.InitializeAsync();

            // 3. Simulate Request Processing Loop
            // In a real K8s environment, this would be an HTTP server (e.g., ASP.NET Core) 
            // listening on port 8080.
            Console.WriteLine("[Agent] Starting request processing loop...");
            
            var tasks = new List<Task>();
            
            // Simulate concurrent requests (e.g., from a Load Balancer)
            for (int i = 0; i < 5; i++)
            {
                if (podContext.GetCancellationToken().IsCancellationRequested) break;

                var requestTask = Task.Run(async () =>
                {
                    var result = await engine.PredictAsync($"Image_{Guid.NewGuid()}");
                    Console.WriteLine($"[Agent] Result: {result.Prediction} | Confidence: {result.Confidence:F2} | Time: {result.ProcessingTimeMs}ms");
                });
                
                tasks.Add(requestTask);
                await Task.Delay(50); // Simulate staggered incoming requests
            }

            try
            {
                await Task.WhenAll(tasks);
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("[Agent] Processing halted due to shutdown signal.");
            }

            Console.WriteLine("[Agent] Simulation complete. Container exiting.");
        }
    }
}
