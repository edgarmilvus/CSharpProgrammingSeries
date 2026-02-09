
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

namespace AIOrchestrationSimulation
{
    // Represents the status of an inference request.
    public enum RequestStatus
    {
        Pending,
        Processing,
        Completed,
        Failed
    }

    // Represents a single inference request from a client.
    public class InferenceRequest
    {
        public Guid Id { get; } = Guid.NewGuid();
        public string InputData { get; set; }
        public RequestStatus Status { get; set; } = RequestStatus.Pending;
        public DateTime CreatedAt { get; } = DateTime.UtcNow;
        public string Result { get; set; }

        public InferenceRequest(string data)
        {
            InputData = data;
        }
    }

    // Simulates a containerized AI Agent (Pod) that processes requests.
    // In a real scenario, this would be a stateless service running in Kubernetes.
    public class AIAgentPod
    {
        public string PodId { get; private set; }
        public bool IsBusy { get; private set; }
        private Random _random = new Random();

        public AIAgentPod(string podId)
        {
            PodId = podId;
        }

        // Simulates the heavy computation of model inference.
        // Includes artificial latency and occasional failures to mimic real-world conditions.
        public async Task<InferenceRequest> ProcessRequestAsync(InferenceRequest request)
        {
            IsBusy = true;
            request.Status = RequestStatus.Processing;
            
            // Simulate processing time (e.g., GPU inference latency)
            int processingTime = _random.Next(500, 2000); 
            await Task.Delay(processingTime);

            // Simulate a random failure (e.g., model timeout or memory error)
            if (_random.Next(0, 10) == 0) 
            {
                request.Status = RequestStatus.Failed;
                request.Result = null;
            }
            else
            {
                request.Status = RequestStatus.Completed;
                request.Result = $"Processed by {PodId} in {processingTime}ms: {request.InputData.ToUpper()}";
            }

            IsBusy = false;
            return request;
        }
    }

    // Manages the pool of AI Agent Pods.
    // This simulates the Kubernetes Pod Controller responsible for maintaining desired state.
    public class PodController
    {
        private List<AIAgentPod> _activePods = new List<AIAgentPod>();
        private int _podCounter = 0;
        private const int MaxPods = 5; // Simulates resource limits (e.g., GPU memory constraints)

        public int CurrentPodCount => _activePods.Count;
        public int ActiveRequests => _activePods.Count(p => p.IsBusy); // Note: Using basic loop logic below instead of LINQ

        public PodController(int initialPods)
        {
            for (int i = 0; i < initialPods; i++)
            {
                ScaleUp();
            }
        }

        // Scales out the deployment by adding a new Pod container.
        public void ScaleUp()
        {
            if (_activePods.Count < MaxPods)
            {
                _podCounter++;
                string podName = $"ai-agent-pod-{_podCounter}";
                _activePods.Add(new AIAgentPod(podName));
                Console.WriteLine($"[K8s Controller] Scaling Up: Created new pod '{podName}'. Total: {_activePods.Count}");
            }
        }

        // Scales in the deployment by removing an idle Pod.
        // In production, this relies on metrics like CPU usage or queue depth.
        public void ScaleDown()
        {
            if (_activePods.Count > 1) // Always keep at least one replica
            {
                // Find an idle pod
                AIAgentPod podToRemove = null;
                foreach (var pod in _activePods)
                {
                    if (!pod.IsBusy)
                    {
                        podToRemove = pod;
                        break;
                    }
                }

                if (podToRemove != null)
                {
                    _activePods.Remove(podToRemove);
                    Console.WriteLine($"[K8s Controller] Scaling Down: Removed idle pod '{podToRemove.PodId}'. Total: {_activePods.Count}");
                }
            }
        }

        // Finds an available pod to handle a new request (Load Balancing logic).
        public AIAgentPod GetAvailablePod()
        {
            foreach (var pod in _activePods)
            {
                if (!pod.IsBusy)
                {
                    return pod;
                }
            }
            return null; // No available resources
        }
    }

    // The Load Balancer / Ingress Gateway.
    // Handles request queueing and dispatching to available pods.
    public class LoadBalancer
    {
        private Queue<InferenceRequest> _requestQueue = new Queue<InferenceRequest>();
        private PodController _podController;

        public LoadBalancer(PodController podController)
        {
            _podController = podController;
        }

        public void ReceiveRequest(InferenceRequest request)
        {
            _requestQueue.Enqueue(request);
            Console.WriteLine($"[LoadBalancer] Received request {request.Id}. Queue Depth: {_requestQueue.Count}");
        }

        // Dispatches requests to available pods based on queue depth and resource availability.
        public async Task ProcessQueueAsync()
        {
            // Check queue depth to trigger autoscaling (Horizontal Pod Autoscaler logic)
            if (_requestQueue.Count > 3 && _podController.CurrentPodCount < 5)
            {
                _podController.ScaleUp();
            }
            else if (_requestQueue.Count == 0 && _podController.CurrentPodCount > 1)
            {
                // Scale down if queue is empty to save costs (Cluster Autoscaler logic)
                _podController.ScaleDown();
            }

            // Dispatch logic
            while (_requestQueue.Count > 0)
            {
                AIAgentPod availablePod = _podController.GetAvailablePod();

                if (availablePod != null)
                {
                    InferenceRequest request = _requestQueue.Dequeue();
                    // Fire and forget to simulate asynchronous processing pipeline
                    _ = Task.Run(async () => 
                    {
                        await availablePod.ProcessRequestAsync(request);
                        LogResult(request);
                    });
                }
                else
                {
                    // If no pods are available, wait briefly before retrying (Backoff strategy)
                    await Task.Delay(100);
                }
            }
        }

        private void LogResult(InferenceRequest request)
        {
            if (request.Status == RequestStatus.Completed)
            {
                Console.WriteLine($"[Result] Success: {request.Result}");
            }
            else
            {
                Console.WriteLine($"[Result] Failed: Request {request.Id} encountered an error.");
                // In a real system, this would go to a Dead Letter Queue (DLQ) for retry.
            }
        }
    }

    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("--- Starting Cloud-Native AI Orchestration Simulation ---");
            
            // 1. Initialize the Kubernetes Controller with 1 initial Pod
            var controller = new PodController(initialPods: 1);
            
            // 2. Initialize the Load Balancer / Service Mesh Entry Point
            var loadBalancer = new LoadBalancer(controller);

            // 3. Simulate incoming traffic (Client Requests)
            // We will burst traffic to trigger autoscaling logic
            Console.WriteLine("\n--- Phase 1: Burst Traffic Injection ---");
            
            for (int i = 1; i <= 8; i++)
            {
                var req = new InferenceRequest($"Image Frame {i}");
                loadBalancer.ReceiveRequest(req);
                
                // Small delay to simulate staggered arrival
                await Task.Delay(50); 
            }

            // 4. Run the orchestration loop
            // This mimics the Kubernetes Control Plane reconciliation loop
            Console.WriteLine("\n--- Phase 2: Orchestration & Scaling ---");
            await loadBalancer.ProcessQueueAsync();

            // 5. Allow time for async processing to complete
            Console.WriteLine("\n--- Phase 3: Stabilization (Waiting for processing) ---");
            await Task.Delay(3000);

            // 6. Simulate idle time to trigger scale-down
            Console.WriteLine("\n--- Phase 4: Idle State & Scale Down ---");
            await loadBalancer.ProcessQueueAsync(); // Check for scale down

            Console.WriteLine("\n--- Simulation Complete ---");
        }
    }
}
