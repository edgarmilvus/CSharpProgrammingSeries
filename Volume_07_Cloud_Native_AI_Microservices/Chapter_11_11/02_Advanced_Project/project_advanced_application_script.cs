
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

namespace CloudNativeInferenceOrchestrator
{
    // REAL-WORLD CONTEXT:
    // Imagine a cloud-native AI application serving a high-traffic sentiment analysis API.
    // Inference requests arrive in bursts (e.g., during product launches or social media events).
    // The system must scale GPU resources dynamically to handle load while minimizing costs during idle times.
    // This simulation models a Kubernetes Horizontal Pod Autoscaler (HPA) logic using KEDA-like triggers,
    // managing a pool of containerized AI agents (inference workers) and a GPU resource manager.

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== Cloud-Native AI Inference Orchestrator Simulation ===");
            Console.WriteLine("Simulating KEDA-driven scaling for GPU-accelerated agents...\n");

            // Initialize the orchestrator with a pool of AI agents
            InferenceOrchestrator orchestrator = new InferenceOrchestrator();
            
            // Simulate a production workload cycle
            orchestrator.RunSimulation();
            
            Console.WriteLine("\n=== Simulation Complete ===");
        }
    }

    // Represents a single containerized AI agent (e.g., a Docker container running a Python/TensorFlow model).
    // In a real cluster, this would map to a Kubernetes Pod.
    public class InferenceAgent
    {
        public string Id { get; private set; }
        public bool IsBusy { get; private set; }
        public int GpuMemoryAllocatedMB { get; private set; }

        public InferenceAgent(string id, int gpuMemoryMB)
        {
            Id = id;
            GpuMemoryAllocatedMB = gpuMemoryMB;
            IsBusy = false;
        }

        // Simulates the inference process (e.g., model loading and prediction).
        // In production, this involves async HTTP calls to the model server (e.g., TensorFlow Serving).
        public async Task<string> ProcessRequestAsync(string inputData)
        {
            if (IsBusy)
            {
                return $"Agent {Id}: Error - Already processing.";
            }

            IsBusy = true;
            Console.WriteLine($"[GPU Allocation] Agent {Id} locking {GpuMemoryAllocatedMB}MB VRAM.");
            
            // Simulate model inference latency (network + compute)
            await Task.Delay(1000); 
            
            IsBusy = false;
            Console.WriteLine($"[GPU Release] Agent {Id} released resources.");
            return $"Agent {Id}: Processed '{inputData}' -> Sentiment: Positive (Confidence: 0.92)";
        }
    }

    // Manages the lifecycle of AI agents and simulates GPU resource constraints.
    // In a real cluster, this logic is distributed across the Kubelet and Device Plugin.
    public class GpuResourceManager
    {
        private const int TotalGpuMemoryMB = 16000; // e.g., 16GB VRAM
        private int _allocatedMemoryMB = 0;

        public bool CanAllocate(int memoryMB)
        {
            return (_allocatedMemoryMB + memoryMB) <= TotalGpuMemoryMB;
        }

        public bool Allocate(int memoryMB)
        {
            if (CanAllocate(memoryMB))
            {
                _allocatedMemoryMB += memoryMB;
                return true;
            }
            return false;
        }

        public void Release(int memoryMB)
        {
            _allocatedMemoryMB -= memoryMB;
            if (_allocatedMemoryMB < 0) _allocatedMemoryMB = 0;
        }

        public int GetAvailableMemory() => TotalGpuMemoryMB - _allocatedMemoryMB;
    }

    // Orchestrator simulates the Kubernetes Control Plane logic combined with KEDA (Kubernetes Event-Driven Autoscaling).
    // It monitors metrics (queue length) and adjusts the number of replicas (agents).
    public class InferenceOrchestrator
    {
        private List<InferenceAgent> _agents;
        private GpuResourceManager _gpuManager;
        private int _requestQueueCount;
        private int _agentCounter;
        private const int AgentGpuRequirementMB = 2000; // Each model container requires 2GB VRAM

        public InferenceOrchestrator()
        {
            _agents = new List<InferenceAgent>();
            _gpuManager = new GpuResourceManager();
            _requestQueueCount = 0;
            _agentCounter = 0;

            // Start with 1 agent (min replicas)
            ScaleUp();
        }

        // Simulates the main loop of a Kubernetes Controller Manager
        public void RunSimulation()
        {
            // Phase 1: Normal Load
            Console.WriteLine("\n--- PHASE 1: Steady State (Low Traffic) ---");
            AddRequests(2);
            ProcessCycle();

            // Phase 2: Traffic Spike (Simulating a social media event)
            Console.WriteLine("\n--- PHASE 2: Traffic Spike (Scaling Event) ---");
            AddRequests(15); // Burst of requests
            ProcessCycle();

            // Phase 3: Scale Down (Cooldown period simulation)
            Console.WriteLine("\n--- PHASE 3: Cooldown & Scale Down ---");
            _requestQueueCount = 0; // Traffic drops
            ProcessCycle();
        }

        // Core logic loop: Monitor metrics -> Evaluate Autoscaling -> Execute
        private void ProcessCycle()
        {
            // Simulate time passing (e.g., the 15-second metric window in KEDA)
            Thread.Sleep(500); 
            
            Console.WriteLine($"\n[Metric Check] Queue Depth: {_requestQueueCount}, Active Agents: {_agents.Count}");

            // KEDA Logic: Calculate desired replicas based on queue length
            // Formula: DesiredReplicas = ceil(CurrentQueue / TargetQueueLength)
            int targetQueueLengthPerAgent = 2; 
            int desiredReplicas = (int)Math.Ceiling((double)_requestQueueCount / targetQueueLengthPerAgent);

            // Ensure we don't exceed GPU memory limits
            int maxPossibleReplicas = 16000 / AgentGpuRequirementMB; // 8 agents max for 16GB GPU

            if (desiredReplicas > maxPossibleReplicas)
            {
                Console.WriteLine($"[Warning] Desired replicas ({desiredReplicas}) exceed GPU capacity. Capping at {maxPossibleReplicas}.");
                desiredReplicas = maxPossibleReplicas;
            }

            // Execute Scaling Logic
            if (desiredReplicas > _agents.Count)
            {
                int scaleCount = desiredReplicas - _agents.Count;
                Console.WriteLine($"[Autoscaler] Scaling UP: Adding {scaleCount} agents.");
                for (int i = 0; i < scaleCount; i++) ScaleUp();
            }
            else if (desiredReplicas < _agents.Count)
            {
                int scaleCount = _agents.Count - desiredReplicas;
                Console.WriteLine($"[Autoscaler] Scaling DOWN: Removing {scaleCount} agents.");
                for (int i = 0; i < scaleCount; i++) ScaleDown();
            }
            else
            {
                Console.WriteLine("[Autoscaler] Target met. No scaling action.");
            }

            // Process Requests with available agents
            ProcessQueue();
        }

        // Spins up a new containerized agent
        private void ScaleUp()
        {
            if (_gpuManager.Allocate(AgentGpuRequirementMB))
            {
                _agentCounter++;
                string agentId = $"agent-pod-{_agentCounter}";
                _agents.Add(new InferenceAgent(agentId, AgentGpuRequirementMB));
                Console.WriteLine($"[K8s Event] Created Pod: {agentId}");
            }
            else
            {
                Console.WriteLine("[K8s Event] Scale Up Failed: Insufficient GPU Memory.");
            }
        }

        // Terminates an agent (simulates pod deletion)
        private void ScaleDown()
        {
            if (_agents.Count > 0)
            {
                InferenceAgent agent = _agents[0];
                _agents.RemoveAt(0);
                _gpuManager.Release(agent.GpuMemoryAllocatedMB);
                Console.WriteLine($"[K8s Event] Deleted Pod: {agent.Id}");
            }
        }

        // Dispatches queued requests to free agents
        private void ProcessQueue()
        {
            // Iterate through agents and assign work
            // Note: In a real system, this is a dispatcher thread or sidecar pattern
            for (int i = 0; i < _agents.Count; i++)
            {
                if (_requestQueueCount > 0 && !_agents[i].IsBusy)
                {
                    _requestQueueCount--;
                    // Fire and forget async task to simulate parallel processing
                    _ = _agents[i].ProcessRequestAsync($"DataChunk_{_requestQueueCount}"); 
                }
            }
        }

        // Helper to simulate incoming HTTP requests
        private void AddRequests(int count)
        {
            _requestQueueCount += count;
            Console.WriteLine($"[Ingress] Added {count} requests to queue.");
        }
    }
}
