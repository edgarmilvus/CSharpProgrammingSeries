
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

// Source File: solution_exercise_1.cs
// Description: Solution for Exercise 1
// ==========================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using k8s;
using k8s.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

// 1. Define the IntelligentAgent Custom Resource Definition (CRD) manifest YAML
/*
apiVersion: apiextensions.k8s.io/v1
kind: CustomResourceDefinition
metadata:
  name: intelligentagents.agent.swarm.io
spec:
  group: agent.swarm.io
  versions:
    - name: v1
      served: true
      storage: true
      schema:
        openAPIV3Schema:
          type: object
          properties:
            spec:
              type: object
              properties:
                image:
                  type: string
                replicas:
                  type: integer
                storageSize:
                  type: string # e.g., "10Gi"
                statefulConfig:
                  type: object
                  additionalProperties:
                    type: string
  scope: Namespaced
  names:
    plural: intelligentagents
    singular: intelligentagent
    kind: IntelligentAgent
    shortNames:
    - iagent
*/

// 2. C# class representing the CRD
public record IntelligentAgentSpec(
    string Image, 
    int Replicas, 
    string StorageSize, 
    Dictionary<string, string> StatefulConfig);

public class IntelligentAgentStatus
{
    public string Phase { get; set; } = "Pending";
    public DateTime? LastReconciled { get; set; }
}

// Using a generic Kubernetes CRD class
public class IntelligentAgentResource : V1CustomResource<IntelligentAgentSpec, IntelligentAgentStatus> 
{
    // Metadata annotations can include 'storageClassName'
}

// 3. The Operator Controller
public class IntelligentAgentController : BackgroundService
{
    private readonly IKubernetes _client;
    private readonly ILogger<IntelligentAgentController> _logger;
    private const string Group = "agent.swarm.io";
    private const string Version = "v1";
    private const string Plural = "intelligentagents";

    public IntelligentAgentController(ILogger<IntelligentAgentController> logger)
    {
        _logger = logger;
        // In a real scenario, load config from KubeConfig or InCluster
        var config = KubernetesClientConfiguration.BuildDefaultConfig();
        _client = new Kubernetes(config);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("IntelligentAgent Operator started.");

        // Use PeriodicTimer for reconciliation loop (Requirement 4)
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(10));
        
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await ReconcileAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during reconciliation loop.");
            }
        }
    }

    private async Task ReconcileAsync(CancellationToken ct)
    {
        var crds = await _client.ListNamespacedCustomObjectAsync<Group, Version, IntelligentAgentResource>(
            namespaceParameter: "default", plural: Plural, cancellationToken: ct);

        if (crds.Items == null) return;

        foreach (var agent in crds.Items)
        {
            await ReconcileAgentAsync(agent, ct);
        }
    }

    private async Task ReconcileAgentAsync(IntelligentAgentResource agent, CancellationToken ct)
    {
        var name = agent.Metadata.Name;
        var ns = agent.Metadata.NamespaceProperty ?? "default";
        
        _logger.LogInformation("Reconciling IntelligentAgent: {Name}", name);

        // Check if StatefulSet exists
        V1StatefulSet existingSts = null;
        try
        {
            existingSts = await _client.ReadNamespacedStatefulSetAsync(name, ns, cancellationToken: ct);
        }
        catch (k8s.Autorest.HttpOperationException ex) when (ex.Response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            // Expected if not created yet
        }

        if (agent.Metadata.DeletionTimestamp != null)
        {
            // 4. Handle Deletion
            if (existingSts != null)
            {
                _logger.LogInformation("Deleting StatefulSet for {Name}", name);
                await _client.DeleteNamespacedStatefulSetAsync(name, ns, cancellationToken: ct);
            }
            // Note: In a real operator, we would also handle finalizers here to ensure PVC cleanup
            // or rely on StatefulSet's 'PersistentVolumeClaimRetentionPolicy' (K8s 1.23+)
            return;
        }

        // 4. Handle Creation/Update
        var desiredSts = GenerateStatefulSet(agent);
        
        if (existingSts == null)
        {
            _logger.LogInformation("Creating StatefulSet for {Name}", name);
            await _client.CreateNamespacedStatefulSetAsync(ns, desiredSts, cancellationToken: ct);
        }
        else
        {
            // Check for updates (e.g., Image or Replicas)
            bool updateNeeded = false;
            
            // Check Image (requires checking container spec)
            var existingImage = existingSts.Spec.Template.Spec.Containers.FirstOrDefault()?.Image;
            if (existingImage != agent.Spec.Image)
            {
                _logger.LogInformation("Image mismatch. Updating StatefulSet {Name}", name);
                updateNeeded = true;
            }

            // Check Replicas
            if (existingSts.Spec.Replicas != agent.Spec.Replicas)
            {
                _logger.LogInformation("Replica count mismatch. Updating StatefulSet {Name}", name);
                updateNeeded = true;
            }

            if (updateNeeded)
            {
                // Patching is often safer, but Replace is used here for simplicity
                await _client.ReplaceNamespacedStatefulSetAsync(desiredSts, name, ns, cancellationToken: ct);
            }
        }
    }

    private V1StatefulSet GenerateStatefulSet(IntelligentAgentResource agent)
    {
        var storageClassName = agent.Annotations?.GetValueOrDefault("storageClassName", "standard");

        return new V1StatefulSet
        {
            Metadata = new V1ObjectMeta
            {
                Name = agent.Metadata.Name,
                NamespaceProperty = agent.Metadata.NamespaceProperty,
                // Ensure owner reference is set so garbage collection handles deletion automatically
                OwnerReferences = new List<V1OwnerReference>
                {
                    new V1OwnerReference
                    {
                        ApiVersion = agent.ApiVersion,
                        Kind = agent.Kind,
                        Name = agent.Metadata.Name,
                        Uid = agent.Metadata.Uid,
                        Controller = true,
                        BlockOwnerDeletion = true
                    }
                }
            },
            Spec = new V1StatefulSetSpec
            {
                ServiceName = $"{agent.Metadata.Name}-svc", // Headless service required for StatefulSet
                Replicas = agent.Spec.Replicas,
                Selector = new V1LabelSelector
                {
                    MatchLabels = new Dictionary<string, string> { { "app", agent.Metadata.Name } }
                },
                Template = new V1PodTemplateSpec
                {
                    Metadata = new V1ObjectMeta
                    {
                        Labels = new Dictionary<string, string> { { "app", agent.Metadata.Name } }
                    },
                    Spec = new V1PodSpec
                    {
                        Containers = new List<V1Container>
                        {
                            new V1Container
                            {
                                Name = "agent-container",
                                Image = agent.Spec.Image,
                                // Inject StatefulConfig as environment variables
                                Env = agent.Spec.StatefulConfig?.Select(kvp => 
                                    new V1EnvVar(kvp.Key, kvp.Value)).ToList(),
                                // Mount the persistent volume
                                VolumeMounts = new List<V1VolumeMount>
                                {
                                    new V1VolumeMount
                                    {
                                        Name = "agent-storage",
                                        MountPath = "/data"
                                    }
                                }
                            }
                        }
                    }
                },
                VolumeClaimTemplates = new List<V1PersistentVolumeClaim>
                {
                    new V1PersistentVolumeClaim
                    {
                        Metadata = new V1ObjectMeta { Name = "agent-storage" },
                        Spec = new V1PersistentVolumeClaimSpec
                        {
                            AccessModes = new List<string> { "ReadWriteOnce" },
                            Resources = new V1ResourceRequirements
                            {
                                Requests = new Dictionary<string, ResourceQuantity>
                                {
                                    { "storage", new ResourceQuantity(agent.Spec.StorageSize) }
                                }
                            },
                            StorageClassName = storageClassName
                        }
                    }
                }
            }
        };
    }
}
