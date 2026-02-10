
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

namespace CloudNativeAgentScaling
{
    // ============================================================================
    // 1. DATA MODEL: Agent Request & Response
    // ============================================================================
    // Represents a single inference request coming from a client (e.g., a user prompt).
    // In a real system, this would be serialized as JSON (e.g., via HTTP POST).
    public class InferenceRequest
    {
        public int RequestId { get; set; }
        public string Prompt { get; set; }
        public int ComplexityLevel { get; set; } // 1-10: Simulates compute cost
    }

    public class InferenceResult
    {
        public int RequestId { get; set; }
        public string Response { get; set; }
        public bool IsSuccess { get; set; }
    }

    // ============================================================================
    // 2. CORE LOGIC: The AI Agent (Containerized Microservice)
    // ============================================================================
    // This class simulates a containerized AI model (e.g., a Python/Torch process 
    // wrapped in C# or a heavy .NET inference engine). 
    // It mimics the "heavy lifting" of neural network inference.
    public class AIModelAgent
    {
        private readonly string _agentId;
        private readonly Random _random = new Random();

        public AIModelAgent(string agentId)
        {
            _agentId = agentId;
        }

        // Simulates the time-consuming inference process.
        // In a real scenario, this calls an ONNX runtime or TensorFlow session.
        public async Task<InferenceResult> ProcessAsync(InferenceRequest request)
        {
            // Simulate processing delay based on complexity (100ms per complexity unit)
            int delay = request.ComplexityLevel * 100;
            
            // Use Task.Delay to simulate non-blocking CPU/GPU work
            await Task.Delay(delay);

            // Simulate occasional failure (e.g., model timeout or GPU OOM)
            bool success = _random.Next(0, 10) > 1; // 90% success rate

            return new InferenceResult
            {
                RequestId = request.RequestId,
                IsSuccess = success,
                Response = success 
                    ? $"Processed by Agent [{_agentId}] in {delay}ms: '{request.Prompt}'" 
                    : $"Agent [{_agentId}] failed to process request."
            };
        }
    }

    // ============================================================================
    // 3. ORCHESTRATION: The Load Balancer & Auto-Scaler
    // ============================================================================
    // This simulates the Kubernetes Service (Load Balancer) and HPA (Horizontal Pod Autoscaler).
    // It manages a pool of AI agents (Pods) and routes traffic based on availability.
    public class InferenceOrchestrator
    {
        // Pool of active "Pods" (AI Agents)
        private readonly List<AIModelAgent> _activeAgents = new List<AIModelAgent>();
        
        // Queue for requests that cannot be immediately processed (Backpressure)
        private readonly Queue<InferenceRequest> _pendingQueue = new Queue<InferenceRequest>();
        
        // Configuration for scaling
        private const int MaxAgents = 5; // Max replicas
        private const int MinAgents = 1; // Min replicas
        private const int ScaleUpThreshold = 3; // Queue length trigger
        private const int ScaleDownThreshold = 1; // Queue length trigger

        private int _agentCounter = 0;
        private readonly object _lock = new object();

        public InferenceOrchestrator()
        {
            // Initialize with minimum pods
            ScaleUp();
        }

        // -------------------------------------------------------------------------
        // 3.1 Request Routing (Service Mesh Logic)
        // -------------------------------------------------------------------------
        public async Task<InferenceResult> HandleRequest(InferenceRequest request)
        {
            lock (_lock)
            {
                // Check if we have an available agent (Pod) to handle the request immediately
                if (_activeAgents.Count > 0)
                {
                    // Simple Round-Robin Load Balancing
                    AIModelAgent selectedAgent = _activeAgents[0];
                    _activeAgents.RemoveAt(0); // Mark as busy
                    
                    // Return the task to be awaited asynchronously
                    return ProcessWithAgent(selectedAgent, request);
                }
                else
                {
                    // No agents available (or all are busy). Queue the request.
                    _pendingQueue.Enqueue(request);
                    Console.WriteLine($"[Queue] Request {request.RequestId} queued. Queue Size: {_pendingQueue.Count}");
                    
                    // Trigger Auto-Scaling Logic
                    CheckAutoScaling();
                    
                    // Return a task that will complete later when an agent picks it up
                    // (Simplified for this console app: we won't await the queue processing here directly,
                    // but in a real async system, this would be a TaskCompletionSource).
                    // For this simulation, we will handle the queue in a separate loop below.
                    return Task.FromResult(new InferenceResult { RequestId = request.RequestId, IsSuccess = false, Response = "Queued" });
                }
            }
        }

        // Helper to simulate async processing and returning the agent to the pool
        private async Task<InferenceResult> ProcessWithAgent(AIModelAgent agent, InferenceRequest request)
        {
            try
            {
                var result = await agent.ProcessAsync(request);
                return result;
            }
            finally
            {
                lock (_lock)
                {
                    // Return agent to pool (Mark as Ready)
                    _activeAgents.Add(agent);
                    // Immediately try to process next item in queue
                    ProcessQueue();
                }
            }
        }

        // -------------------------------------------------------------------------
        // 3.2 Queue Processing Logic
        // -------------------------------------------------------------------------
        private void ProcessQueue()
        {
            while (_pendingQueue.Count > 0 && _activeAgents.Count > 0)
            {
                var request = _pendingQueue.Dequeue();
                var agent = _activeAgents[0];
                _activeAgents.RemoveAt(0);
                
                // Fire and forget processing for queued items (in a real app, we'd await properly)
                // We use a separate task to avoid blocking the lock
                Task.Run(() => ProcessWithAgent(agent, request));
            }
        }

        // -------------------------------------------------------------------------
        // 3.3 Auto-Scaling Logic (Kubernetes HPA Simulation)
        // -------------------------------------------------------------------------
        private void CheckAutoScaling()
        {
            // Logic: If queue length > Threshold, add more pods (Scale Up)
            if (_pendingQueue.Count >= ScaleUpThreshold && _activeAgents.Count + _pendingQueue.Count < MaxAgents)
            {
                ScaleUp();
            }
            // Logic: If queue is empty and we have more than MinAgents, remove pods (Scale Down)
            else if (_pendingQueue.Count <= ScaleDownThreshold && _activeAgents.Count > MinAgents)
            {
                // In a real system, scale down happens after a stabilization window.
                // Here we simulate it immediately for demonstration.
                ScaleDown();
            }
        }

        private void ScaleUp()
        {
            if (_activeAgents.Count < MaxAgents)
            {
                _agentCounter++;
                var newAgent = new AIModelAgent($"Pod-{_agentCounter}");
                _activeAgents.Add(newAgent);
                Console.WriteLine($"[Scaling] SCALE UP: Added Agent Pod-{_agentCounter}. Total Active: {_activeAgents.Count}");
            }
        }

        private void ScaleDown()
        {
            if (_activeAgents.Count > MinAgents)
            {
                var agent = _activeAgents[0];
                _activeAgents.RemoveAt(0);
                Console.WriteLine($"[Scaling] SCALE DOWN: Removed Agent {GetAgentId(agent)}. Total Active: {_activeAgents.Count}");
            }
        }
        
        // Helper for display
        private string GetAgentId(AIModelAgent agent)
        {
            // Reflection hack just for display purposes since ID is private in this simplified model
            return "Pod"; 
        }

        public int GetQueueSize() => _pendingQueue.Count;
        public int GetActiveAgentCount() => _activeAgents.Count;
    }

    // ============================================================================
    // 4. MAIN PROGRAM: Simulation Entry Point
    // ============================================================================
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("=== Cloud-Native AI Inference Scaling Simulation ===");
            Console.WriteLine("Simulating: Kubernetes Pods, Load Balancing, and HPA Autoscaling");
            Console.WriteLine("========================================================\n");

            var orchestrator = new InferenceOrchestrator();
            var random = new Random();
            int totalRequests = 0;

            // Simulation Loop: Acts as the external traffic generator
            while (totalRequests < 20) // Run a short simulation
            {
                // 1. Generate Traffic (Bursty pattern)
                bool generateTraffic = random.Next(0, 3) > 0; // 66% chance of request per tick
                
                if (generateTraffic)
                {
                    totalRequests++;
                    var request = new InferenceRequest
                    {
                        RequestId = totalRequests,
                        Prompt = "Generate a summary of cloud-native patterns",
                        ComplexityLevel = random.Next(2, 6) // Varying load
                    };

                    Console.WriteLine($"[Client] Sending Request #{request.RequestId} (Complexity: {request.ComplexityLevel})");
                    
                    // 2. Send to Orchestrator (Ingress -> Service)
                    var task = orchestrator.HandleRequest(request);
                    
                    // Note: In a real async app, we wouldn't await immediately here to allow 
                    // the main loop to continue accepting requests. 
                    // We simulate this by handling responses in a separate logical flow or just logging.
                }

                // 3. Simulate Time Passing (Tick)
                await Task.Delay(500); // 500ms simulation tick

                // 4. Display System State
                Console.WriteLine($"   [System State] Active Pods: {orchestrator.GetActiveAgentCount()} | Queue Size: {orchestrator.GetQueueSize()}");
                Console.WriteLine("--------------------------------------------------------");
            }

            // Allow remaining queue to drain
            Console.WriteLine("\nTraffic generation stopped. Waiting for queue to drain...");
            while (orchestrator.GetQueueSize() > 0)
            {
                await Task.Delay(500);
            }

            Console.WriteLine("Simulation Complete. System Stabilized.");
        }
    }
}
