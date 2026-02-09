
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

namespace KubernetesAgentOrchestrator
{
    // Simulated Kubernetes API Client (Mock for the purpose of this chapter's scope)
    // In a real scenario, this would wrap the Kubernetes C# Client library.
    public class KubernetesApiClient
    {
        private int _activePodCount = 2;
        private readonly object _lock = new object();

        public int GetActivePodCount()
        {
            lock (_lock)
            {
                return _activePodCount;
            }
        }

        public void ScaleDeployment(int desiredReplicas)
        {
            lock (_lock)
            {
                Console.WriteLine($"[K8s API] Scaling Agent Deployment from {_activePodCount} to {desiredReplicas} replicas.");
                _activePodCount = desiredReplicas;
            }
        }
    }

    // Represents a single inference agent running in a Pod
    public class InferenceAgent
    {
        public string PodName { get; private set; }
        public bool IsBusy { get; private set; }
        private Random _rng;

        public InferenceAgent(string podName)
        {
            PodName = podName;
            IsBusy = false;
            _rng = new Random();
        }

        // Simulates the heavy lifting of AI model inference
        public async Task<string> ProcessRequestAsync(string prompt)
        {
            IsBusy = true;
            Console.WriteLine($"[Agent {PodName}] Received request: '{prompt}'. Processing...");
            
            // Simulate variable inference latency (500ms - 2000ms)
            int latency = _rng.Next(500, 2000);
            await Task.Delay(latency);

            IsBusy = false;
            return $"[Agent {PodName}] Result: Processed '{prompt}' in {latency}ms.";
        }
    }

    // The Load Balancer / Service Mesh component
    public class ServiceMeshRouter
    {
        private List<InferenceAgent> _availableAgents;
        private KubernetesApiClient _k8sClient;
        private int _requestQueueCount = 0;

        public ServiceMeshRouter(KubernetesApiClient k8sClient)
        {
            _k8sClient = k8sClient;
            _availableAgents = new List<InferenceAgent>();
            InitializeAgents();
        }

        private void InitializeAgents()
        {
            int count = _k8sClient.GetActivePodCount();
            for (int i = 1; i <= count; i++)
            {
                _availableAgents.Add(new InferenceAgent($"agent-pod-{i}"));
            }
        }

        // Logic to find a free agent or queue the request
        public async Task RouteRequestAsync(string prompt)
        {
            InferenceAgent freeAgent = null;

            // Simple Round-Robin / First-Available logic
            foreach (var agent in _availableAgents)
            {
                if (!agent.IsBusy)
                {
                    freeAgent = agent;
                    break;
                }
            }

            if (freeAgent != null)
            {
                // Offload to agent
                _ = Task.Run(async () => 
                {
                    string result = await freeAgent.ProcessRequestAsync(prompt);
                    Console.WriteLine(result);
                });
            }
            else
            {
                // No agents available, queue locally or reject
                _requestQueueCount++;
                Console.WriteLine($"[Router] Queueing request. Current queue depth: {_requestQueueCount}");
                
                // In a real scenario, we might trigger scaling here immediately
                CheckAndScale();
            }
        }

        // HPA Logic: Check load and trigger scaling
        private void CheckAndScale()
        {
            // Threshold: If queue depth > 2 * active agents, scale up
            int currentPods = _k8sClient.GetActivePodCount();
            if (_requestQueueCount > (currentPods * 2))
            {
                int newReplicaCount = currentPods + 1;
                _k8sClient.ScaleDeployment(newReplicaCount);
                
                // Add new agent to the pool (simulating Pod creation)
                _availableAgents.Add(new InferenceAgent($"agent-pod-{newReplicaCount}"));
                Console.WriteLine($"[HPA] Scaling triggered. New Agent {newReplicaCount} added to pool.");
            }
        }
    }

    // Main Application Entry Point
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("--- Kubernetes AI Agent Orchestrator Simulation ---");
            
            // 1. Initialize K8s API Connection
            var k8sClient = new KubernetesApiClient();
            
            // 2. Initialize Service Mesh Router
            var router = new ServiceMeshRouter(k8sClient);

            // 3. Simulate Incoming Traffic (User Requests)
            // This represents the external load balancer forwarding requests to the service mesh.
            string[] prompts = new string[] 
            { 
                "Analyze sentiment of this tweet", 
                "Generate code snippet for Python", 
                "Summarize article text", 
                "Translate English to French",
                "Identify objects in image",
                "Calculate math probability",
                "Draft an email response"
            };

            Random rng = new Random();

            // 4. Run Simulation Loop
            for (int i = 0; i < prompts.Length; i++)
            {
                // Simulate bursty traffic
                int delay = rng.Next(200, 800);
                await Task.Delay(delay);

                Console.WriteLine($"\n[Ingress] New request received: {prompts[i]}");
                
                // Route through Service Mesh
                await router.RouteRequestAsync(prompts[i]);
            }

            // 5. Wait for final processing
            Console.WriteLine("\n--- Simulation Complete. Waiting for remaining tasks... ---");
            await Task.Delay(3000); // Allow background tasks to finish
        }
    }
}
