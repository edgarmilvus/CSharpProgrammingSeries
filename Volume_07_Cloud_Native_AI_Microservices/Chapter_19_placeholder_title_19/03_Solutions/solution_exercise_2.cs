
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
using System.Collections.Generic;
using System.Threading.Tasks;

public class ModelDeployer
{
    private readonly IKubernetes _client;

    public ModelDeployer(IKubernetes client)
    {
        _client = client;
    }

    public async Task CreateDeploymentWithModelStorage(string deploymentName, string pvcName, string storageSize, string mountPath)
    {
        // 1. Define the Volume (referencing the PVC)
        var volume = new V1Volume
        {
            Name = "model-weights",
            PersistentVolumeClaim = new V1PersistentVolumeClaimVolumeSource
            {
                ClaimName = pvcName
            }
        };

        // 2. Define the VolumeMount (where it goes inside the container)
        var volumeMount = new V1VolumeMount
        {
            Name = "model-weights",
            MountPath = mountPath
        };

        // 3. Construct the Deployment Object
        var deployment = new V1Deployment
        {
            ApiVersion = "apps/v1",
            Kind = "Deployment",
            Metadata = new V1ObjectMeta { Name = deploymentName },
            Spec = new V1DeploymentSpec
            {
                Replicas = 1,
                Selector = new V1LabelSelector
                {
                    MatchLabels = new Dictionary<string, string> { { "app", deploymentName } }
                },
                Template = new V1PodTemplateSpec
                {
                    Metadata = new V1ObjectMeta
                    {
                        Labels = new Dictionary<string, string> { { "app", deploymentName } }
                    },
                    Spec = new V1PodSpec
                    {
                        // Attach the volume definition to the pod spec
                        Volumes = new List<V1Volume> { volume },
                        Containers = new List<V1Container>
                        {
                            new V1Container
                            {
                                Name = "inference-engine",
                                Image = "my-inference-image:latest",
                                // Attach the mount to the container spec
                                VolumeMounts = new List<V1VolumeMount> { volumeMount }
                            }
                        }
                    }
                }
            }
        };

        // 4. Create the Deployment in the cluster
        await _client.CreateNamespacedDeploymentAsync(deployment, "default");
    }
}
