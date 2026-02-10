
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

namespace KubernetesAgentSimulation
{
    // ==========================================================================================================================================================
    // 1. CORE INFRASTRUCTURE: KUBERNETES RESOURCE MODEL (SIMULATED)
    // ==========================================================================================================================================================
    // This section simulates the behavior of the Kubernetes API Server and the underlying container runtime.
    // In a real cluster, these would be distributed components interacting via REST API calls (etcd for storage).
    // We simulate them here to demonstrate the logic of resource allocation and lifecycle management.

    /// <summary>
    /// Represents the abstract definition of a compute resource request.
    /// Mirrors the 'requests' field in a Kubernetes Pod spec.
    /// </summary>
    public class ResourceProfile
    {
        public string Name { get; set; } // e.g., "small-inference", "gpu-heavy"
        public double CpuCores { get; set; }
        public double MemoryGiB { get; set; }
    }

    /// <summary>
    /// Represents a running instance of a containerized agent.
    /// Mirrors a Kubernetes Pod.
    /// </summary>
    public class AgentPod
    {
        public string PodName { get; set; }
        public string Status { get; set; } // Pending, Running, Terminating
        public ResourceProfile Resources { get; set; }
        public double CurrentCpuUtilization { get; set; } // Simulated metric
        
        public AgentPod(string name, ResourceProfile profile)
        {
            PodName = name;
            Resources = profile;
            Status = "Pending";
            CurrentCpuUtilization = 0.0;
        }

        public void Start()
        {
            // Simulate container startup latency
            Thread.Sleep(100); 
            Status = "Running";
            Console.WriteLine($"[API Server] Pod '{PodName}' started. Assigned {Resources.CpuCores} CPU / {Resources.MemoryGiB} GiB.");
        }

        public void Terminate()
        {
            Status = "Terminating";
            Console.WriteLine($"[API Server] Pod '{PodName}' terminating...");
            Thread.Sleep(50); // Graceful shutdown simulation
        }
    }

    /// <summary>
    /// Manages the lifecycle of Pods on a simulated Node.
    /// </summary>
    public class NodeManager
    {
        private List<AgentPod> _runningPods = new List<AgentPod>();
        private double _totalCpu;
        private double _totalMemory;

        public NodeManager(double totalCpu, double totalMemory)
        {
            _totalCpu = totalCpu;
            _totalMemory = totalMemory;
        }

        // Thread-safe-ish wrapper for adding pods
        public bool SchedulePod(AgentPod pod)
        {
            double usedCpu = 0;
            double usedMem = 0;
            foreach(var p in _runningPods) 
            {
                usedCpu += p.Resources.CpuCores;
                usedMem += p.Resources.MemoryGiB;
            }

            if (usedCpu + pod.Resources.CpuCores <= _totalCpu && usedMem + pod.Resources.MemoryGiB <= _totalMemory)
            {
                _runningPods.Add(pod);
                pod.Start();
                return true;
            }
            
            Console.WriteLine($"[Node Manager] Insufficient resources to schedule {pod.PodName}.");
            return false;
        }

        public void RemovePod(AgentPod pod)
        {
            if (_runningPods.Contains(pod))
            {
                pod.Terminate();
                _runningPods.Remove(pod);
            }
        }

        public int GetPodCount() => _runningPods.Count;
        
        // Simulates metrics scraping for HPA
        public double GetAverageCpuUtilization()
        {
            if (_runningPods.Count == 0) return 0;
            double total = 0;
            foreach(var p in _runningPods) total += p.CurrentCpuUtilization;
            return total / _runningPods.Count;
        }

        // Simulates load generation to affect CPU metrics
        public void SimulateLoadOnPods(double intensity)
        {
            Random rnd = new Random();
            foreach(var p in _runningPods)
            {
                // Add some noise to the load
                p.CurrentCpuUtilization = intensity + (rnd.NextDouble() * 10.0); 
            }
        }
    }

    // ==========================================================================================================================================================
    // 2. SCALING LOGIC: HORIZONTAL POD AUTOSCALER (HPA)
    // ==========================================================================================================================================================
    // This simulates the HPA controller loop. It watches the Metrics API and adjusts the
    // Deployment's replica count. It implements the "Scale" operation.

    public class HpaController
    {
        // Configuration (from HPA Spec)
        private int _minReplicas;
        private int _maxReplicas;
        private double _targetCpuUtilization;
        
        // State (Current desired replicas)
        public int DesiredReplicas { get; private set; }

        public HpaController(int min, int max, double targetCpu)
        {
            _minReplicas = min;
            _maxReplicas = max;
            _targetCpuUtilization = targetCpu;
            DesiredReplicas = min; // Initial state
        }

        /// <summary>
        /// The core reconciliation loop.
        /// Formula: DesiredReplicas = ceil(CurrentReplicas * (CurrentUtilization / TargetUtilization))
        /// </summary>
        public void ComputeScalingDecision(int currentReplicas, double currentAvgCpu)
        {
            Console.WriteLine($"\n[HPA Controller] Metrics Check: Current Avg CPU: {currentAvgCpu:F2}% | Target: {_targetCpuUtilization}%");

            if (currentAvgCpu == 0 && currentReplicas == 0)
            {
                DesiredReplicas = _minReplicas; // Wake up from zero
                return;
            }

            if (currentAvgCpu == 0) 
            {
                // Avoid division by zero, but we might want to scale down to min
                DesiredReplicas = _minReplicas;
                return;
            }

            double ratio = currentAvgCpu / _targetCpuUtilization;
            double calculatedReplicas = Math.Ceiling(currentReplicas * ratio);

            // Clamp to bounds
            if (calculatedReplicas < _minReplicas) calculatedReplicas = _minReplicas;
            if (calculatedReplicas > _maxReplicas) calculatedReplicas = _maxReplicas;

            int newReplicas = (int)calculatedReplicas;

            if (newReplicas != currentReplicas)
            {
                Console.WriteLine($"[HPA Controller] Scaling Triggered: {currentReplicas} -> {newReplicas} replicas.");
                DesiredReplicas = newReplicas;
            }
            else
            {
                Console.WriteLine($"[HPA Controller] Metrics within tolerance. Maintaining {currentReplicas} replicas.");
            }
        }
    }

    // ==========================================================================================================================================================
    // 3. TRAFFIC MANAGEMENT: SERVICE MESH (ISTIO) SIMULATION
    // ==========================================================================================================================================================
    // Simulates an Istio VirtualService managing traffic shifting between versions (Canary Deployment).
    // This decouples the client from the specific pod instances.

    public class ServiceMeshGateway
    {
        // Weighted routing configuration
        private int _stableVersionWeight = 100; // v1
        private int _canaryVersionWeight = 0;   // v2

        public void UpdateTrafficSplit(int stableWeight, int canaryWeight)
        {
            _stableVersionWeight = stableWeight;
            _canaryVersionWeight = canaryWeight;
            Console.WriteLine($"[Istio VirtualService] Traffic Updated: Stable (v1): {stableWeight}%, Canary (v2): {canaryWeight}%");
        }

        public void RouteRequest()
        {
            Random rnd = new Random();
            int roll = rnd.Next(100);
            if (roll < _stableVersionWeight)
            {
                Console.WriteLine($"   -> Routing to STABLE Agent (v1)");
            }
            else
            {
                Console.WriteLine($"   -> Routing to CANARY Agent (v2)");
            }
        }
    }

    // ==========================================================================================================================================================
    // 4. MAIN ORCHESTRATOR: THE SYSTEM CONTROLLER
    // ==========================================================================================================================================================
    // This ties everything together. It acts as the Kubernetes Control Plane loop.
    // It continuously reconciles the desired state (HPA) with the actual state (NodeManager).

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=================================================================");
            Console.WriteLine("KUBERNETES AI AGENT SCALING SIMULATION");
            Console.WriteLine("Concepts: HPA, Resource Management, Service Mesh Traffic Shifting");
            Console.WriteLine("=================================================================\n");

            // --- SETUP ---
            
            // 1. Define the Node Capacity (The physical infrastructure)
            NodeManager clusterNode = new NodeManager(totalCpu: 16.0, totalMemory: 64.0);

            // 2. Define the Agent Profile (The Container Spec)
            ResourceProfile agentProfile = new ResourceProfile { Name = "ai-agent", CpuCores = 0.5, MemoryGiB = 1.0 };

            // 3. Define HPA Policy (The Autoscaler Spec)
            HpaController hpa = new HpaController(min: 1, max: 10, targetCpu: 60.0);

            // 4. Define Service Mesh (The Ingress Gateway)
            ServiceMeshGateway istio = new ServiceMeshGateway();

            // 5. State Tracking
            List<AgentPod> activePods = new List<AgentPod>();
            int replicaVersionCounter = 0; // Used to name pods uniquely

            // --- SIMULATION LOOP (The Reconciliation Loop) ---
            
            // Simulate 10 ticks of the cluster lifecycle
            for (int tick = 1; tick <= 10; tick++)
            {
                Console.WriteLine($"\n--- TICK {tick} ---");
                
                // A. SIMULATE LOAD FLUCTUATION (External Traffic)
                // In real world: Users query the AI agents.
                // We simulate this by adjusting the CPU utilization metric of running pods.
                double incomingLoad = 0;
                if (tick == 1) incomingLoad = 40;  // Low load
                if (tick == 2) incomingLoad = 45;
                if (tick == 3) incomingLoad = 80;  // Spike! Should trigger scale up
                if (tick == 4) incomingLoad = 85;
                if (tick == 5) incomingLoad = 90;
                if (tick == 6) incomingLoad = 30;  // Drop! Should trigger scale down
                if (tick >= 7) incomingLoad = 20;

                // Apply load to existing pods to update their metrics
                clusterNode.SimulateLoadOnPods(incomingLoad);

                // B. HPA LOGIC (The Control Loop)
                // 1. Scrape Metrics
                double currentAvgCpu = clusterNode.GetAverageCpuUtilization();
                
                // 2. Compute Desired State
                hpa.ComputeScalingDecision(activePods.Count, currentAvgCpu);
                
                // C. RECONCILIATION (The Scheduler)
                // The Control Plane compares 'Desired' vs 'Actual' and acts.
                int desiredCount = hpa.DesiredReplicas;
                int currentCount = activePods.Count;

                if (desiredCount > currentCount)
                {
                    // Scale Up Logic
                    int podsToCreate = desiredCount - currentCount;
                    for (int i = 0; i < podsToCreate; i++)
                    {
                        string podName = $"ai-agent-v2-{++replicaVersionCounter}"; // Deploying v2 agents
                        AgentPod newPod = new AgentPod(podName, agentProfile);
                        
                        // The Scheduler places the pod on the node
                        if (clusterNode.SchedulePod(newPod))
                        {
                            activePods.Add(newPod);
                        }
                        else
                        {
                            Console.WriteLine($"[CRITICAL] Failed to schedule pod. Node capacity full.");
                            // In a real cluster, this pods stays in 'Pending' until resources free up.
                        }
                    }
                }
                else if (desiredCount < currentCount)
                {
                    // Scale Down Logic
                    int podsToRemove = currentCount - desiredCount;
                    for (int i = 0; i < podsToRemove; i++)
                    {
                        // Kubernetes usually terminates the oldest or least stable pods.
                        AgentPod podToRemove = activePods[0]; 
                        
                        clusterNode.RemovePod(podToRemove);
                        activePods.Remove(podToRemove);
                    }
                }

                // D. SERVICE MESH LOGIC (Traffic Management)
                // Simulate a Canary Deployment strategy.
                // If we have > 5 pods, start shifting 20% traffic to the new version (v2).
                if (activePods.Count > 5)
                {
                    istio.UpdateTrafficSplit(80, 20);
                }
                else
                {
                    istio.UpdateTrafficSplit(100, 0);
                }

                // E. SIMULATE INCOMING REQUESTS
                // Visualize how traffic is routed based on the mesh config
                if (activePods.Count > 0)
                {
                    Console.WriteLine("Incoming User Requests:");
                    for(int r=0; r<3; r++) istio.RouteRequest();
                }

                // F. CLEANUP VISUALIZATION
                Console.WriteLine($"Cluster State: {activePods.Count} Pods Running. Avg CPU: {currentAvgCpu:F2}%");

                // Sleep to make the simulation readable
                Thread.Sleep(500);
            }
        }
    }
}
