
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

namespace CloudNativeAgentScaling
{
    // Simulates a containerized AI agent that processes inference requests.
    // In a real Kubernetes environment, this would be a Docker container running this process.
    public class InferenceAgent
    {
        private readonly string _agentId;
        private readonly int _maxConcurrentRequests;
        private int _currentLoad;
        private readonly object _lock = new object();

        public InferenceAgent(string agentId, int maxConcurrentRequests)
        {
            _agentId = agentId;
            _maxConcurrentRequests = maxConcurrentRequests;
            _currentLoad = 0;
        }

        // Simulates processing an AI inference request (e.g., text generation).
        // In a real scenario, this would call a TensorFlow/PyTorch model.
        public async Task<string> ProcessRequestAsync(string input)
        {
            lock (_lock)
            {
                if (_currentLoad >= _maxConcurrentRequests)
                {
                    throw new InvalidOperationException($"Agent {_agentId} is at capacity.");
                }
                _currentLoad++;
            }

            // Simulate network latency and model inference time
            await Task.Delay(new Random().Next(200, 500));

            lock (_lock)
            {
                _currentLoad--;
            }

            return $"Processed '{input}' by Agent {_agentId}";
        }

        public int GetCurrentLoad() => _currentLoad;
        public string GetId() => _agentId;
    }

    // Simulates a Redis-like distributed cache for managing agent state and session memory.
    // This ensures state persistence across container restarts.
    public class DistributedStateStore
    {
        private readonly Dictionary<string, string> _cache = new Dictionary<string, string>();

        public void Set(string key, string value)
        {
            // In a real app, this handles serialization and network calls to Redis
            lock (_cache)
            {
                _cache[key] = value;
            }
        }

        public string Get(string key)
        {
            lock (_cache)
            {
                return _cache.ContainsKey(key) ? _cache[key] : null;
            }
        }
    }

    // Orchestrator managing the pool of agents and scaling logic.
    // Mimics Kubernetes Horizontal Pod Autoscaler (HPA) logic.
    public class AgentOrchestrator
    {
        private readonly List<InferenceAgent> _agents = new List<InferenceAgent>();
        private readonly DistributedStateStore _stateStore;
        private readonly int _maxAgents;
        private int _requestCount = 0;

        public AgentOrchestrator(int initialAgents, int maxAgents)
        {
            _stateStore = new DistributedStateStore();
            _maxAgents = maxAgents;
            
            // Initial scaling (Bootstrapping)
            for (int i = 0; i < initialAgents; i++)
            {
                ScaleUp();
            }
        }

        // Routes request to the least loaded agent.
        // Implements basic Load Balancing.
        public async Task<string> RouteRequestAsync(string input)
        {
            InferenceAgent selectedAgent = null;
            int minLoad = int.MaxValue;

            // Simple Round Robin / Least Connection strategy
            lock (_agents)
            {
                foreach (var agent in _agents)
                {
                    int load = agent.GetCurrentLoad();
                    if (load < minLoad)
                    {
                        minLoad = load;
                        selectedAgent = agent;
                    }
                }
            }

            if (selectedAgent == null || minLoad >= 5) // Threshold for scaling
            {
                EvaluateScaling();
            }

            if (selectedAgent == null)
            {
                return "No agents available. Scaling in progress...";
            }

            // Store request context in 'Redis' for observability
            _stateStore.Set($"req:{_requestCount++}", $"Routed to {selectedAgent.GetId()}");

            try
            {
                return await selectedAgent.ProcessRequestAsync(input);
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }

        // Logic to decide when to add more containers (Horizontal Scaling)
        private void EvaluateScaling()
        {
            lock (_agents)
            {
                if (_agents.Count < _maxAgents)
                {
                    Console.WriteLine($"[Autoscaler] High load detected. Scaling UP...");
                    ScaleUp();
                }
                else
                {
                    Console.WriteLine($"[Autoscaler] Max agents reached. Throttling.");
                }
            }
        }

        // Adds a new container instance
        private void ScaleUp()
        {
            if (_agents.Count < _maxAgents)
            {
                var newAgent = new InferenceAgent($"Agent-{_agents.Count + 1}", 10);
                _agents.Add(newAgent);
                Console.WriteLine($"[Orchestrator] New Agent {newAgent.GetId()} started.");
            }
        }

        // Simulates Kubernetes Liveness Probe
        public void RunHealthChecks()
        {
            lock (_agents)
            {
                // In real K8s, this checks /healthz endpoint
                // Here we simulate a random failure
                for (int i = _agents.Count - 1; i >= 0; i--)
                {
                    if (new Random().Next(0, 10) == 0) // 10% chance of failure
                    {
                        Console.WriteLine($"[HealthCheck] Agent {_agents[i].GetId()} failed. Removing.");
                        _agents.RemoveAt(i);
                    }
                }
            }
        }
    }

    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Initializing Cloud-Native AI Agent System...");
            
            // Initialize Orchestrator (Simulates Kubernetes Deployment)
            // Initial: 2 pods, Max: 5 pods
            var orchestrator = new AgentOrchestrator(initialAgents: 2, maxAgents: 5);

            // Simulate a burst of traffic (Ingress requests)
            var tasks = new List<Task>();
            
            Console.WriteLine("\n--- Incoming Traffic Burst (Simulating User Requests) ---\n");

            for (int i = 0; i < 15; i++)
            {
                // Simulate async request handling
                tasks.Add(Task.Run(async () => 
                {
                    string result = await orchestrator.RouteRequestAsync($"Data_Packet_{Guid.NewGuid()}");
                    Console.WriteLine($"[Result] {result}");
                }));
                
                // Stagger requests slightly to simulate real network jitter
                await Task.Delay(100);
                
                // Periodic Health Check (Simulating K8s Kubelet)
                if (i % 5 == 0) orchestrator.RunHealthChecks();
            }

            await Task.WhenAll(tasks);
            
            Console.WriteLine("\n--- Traffic Stabilized ---");
        }
    }
}
