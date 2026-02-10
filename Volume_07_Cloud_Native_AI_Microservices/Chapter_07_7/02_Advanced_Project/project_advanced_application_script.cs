
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

namespace CloudNativeAgentOrchestrator
{
    // Represents the state of a long-running agent process
    public enum AgentState
    {
        Pending,
        Processing,
        Completed,
        Failed
    }

    // Represents a single inference task or job
    public class InferenceTask
    {
        public int TaskId { get; set; }
        public string InputData { get; set; }
        public AgentState Status { get; set; }
        public DateTime CreatedAt { get; set; }

        public InferenceTask(int id, string data)
        {
            TaskId = id;
            InputData = data;
            Status = AgentState.Pending;
            CreatedAt = DateTime.Now;
        }
    }

    // Simulates a containerized agent runtime (Pod)
    // Handles stateful processing of tasks
    public class AgentPod
    {
        public string PodId { get; private set; }
        public bool IsBusy { get; private set; }
        private List<InferenceTask> processedTasks;

        public AgentPod(string id)
        {
            PodId = id;
            IsBusy = false;
            processedTasks = new List<InferenceTask>();
        }

        // Simulates the container receiving a workload
        public void AcceptTask(InferenceTask task)
        {
            if (IsBusy)
            {
                throw new InvalidOperationException($"Pod {PodId} is currently busy.");
            }

            IsBusy = true;
            task.Status = AgentState.Processing;
            Console.WriteLine($"[Pod {PodId}] Accepted Task {task.TaskId}. Processing started.");

            // Simulate heavy inference work (e.g., model prediction)
            // In a real scenario, this would be an async gRPC call or HTTP request
            ProcessInternal(task);
        }

        private void ProcessInternal(InferenceTask task)
        {
            // Simulate processing time
            Thread.Sleep(1000); 

            // Random failure simulation for resilience testing
            Random rnd = new Random();
            if (rnd.Next(0, 10) == 0) // 10% chance of failure
            {
                task.Status = AgentState.Failed;
                Console.WriteLine($"[Pod {PodId}] Task {task.TaskId} FAILED. Retrying required.");
            }
            else
            {
                task.Status = AgentState.Completed;
                processedTasks.Add(task);
                Console.WriteLine($"[Pod {PodId}] Task {task.TaskId} COMPLETED successfully.");
            }
            
            IsBusy = false;
        }

        public int GetQueueDepth()
        {
            return processedTasks.Count;
        }
    }

    // Orchestrator managing the cluster of pods
    // Implements Horizontal Pod Autoscaling (HPA) logic
    public class ClusterOrchestrator
    {
        private List<AgentPod> activePods;
        private Queue<InferenceTask> pendingQueue;
        private int maxPods;
        private int taskCounter;

        public ClusterOrchestrator(int initialPods, int maxPodsLimit)
        {
            activePods = new List<AgentPod>();
            pendingQueue = new Queue<InferenceTask>();
            maxPods = maxPodsLimit;
            taskCounter = 1;

            // Initialize cluster
            for (int i = 0; i < initialPods; i++)
            {
                SpawnPod();
            }
        }

        // Ingress: Receive external requests
        public void SubmitTask(string inputData)
        {
            var task = new InferenceTask(taskCounter++, inputData);
            pendingQueue.Enqueue(task);
            Console.WriteLine($"[Orchestrator] Received Task {task.TaskId}. Queue Size: {pendingQueue.Count}");
        }

        // Scheduler Loop
        public void RunCycle()
        {
            Console.WriteLine("\n--- Orchestrator Cycle Start ---");
            
            // 1. Dispatch Logic
            while (pendingQueue.Count > 0)
            {
                AgentPod availablePod = FindAvailablePod();
                
                if (availablePod != null)
                {
                    InferenceTask task = pendingQueue.Dequeue();
                    try
                    {
                        availablePod.AcceptTask(task);
                    }
                    catch (Exception ex)
                    {
                        // Handle race conditions or pod state errors
                        Console.WriteLine($"[Error] Dispatch failed: {ex.Message}");
                        pendingQueue.Enqueue(task); // Re-queue
                    }
                }
                else
                {
                    // No pods available, trigger scaling logic
                    Console.WriteLine("[Orchestrator] No available pods. Checking scaling requirements...");
                    ScaleUp();
                    break; // Wait for next cycle to allow pods to initialize
                }
            }

            // 2. Autoscaling Logic (HPA)
            // Scale down if utilization is low
            if (pendingQueue.Count == 0 && activePods.Count > 1)
            {
                // Check if pods are idle
                bool allIdle = true;
                foreach (var pod in activePods)
                {
                    if (pod.IsBusy) allIdle = false;
                }

                if (allIdle)
                {
                    ScaleDown();
                }
            }

            Console.WriteLine($"[Orchestrator] Status: Pods={activePods.Count}, Pending={pendingQueue.Count}");
            Console.WriteLine("--- Orchestrator Cycle End ---\n");
        }

        private AgentPod FindAvailablePod()
        {
            foreach (var pod in activePods)
            {
                if (!pod.IsBusy)
                {
                    return pod;
                }
            }
            return null;
        }

        private void SpawnPod()
        {
            if (activePods.Count < maxPods)
            {
                string podId = $"pod-{activePods.Count + 1}";
                activePods.Add(new AgentPod(podId));
                Console.WriteLine($"[K8s API] Spawning new pod: {podId}");
            }
        }

        private void ScaleUp()
        {
            if (activePods.Count < maxPods)
            {
                Console.WriteLine("[HPA] Scaling Up: High load detected.");
                SpawnPod();
            }
            else
            {
                Console.WriteLine("[HPA] Max pods reached. Load shedding may be required.");
            }
        }

        private void ScaleDown()
        {
            if (activePods.Count > 1)
            {
                Console.WriteLine("[HPA] Scaling Down: Low load detected.");
                var podToRemove = activePods[activePods.Count - 1];
                // In real K8s, this would trigger a graceful shutdown/sigterm
                Console.WriteLine($"[K8s API] Terminating pod: {podToRemove.PodId}");
                activePods.RemoveAt(activePods.Count - 1);
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Initializing Cloud-Native Agent Orchestrator...");
            
            // Initialize with 1 pod, max 3 pods
            ClusterOrchestrator orchestrator = new ClusterOrchestrator(1, 3);

            // Simulate incoming traffic
            Console.WriteLine("\nSimulating incoming inference requests...");
            
            // Batch 1: Normal load
            orchestrator.SubmitTask("Image: cat.jpg");
            orchestrator.SubmitTask("Image: dog.png");
            
            // Run a few cycles to process initial load
            for (int i = 0; i < 2; i++) orchestrator.RunCycle();

            // Batch 2: Spike in traffic (trigger scale up)
            Console.WriteLine("\n--- TRAFFIC SPIKE DETECTED ---");
            for (int i = 0; i < 5; i++)
            {
                orchestrator.SubmitTask($"Video Chunk {i+1}");
            }

            // Run cycles to observe scaling
            for (int i = 0; i < 4; i++) orchestrator.RunCycle();

            // Batch 3: Traffic dies down (trigger scale down)
            Console.WriteLine("\n--- TRAFFIC DROP DETECTED ---");
            // Wait for pods to finish current work
            for (int i = 0; i < 3; i++) orchestrator.RunCycle();

            Console.WriteLine("\nSimulation Complete.");
        }
    }
}
