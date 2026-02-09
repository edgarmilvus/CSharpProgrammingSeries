
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

# Source File: project_advanced_application_script.cs
# Description: Advanced Application Script
# ==========================================

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ContainerizedInferenceOrchestrator
{
    // =================================================================================================
    // REAL-WORLD CONTEXT:
    // A manufacturing plant uses an AI agent to detect defects on a conveyor belt via camera feeds.
    // The system must process frames in real-time. To handle varying loads (e.g., peak hours vs. idle),
    // we implement a "Micro-Batching" Inference Scaler. This simulates a Kubernetes Horizontal Pod Autoscaler
    // logic combined with a GPU-aware scheduler, optimizing throughput by batching small requests before
    // sending them to the inference engine.
    // =================================================================================================

    // Represents a single unit of work (e.g., a camera frame or a sensor reading)
    public class InferenceRequest
    {
        public Guid Id { get; set; }
        public string DataPayload { get; set; } // Simulating image data or sensor stream
        public DateTime Timestamp { get; set; }
    }

    // Represents the result of the AI analysis
    public class InferenceResult
    {
        public Guid RequestId { get; set; }
        public bool IsDefectDetected { get; set; }
        public float ConfidenceScore { get; set; }
    }

    // Simulates the AI Inference Engine (e.g., ONNX Runtime or TensorFlow Serving)
    // In a real scenario, this would interface with a GPU-accelerated container.
    public class InferenceEngine
    {
        // Simulates the cost of GPU computation
        public InferenceResult[] ProcessBatch(InferenceRequest[] batch)
        {
            Console.WriteLine($"[GPU Engine] Processing batch of {batch.Length} requests...");
            
            // Simulate processing delay (GPU compute time)
            Thread.Sleep(batch.Length * 50); 

            var results = new InferenceResult[batch.Length];
            var random = new Random();

            for (int i = 0; i < batch.Length; i++)
            {
                results[i] = new InferenceResult
                {
                    RequestId = batch[i].Id,
                    IsDefectDetected = random.Next(0, 10) > 8, // 20% chance of defect
                    ConfidenceScore = (float)random.NextDouble() * (1.0f - 0.7f) + 0.7f // 0.7 to 1.0
                };
            }
            return results;
        }
    }

    // The Core Orchestrator: Manages the queue, batching logic, and scaling decisions.
    // This mimics a Kubernetes Operator managing Pods based on queue depth.
    public class InferenceOrchestrator
    {
        private readonly Queue<InferenceRequest> _requestQueue;
        private readonly InferenceEngine _engine;
        private readonly int _maxBatchSize;
        private readonly int _maxQueueCapacity;
        
        // Simulates the "Autoscaler" state
        private int _activePods; 
        private readonly int _maxPods;

        public InferenceOrchestrator(int maxBatchSize, int maxQueueCapacity, int maxPods)
        {
            _requestQueue = new Queue<InferenceRequest>();
            _engine = new InferenceEngine();
            _maxBatchSize = maxBatchSize;
            _maxQueueCapacity = maxQueueCapacity;
            _maxPods = maxPods;
            _activePods = 1; // Start with 1 replica
        }

        // Simulates receiving traffic from an API Gateway / Service Mesh
        public bool EnqueueRequest(InferenceRequest request)
        {
            lock (_requestQueue)
            {
                if (_requestQueue.Count >= _maxQueueCapacity)
                {
                    Console.WriteLine($"[Warning] Queue full. Dropping request {request.Id}");
                    return false; // Backpressure mechanism
                }
                _requestQueue.Enqueue(request);
                Console.WriteLine($"[Ingress] Request {request.Id} enqueued. Queue Size: {_requestQueue.Count}");
                return true;
            }
        }

        // The main processing loop, simulating a Kubernetes Pod lifecycle
        public async Task StartProcessingAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine($"[System] Orchestrator started with {_activePods} active pod(s).");
            
            while (!cancellationToken.IsCancellationRequested)
            {
                // 1. Autoscaling Logic: Check if we need to scale up/down
                AdjustScaling();

                // 2. Batching Logic: Check if we have enough requests to fill a batch
                InferenceRequest[] batch = null;
                lock (_requestQueue)
                {
                    if (_requestQueue.Count >= _maxBatchSize)
                    {
                        batch = new InferenceRequest[Math.Min(_requestQueue.Count, _maxBatchSize)];
                        for (int i = 0; i < batch.Length; i++)
                        {
                            batch[i] = _requestQueue.Dequeue();
                        }
                    }
                }

                // 3. Inference Execution
                if (batch != null)
                {
                    var results = _engine.ProcessBatch(batch);
                    LogResults(results);
                }
                else
                {
                    // Idle wait to prevent CPU spinning
                    await Task.Delay(100, cancellationToken);
                }
            }
        }

        // Simulates K8s HPA (Horizontal Pod Autoscaler) logic
        private void AdjustScaling()
        {
            int queueCount;
            lock (_requestQueue) { queueCount = _requestQueue.Count; }

            // Simple scaling policy: Scale up if queue > threshold, scale down if empty
            if (queueCount > _maxBatchSize * 2 && _activePods < _maxPods)
            {
                _activePods++;
                Console.WriteLine($"[Autoscaler] Scaling UP. Active Pods: {_activePods} (Queue: {queueCount})");
            }
            else if (queueCount == 0 && _activePods > 1)
            {
                _activePods--;
                Console.WriteLine($"[Autoscaler] Scaling DOWN. Active Pods: {_activePods}");
            }
        }

        private void LogResults(InferenceResult[] results)
        {
            foreach (var r in results)
            {
                string status = r.IsDefectDetected ? "DEFECT" : "OK";
                Console.WriteLine($"[Result] Req: {r.RequestId} | Status: {status} | Conf: {r.ConfidenceScore:P0}");
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== Containerized AI Inference Orchestrator Simulation ===");
            
            // Configuration: Simulating K8s Deployment YAML
            // Batch Size = 4 (Optimizes GPU memory usage)
            // Queue Capacity = 20 (Prevents OOM on the ingress side)
            // Max Pods = 3 (Resource limits)
            var orchestrator = new InferenceOrchestrator(maxBatchSize: 4, maxQueueCapacity: 20, maxPods: 3);

            var cts = new CancellationTokenSource();

            // Start the orchestrator in a background task (Simulating a Kubernetes Pod)
            Task processingTask = orchestrator.StartProcessingAsync(cts.Token);

            // Simulate incoming traffic (Service Mesh injecting requests)
            var random = new Random();
            Console.WriteLine("\n[Simulation] Generating synthetic traffic...\n");
            
            for (int i = 0; i < 25; i++)
            {
                var req = new InferenceRequest
                {
                    Id = Guid.NewGuid(),
                    DataPayload = $"Frame_{i}_Data",
                    Timestamp = DateTime.Now
                };

                orchestrator.EnqueueRequest(req);

                // Randomize arrival time to simulate real-world network jitter
                Thread.Sleep(random.Next(50, 200)); 
            }

            // Allow time for the remaining queue to drain
            Console.WriteLine("\n[Traffic] Generation complete. Draining remaining queue...\n");
            Thread.Sleep(2000); 

            // Stop the simulation
            cts.Cancel();
            Task.WaitAll(new[] { processingTask }, 1000);

            Console.WriteLine("\n=== Simulation Complete ===");
        }
    }
}
