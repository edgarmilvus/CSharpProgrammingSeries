
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

namespace CloudNativeAgentSwarm
{
    // REAL-WORLD CONTEXT:
    // In a cloud-native AI environment, we manage a "swarm" of inference agents (microservices).
    // These agents might be containerized Python scripts, .NET workers, or specialized AI models.
    // The operational challenge is orchestrating these agents: distributing workload, handling failures,
    // and managing state (like session affinity) without a heavy orchestration engine like Kubernetes.
    // This console application simulates a "Kubernetes-lite" controller that manages a pool of AI agents,
    // routes incoming inference requests, and handles autoscaling based on CPU load (simulated).

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== Cloud-Native AI Agent Swarm Controller (Simulation) ===\n");

            // 1. Initialization
            // We initialize a controller that manages the lifecycle of our agents.
            SwarmController controller = new SwarmController();

            // 2. Bootstrap
            // Start with a baseline number of agents (replicas) to handle initial traffic.
            controller.InitializeSwarm(2);

            // 3. Simulation Loop
            // Simulate a burst of incoming inference requests.
            // In a real scenario, this would be an HTTP endpoint (e.g., ASP.NET Core Minimal API).
            Console.WriteLine("\n--- Incoming Traffic Burst (Simulating 10 Requests) ---\n");
            for (int i = 1; i <= 10; i++)
            {
                // Create a mock request with a payload (e.g., an image or text prompt).
                var request = new InferenceRequest { Id = i, PayloadSizeKB = new Random().Next(1, 10) };
                
                // Route the request to an available agent.
                controller.RouteRequest(request);
                
                // Simulate network latency and processing time.
                Thread.Sleep(200);
            }

            // 4. Autoscaling Check
            // After the burst, check metrics and scale the swarm if necessary.
            // This mimics the Kubernetes Horizontal Pod Autoscaler (HPA).
            Console.WriteLine("\n--- Autoscaling Analysis ---\n");
            controller.EvaluateAndScale();

            // 5. Stateful Cleanup
            // Gracefully shutdown agents and persist state if needed.
            Console.WriteLine("\n--- Shutting Down Swarm ---\n");
            controller.Shutdown();
        }
    }

    // Represents an individual AI inference agent (e.g., a container running a model).
    // This class encapsulates the state and behavior of a single microservice instance.
    class InferenceAgent
    {
        public int Id { get; private set; }
        public bool IsBusy { get; private set; }
        public int RequestsProcessed { get; private set; }
        public double CpuLoad { get; private set; } // Simulated metric (0.0 to 1.0)

        public InferenceAgent(int id)
        {
            Id = id;
            IsBusy = false;
            RequestsProcessed = 0;
            CpuLoad = 0.0;
        }

        // Simulates processing an inference request.
        // In a real implementation, this would invoke an ML.NET model or an external API.
        public void ProcessRequest(InferenceRequest request)
        {
            if (IsBusy)
            {
                Console.WriteLine($"[Agent {Id}] ERROR: Received request while busy. (Queue overflow simulation)");
                return;
            }

            IsBusy = true;
            Console.WriteLine($"[Agent {Id}] Processing Request #{request.Id} (Payload: {request.PayloadSizeKB}KB)...");

            // Simulate work duration based on payload size.
            int processingTime = request.PayloadSizeKB * 50;
            Thread.Sleep(processingTime);

            // Update internal metrics (Stateful behavior).
            RequestsProcessed++;
            
            // Simulate CPU load increase after processing.
            // Load decays over time, but processing adds immediate load.
            CpuLoad = Math.Min(1.0, CpuLoad + 0.2); 

            IsBusy = false;
            Console.WriteLine($"[Agent {Id}] Completed Request #{request.Id}. Status: OK.");
        }

        // Simulates the background decay of CPU load (housekeeping).
        public void DecayLoad()
        {
            if (CpuLoad > 0)
            {
                CpuLoad = Math.Max(0, CpuLoad - 0.05);
            }
        }
    }

    // Represents an incoming request to the system.
    struct InferenceRequest
    {
        public int Id { get; set; }
        public int PayloadSizeKB { get; set; }
    }

    // The Orchestrator. In Kubernetes terms, this acts as the Control Plane (specifically the Controller Manager).
    // It maintains the desired state (DesiredReplicas) and reconciles the actual state (CurrentAgents).
    class SwarmController
    {
        private List<InferenceAgent> _agents;
        private int _desiredReplicas;
        private int _agentCounter;

        public SwarmController()
        {
            _agents = new List<InferenceAgent>();
            _desiredReplicas = 0;
            _agentCounter = 1;
        }

        // --- ORCHESTRATION LOGIC ---

        // 1. Initialization: Bootstraps the initial set of agents.
        public void InitializeSwarm(int initialReplicas)
        {
            Console.WriteLine($"[Controller] Initializing Swarm with {initialReplicas} replicas...");
            _desiredReplicas = initialReplicas;
            ScaleUp(initialReplicas);
        }

        // 2. Request Routing: Load Balancing Logic.
        // Implements a "Round Robin" style load balancing strategy.
        // In production, this might be a Sidecar Proxy (Envoy) or Service Mesh.
        public void RouteRequest(InferenceRequest request)
        {
            InferenceAgent selectedAgent = null;

            // Strategy: Find the least busy agent.
            // We iterate through the list to find an available agent.
            // This is O(N) complexity. For massive scale, we'd use a priority queue or consistent hashing.
            foreach (var agent in _agents)
            {
                agent.DecayLoad(); // Background maintenance tick.
                
                if (!agent.IsBusy && agent.CpuLoad < 0.8)
                {
                    selectedAgent = agent;
                    break; // Found a suitable agent.
                }
            }

            // Fallback: If no agent is perfectly available, pick the one with the lowest load.
            if (selectedAgent == null)
            {
                Console.WriteLine($"[Controller] No ideal agent found. Selecting best effort...");
                double lowestLoad = 1.0;
                foreach (var agent in _agents)
                {
                    if (agent.CpuLoad < lowestLoad)
                    {
                        lowestLoad = agent.CpuLoad;
                        selectedAgent = agent;
                    }
                }
            }

            if (selectedAgent != null)
            {
                // Offload processing to the agent.
                // In a real microservice, this would be an async HTTP call.
                // Here we simulate the execution directly.
                selectedAgent.ProcessRequest(request);
            }
            else
            {
                Console.WriteLine($"[Controller] CRITICAL: All agents saturated. Request #{request.Id} queued or dropped.");
            }
        }

        // 3. Autoscaling Logic (HPA Simulation).
        // Checks metrics and adjusts the replica count to match load.
        public void EvaluateAndScale()
        {
            int totalRequests = 0;
            double totalLoad = 0;

            foreach (var agent in _agents)
            {
                totalRequests += agent.RequestsProcessed;
                totalLoad += agent.CpuLoad;
            }

            double avgLoad = totalLoad / _agents.Count;
            Console.WriteLine($"[Controller] Metrics: Avg Load = {avgLoad:P2}, Total Processed = {totalRequests}");

            // Autoscaling Policy:
            // If average load > 70%, scale up.
            // If average load < 20% and we have > 2 agents, scale down.
            if (avgLoad > 0.70)
            {
                int scaleAmount = 2; // Scale up aggressively to handle burst.
                Console.WriteLine($"[Controller] High Load detected. Scaling UP by {scaleAmount}...");
                ScaleUp(scaleAmount);
            }
            else if (avgLoad < 0.20 && _agents.Count > 2)
            {
                int scaleDownAmount = 1;
                Console.WriteLine($"[Controller] Low Load detected. Scaling DOWN by {scaleDownAmount}...");
                ScaleDown(scaleDownAmount);
            }
            else
            {
                Console.WriteLine($"[Controller] Load within acceptable limits. No scaling action taken.");
            }
        }

        // --- HELPER METHODS (Private) ---

        private void ScaleUp(int count)
        {
            for (int i = 0; i < count; i++)
            {
                var newAgent = new InferenceAgent(_agentCounter++);
                _agents.Add(newAgent);
                Console.WriteLine($"  -> Agent {newAgent.Id} provisioned and started.");
            }
        }

        private void ScaleDown(int count)
        {
            // Graceful shutdown: Process remaining requests before removing.
            // In Kubernetes, this is the 'TerminationGracePeriodSeconds'.
            for (int i = 0; i < count; i++)
            {
                if (_agents.Count > 0)
                {
                    var agentToRemove = _agents[_agents.Count - 1]; // Remove newest first (LIFO)
                    if (agentToRemove.IsBusy)
                    {
                        Console.WriteLine($"  -> Agent {agentToRemove.Id} is busy. Waiting for drain...");
                        Thread.Sleep(500); // Simulate waiting for drain
                    }
                    Console.WriteLine($"  -> Agent {agentToRemove.Id} deprovisioned.");
                    _agents.Remove(agentToRemove);
                }
            }
        }

        public void Shutdown()
        {
            Console.WriteLine("[Controller] Graceful Shutdown initiated...");
            // Drain all agents
            foreach (var agent in _agents)
            {
                if (agent.IsBusy)
                {
                    Console.WriteLine($"[Controller] Waiting for Agent {agent.Id} to finish current task...");
                    Thread.Sleep(200);
                }
            }
            _agents.Clear();
            Console.WriteLine("[Controller] Swarm terminated.");
        }
    }
}
