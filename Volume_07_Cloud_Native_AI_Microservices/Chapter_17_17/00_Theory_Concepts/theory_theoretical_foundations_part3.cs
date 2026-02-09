
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

# Source File: theory_theoretical_foundations_part3.cs
# Description: Theoretical Foundations
# ==========================================

using System;
using System.Threading;
using System.Threading.Tasks;
using k8s;
using k8s.Models;

namespace AgentOperator
{
    // This represents the Custom Resource Definition (CRD)
    public class AutonomousAgentResource : V1CustomResourceDefinition<AutonomousAgentSpec, AutonomousAgentStatus> { }
    
    public class AutonomousAgentSpec 
    { 
        public string ModelName { get; set; }
        public int Replicas { get; set; }
        public string GpuType { get; set; } // e.g., "nvidia-tesla-t4"
    }

    public class AutonomousAgentStatus 
    { 
        public string Phase { get; set; } // e.g., "Pending", "Running", "Scaling"
        public int ReadyReplicas { get; set; }
    }

    public class OperatorService : BackgroundService
    {
        private readonly IKubernetes _kubernetesClient;

        public OperatorService(IKubernetes kubernetesClient)
        {
            _kubernetesClient = kubernetesClient;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Watch for changes to AutonomousAgent resources
            var watcher = _kubernetesClient.WatchNamespacedCustomObject<AutonomousAgentResource>(
                group: "ai.agent.io",
                version: "v1",
                namespaceParameter: "default",
                plural: "autonomousagents",
                onEvent: async (type, item) =>
                {
                    switch (type)
                    {
                        case WatchEventType.Added:
                        case WatchEventType.Modified:
                            await ReconcileAsync(item);
                            break;
                        case WatchEventType.Deleted:
                            // Cleanup logic
                            break;
                    }
                },
                onClosed: () => { /* Handle reconnect */ },
                onError: e => { /* Handle error */ }
            );

            await Task.Delay(Timeout.Infinite, stoppingToken);
        }

        private async Task ReconcileAsync(AutonomousAgentResource agent)
        {
            // 1. Check current state (e.g., count running Pods)
            var pods = await _kubernetesClient.ListNamespacedPodAsync(
                labelSelector: $"app={agent.Spec.ModelName}",
                namespaceParameter: "default");

            // 2. Compare with desired state (Spec.Replicas)
            int currentReplicas = pods.Items.Count;
            int desiredReplicas = agent.Spec.Replicas;

            if (currentReplicas < desiredReplicas)
            {
                // 3. Scale Up: Create a new Pod
                // Here we would construct a V1Pod object with specific GPU tolerations
                // based on agent.Spec.GpuType
                await ScaleUpAsync(agent, desiredReplicas - currentReplicas);
            }
            else if (currentReplicas > desiredReplicas)
            {
                // 3. Scale Down: Delete excess Pods
                await ScaleDownAsync(pods, currentReplicas - desiredReplicas);
            }

            // 4. Update Status
            agent.Status.Phase = "Running";
            agent.Status.ReadyReplicas = desiredReplicas;
            await _kubernetesClient.ReplaceNamespacedCustomObjectStatusAsync(
                agent, "default", "autonomousagents", agent.Metadata.Name);
        }

        private async Task ScaleUpAsync(AutonomousAgentResource agent, int count)
        {
            // Logic to create V1Pods with specific resource requests (GPU)
            // This ensures the scheduler places the pod on a GPU node.
            Console.WriteLine($"Scaling up {agent.Spec.ModelName} by {count} replicas.");
            // Implementation of V1Pod creation omitted for brevity
        }

        private async Task ScaleDownAsync(V1PodList pods, int count)
        {
            // Logic to gracefully terminate pods (e.g., send SIGTERM to save state)
            Console.WriteLine($"Scaling down {count} replicas.");
            // Implementation of Pod deletion omitted for brevity
        }
    }
}
