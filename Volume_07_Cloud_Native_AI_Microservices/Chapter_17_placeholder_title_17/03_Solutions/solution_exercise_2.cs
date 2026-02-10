
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

// Source File: solution_exercise_2.cs
// Description: Solution for Exercise 2
// ==========================================

using k8s;
using k8s.Models;
using System.Text.Json;

// Custom Resource Definition Model
public class AutonomousAgent : CustomResource<AutonomousAgentSpec, AutonomousAgentStatus> { }

public class AutonomousAgentSpec
{
    public string AgentType { get; set; }
    public string ModelEndpoint { get; set; }
    public PersistenceConfig PersistenceConfig { get; set; }
    public int Replicas { get; set; } = 1;
}

public class PersistenceConfig
{
    public string StorageSize { get; set; } // e.g., "1Gi"
}

public class AutonomousAgentStatus
{
    public string Phase { get; set; }
}

// Operator Logic Skeleton
public class AgentOperator
{
    private readonly IKubernetes _client;

    public AgentOperator(IKubernetes client)
    {
        _client = client;
    }

    public async Task WatchAgents()
    {
        var watch = _client.WatchNamespacedCustomObject<AutonomousAgent>(
            group: "ai.example.com", version: "v1", plural: "autonomousagents", 
            namespaceParameter: "default");

        watch.OnEvent += async (type, item) =>
        {
            if (item is AutonomousAgent agent)
            {
                await Reconcile(agent);
            }
        };
    }

    public async Task Reconcile(AutonomousAgent resource)
    {
        // 1. Check if resource is being deleted
        if (resource.Metadata.DeletionTimestamp != null)
        {
            // Handle Finalizer Logic (Interactive Challenge)
            await HandleDeletion(resource);
            return;
        }

        // 2. Ensure Finalizer is present
        if (!resource.Metadata.Finalizers.Contains("autonomousagents.ai.example.com/finalizer"))
        {
            resource.Metadata.Finalizers.Add("autonomousagents.ai.example.com/finalizer");
            await _client.ReplaceNamespacedCustomObject(
                resource, "ai.example.com", "v1", "default", "autonomousagents", resource.Name());
            return;
        }

        // 3. Provision PVC if not exists
        await ProvisionPVC(resource);

        // 4. Reconcile Replicas (Simplified Deployment Logic)
        // In a real operator, you would manage Pods directly or via a Deployment
        await ReconcileReplicas(resource);
    }

    private async Task ProvisionPVC(AutonomousAgent resource)
    {
        var pvcName = $"pvc-{resource.Name()}";
        try 
        {
            await _client.ReadNamespacedPersistentVolumeClaim(pvcName, "default");
        }
        catch (k8s.Autorest.HttpOperationException)
        {
            var pvc = new V1PersistentVolumeClaim
            {
                Metadata = new V1ObjectMeta { Name = pvcName },
                Spec = new V1PersistentVolumeClaimSpec
                {
                    AccessModes = new[] { "ReadWriteOnce" },
                    Resources = new V1ResourceRequirements
                    {
                        Requests = new Dictionary<string, ResourceQuantity> 
                        { 
                            ["storage"] = new(resource.Spec.PersistenceConfig.StorageSize) 
                        }
                    }
                }
            };
            await _client.CreateNamespacedPersistentVolumeClaim(pvcName, "default", pvc);
        }
    }

    // Interactive Challenge: Finalizer Implementation
    private async Task HandleDeletion(AutonomousAgent resource)
    {
        var finalizerName = "autonomousagents.ai.example.com/finalizer";
        
        // 1. Check if our finalizer is still present
        if (!resource.Metadata.Finalizers.Contains(finalizerName)) return;

        Console.WriteLine($"Draining and saving state for {resource.Name()}...");

        // 2. Logic to dump state to PVC
        // In a real scenario, we would mount the PVC to a temporary pod or access the volume directly.
        // Here we simulate writing a state file.
        var stateDump = new { Timestamp = DateTime.UtcNow, State = "Draining", AgentId = resource.Name() };
        var jsonState = JsonSerializer.Serialize(stateDump);
        
        // Simulate writing to the PVC (e.g., via API or mounted volume)
        // File.WriteAllText($"/mnt/pvc/{resource.Name()}/state.json", jsonState);
        Console.WriteLine($"State saved: {jsonState}");

        // 3. Remove finalizer to allow Kubernetes to delete the resource
        resource.Metadata.Finalizers.Remove(finalizerName);
        
        // 4. Update the resource to remove the finalizer
        await _client.ReplaceNamespacedCustomObject(
            resource, "ai.example.com", "v1", "default", "autonomousagents", resource.Name());
    }

    private async Task ReconcileReplicas(AutonomousAgent resource)
    {
        // Implementation to ensure desired replica count matches actual pods
        // This would typically involve listing pods with a label selector matching the agent name
    }
}
