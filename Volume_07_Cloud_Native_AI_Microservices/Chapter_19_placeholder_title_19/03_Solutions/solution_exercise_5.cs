
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

// Source File: solution_exercise_5.cs
// Description: Solution for Exercise 5
// ==========================================

using k8s;
using k8s.Models;
using System.Collections.Generic;

public class GpuNodeScheduler
{
    public V1Deployment CreateGpuDeployment(string name, string image)
    {
        // 1. Define Resource Limits for GPU
        var resourceLimits = new Dictionary<string, string>
        {
            { "nvidia.com/gpu", "1" }
        };

        // 2. Define Node Affinity (Required During Scheduling)
        var nodeAffinity = new V1NodeAffinity
        {
            RequiredDuringSchedulingIgnoredDuringExecution = new V1NodeSelector
            {
                NodeSelectorTerms = new List<V1NodeSelectorTerm>
                {
                    new V1NodeSelectorTerm
                    {
                        MatchExpressions = new List<V1NodeSelectorRequirement>
                        {
                            new V1NodeSelectorRequirement
                            {
                                Key = "accelerator",
                                Operator = "In",
                                Values = new List<string> { "nvidia-tesla-v100" }
                            }
                        }
                    }
                }
            }
        };

        // 3. Define Tolerations (Allow scheduling on tainted nodes)
        var tolerations = new List<V1Toleration>
        {
            new V1Toleration
            {
                Key = "gpu",
                Operator = "Equal",
                Value = "true",
                Effect = "NoSchedule"
            }
        };

        // 4. Construct the Deployment
        var deployment = new V1Deployment
        {
            Metadata = new V1ObjectMeta { Name = name },
            Spec = new V1DeploymentSpec
            {
                Replicas = 1,
                Selector = new V1LabelSelector
                {
                    MatchLabels = new Dictionary<string, string> { { "app", name } }
                },
                Template = new V1PodTemplateSpec
                {
                    Metadata = new V1ObjectMeta
                    {
                        Labels = new Dictionary<string, string> { { "app", name } }
                    },
                    Spec = new V1PodSpec
                    {
                        // Inject Affinity
                        Affinity = new V1Affinity
                        {
                            NodeAffinity = nodeAffinity
                        },
                        // Inject Tolerations
                        Tolerations = tolerations,
                        Containers = new List<V1Container>
                        {
                            new V1Container
                            {
                                Name = "inference",
                                Image = image,
                                // Inject GPU Resource Limit
                                Resources = new V1ResourceRequirements
                                {
                                    Limits = resourceLimits,
                                    Requests = resourceLimits // Usually request = limit for exclusive devices
                                }
                            }
                        }
                    }
                }
            }
        };

        return deployment;
    }
}
