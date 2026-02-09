
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

namespace DistributedInferenceOrchestrator
{
    // -------------------------------------------------------------------------
    // CORE CONCEPT: Stateful Orchestration vs. Stateless Inference
    // -------------------------------------------------------------------------
    // In a distributed microservices architecture, we separate the "brain" 
    // (Orchestrator) from the "muscle" (Inference Workers).
    // The Orchestration Service is STATEFUL: It tracks request lifecycles, 
    // manages queues, and handles retries. It uses Kubernetes CRDs (simulated 
    // here via C# classes) to define workflow specs.
    // The Inference Service is STATELESS: It receives data, processes it on a 
    // GPU/CPU, and returns the result. It knows nothing about the broader workflow.
    // -------------------------------------------------------------------------

    /// <summary>
    /// Represents a Custom Resource Definition (CRD) for an Inference Workflow.
    /// In Kubernetes, this would be a YAML file applied to the cluster. 
    /// Here, we model it as a C# class to demonstrate the orchestration logic.
    /// </summary>
    public class InferenceWorkflowSpec
    {
        public string WorkflowId { get; set; }
        public string ModelName { get; set; }      // e.g., "bert-base-uncased"
        public int MinReplicas { get; set; }       // Autoscaling lower bound
        public int MaxReplicas { get; set; }       // Autoscaling upper bound
        public int BatchSize { get; set; }         // Dynamic batching threshold
        public double TargetLatencyMs { get; set; } // SLO (Service Level Objective)
    }

    /// <summary>
    /// Represents a single inference request payload.
    /// </summary>
    public class InferenceRequest
    {
        public string Id { get; set; }
        public string InputData { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// Represents the result of an inference operation.
    /// </summary>
    public class InferenceResult
    {
        public string RequestId { get; set; }
        public string Prediction { get; set; }
        public double ProcessingTimeMs { get; set; }
    }

    // -------------------------------------------------------------------------
    // COMPONENT: Stateless Inference Worker (Simulated)
    // -------------------------------------------------------------------------
    // This simulates a containerized microservice (e.g., a Python Flask app 
    // wrapped in Docker) that exposes an HTTP endpoint for inference.
    // It does not store state. If it crashes, Kubernetes restarts it, and 
    // it picks up the next job from the queue.
    // -------------------------------------------------------------------------
    public class InferenceWorker
    {
        private readonly string _workerId;
        private readonly Random _random = new Random();

        public InferenceWorker(string workerId)
        {
            _workerId = workerId;
        }

        /// <summary>
        /// Simulates the heavy lifting of model inference.
        /// In a real scenario, this would call TensorFlow/PyTorch runtime.
        /// </summary>
        public InferenceResult Process(InferenceRequest request)
        {
            // Simulate variable processing time (jitter) based on batch size/load
            int processingTime = _random.Next(50, 200); 
            Thread.Sleep(processingTime); // Simulate GPU compute time

            return new InferenceResult
            {
                RequestId = request.Id,
                ProcessingTimeMs = processingTime,
                Prediction = $"[Result from Worker {_workerId}]: Processed '{request.InputData}'"
            };
        }
    }

    // -------------------------------------------------------------------------
    // COMPONENT: Service Mesh & Dynamic Batching Layer
    // -------------------------------------------------------------------------
    // This component acts as the "Sidecar" or Ingress Gateway. It intercepts 
    // incoming requests and applies dynamic batching logic before dispatching 
    // to workers. This optimizes GPU utilization by reducing kernel launch overhead.
    // -------------------------------------------------------------------------
    public class DynamicBatcher
    {
        private readonly Queue<InferenceRequest> _requestQueue = new Queue<InferenceRequest>();
        private readonly object _lock = new object();
        private readonly int _maxBatchSize;
        private readonly Timer _flushTimer;

        public event Action<List<InferenceRequest>> OnBatchReady;

        public DynamicBatcher(int maxBatchSize)
        {
            _maxBatchSize = maxBatchSize;
            // Flush the batch every 50ms even if not full (to meet latency SLOs)
            _flushTimer = new Timer(FlushBatch, null, 50, 50);
        }

        public void Enqueue(InferenceRequest request)
        {
            lock (_lock)
            {
                _requestQueue.Enqueue(request);
                // Trigger immediate dispatch if batch size is reached
                if (_requestQueue.Count >= _maxBatchSize)
                {
                    FlushBatch(null);
                }
            }
        }

        private void FlushBatch(object state)
        {
            List<InferenceRequest> batch = new List<InferenceRequest>();
            lock (_lock)
            {
                while (_requestQueue.Count > 0 && batch.Count < _maxBatchSize)
                {
                    batch.Add(_requestQueue.Dequeue());
                }
            }

            if (batch.Count > 0)
            {
                OnBatchReady?.Invoke(batch);
            }
        }
    }

    // -------------------------------------------------------------------------
    // COMPONENT: Orchestration Service (The Stateful Brain)
    // -------------------------------------------------------------------------
    // This service manages the lifecycle of the inference pipeline. It handles:
    // 1. Autoscaling: Adjusting the number of active InferenceWorker containers.
    // 2. Routing: Dispatching batches to available workers.
    // 3. Monitoring: Tracking latency to ensure SLO compliance.
    // -------------------------------------------------------------------------
    public class OrchestrationService
    {
        private readonly InferenceWorkflowSpec _spec;
        private readonly List<InferenceWorker> _activeWorkers = new List<InferenceWorker>();
        private readonly DynamicBatcher _batcher;
        
        // Metrics tracking for autoscaling decisions
        private readonly List<double> _recentLatencies = new List<double>();
        private const int MetricsWindowSize = 10;

        public OrchestrationService(InferenceWorkflowSpec spec)
        {
            _spec = spec;
            // Initialize with minimum replicas
            for (int i = 0; i < _spec.MinReplicas; i++)
            {
                ScaleUpWorker();
            }

            // Configure the dynamic batcher based on CRD spec
            _batcher = new DynamicBatcher(spec.BatchSize);
            _batcher.OnBatchReady += DispatchBatch;
        }

        /// <summary>
        /// Public API endpoint: Receives a request and queues it.
        /// </summary>
        public void ReceiveRequest(InferenceRequest request)
        {
            Console.WriteLine($"[Orchestrator] Received request {request.Id}");
            _batcher.Enqueue(request);
        }

        /// <summary>
        /// Dispatches a batch of requests to a specific worker.
        /// In a real Service Mesh (e.g., Istio), this would be an HTTP call 
        /// with load balancing (Round Robin, Least Connections).
        /// </summary>
        private void DispatchBatch(List<InferenceRequest> batch)
        {
            if (_activeWorkers.Count == 0)
            {
                Console.WriteLine("ERROR: No active workers available! Scaling up emergency...");
                ScaleUpWorker();
            }

            // Simple Round Robin selection
            // In production, use a proper load balancing algorithm
            var worker = _activeWorkers[0]; 
            
            // Simulate network transmission (Service Mesh overhead)
            Thread.Sleep(5); 

            // Process the batch
            foreach (var req in batch)
            {
                var result = worker.Process(req);
                RecordMetrics(result.ProcessingTimeMs);
                Console.WriteLine($"[Dispatch] {result.Prediction} | Latency: {result.ProcessingTimeMs}ms");
            }

            // Check autoscaling needs after processing
            EvaluateAutoscaling();
        }

        /// <summary>
        /// Records processing latency to calculate moving average.
        /// </summary>
        private void RecordMetrics(double latency)
        {
            _recentLatencies.Add(latency);
            if (_recentLatencies.Count > MetricsWindowSize)
            {
                _recentLatencies.RemoveAt(0);
            }
        }

        /// <summary>
        /// Autoscaling Logic:
        /// 1. Calculate average latency of the last N requests.
        //  2. If latency > TargetLatency, scale up (if under MaxReplicas).
        /// 3. If latency < TargetLatency / 2, scale down (if over MinReplicas).
        /// This is a basic implementation of the Kubernetes HPA (Horizontal Pod Autoscaler).
        /// </summary>
        private void EvaluateAutoscaling()
        {
            if (_recentLatencies.Count == 0) return;

            double avgLatency = 0;
            foreach (var l in _recentLatencies) avgLatency += l;
            avgLatency /= _recentLatencies.Count;

            Console.WriteLine($"[Autoscaler] Current Avg Latency: {avgLatency:F2}ms | Target: {_spec.TargetLatencyMs}ms");

            if (avgLatency > _spec.TargetLatencyMs && _activeWorkers.Count < _spec.MaxReplicas)
            {
                Console.WriteLine("--> Scaling UP (High Latency detected)");
                ScaleUpWorker();
            }
            else if (avgLatency < _spec.TargetLatencyMs / 2 && _activeWorkers.Count > _spec.MinReplicas)
            {
                Console.WriteLine("--> Scaling DOWN (Low Latency detected)");
                ScaleDownWorker();
            }
        }

        private void ScaleUpWorker()
        {
            var newWorker = new InferenceWorker($"Worker-{_activeWorkers.Count + 1}");
            _activeWorkers.Add(newWorker);
            Console.WriteLine($"[K8s Simulation] Created Pod: {newWorker.GetType().Name} (Total: {_activeWorkers.Count})");
        }

        private void ScaleDownWorker()
        {
            if (_activeWorkers.Count > 0)
            {
                var removed = _activeWorkers[_activeWorkers.Count - 1];
                _activeWorkers.RemoveAt(_activeWorkers.Count - 1);
                Console.WriteLine($"[K8s Simulation] Terminated Pod: {removed.GetType().Name} (Total: {_activeWorkers.Count})");
            }
        }
    }

    // -------------------------------------------------------------------------
    // MAIN PROGRAM: Simulation of Distributed Inference Pipeline
    // -------------------------------------------------------------------------
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== Distributed Inference Orchestrator Simulation ===\n");

            // 1. Define the Workflow via CRD (Custom Resource Definition)
            // This YAML-like configuration drives the orchestration behavior.
            var workflowSpec = new InferenceWorkflowSpec
            {
                WorkflowId = "wf-bert-001",
                ModelName = "bert-base-uncased",
                MinReplicas = 1,
                MaxReplicas = 5,
                BatchSize = 3, // Small batch size for demonstration
                TargetLatencyMs = 150.0 // Strict SLO
            };

            // 2. Initialize the Orchestration Service
            var orchestrator = new OrchestrationService(workflowSpec);

            // 3. Simulate Incoming Traffic (The "Load Generator")
            // We will burst traffic to trigger autoscaling logic.
            Console.WriteLine("Injecting high-throughput traffic...\n");

            for (int i = 1; i <= 15; i++)
            {
                var request = new InferenceRequest
                {
                    Id = $"req-{i:00}",
                    InputData = $"Sentence fragment #{i}",
                    CreatedAt = DateTime.Now
                };

                orchestrator.ReceiveRequest(request);

                // Artificial delay between requests to simulate arrival pattern
                // If we send too fast, the queue fills up, triggering batch flushes
                Thread.Sleep(100); 
            }

            // Allow background threads to finish processing
            Thread.Sleep(2000);
            Console.WriteLine("\n=== Simulation Complete ===");
        }
    }
}
