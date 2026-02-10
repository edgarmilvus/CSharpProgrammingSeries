
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

// Source File: project_advanced_application_script.cs
// Description: Advanced Application Script
// ==========================================

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ScalableInferenceOrchestrator
{
    // 1. CORE DATA MODELS
    // Represents an inference request from a client (e.g., a user query).
    public class InferenceRequest
    {
        public int RequestId { get; set; }
        public string InputData { get; set; }
        public DateTime Timestamp { get; set; }
        public int Priority { get; set; } // 1 (Low) to 5 (Critical)
    }

    // Represents the result of an AI model inference.
    public class InferenceResult
    {
        public int RequestId { get; set; }
        public string OutputData { get; set; }
        public string ModelVersion { get; set; }
        public double ProcessingTimeMs { get; set; }
    }

    // 2. MODEL SERVING CONTAINER (Simulated)
    // Mimics a containerized model server (e.g., KServe/Triton) holding a specific model version.
    public class ModelContainer
    {
        public string ContainerId { get; private set; }
        public string ModelName { get; private set; }
        public string ModelVersion { get; private set; }
        public bool IsBusy { get; private set; }
        public int RequestsProcessed { get; private set; }

        // Simulates GPU memory allocation (e.g., 16GB VRAM)
        public int VramCapacity { get; private set; } 
        public int VramUsage { get; private set; }

        public ModelContainer(string id, string model, string version, int vram)
        {
            ContainerId = id;
            ModelName = model;
            ModelVersion = version;
            VramCapacity = vram;
            VramUsage = 0;
            IsBusy = false;
            RequestsProcessed = 0;
        }

        // Simulates the heavy computation of AI inference.
        public async Task<InferenceResult> ProcessRequestAsync(InferenceRequest request)
        {
            if (IsBusy) throw new InvalidOperationException("Container is overloaded");
            
            IsBusy = true;
            VramUsage += 2000; // Simulate loading model weights into VRAM (2GB per request context)

            // Simulate network latency and GPU computation time
            // Random delay between 100ms and 500ms
            int delay = new Random().Next(100, 500);
            await Task.Delay(delay);

            // Release resources
            VramUsage -= 2000;
            IsBusy = false;
            RequestsProcessed++;

            return new InferenceResult
            {
                RequestId = request.RequestId,
                OutputData = $"Processed '{request.InputData}' using {ModelName} v{ModelVersion}",
                ModelVersion = ModelVersion,
                ProcessingTimeMs = delay
            };
        }

        public bool CanAcceptWork() => !IsBusy && (VramCapacity - VramUsage) > 2000;
    }

    // 3. ORCHESTRATOR & AUTO-SCALER LOGIC
    // Manages the pool of containers and handles scaling logic.
    public class InferenceOrchestrator
    {
        private List<ModelContainer> _activeContainers;
        private Queue<InferenceRequest> _requestQueue;
        private int _maxContainers;
        private int _containerCounter;
        private string _modelName;

        // Metrics for scaling decisions
        public int QueueLength => _requestQueue.Count;
        public int ActiveContainerCount => _activeContainers.Count;

        public InferenceOrchestrator(string modelName, int maxContainers)
        {
            _modelName = modelName;
            _maxContainers = maxContainers;
            _activeContainers = new List<ModelContainer>();
            _requestQueue = new Queue<InferenceRequest>();
            _containerCounter = 0;

            // Initial scale: Start with 1 container
            ScaleUp();
        }

        // Receives traffic from the load balancer
        public void ReceiveRequest(InferenceRequest request)
        {
            _requestQueue.Enqueue(request);
            Console.WriteLine($"[Orchestrator] Request {request.RequestId} enqueued. Queue Size: {_requestQueue.Count}");
        }

        // The main processing loop (Simulates K8s ReplicaSet controller reconciliation loop)
        public async Task RunProcessingCycle()
        {
            // 1. Auto-Scaling Logic
            AdjustScale();

            // 2. Dispatch Logic
            // Process up to N requests per cycle to simulate parallel processing
            int concurrentProcessingLimit = _activeContainers.Count * 2; 
            
            while (_requestQueue.Count > 0 && concurrentProcessingLimit > 0)
            {
                var container = FindAvailableContainer();
                
                if (container == null)
                {
                    // No resources available, wait for next cycle
                    break;
                }

                var request = _requestQueue.Dequeue();
                
                // Fire and forget to simulate async handling, but track task for observability
                _ = Task.Run(async () => 
                {
                    try 
                    {
                        var result = await container.ProcessRequestAsync(request);
                        LogResult(result);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[Error] Processing failed: {ex.Message}");
                        // In a real system, we would requeue this request
                    }
                });

                concurrentProcessingLimit--;
            }
        }

        // Heuristic-based scaling (Simulates Kubernetes HPA)
        private void AdjustScale()
        {
            // Rule 1: Scale Up if queue backlog > threshold (e.g., 5 requests waiting)
            if (_requestQueue.Count > 5 && _activeContainers.Count < _maxContainers)
            {
                ScaleUp();
            }
            // Rule 2: Scale Down if queue is empty and we have > 1 container
            else if (_requestQueue.Count == 0 && _activeContainers.Count > 1)
            {
                ScaleDown();
            }
        }

        private void ScaleUp()
        {
            if (_activeContainers.Count >= _maxContainers) return;

            _containerCounter++;
            string version = _containerCounter % 2 == 0 ? "2.0" : "1.5"; // Simulate rolling updates
            var newContainer = new ModelContainer($"container-{_containerCounter}", _modelName, version, 16000);
            
            _activeContainers.Add(newContainer);
            Console.WriteLine($"[Scaling] SCALED UP. New container added: {newContainer.ContainerId} (Model: {version}). Total: {_activeContainers.Count}");
        }

        private void ScaleDown()
        {
            if (_activeContainers.Count <= 1) return;

            var containerToRemove = _activeContainers[_activeContainers.Count - 1];
            _activeContainers.RemoveAt(_activeContainers.Count - 1);
            
            Console.WriteLine($"[Scaling] SCALED DOWN. Removed container: {containerToRemove.ContainerId}. Total: {_activeContainers.Count}");
        }

        private ModelContainer FindAvailableContainer()
        {
            // Simple Round-Robin load balancing strategy
            foreach (var container in _activeContainers)
            {
                if (container.CanAcceptWork())
                {
                    return container;
                }
            }
            return null;
        }

        private void LogResult(InferenceResult result)
        {
            // Simulates an observability sink (Prometheus/ELK)
            Console.WriteLine($"[Result] Req {result.RequestId}: '{result.OutputData}' [Time: {result.ProcessingTimeMs}ms]");
        }
    }

    // 4. MAIN PROGRAM (Entry Point)
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("--- Scalable AI Inference System Simulation ---");
            
            // Initialize Orchestrator with a limit of 5 containers
            var orchestrator = new InferenceOrchestrator("SentimentAnalysisModel", maxContainers: 5);

            // Simulate Traffic Spikes
            // We will run a loop for 10 seconds, injecting varying loads
            var random = new Random();
            int cycleCount = 0;

            while (cycleCount < 20) // Run for 20 cycles
            {
                cycleCount++;
                Console.WriteLine($"\n--- Cycle {cycleCount} ---");

                // Simulate incoming traffic (Bursty pattern)
                int incomingRequests = random.Next(0, 12); 
                
                if (cycleCount == 5) incomingRequests = 20; // Artificial Spike
                if (cycleCount == 15) incomingRequests = 0; // Traffic Drop

                for (int i = 0; i < incomingRequests; i++)
                {
                    var req = new InferenceRequest
                    {
                        RequestId = cycleCount * 100 + i,
                        InputData = $"User Query {i}",
                        Priority = random.Next(1, 6),
                        Timestamp = DateTime.Now
                    };
                    orchestrator.ReceiveRequest(req);
                }

                // Run the orchestration logic (Reconciliation Loop)
                await orchestrator.RunProcessingCycle();

                // Visualizing State
                Console.WriteLine($"[Metrics] Active Pods: {orchestrator.ActiveContainerCount} | Queue Backlog: {orchestrator.QueueLength}");

                // Simulate time passing (e.g., 1 second per cycle)
                await Task.Delay(1000);
            }

            Console.WriteLine("\n--- Simulation Complete ---");
        }
    }
}
