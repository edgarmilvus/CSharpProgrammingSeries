
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
using System.Threading;

namespace CloudNativeAI.Orchestrator
{
    // Represents the core resource unit (GPU) required for inference.
    // In a real Kubernetes cluster, this maps to node resources.
    struct GpuResource
    {
        public string Id;
        public int MemoryGB; // VRAM
        public bool IsOccupied;
    }

    // Represents a containerized AI Agent (e.g., a model serving container).
    class AiAgent
    {
        public string AgentId { get; private set; }
        public string ModelName { get; private set; }
        public int CurrentLoad { get; set; } // Simulated requests per second
        public bool IsActive { get; set; }

        public AiAgent(string id, string model)
        {
            AgentId = id;
            ModelName = model;
            CurrentLoad = 0;
            IsActive = true;
        }

        public void ProcessInference()
        {
            if (IsActive && CurrentLoad > 0)
            {
                // Simulate processing time
                Thread.Sleep(10); 
                CurrentLoad--; // Load decreases as work is done
            }
        }
    }

    class Program
    {
        // Configuration for the cluster
        const int MaxGpus = 4;
        const int MaxLoadPerGpu = 100; // Max requests per second a GPU can handle
        const int ScaleUpThreshold = 80; // % Load threshold to scale up
        const int ScaleDownThreshold = 20; // % Load threshold to scale down
        
        static void Main(string[] args)
        {
            Console.WriteLine("--- Cloud-Native AI Inference Orchestrator V1.0 ---");
            Console.WriteLine("Initializing Kubernetes-like Node with GPU resources...");

            // 1. Initialize Infrastructure (Simulating K8s Node with GPU)
            GpuResource[] gpuCluster = new GpuResource[MaxGpus];
            InitializeGpuCluster(gpuCluster);

            // 2. Initialize Agents (Simulating Pod Deployment)
            // We start with 1 agent handling all load initially
            AiAgent[] activeAgents = new AiAgent[MaxGpus]; 
            int agentCount = 1;
            activeAgents[0] = new AiAgent("agent-0", "ResNet-50-Inference");

            // 3. Simulation Loop (Simulating the Control Plane / Scheduler)
            Random loadGenerator = new Random();
            int simulationCycle = 0;

            while (simulationCycle < 50) // Run for 50 cycles
            {
                Console.WriteLine($"\n--- Cycle {simulationCycle} ---");
                
                // A. Simulate Incoming Traffic (Inference Requests)
                int incomingLoad = loadGenerator.Next(10, 150); 
                Console.WriteLine($"Incoming Load: {incomingLoad} req/sec");

                // B. Distribute Load & Check Health
                int totalCapacity = 0;
                int currentTotalLoad = 0;

                for (int i = 0; i < agentCount; i++)
                {
                    if (activeAgents[i] != null && activeAgents[i].IsActive)
                    {
                        // Add new load to existing agents
                        activeAgents[i].CurrentLoad += (incomingLoad / agentCount);
                        currentTotalLoad += activeAgents[i].CurrentLoad;
                        
                        // Simulate GPU processing
                        activeAgents[i].ProcessInference();
                        
                        // Calculate capacity based on GPU allocation
                        totalCapacity += MaxLoadPerGpu;
                    }
                }

                // C. Autoscaling Logic (The Core Algorithm)
                double utilization = (double)currentTotalLoad / totalCapacity * 100;
                Console.WriteLine($"Cluster Utilization: {utilization:F2}% ({currentTotalLoad}/{totalCapacity} req/sec)");

                if (utilization > ScaleUpThreshold && agentCount < MaxGpus)
                {
                    // SCALE UP: Deploy new agent pod
                    Console.WriteLine($"[ALERT] High Load detected. Scaling UP...");
                    
                    // Find free GPU
                    int freeGpuIndex = -1;
                    for(int g = 0; g < MaxGpus; g++)
                    {
                        if(!gpuCluster[g].IsOccupied)
                        {
                            gpuCluster[g].IsOccupied = true;
                            freeGpuIndex = g;
                            break;
                        }
                    }

                    if(freeGpuIndex != -1)
                    {
                        activeAgents[agentCount] = new AiAgent($"agent-{agentCount}", "ResNet-50-Inference");
                        Console.WriteLine($"Deployed new Agent pod on GPU {gpuCluster[freeGpuIndex].Id}.");
                        agentCount++;
                    }
                }
                else if (utilization < ScaleDownThreshold && agentCount > 1)
                {
                    // SCALE DOWN: Terminate agent pod to save costs
                    Console.WriteLine($"[ALERT] Low Load detected. Scaling DOWN...");
                    
                    // Find occupied GPU to free
                    int occupiedGpuIndex = -1;
                    for(int g = MaxGpus - 1; g >= 0; g--)
                    {
                        if(gpuCluster[g].IsOccupied)
                        {
                            gpuCluster[g].IsOccupied = false;
                            occupiedGpuIndex = g;
                            break;
                        }
                    }

                    if(occupiedGpuIndex != -1)
                    {
                        // Graceful shutdown simulation
                        activeAgents[agentCount - 1].IsActive = false;
                        activeAgents[agentCount - 1] = null; // Garbage collection simulation
                        agentCount--;
                        Console.WriteLine($"Terminated Agent pod. Freed GPU {gpuCluster[occupiedGpuIndex].Id}.");
                    }
                }
                else
                {
                    Console.WriteLine("Status: Stable. No scaling actions taken.");
                }

                simulationCycle++;
                Thread.Sleep(500); // Simulate time passing
            }

            Console.WriteLine("\n--- Simulation Complete ---");
            Console.WriteLine($"Final Agent Count: {agentCount}");
            Console.WriteLine("Shutting down cluster...");
        }

        // Helper method to initialize the GPU infrastructure
        static void InitializeGpuCluster(GpuResource[] cluster)
        {
            string[] gpuIds = { "gpu-node-01-a", "gpu-node-01-b", "gpu-node-02-a", "gpu-node-02-b" };
            for (int i = 0; i < cluster.Length; i++)
            {
                cluster[i] = new GpuResource
                {
                    Id = gpuIds[i],
                    MemoryGB = 16, // Standard VRAM
                    IsOccupied = (i == 0) // First GPU occupied by initial agent
                };
            }
        }
    }
}
