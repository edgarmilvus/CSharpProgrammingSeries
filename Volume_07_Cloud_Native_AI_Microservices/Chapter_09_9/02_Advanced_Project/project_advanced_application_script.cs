
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
    // ---------------------------------------------------------
    // REAL-WORLD CONTEXT: MANUFACTURING DEFECT DETECTION
    // ---------------------------------------------------------
    // In a smart factory, hundreds of cameras capture images of products
    // moving on a conveyor belt. We need to process these images in real-time
    // to detect defects. This application simulates a distributed microservice
    // architecture where multiple "Agent" containers process images concurrently,
    // and a central Orchestrator manages the workload, scaling resources dynamically
    // based on the queue pressure (simulating Kubernetes Horizontal Pod Autoscaling).

    // ---------------------------------------------------------
    // 1. CORE AGENT LOGIC (Containerized Worker)
    // ---------------------------------------------------------
    // Represents a single AI inference container (e.g., a pod in Kubernetes).
    // It encapsulates state and processing logic, isolated from other agents.
    public class InferenceAgent
    {
        public string AgentId { get; private set; }
        public bool IsBusy { get; private set; }
        private Random _rng;

        public InferenceAgent(string id)
        {
            AgentId = id;
            IsBusy = false;
            _rng = new Random(Guid.NewGuid().GetHashCode()); // Seed for uniqueness
        }

        // Simulates heavy AI model inference (e.g., running a CNN on an image).
        // In a real scenario, this would call an ONNX runtime or TensorFlow Serving.
        public async Task<string> ProcessImageAsync(string imageId)
        {
            if (IsBusy) throw new InvalidOperationException($"Agent {AgentId} is overloaded.");

            IsBusy = true;
            Console.WriteLine($"[Agent {AgentId}] Starting inference on Image: {imageId}");

            // Simulate variable processing time (latency) based on image complexity
            int processingTimeMs = _rng.Next(500, 2000); 
            await Task.Delay(processingTimeMs);

            // Simulate inference result (Defect vs. No Defect)
            bool isDefect = _rng.Next(0, 10) > 7; // 30% chance of defect
            string result = isDefect ? "DEFECT_DETECTED" : "PASS";

            Console.WriteLine($"[Agent {AgentId}] Finished Image: {imageId} | Result: {result} | Time: {processingTimeMs}ms");
            IsBusy = false;
            return result;
        }
    }

    // ---------------------------------------------------------
    // 2. ORCHESTRATOR LOGIC (Kubernetes Controller Pattern)
    // ---------------------------------------------------------
    // This class simulates the logic of a Kubernetes Controller or an
    // Horizontal Pod Autoscaler (HPA). It monitors the workload and
    // adjusts the number of active agent containers.
    public class AgentOrchestrator
    {
        private List<InferenceAgent> _activeAgents;
        private Queue<string> _imageQueue;
        private const int MAX_AGENTS = 5; // Simulates resource limits (CPU/Memory constraints)
        private const int SCALE_UP_THRESHOLD = 3; // Queue length to trigger scaling
        private const int SCALE_DOWN_THRESHOLD = 1; // Queue length to trigger downscaling

        public AgentOrchestrator()
        {
            _activeAgents = new List<InferenceAgent>();
            _imageQueue = new Queue<string>();

            // Initial bootstrapping: Start with 1 agent (Minimum Replica Count)
            ScaleAgents(1);
        }

        // Ingests images from the camera feed
        public void EnqueueImage(string imageId)
        {
            _imageQueue.Enqueue(imageId);
            Console.WriteLine($"[Orchestrator] Image {imageId} added to queue. Queue Size: {_imageQueue.Count}");
        }

        // Main processing loop (Simulates the Kubernetes Control Loop)
        public async Task RunProcessingCycle()
        {
            // 1. METRICS COLLECTION (Observability)
            // In a real system, we would query Prometheus for queue depth and agent CPU usage.
            int queueCount = _imageQueue.Count;
            int agentCount = _activeAgents.Count;

            // 2. AUTOSCALING LOGIC (Reconciliation Loop)
            if (queueCount > SCALE_UP_THRESHOLD && agentCount < MAX_AGENTS)
            {
                Console.WriteLine($"[Orchestrator] High Load detected (Queue: {queueCount}). Scaling UP...");
                ScaleAgents(agentCount + 1);
            }
            else if (queueCount < SCALE_DOWN_THRESHOLD && agentCount > 1)
            {
                Console.WriteLine($"[Orchestrator] Low Load detected (Queue: {queueCount}). Scaling DOWN...");
                ScaleAgents(agentCount - 1);
            }

            // 3. WORKLOAD DISTRIBUTION (Load Balancing)
            // Iterate through agents and assign available work.
            foreach (var agent in _activeAgents)
            {
                if (_imageQueue.Count > 0 && !agent.IsBusy)
                {
                    string imageId = _imageQueue.Dequeue();
                    
                    // Fire and Forget (Async Processing)
                    // In a real microservice, this would be an HTTP call to the agent pod.
                    // We use Task.Run to simulate the non-blocking network call.
                    _ = Task.Run(async () => 
                    {
                        try 
                        {
                            await agent.ProcessImageAsync(imageId);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[Error] Agent {agent.AgentId} failed: {ex.Message}");
                        }
                    });
                }
            }
        }

        // Simulates Kubernetes scaling a Deployment (Adding/Removing Pods)
        private void ScaleAgents(int targetCount)
        {
            // Scale Up
            while (_activeAgents.Count < targetCount)
            {
                string newId = Guid.NewGuid().ToString().Substring(0, 4);
                _activeAgents.Add(new InferenceAgent(newId));
                Console.WriteLine($"[Orchestrator] New Agent Container started: {newId}");
            }

            // Scale Down (Graceful Termination)
            // We remove the oldest idle agents first.
            while (_activeAgents.Count > targetCount)
            {
                var agentToRemove = _activeAgents[0];
                if (!agentToRemove.IsBusy)
                {
                    _activeAgents.RemoveAt(0);
                    Console.WriteLine($"[Orchestrator] Agent Container terminated: {agentToRemove.AgentId}");
                }
                else
                {
                    // If agent is busy, we wait for it to finish in a real system (Pod Disruption Budget).
                    // Here, we just skip removal for this cycle.
                    Console.WriteLine($"[Orchestrator] Cannot terminate {agentToRemove.AgentId} (Busy). Waiting...");
                    break; 
                }
            }
        }
    }

    // ---------------------------------------------------------
    // 3. MAIN EXECUTION (Simulation Entry Point)
    // ---------------------------------------------------------
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("=== Starting Cloud-Native AI Inference Simulation ===\n");
            
            AgentOrchestrator orchestrator = new AgentOrchestrator();
            int simulationCycle = 0;

            // Simulation runs for a fixed number of cycles to demonstrate scaling behavior
            while (simulationCycle < 15)
            {
                Console.WriteLine($"\n--- Cycle {simulationCycle + 1} ---");

                // Simulate incoming traffic (Image ingestion)
                // Random burstiness mimics real-world API spikes
                Random rng = new Random();
                int imagesInThisCycle = rng.Next(0, 5); 
                
                for (int i = 0; i < imagesInThisCycle; i++)
                {
                    orchestrator.EnqueueImage($"IMG_{simulationCycle}_{i}");
                }

                // Execute the Orchestration Logic
                await orchestrator.RunProcessingCycle();

                // Simulate time passing (e.g., 1 second per cycle)
                await Task.Delay(1000);
                simulationCycle++;
            }

            Console.WriteLine("\n=== Simulation Complete ===");
            Console.WriteLine("Note: In a production K8s environment, this logic would be split into:");
            Console.WriteLine("1. Agent Containers (Python/C++ ONNX Runtime)");
            Console.WriteLine("2. K8s Deployment YAML (Resource limits, Probes)");
            Console.WriteLine("3. Horizontal Pod Autoscaler (HPA) Config");
        }
    }
}
