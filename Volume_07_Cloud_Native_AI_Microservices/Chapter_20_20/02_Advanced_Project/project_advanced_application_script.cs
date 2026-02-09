
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

namespace AgentSwarmOrchestrator
{
    // 1. PROBLEM DEFINITION:
    // In a high-throughput inference environment (e.g., a real-time fraud detection system),
    // incoming requests vary wildly in complexity. A simple transaction takes 10ms to process,
    // while a complex multi-layered graph analysis might take 2000ms.
    // A static thread pool is inefficient: it wastes resources on simple tasks or creates latency
    // queues for complex ones. We need a dynamic "Agent Swarm" that scales its workers based on
    // the current queue depth and the priority of tasks, simulating KEDA's scaling logic in C#.

    // 2. ARCHITECTURAL PATTERN: Distributed Task Queue with Dynamic Worker Scaling.
    // This code simulates the core logic of a Kubernetes Horizontal Pod Autoscaler (HPA)
    // driven by an external metric (Queue Length).

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Initializing Cloud-Native Agent Swarm...");
            Console.WriteLine("--------------------------------------------------");

            // Initialize the Orchestrator
            // We use a shared Job Queue (simulating a message broker like RabbitMQ or Kafka)
            // and a Worker Pool (simulating Kubernetes Pods).
            JobQueue jobQueue = new JobQueue();
            AgentSwarm orchestrator = new AgentSwarm(jobQueue);

            // Start the Scaling Logic (Simulating KEDA Operator)
            // This runs in a separate thread to monitor metrics and adjust worker count.
            Thread scalingThread = new Thread(orchestrator.RunScalingLogic);
            scalingThread.IsBackground = true;
            scalingThread.Start();

            // Simulate Incoming Traffic (The "Inference Gateway")
            // We generate a burst of mixed-priority jobs to test the system's elasticity.
            Thread producerThread = new Thread(() => GenerateTraffic(jobQueue));
            producerThread.Start();
            producerThread.Join(); // Wait for traffic generation to finish

            // Allow time for remaining jobs to process
            Thread.Sleep(5000);
            Console.WriteLine("\nSimulation Complete. Final State:");
            orchestrator.ReportStatus();
        }

        // Simulates an external API Gateway pushing requests into the system
        static void GenerateTraffic(JobQueue queue)
        {
            Random rnd = new Random();
            for (int i = 0; i < 20; i++)
            {
                // 70% chance of High Priority (Low Latency), 30% chance of Low Priority (Batch)
                JobType type = (rnd.Next(0, 10) < 7) ? JobType.HighPriority : JobType.LowPriority;
                
                var job = new InferenceJob($"Job_{i}", type);
                queue.Enqueue(job);
                Console.WriteLine($"[Gateway] Received: {job.Id} (Priority: {job.Type})");

                // Burst traffic simulation: Sleep less between items initially
                Thread.Sleep(rnd.Next(50, 200)); 
            }
        }
    }

    // ---------------------------------------------------------
    // DATA MODELS & MESSAGING
    // ---------------------------------------------------------

    public enum JobType
    {
        HighPriority, // Requires immediate inference (e.g., real-time user interaction)
        LowPriority   // Can be batched (e.g., nightly analytics)
    }

    public class InferenceJob
    {
        public string Id { get; }
        public JobType Type { get; }
        public DateTime CreatedAt { get; }

        public InferenceJob(string id, JobType type)
        {
            Id = id;
            Type = type;
            CreatedAt = DateTime.Now;
        }

        // Simulates the computational cost of the inference
        public int GetProcessingTimeMs()
        {
            return Type == JobType.HighPriority ? 100 : 500; 
        }
    }

    // ---------------------------------------------------------
    // CORE COMPONENTS
    // ---------------------------------------------------------

    /// <summary>
    /// Simulates a distributed message queue (e.g., RabbitMQ).
    /// Thread-safe for basic operations.
    /// </summary>
    public class JobQueue
    {
        private readonly Queue<InferenceJob> _queue = new Queue<InferenceJob>();
        private readonly object _lock = new object();

        public void Enqueue(InferenceJob job)
        {
            lock (_lock)
            {
                _queue.Enqueue(job);
            }
        }

        public InferenceJob Dequeue()
        {
            lock (_lock)
            {
                if (_queue.Count > 0)
                    return _queue.Dequeue();
                return null;
            }
        }

        public int Count()
        {
            lock (_lock)
            {
                return _queue.Count;
            }
        }
    }

    /// <summary>
    /// Represents a single Agent (Kubernetes Pod) capable of processing inference jobs.
    /// </summary>
    public class Agent
    {
        public int Id { get; }
        public bool IsBusy { get; private set; }
        private Thread _workerThread;

        public Agent(int id)
        {
            Id = id;
            IsBusy = false;
        }

        public void ProcessJob(InferenceJob job)
        {
            if (IsBusy) throw new InvalidOperationException("Agent is already occupied.");

            IsBusy = true;
            // Simulate asynchronous processing within a dedicated thread
            _workerThread = new Thread(() => SimulateInference(job));
            _workerThread.Start();
        }

        private void SimulateInference(InferenceJob job)
        {
            Console.WriteLine($"[Agent-{Id}] Started processing {job.Id}...");
            
            // Simulate CPU/IO work
            Thread.Sleep(job.GetProcessingTimeMs());
            
            Console.WriteLine($"[Agent-{Id}] Finished {job.Id} in {job.GetProcessingTimeMs()}ms.");
            IsBusy = false;
        }
    }

    /// <summary>
    /// The Orchestrator managing the Agent Swarm.
    /// Implements the scaling logic similar to KEDA (Kubernetes Event-driven Autoscaling).
    /// </summary>
    public class AgentSwarm
    {
        private readonly JobQueue _queue;
        private readonly List<Agent> _agents;
        private const int MAX_AGENTS = 5; // Simulates Max Pod Limit
        private const int SCALE_UP_THRESHOLD = 2; // Jobs waiting per agent to trigger scale up
        private const int SCALE_DOWN_THRESHOLD = 0; // Jobs waiting to trigger scale down
        private const int COOLDOWN_SECONDS = 3; // Prevent thrashing

        public AgentSwarm(JobQueue queue)
        {
            _queue = queue;
            _agents = new List<Agent>();
            // Start with 1 agent
            AddAgent();
        }

        /// <summary>
        /// The main control loop. This simulates the KEDA Operator reconciling state.
        /// </summary>
        public void RunScalingLogic()
        {
            while (true)
            {
                int queueDepth = _queue.Count();
                int activeAgents = GetActiveAgentCount();

                // 1. ASSIGN WORK: Distribute jobs to available agents
                AssignWork();

                // 2. CALCULATE DESIRED STATE: Determine if we need to scale
                int desiredAgents = CalculateDesiredAgents(queueDepth, activeAgents);

                // 3. RECONCILE: Adjust the actual cluster state to match desired state
                if (desiredAgents > activeAgents)
                {
                    Console.WriteLine($"[Scaler] Metric: QueueDepth={queueDepth}. Scaling UP to {desiredAgents} agents.");
                    AddAgent();
                }
                else if (desiredAgents < activeAgents)
                {
                    Console.WriteLine($"[Scaler] Metric: QueueDepth={queueDepth}. Scaling DOWN to {desiredAgents} agents.");
                    RemoveAgent();
                }

                // 4. COOLDOWN: Wait before next check to prevent oscillation
                Thread.Sleep(COOLDOWN_SECONDS * 1000);
            }
        }

        private int CalculateDesiredAgents(int queueDepth, int currentAgents)
        {
            // Simple algorithm: 1 agent per 2 queued jobs, capped at MAX_AGENTS
            // In a real K8s environment, this is calculated via: DesiredReplicas = ceil(CurrentReplicas * (CurrentMetricValue / DesiredMetricValue))
            int target = (int)Math.Ceiling((double)queueDepth / SCALE_UP_THRESHOLD);

            if (target < 1) target = 1; // Always keep at least 1 warm
            if (target > MAX_AGENTS) target = MAX_AGENTS;

            return target;
        }

        private void AssignWork()
        {
            // Find idle agents
            foreach (var agent in _agents)
            {
                if (!agent.IsBusy)
                {
                    var job = _queue.Dequeue();
                    if (job != null)
                    {
                        agent.ProcessJob(job);
                    }
                }
            }
        }

        private int GetActiveAgentCount()
        {
            // In this simplified model, all agents in the list are "active" (running).
            // In a real K8s scenario, this would check Pod Ready status.
            return _agents.Count;
        }

        private void AddAgent()
        {
            if (_agents.Count < MAX_AGENTS)
            {
                var newAgent = new Agent(_agents.Count + 1);
                _agents.Add(newAgent);
                Console.WriteLine($"   -> Agent-{newAgent.Id} Provisioned (Total: {_agents.Count})");
            }
        }

        private void RemoveAgent()
        {
            // Find an idle agent to terminate
            // In a real system, we would drain connections first.
            for (int i = _agents.Count - 1; i >= 0; i--)
            {
                if (!_agents[i].IsBusy)
                {
                    Console.WriteLine($"   -> Agent-{_agents[i].Id} Terminated (Total: {_agents.Count - 1})");
                    _agents.RemoveAt(i);
                    return;
                }
            }
        }

        public void ReportStatus()
        {
            Console.WriteLine($"\n[Orchestrator] Active Agents: {_agents.Count}, Queue Backlog: {_queue.Count()}");
            foreach(var agent in _agents)
            {
                Console.WriteLine($"   - Agent-{agent.Id} Status: {(agent.IsBusy ? "Busy" : "Idle")}");
            }
        }
    }
}
