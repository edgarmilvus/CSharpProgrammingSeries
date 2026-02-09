
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

namespace ScalableInferenceOrchestrator
{
    // Represents the state of a single inference request.
    public enum RequestStatus
    {
        Pending,
        Processing,
        Completed,
        Failed
    }

    // Represents a single AI inference task (e.g., "Generate a summary for text X").
    // This mimics a microservice payload passed between containerized agents.
    public class InferenceTask
    {
        public int TaskId { get; set; }
        public string InputData { get; set; }
        public RequestStatus Status { get; set; }
        public string Result { get; set; }
        public DateTime CreatedAt { get; set; }

        public InferenceTask(int id, string data)
        {
            TaskId = id;
            InputData = data;
            Status = RequestStatus.Pending;
            CreatedAt = DateTime.Now;
        }
    }

    // Simulates a GPU-accelerated Model Server.
    // In a real Kubernetes cluster, this would be a Pod running a TensorFlow Serving or TorchServe container.
    public class ModelServerNode
    {
        public string NodeId { get; private set; }
        public int MaxConcurrentInferences { get; private set; }
        private int _currentLoad;
        private readonly object _lock = new object();

        public ModelServerNode(string id, int capacity)
        {
            NodeId = id;
            MaxConcurrentInferences = capacity;
            _currentLoad = 0;
        }

        // Simulates the computational cost of inference (e.g., GPU VRAM usage and compute time).
        public bool CanAcceptWork()
        {
            lock (_lock)
            {
                return _currentLoad < MaxConcurrentInferences;
            }
        }

        public void AssignTask(InferenceTask task)
        {
            lock (_lock)
            {
                if (_currentLoad >= MaxConcurrentInferences)
                {
                    throw new InvalidOperationException($"Node {NodeId} is at capacity.");
                }
                _currentLoad++;
                task.Status = RequestStatus.Processing;
            }

            // Simulate asynchronous model inference latency (e.g., 200ms - 500ms).
            Task.Run(async () =>
            {
                Random rnd = new Random();
                await Task.Delay(rnd.Next(200, 500));
                
                CompleteTask(task);
            });
        }

        private void CompleteTask(InferenceTask task)
        {
            lock (_lock)
            {
                _currentLoad--;
                task.Status = RequestStatus.Completed;
                task.Result = $"Processed by Node {NodeId} at {DateTime.Now:HH:mm:ss}";
            }
        }

        public int GetCurrentLoad()
        {
            lock (_lock) return _currentLoad;
        }
    }

    // The Orchestrator: Manages the pool of Model Servers.
    // This mimics a Kubernetes Horizontal Pod Autoscaler (HPA) and Load Balancer logic.
    public class InferenceOrchestrator
    {
        private List<ModelServerNode> _nodes;
        private Queue<InferenceTask> _pendingQueue;
        private bool _isRunning;

        public InferenceOrchestrator()
        {
            _nodes = new List<ModelServerNode>();
            _pendingQueue = new Queue<InferenceTask>();
            _isRunning = false;
        }

        // Simulates scaling up (adding a new container/pod).
        public void ScaleUp(string nodeId, int capacity)
        {
            var newNode = new ModelServerNode(nodeId, capacity);
            _nodes.Add(newNode);
            Console.WriteLine($"[SCALING EVENT] Added Node: {nodeId} (Capacity: {capacity}). Total Nodes: {_nodes.Count}");
        }

        // Simulates scaling down (removing a container/pod).
        // In a real system, this would drain connections before termination.
        public void ScaleDown(string nodeId)
        {
            var node = _nodes.Find(n => n.NodeId == nodeId);
            if (node != null)
            {
                if (node.GetCurrentLoad() == 0)
                {
                    _nodes.Remove(node);
                    Console.WriteLine($"[SCALING EVENT] Removed Node: {nodeId}. Total Nodes: {_nodes.Count}");
                }
                else
                {
                    Console.WriteLine($"[WARNING] Cannot remove Node {nodeId}: Active load detected. Draining required.");
                }
            }
        }

        // Accepts a new request from the client (e.g., API Gateway).
        public void SubmitRequest(InferenceTask task)
        {
            lock (_pendingQueue)
            {
                _pendingQueue.Enqueue(task);
                Console.WriteLine($"[REQUEST ACCEPTED] Task ID: {task.TaskId} queued.");
            }
        }

        // The main dispatch loop. In K8s, this logic is distributed (Load Balancer -> Service -> Pod).
        public void StartDispatcher()
        {
            _isRunning = true;
            Console.WriteLine("[SYSTEM] Orchestrator Dispatcher Started.");
            
            // Background thread for processing the queue.
            Task.Run(() =>
            {
                while (_isRunning)
                {
                    InferenceTask nextTask = null;

                    lock (_pendingQueue)
                    {
                        if (_pendingQueue.Count > 0)
                        {
                            nextTask = _pendingQueue.Dequeue();
                        }
                    }

                    if (nextTask != null)
                    {
                        DispatchToNode(nextTask);
                    }
                    else
                    {
                        // Idle wait to prevent CPU spinning
                        Thread.Sleep(50);
                    }
                }
            });
        }

        // Logic to find the best node (Least Connection Load Balancing).
        private void DispatchToNode(InferenceTask task)
        {
            ModelServerNode bestNode = null;
            int minLoad = int.MaxValue;

            // Iterate to find the node with the most available capacity.
            foreach (var node in _nodes)
            {
                if (node.CanAcceptWork())
                {
                    int currentLoad = node.GetCurrentLoad();
                    if (currentLoad < minLoad)
                    {
                        minLoad = currentLoad;
                        bestNode = node;
                    }
                }
            }

            if (bestNode != null)
            {
                try
                {
                    bestNode.AssignTask(task);
                    Console.WriteLine($"[DISPATCH] Task {task.TaskId} -> Node {bestNode.NodeId} (Load: {bestNode.GetCurrentLoad()})");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] Dispatch failed: {ex.Message}");
                    task.Status = RequestStatus.Failed;
                }
            }
            else
            {
                // Buffer strategy: Re-queue if no nodes available (or implement circuit breaker).
                Console.WriteLine($"[BACKPRESSURE] No available nodes. Re-queuing Task {task.TaskId}.");
                lock (_pendingQueue)
                {
                    _pendingQueue.Enqueue(task);
                }
                Thread.Sleep(100); // Backoff
            }
        }

        public void Stop()
        {
            _isRunning = false;
        }

        public int GetPendingCount()
        {
            lock (_pendingQueue) return _pendingQueue.Count;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== Cloud-Native AI Inference Orchestrator Simulation ===\n");

            // 1. Initialize Orchestrator (Simulates Kubernetes Control Plane)
            var orchestrator = new InferenceOrchestrator();
            orchestrator.StartDispatcher();

            // 2. Initial Scaling: Deploy 2 Model Server Pods
            orchestrator.ScaleUp("gpu-node-1", 2); // Capacity: 2 concurrent inferences
            orchestrator.ScaleUp("gpu-node-2", 2);

            // 3. Simulate Traffic Spike: Client sends 10 requests
            Console.WriteLine("\n--- Simulating Traffic Spike (10 Requests) ---\n");
            for (int i = 1; i <= 10; i++)
            {
                var task = new InferenceTask(i, $"Prompt Data for Request {i}");
                orchestrator.SubmitRequest(task);
                Thread.Sleep(50); // Staggered arrival
            }

            // 4. Monitor System State
            MonitorSystem(orchestrator);

            // 5. Auto-Scaling Logic: If pending > threshold, scale up
            // This simulates a Kubernetes HPA (Horizontal Pod Autoscaler) watching queue depth.
            if (orchestrator.GetPendingCount() > 2)
            {
                Console.WriteLine("\n--- [HPA] Scaling Triggered: High Latency Detected ---\n");
                orchestrator.ScaleUp("gpu-node-3", 2);
            }

            // 6. Simulate Load Drop: Scale Down a node
            // In real K8s, this happens after metrics drop and a cool-down period.
            Console.WriteLine("\n--- Simulating Load Drop ---\n");
            Thread.Sleep(2000); // Wait for processing
            orchestrator.ScaleDown("gpu-node-2"); 

            // 7. Final State Check
            MonitorSystem(orchestrator);

            Console.WriteLine("\n=== Simulation Complete ===");
            orchestrator.Stop();
        }

        // Helper to visualize the system state
        static void MonitorSystem(InferenceOrchestrator orch)
        {
            Console.WriteLine($"\n[STATUS] Pending Queue: {orch.GetPendingCount()}");
            // In a real dashboard, this would be Prometheus metrics
        }
    }
}
