
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

namespace CloudNativeInferenceScaling
{
    // REASONING: 
    // 1. Real-world context: An e-commerce platform needs to process product images for real-time 
    //    background removal and tagging. Workloads are bursty (peak during sales).
    // 2. Concept application: We simulate a microservices architecture where an "Inference Agent" 
    //    processes tasks. The "Orchestrator" manages these agents, scaling them up/down based on 
    //    queue depth (simulating Kubernetes HPA metrics).
    // 3. Constraints: We avoid LINQ/Lambdas. We use basic loops, arrays, and async/await (if 
    //    introduced, otherwise synchronous Task simulation) to demonstrate concurrency handling.
    
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== Cloud-Native AI Inference Orchestrator Simulation ===");
            Console.WriteLine("Simulating Dynamic Scaling of Containerized Agents...\n");

            // 1. Initialize the Orchestrator with a max capacity (simulating cluster limits)
            InferenceOrchestrator orchestrator = new InferenceOrchestrator(maxAgents: 5);

            // 2. Start the workload generator (simulating incoming user requests)
            // We use a separate thread to simulate asynchronous request arrival
            Thread workloadThread = new Thread(() => GenerateWorkload(orchestrator));
            workloadThread.Start();

            // 3. Start the monitoring loop (simulating the Kubernetes Controller loop)
            // This runs on the main thread
            RunMonitoringLoop(orchestrator);

            // Keep console open
            Console.ReadLine();
        }

        // Simulates incoming user requests (e.g., mobile app uploading images)
        static void GenerateWorkload(InferenceOrchestrator orchestrator)
        {
            Random rnd = new Random();
            for (int i = 0; i < 20; i++)
            {
                // Simulate variable arrival times
                Thread.Sleep(rnd.Next(500, 1500)); 
                
                string taskId = $"IMG-{1000 + i}";
                Console.WriteLine($"[Request] New image received: {taskId}");
                orchestrator.EnqueueTask(taskId);
            }
        }

        // Simulates the Kubernetes Control Plane monitoring loop
        static void RunMonitoringLoop(InferenceOrchestrator orchestrator)
        {
            int cycle = 0;
            while (cycle < 15) // Run for a limited time for this demo
            {
                Thread.Sleep(1000); // Check every second
                cycle++;
                
                Console.WriteLine($"\n--- Monitoring Cycle {cycle} ---");
                orchestrator.AutoScale();
                orchestrator.ProcessQueue();
                orchestrator.ReportStatus();
            }
        }
    }

    // Represents a single containerized inference agent (e.g., a Pod running a TensorFlow serving image)
    public class InferenceAgent
    {
        public int AgentId { get; private set; }
        public bool IsBusy { get; private set; }
        public int TasksProcessed { get; private set; }

        public InferenceAgent(int id)
        {
            AgentId = id;
            IsBusy = false;
            TasksProcessed = 0;
        }

        // Simulates the inference execution (Model prediction)
        public void ProcessTask(string taskId)
        {
            if (IsBusy) throw new InvalidOperationException("Agent is already busy.");

            IsBusy = true;
            Console.WriteLine($"  [Agent {AgentId}] Starting inference on {taskId}...");

            // Simulate processing time (e.g., GPU computation)
            Thread.Sleep(1500); 

            TasksProcessed++;
            IsBusy = false;
            Console.WriteLine($"  [Agent {AgentId}] Completed {taskId}. Total processed: {TasksProcessed}");
        }
    }

    // The Orchestrator simulates the Kubernetes Deployment Controller and Horizontal Pod Autoscaler (HPA)
    public class InferenceOrchestrator
    {
        private List<InferenceAgent> _activeAgents;
        private Queue<string> _taskQueue;
        private int _maxAgents;
        private int _agentCounter;

        public InferenceOrchestrator(int maxAgents)
        {
            _maxAgents = maxAgents;
            _activeAgents = new List<InferenceAgent>();
            _taskQueue = new Queue<string>();
            _agentCounter = 0;

            // Start with minimal agents (e.g., 1 replica)
            ScaleUp();
        }

        // 1. Ingestion: Accept tasks into the buffer (Message Queue simulation)
        public void EnqueueTask(string taskId)
        {
            _taskQueue.Enqueue(taskId);
        }

        // 2. Autoscaling Logic: The core decision maker
        // Logic: If QueueDepth > (ActiveAgents * Threshold), Scale Up.
        // Logic: If QueueDepth == 0 and ActiveAgents > 1, Scale Down.
        public void AutoScale()
        {
            int queueDepth = _taskQueue.Count;
            int activeCount = _activeAgents.Count;

            // Calculate load factor (simplified K8s HPA metric)
            // We assume each agent can handle 1 task at a time.
            int pendingTasks = queueDepth;
            
            // Check if we need to scale up
            // Threshold: If pending tasks exceed active agents by a buffer of 2
            if (pendingTasks > (activeCount * 1) + 2 && activeCount < _maxAgents)
            {
                Console.WriteLine($"[Orchestrator] High Load detected (Queue: {pendingTasks}). Scaling Up...");
                ScaleUp();
            }
            // Check if we can scale down to save resources
            else if (pendingTasks == 0 && activeCount > 1)
            {
                // Only scale down if idle for a while (simplified here to immediate)
                Console.WriteLine($"[Orchestrator] Low Load detected. Scaling Down...");
                ScaleDown();
            }
            else
            {
                Console.WriteLine($"[Orchestrator] Stable. Agents: {activeCount}, Queue: {pendingTasks}");
            }
        }

        // 3. Execution: Dispatch tasks to available agents
        public void ProcessQueue()
        {
            // Iterate through all agents to find available ones
            // Note: We use a simple for-loop instead of LINQ .FirstOrDefault()
            for (int i = 0; i < _activeAgents.Count; i++)
            {
                InferenceAgent agent = _activeAgents[i];

                if (!agent.IsBusy && _taskQueue.Count > 0)
                {
                    string task = _taskQueue.Dequeue();
                    
                    // In a real microservice, this would be an async HTTP call or gRPC call
                    // Here we simulate the offloading to a background thread to keep the orchestrator responsive
                    InferenceAgent capturedAgent = agent; // Capture for closure
                    string capturedTask = task;
                    
                    // Fire and forget simulation (Non-blocking)
                    Task.Run(() => capturedAgent.ProcessTask(capturedTask));
                }
            }
        }

        // Kubernetes-like Scale Up operation
        private void ScaleUp()
        {
            if (_activeAgents.Count < _maxAgents)
            {
                _agentCounter++;
                var newAgent = new InferenceAgent(_agentCounter);
                _activeAgents.Add(newAgent);
                Console.WriteLine($"  -> New Agent Container provisioned (ID: {newAgent.AgentId}). Total Agents: {_activeAgents.Count}");
            }
        }

        // Kubernetes-like Scale Down operation
        private void ScaleDown()
        {
            if (_activeAgents.Count > 1)
            {
                var agentToRemove = _activeAgents[_activeAgents.Count - 1];
                // Ensure agent is not busy before terminating (Pod Disruption Budget logic)
                if (!agentToRemove.IsBusy)
                {
                    _activeAgents.RemoveAt(_activeAgents.Count - 1);
                    Console.WriteLine($"  -> Agent Container terminated (ID: {agentToRemove.AgentId}). Total Agents: {_activeAgents.Count}");
                }
                else
                {
                    Console.WriteLine($"  -> Delaying scale down: Agent {agentToRemove.AgentId} is busy processing.");
                }
            }
        }

        // 4. Observability: Report current state
        public void ReportStatus()
        {
            Console.WriteLine("   [Status] Agent Pool:");
            foreach (var agent in _activeAgents)
            {
                string status = agent.IsBusy ? "[BUSY]" : "[IDLE]";
                Console.WriteLine($"      Agent {agent.AgentId}: {status} (Processed: {agent.TasksProcessed})");
            }
            Console.WriteLine($"   [Status] Pending Queue Size: {_taskQueue.Count}");
        }
    }
}
