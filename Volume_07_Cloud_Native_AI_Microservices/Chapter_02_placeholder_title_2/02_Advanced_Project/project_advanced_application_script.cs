
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

namespace CloudNativeAgents
{
    // ==========================================
    // REAL-WORLD CONTEXT
    // ==========================================
    // Problem: An e-commerce platform uses specialized AI agents to handle customer queries.
    // - Agent 1: Inventory Agent (Checks stock levels from a database)
    // - Agent 2: Pricing Agent (Calculates discounts based on user tier)
    // - Agent 3: Recommendation Agent (Suggests similar products)
    //
    // Constraint: These agents must be containerized microservices. They communicate via
    // asynchronous message passing (simulating a service mesh like Istio or a message queue like RabbitMQ).
    // They must scale independently based on workload (CPU/GPU inference load).
    // ==========================================

    // ==========================================
    // SHARED DATA MODELS (DTOs)
    // ==========================================
    // Represents a request payload sent between microservices.
    // In a real scenario, this would be JSON/Protobuf over HTTP/gRPC.
    public class ServiceRequest
    {
        public string RequestId { get; set; }
        public string ProductId { get; set; }
        public string UserId { get; set; }
        public string Intent { get; set; } // e.g., "buy", "query", "recommend"
        public Dictionary<string, object> Metadata { get; set; }

        public ServiceRequest(string productId, string userId, string intent)
        {
            RequestId = Guid.NewGuid().ToString();
            ProductId = productId;
            UserId = userId;
            Intent = intent;
            Metadata = new Dictionary<string, object>();
        }
    }

    // Represents the response payload.
    public class ServiceResponse
    {
        public string RequestId { get; set; }
        public bool Success { get; set; }
        public string Data { get; set; } // JSON string or serialized data
        public string SourceAgent { get; set; }

        public ServiceResponse(string requestId, bool success, string data, string sourceAgent)
        {
            RequestId = requestId;
            Success = success;
            Data = data;
            SourceAgent = sourceAgent;
        }
    }

    // ==========================================
    // AGENT BASE CLASS (Microservice Abstraction)
    // ==========================================
    // Simulates a containerized agent with a specific responsibility.
    // In a real deployment, this class would wrap an HTTP listener or gRPC server.
    public abstract class AgentMicroservice
    {
        protected string AgentName;
        protected int ProcessingDelayMs; // Simulates inference latency (CPU/GPU bound)

        public AgentMicroservice(string name, int delay)
        {
            AgentName = name;
            ProcessingDelayMs = delay;
        }

        // Simulates the container's entry point (e.g., HTTP POST handler)
        public virtual async Task<ServiceResponse> ProcessRequestAsync(ServiceRequest request)
        {
            // Simulate network latency and processing time
            await Task.Delay(ProcessingDelayMs);

            // Core logic defined in derived classes
            string result = ExecuteCoreLogic(request);
            
            return new ServiceResponse(request.RequestId, true, result, AgentName);
        }

        protected abstract string ExecuteCoreLogic(ServiceRequest request);
    }

    // ==========================================
    // CONCRETE AGENT IMPLEMENTATIONS
    // ==========================================

    // 1. Inventory Agent: Checks stock availability
    public class InventoryAgent : AgentMicroservice
    {
        public InventoryAgent() : base("Inventory-Service-v1", 100) { }

        protected override string ExecuteCoreLogic(ServiceRequest request)
        {
            // Simulate database lookup
            bool inStock = request.ProductId.GetHashCode() % 2 == 0; // Pseudo-random logic
            return $"{{\"stock\": {(inStock ? 50 : 0)}, \"warehouse\": \"US-East\"}}";
        }
    }

    // 2. Pricing Agent: Calculates dynamic pricing
    public class PricingAgent : AgentMicroservice
    {
        public PricingAgent() : base("Pricing-Service-v1", 150) { }

        protected override string ExecuteCoreLogic(ServiceRequest request)
        {
            // Simulate complex calculation (GPU intensive in real scenarios)
            double basePrice = 99.99;
            double discount = request.UserId.GetHashCode() % 10 == 0 ? 0.15 : 0.05; // VIP discount
            double finalPrice = basePrice * (1 - discount);
            return $"{{\"base\": {basePrice}, \"discount\": {discount}, \"final\": {finalPrice:F2}}}";
        }
    }

    // 3. Recommendation Agent: Suggests items
    public class RecommendationAgent : AgentMicroservice
    {
        public RecommendationAgent() : base("Recommendation-Service-v2", 200) { }

        protected override string ExecuteCoreLogic(ServiceRequest request)
        {
            // Simulate ML model inference
            string[] similarItems = { "prod_101", "prod_102", "prod_103" };
            return $"{{\"suggestions\": [\"{string.Join("\",\"", similarItems)}\"]}}";
        }
    }

    // ==========================================
    // ORCHESTRATION LAYER (Service Mesh / Workflow Engine)
    // ==========================================
    // This represents the logic that routes requests between microservices.
    // In Kubernetes, this could be an API Gateway or an orchestrator pod.
    public class WorkflowOrchestrator
    {
        private readonly Dictionary<string, AgentMicroservice> _agents;

        public WorkflowOrchestrator()
        {
            _agents = new Dictionary<string, AgentMicroservice>
            {
                { "inventory", new InventoryAgent() },
                { "pricing", new PricingAgent() },
                { "recommendation", new RecommendationAgent() }
            };
        }

        // Routes the request to the appropriate agent or chain of agents
        public async Task<ServiceResponse> RouteRequestAsync(ServiceRequest request)
        {
            Console.WriteLine($"[Orchestrator] Received Request ID: {request.RequestId} | Intent: {request.Intent}");

            AgentMicroservice targetAgent = null;

            // Basic routing logic based on intent
            if (request.Intent == "query")
            {
                targetAgent = _agents["inventory"];
            }
            else if (request.Intent == "buy")
            {
                // In a real scenario, we might fan-out to multiple agents (Inventory + Pricing)
                // For this example, we chain them: Inventory -> Pricing
                var inventoryResp = await _agents["inventory"].ProcessRequestAsync(request);
                
                // Check stock before calling pricing
                if (inventoryResp.Data.Contains("\"stock\": 0"))
                {
                    return new ServiceResponse(request.RequestId, false, "Out of Stock", "Orchestrator");
                }

                var pricingResp = await _agents["pricing"].ProcessRequestAsync(request);
                
                // Aggregate results
                return new ServiceResponse(
                    request.RequestId, 
                    true, 
                    $"{{\"inventory\": {inventoryResp.Data}, \"pricing\": {pricingResp.Data}}}", 
                    "Orchestrator-Aggregator"
                );
            }
            else if (request.Intent == "recommend")
            {
                targetAgent = _agents["recommendation"];
            }

            if (targetAgent != null)
            {
                return await targetAgent.ProcessRequestAsync(request);
            }

            return new ServiceResponse(request.RequestId, false, "Unknown Intent", "Orchestrator");
        }
    }

    // ==========================================
    // SCALING SIMULATOR (Horizontal Pod Autoscaler)
    // ==========================================
    // Simulates Kubernetes HPA behavior based on CPU/Memory or Queue Depth.
    public class ScalingManager
    {
        private int _currentPodCount = 1;
        private const int MaxPods = 10;
        private const int Threshold = 5; // Requests per second threshold per pod

        // Simulates the metrics server checking load
        public void EvaluateScaling(int incomingRequestRate)
        {
            int targetPods = (incomingRequestRate / Threshold) + 1; // +1 for buffer
            if (targetPods > MaxPods) targetPods = MaxPods;

            if (targetPods > _currentPodCount)
            {
                Console.WriteLine($"[HPA] Scaling UP: {_currentPodCount} -> {targetPods} Pods (Load: {incomingRequestRate} req/s)");
                _currentPodCount = targetPods;
            }
            else if (targetPods < _currentPodCount)
            {
                Console.WriteLine($"[HPA] Scaling DOWN: {_currentPodCount} -> {targetPods} Pods");
                _currentPodCount = targetPods;
            }
            else
            {
                Console.WriteLine($"[HPA] Stable: {_currentPodCount} Pods");
            }
        }
    }

    // ==========================================
    // MAIN PROGRAM (Entry Point)
    // ==========================================
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("=== Cloud-Native AI Agent System Simulation ===");
            Console.WriteLine("Simulating Container Orchestration & Scaling...\n");

            var orchestrator = new WorkflowOrchestrator();
            var scaler = new ScalingManager();

            // Simulate a burst of incoming traffic (e.g., Black Friday sale)
            int requestRate = 25; // Requests per second
            scaler.EvaluateScaling(requestRate);

            Console.WriteLine($"\n--- Processing Batch of {requestRate} Requests ---\n");

            // Simulate concurrent requests hitting the service mesh
            var tasks = new List<Task>();
            for (int i = 0; i < requestRate; i++)
            {
                // Distribute intents randomly to simulate real user behavior
                string[] intents = { "query", "buy", "recommend" };
                string intent = intents[i % 3];
                string productId = $"prod_{100 + i}";
                string userId = $"user_{i}";

                var request = new ServiceRequest(productId, userId, intent);

                // Fire and forget to simulate async processing
                tasks.Add(Task.Run(async () => 
                {
                    var response = await orchestrator.RouteRequestAsync(request);
                    if (response.Success)
                    {
                        Console.WriteLine($"[Success] {response.SourceAgent} processed {request.Intent} for {request.ProductId}");
                    }
                    else
                    {
                        Console.WriteLine($"[Fail] {response.SourceAgent}: {response.Data}");
                    }
                }));
            }

            // Wait for all "microservices" to complete processing
            await Task.WhenAll(tasks);

            Console.WriteLine("\n--- Simulation Complete ---");
            
            // Simulate scaling down after traffic drops
            Thread.Sleep(1000);
            scaler.EvaluateScaling(2); // Low traffic
        }
    }
}
