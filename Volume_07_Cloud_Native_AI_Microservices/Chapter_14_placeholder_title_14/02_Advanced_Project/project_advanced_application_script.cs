
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

namespace CloudNativeAgentOrchestrator
{
    /// <summary>
    /// Simulates a high-throughput AI inference pipeline.
    /// This application models a microservice architecture where incoming requests
    /// are routed to containerized AI agents. It demonstrates load balancing,
    /// autoscaling logic based on GPU utilization, and request queuing.
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== Cloud-Native AI Agent Orchestrator Simulation ===");
            Console.WriteLine("Initializing Inference Pipeline...\n");

            // 1. Initialize the Orchestrator
            // The orchestrator manages the pool of agents and the request queue.
            Orchestrator orchestrator = new Orchestrator();

            // 2. Start the Simulation Loop
            // We simulate a burst of incoming inference requests.
            Random rng = new Random();
            int totalRequests = 20;
            int simulatedDurationSeconds = 10;

            for (int i = 1; i <= totalRequests; i++)
            {
                // Simulate variable arrival times (bursty traffic)
                int delay = rng.Next(200, 800); 
                Thread.Sleep(delay);

                // Create a new inference request
                // In a real scenario, this would be an HTTP POST with image data.
                string requestId = $"REQ-{i:D3}";
                string payload = $"Image_Data_Block_{i}";
                
                Console.WriteLine($"[Ingress] New request received: {requestId}");

                // Submit to the orchestrator
                orchestrator.SubmitRequest(requestId, payload);
            }

            // 3. Run the Processing Engine
            // This simulates the background processing loop of the orchestrator.
            // It handles scaling decisions and dispatching work to agents.
            Console.WriteLine("\n--- Pipeline Active: Processing Queue ---\n");
            
            // We run the loop for a fixed time to observe scaling behavior
            for (int tick = 0; tick < simulatedDurationSeconds; tick++)
            {
                orchestrator.ProcessCycle();
                Thread.Sleep(1000); // 1-second tick
            }

            // 4. Final Report
            Console.WriteLine("\n--- Simulation Complete: Final Metrics ---");
            orchestrator.PrintMetrics();
        }
    }

    /// <summary>
    /// Represents a single AI Inference Request.
    /// </summary>
    public class InferenceRequest
    {
        public string Id { get; set; }
        public string Payload { get; set; }
        public DateTime CreatedAt { get; set; }

        public InferenceRequest(string id, string payload)
        {
            Id = id;
            Payload = payload;
            CreatedAt = DateTime.Now;
        }
    }

    /// <summary>
    /// Represents a Containerized AI Agent (e.g., a GPU-backed container).
    /// Handles the actual inference computation.
    /// </summary>
    public class AIInferenceAgent
    {
        public int AgentId { get; private set; }
        public bool IsBusy { get; private set; }
        public int TasksCompleted { get; private set; }
        
        // Simulates GPU VRAM usage (e.g., 8GB, 16GB, 24GB)
        public int VramCapacityMB { get; private set; } 
        public int VramUsageMB { get; private set; }

        private Random _rng;

        public AIInferenceAgent(int id, int vramCapacity)
        {
            AgentId = id;
            VramCapacityMB = vramCapacity;
            IsBusy = false;
            TasksCompleted = 0;
            VramUsageMB = 0;
            _rng = new Random();
        }

        /// <summary>
        /// Accepts a request and begins processing.
        /// In a real system, this would invoke the model via a gRPC or HTTP client.
        /// </summary>
        public void StartProcessing(InferenceRequest request)
        {
            if (IsBusy) throw new InvalidOperationException("Agent is already busy.");

            IsBusy = true;
            
            // Simulate allocating VRAM for the model weights and input tensor
            int requiredVram = _rng.Next(1000, 3000); // 1GB to 3GB usage
            VramUsageMB = requiredVram;

            Console.WriteLine($"  [Agent {AgentId}] Starting inference for {request.Id} (VRAM: {VramUsageMB}MB)");

            // Simulate async processing time (e.g., 500ms to 1500ms)
            int processingTime = _rng.Next(500, 1500);

            // We use a Task to simulate non-blocking execution.
            // Note: In a strict "no advanced features" mode, we avoid async/await keywords
            // if the chapter hasn't introduced them, but Task is essential for threading simulation.
            Task.Run(async () => 
            {
                await Task.Delay(processingTime);
                
                // Completion logic (must be thread-safe when accessing shared state)
                CompleteProcessing();
            });
        }

        private void CompleteProcessing()
        {
            IsBusy = false;
            TasksCompleted++;
            VramUsageMB = 0; // Release VRAM
            Console.WriteLine($"  [Agent {AgentId}] Completed inference. Result: 'Detected Object'.");
        }

        /// <summary>
        /// Returns current utilization percentage (0.0 to 1.0).
        /// </summary>
        public double GetUtilization()
        {
            if (VramCapacityMB == 0) return 0;
            return (double)VramUsageMB / VramCapacityMB;
        }
    }

    /// <summary>
    /// The Orchestrator manages the lifecycle of agents, scales them based on load,
    /// and balances incoming requests using a queue.
    /// </summary>
    public class Orchestrator
    {
        private List<AIInferenceAgent> _activeAgents;
        private Queue<InferenceRequest> _requestQueue;
        
        // Configuration
        private const int MAX_AGENTS = 5;
        private const int VRAM_PER_AGENT = 8000; // 8GB GPU limit
        private const double SCALE_UP_THRESHOLD = 0.75; // 75% utilization triggers scale up
        private const double SCALE_DOWN_THRESHOLD = 0.20; // 20% utilization triggers scale down

        // Metrics
        private int _totalRequestsProcessed = 0;
        private int _totalScaleEvents = 0;

        public Orchestrator()
        {
            _activeAgents = new List<AIInferenceAgent>();
            _requestQueue = new Queue<InferenceRequest>();

            // Start with 1 agent (Kubernetes style: start small, scale out)
            ScaleOut();
        }

        /// <summary>
        /// Accepts a request from the ingress controller.
        /// </summary>
        public void SubmitRequest(string id, string payload)
        {
            var req = new InferenceRequest(id, payload);
            _requestQueue.Enqueue(req);
        }

        /// <summary>
        /// The main processing loop. In a real app, this runs on a timer or background service.
        /// </summary>
        public void ProcessCycle()
        {
            // 1. Check Queue and Dispatch
            // We attempt to match waiting requests to available agents.
            if (_requestQueue.Count > 0)
            {
                DispatchRequests();
            }

            // 2. Evaluate Metrics for Autoscaling
            EvaluateScaling();

            // 3. Report Status
            PrintStatus();
        }

        /// <summary>
        /// Dispatches requests from queue to idle agents.
        /// </summary>
        private void DispatchRequests()
        {
            // Find idle agents
            List<AIInferenceAgent> idleAgents = new List<AIInferenceAgent>();
            foreach (var agent in _activeAgents)
            {
                if (!agent.IsBusy)
                {
                    idleAgents.Add(agent);
                }
            }

            // Dispatch up to the number of available agents or queue depth
            int dispatchCount = Math.Min(idleAgents.Count, _requestQueue.Count);
            
            for (int i = 0; i < dispatchCount; i++)
            {
                InferenceRequest req = _requestQueue.Dequeue();
                AIInferenceAgent agent = idleAgents[i];
                
                agent.StartProcessing(req);
                _totalRequestsProcessed++;
            }
        }

        /// <summary>
        /// Calculates average utilization and decides to scale in/out.
        /// </summary>
        private void EvaluateScaling()
        {
            if (_activeAgents.Count == 0) return;

            double totalUtilization = 0;
            int busyCount = 0;

            foreach (var agent in _activeAgents)
            {
                if (agent.IsBusy)
                {
                    totalUtilization += agent.GetUtilization();
                    busyCount++;
                }
            }

            // Avoid division by zero if no agents are busy
            double avgUtilization = (busyCount > 0) ? (totalUtilization / busyCount) : 0;

            // Autoscaling Logic
            if (avgUtilization > SCALE_UP_THRESHOLD && _activeAgents.Count < MAX_AGENTS)
            {
                ScaleOut();
            }
            else if (avgUtilization < SCALE_DOWN_THRESHOLD && _activeAgents.Count > 1)
            {
                ScaleIn();
            }
        }

        /// <summary>
        /// Adds a new agent to the pool.
        /// </summary>
        private void ScaleOut()
        {
            int newId = _activeAgents.Count + 1;
            var newAgent = new AIInferenceAgent(newId, VRAM_PER_AGENT);
            _activeAgents.Add(newAgent);
            _totalScaleEvents++;
            Console.WriteLine($"  [Orchestrator] SCALE-OUT: Agent {newId} provisioned. Total Agents: {_activeAgents.Count}");
        }

        /// <summary>
        /// Removes an agent from the pool (gracefully).
        /// </summary>
        private void ScaleIn()
        {
            // In a real system, we would drain connections before termination.
            // Here, we simply remove the last added agent if it's idle.
            var lastAgent = _activeAgents[_activeAgents.Count - 1];
            if (!lastAgent.IsBusy)
            {
                _activeAgents.RemoveAt(_activeAgents.Count - 1);
                _totalScaleEvents++;
                Console.WriteLine($"  [Orchestrator] SCALE-IN: Agent {lastAgent.AgentId} decommissioned. Total Agents: {_activeAgents.Count}");
            }
        }

        /// <summary>
        /// Prints the current state of the cluster.
        /// </summary>
        public void PrintStatus()
        {
            Console.Write($"[Status] Queue: {_requestQueue.Count} | Agents: {_activeAgents.Count} [");
            foreach (var agent in _activeAgents)
            {
                Console.Write(agent.IsBusy ? "#" : ".");
            }
            Console.WriteLine("]");
        }

        public void PrintMetrics()
        {
            Console.WriteLine($"Total Requests Processed: {_totalRequestsProcessed}");
            Console.WriteLine($"Total Autoscaling Events: {_totalScaleEvents}");
            Console.WriteLine($"Remaining Queue Depth:    {_requestQueue.Count}");
        }
    }
}
