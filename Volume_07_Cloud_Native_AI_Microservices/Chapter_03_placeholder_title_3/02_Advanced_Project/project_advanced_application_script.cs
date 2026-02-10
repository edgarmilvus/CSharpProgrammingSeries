
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

namespace ScalableInferencePipeline
{
    // 1. DATA MODEL: Represents a single inference request.
    // In a microservices architecture, data contracts must be strictly defined.
    // We use a simple class here to encapsulate the input payload.
    public class InferenceRequest
    {
        public string RequestId { get; set; }
        public string InputData { get; set; }
        public DateTime Timestamp { get; set; }
    }

    // 2. DATA MODEL: Represents the result of an inference.
    // Includes metadata for observability (processing time, model version).
    public class InferenceResult
    {
        public string RequestId { get; set; }
        public string Prediction { get; set; }
        public double ConfidenceScore { get; set; }
        public TimeSpan ProcessingTime { get; set; }
        public string ModelVersion { get; set; }
    }

    // 3. ABSTRACTION: The "Inference Agent".
    // This represents a containerized worker node. It manages its own lifecycle
    // and resource utilization (simulated via thread sleep).
    public class InferenceAgent
    {
        private readonly string _agentId;
        private readonly string _modelVersion;
        private readonly Random _random;

        public InferenceAgent(string agentId, string modelVersion)
        {
            _agentId = agentId;
            _modelVersion = modelVersion;
            _random = new Random();
        }

        // Simulates the heavy computation of a Neural Network forward pass.
        // In a real scenario, this would call a TensorFlow/PyTorch runtime via FFI.
        public InferenceResult Process(InferenceRequest request)
        {
            var startTime = DateTime.Now;

            // Simulate computational load (100ms to 500ms)
            int loadMilliseconds = _random.Next(100, 500);
            Thread.Sleep(loadMilliseconds);

            // Simulate model logic
            string prediction = request.InputData.Contains("cat") ? "Class: Feline" : "Class: Unknown";
            double confidence = _random.NextDouble() * (1.0 - 0.75) + 0.75; // 0.75 to 1.0

            var endTime = DateTime.Now;

            return new InferenceResult
            {
                RequestId = request.RequestId,
                Prediction = prediction,
                ConfidenceScore = Math.Round(confidence, 4),
                ProcessingTime = endTime - startTime,
                ModelVersion = _modelVersion
            };
        }
    }

    // 4. ORCHESTRATION LOGIC: The Auto-Scaler & Load Balancer.
    // This class simulates the Kubernetes Horizontal Pod Autoscaler (HPA) logic.
    // It monitors "queue depth" (pending requests) and adjusts the number of active agents.
    public class AgentOrchestrator
    {
        private readonly List<InferenceAgent> _activeAgents;
        private readonly Queue<InferenceRequest> _requestQueue;
        private readonly object _lock = new object(); // Thread safety for concurrent access
        private const int MAX_AGENTS = 5; // Simulates resource limits (CPU/Memory constraints)
        private const int SCALE_UP_THRESHOLD = 3; // Queue length triggers scale up
        private const int SCALE_DOWN_THRESHOLD = 1; // Queue length triggers scale down

        public AgentOrchestrator()
        {
            _activeAgents = new List<InferenceAgent>();
            _requestQueue = new Queue<InferenceRequest>();

            // Initialize with 1 agent (Baseline)
            ScaleUp();
        }

        // Ingests requests from the API Gateway
        public void EnqueueRequest(InferenceRequest request)
        {
            lock (_lock)
            {
                _requestQueue.Enqueue(request);
                Console.WriteLine($"[Orchestrator] Request {request.RequestId} queued. Queue Depth: {_requestQueue.Count}");
            }
            CheckScalingNeeds();
        }

        // The core scaling logic
        private void CheckScalingNeeds()
        {
            lock (_lock)
            {
                int queueDepth = _requestQueue.Count;
                int agentCount = _activeAgents.Count;

                // Logic: If queue is growing and we have capacity, scale up.
                if (queueDepth >= SCALE_UP_THRESHOLD && agentCount < MAX_AGENTS)
                {
                    Console.WriteLine($"[Orchestrator] High Load detected (Queue: {queueDepth}). Scaling UP...");
                    ScaleUp();
                }
                // Logic: If queue is emptying, scale down to save costs.
                else if (queueDepth <= SCALE_DOWN_THRESHOLD && agentCount > 1)
                {
                    Console.WriteLine($"[Orchestrator] Low Load detected (Queue: {queueDepth}). Scaling DOWN...");
                    ScaleDown();
                }
            }
        }

        private void ScaleUp()
        {
            // Simulate container startup time
            Thread.Sleep(200); 
            string version = "v1.2.0";
            var newAgent = new InferenceAgent($"Agent-{_activeAgents.Count + 1}", version);
            _activeAgents.Add(newAgent);
            Console.WriteLine($"[Orchestrator] New Agent deployed: {newAgent.GetHashCode()}. Total Agents: {_activeAgents.Count}");
        }

        private void ScaleDown()
        {
            if (_activeAgents.Count > 0)
            {
                // Remove the oldest agent (standard rolling update strategy)
                var removedAgent = _activeAgents[0];
                _activeAgents.RemoveAt(0);
                Console.WriteLine($"[Orchestrator] Agent terminated: {removedAgent.GetHashCode()}. Total Agents: {_activeAgents.Count}");
            }
        }

        // Dispatches work to available agents (Load Balancing)
        public void ProcessQueue()
        {
            InferenceRequest request = null;

            lock (_lock)
            {
                if (_requestQueue.Count > 0)
                {
                    request = _requestQueue.Dequeue();
                }
            }

            if (request != null)
            {
                // Round-robin selection (simplified)
                var agent = _activeAgents[0]; 
                var result = agent.Process(request);
                LogResult(result);
            }
        }

        private void LogResult(InferenceResult result)
        {
            Console.WriteLine($"[Result] Req: {result.RequestId} | Pred: {result.Prediction} | Score: {result.ConfidenceScore} | Time: {result.ProcessingTime.TotalMilliseconds}ms | AgentVer: {result.ModelVersion}");
        }

        public int GetQueueCount()
        {
            lock (_lock) return _requestQueue.Count;
        }

        public int GetAgentCount()
        {
            lock (_lock) return _activeAgents.Count;
        }
    }

    // 5. MAIN PROGRAM: Simulates the Microservices Ecosystem
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== Cloud-Native AI Inference Pipeline Simulation ===");
            Console.WriteLine("Simulating K8s Orchestrator, Auto-Scaling, and Agent Processing...");
            Console.WriteLine("----------------------------------------------------------");

            var orchestrator = new AgentOrchestrator();
            var random = new Random();

            // Simulation Loop: Represents the Kubernetes Event Loop
            // In a real deployment, this would be an infinite loop handling HTTP requests.
            for (int cycle = 1; cycle <= 20; cycle++)
            {
                Console.WriteLine($"\n--- Cycle {cycle} ---");

                // 1. Traffic Ingestion (Simulating API Gateway)
                // Randomly generate bursty traffic to test auto-scaling
                int incomingRequests = random.Next(0, 5); 
                for (int i = 0; i < incomingRequests; i++)
                {
                    var req = new InferenceRequest
                    {
                        RequestId = Guid.NewGuid().ToString().Substring(0, 8),
                        InputData = (i % 2 == 0) ? "image_of_a_cat" : "image_of_noise",
                        Timestamp = DateTime.Now
                    };
                    orchestrator.EnqueueRequest(req);
                }

                // 2. Worker Processing Cycle
                // Agents pick up work from the queue. 
                // We process a batch equal to the number of active agents to simulate parallel processing.
                int agents = orchestrator.GetAgentCount();
                int queueDepth = orchestrator.GetQueueCount();
                int itemsToProcess = (queueDepth < agents) ? queueDepth : agents;

                for (int i = 0; i < itemsToProcess; i++)
                {
                    orchestrator.ProcessQueue();
                }

                // 3. Observability Check
                Console.WriteLine($"[Metrics] Active Agents: {orchestrator.GetAgentCount()} | Pending Queue: {orchestrator.GetQueueCount()}");

                // 4. Stabilization Delay
                // Simulates the tick rate of the controller manager
                Thread.Sleep(500); 
            }

            Console.WriteLine("----------------------------------------------------------");
            Console.WriteLine("Simulation Complete. Final State:");
            Console.WriteLine($"Total Active Agents: {orchestrator.GetAgentCount()}");
            Console.WriteLine($"Remaining Queue Items: {orchestrator.GetQueueCount()}");
        }
    }
}
