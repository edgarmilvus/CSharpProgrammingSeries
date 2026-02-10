
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

# Source File: solution_exercise_4.cs
# Description: Solution for Exercise 4
# ==========================================

using System;
using System.Collections.Generic;
using System.IO;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace CostOptimizationGenerator
{
    // 1. Resource Quota & Limit Range Classes
    public class ResourceQuota
    {
        public string ApiVersion { get; set; } = "v1";
        public string Kind { get; set; } = "ResourceQuota";
        public Metadata Metadata { get; set; }
        public Spec Spec { get; set; }

        public class Spec
        {
            public Dictionary<string, string> Hard { get; set; }
        }
    }

    public class LimitRange
    {
        public string ApiVersion { get; set; } = "v1";
        public string Kind { get; set; } = "LimitRange";
        public Metadata Metadata { get; set; }
        public LRSpec Spec { get; set; }

        public class LRSpec
        {
            public List<LimitRangeItem> Limits { get; set; }
        }

        public class LimitRangeItem
        {
            public string Type { get; set; } = "Container";
            public string DefaultRequest { get; set; }
            public string Default { get; set; }
            public string Max { get; set; }
            public string Min { get; set; }
        }
    }

    // 2. Extended Deployment Class for Affinity/Tolerations
    public class Deployment
    {
        public string ApiVersion { get; set; } = "apps/v1";
        public string Kind { get; set; } = "Deployment";
        public Metadata Metadata { get; set; }
        public DeploymentSpec Spec { get; set; }
    }

    public class DeploymentSpec
    {
        public int Replicas { get; set; }
        public Selector Selector { get; set; }
        public PodTemplateSpec Template { get; set; }
    }

    public class PodTemplateSpec
    {
        public Metadata Metadata { get; set; }
        public PodSpec Spec { get; set; }
    }

    public class PodSpec
    {
        public List<ContainerSpec> Containers { get; set; }
        public List<Toleration> Tolerations { get; set; }
        public Affinity Affinity { get; set; }
    }

    public class Toleration
    {
        public string Key { get; set; }
        public string Operator { get; set; } = "Equal";
        public string Value { get; set; }
        public string Effect { get; set; }
    }

    public class Affinity
    {
        public PodAntiAffinity PodAntiAffinity { get; set; }
    }

    public class PodAntiAffinity
    {
        public string TopologyKey { get; set; } = "kubernetes.io/hostname";
        public List<LabelSelectorRequirement> RequiredDuringSchedulingIgnoredDuringExecution { get; set; }
    }

    public class LabelSelectorRequirement
    {
        public string Key { get; set; }
        public string Operator { get; set; }
        public List<string> Values { get; set; }
    }

    // Helper classes (reuse from Ex 1 or define simplified versions here)
    public class Metadata { public string Name { get; set; } }
    public class Selector { public Dictionary<string, string> MatchLabels { get; set; } }
    public class ContainerSpec { public string Name { get; set; } }

    class Program
    {
        static void Main(string[] args)
        {
            var serializer = new SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            // 1. Generate ResourceQuota
            var quota = new ResourceQuota
            {
                Metadata = new Metadata { Name = "ai-agent-quota" },
                Spec = new ResourceQuota.Spec
                {
                    Hard = new Dictionary<string, string>
                    {
                        { "requests.cpu", "4" },
                        { "requests.memory", "8Gi" },
                        { "limits.cpu", "8" },
                        { "limits.memory", "16Gi" },
                        { "pods", "10" }
                    }
                }
            };
            File.WriteAllText("quota.yaml", serializer.Serialize(quota));

            // 2. Generate LimitRange (Interactive Challenge Scenario)
            var limitRange = new LimitRange
            {
                Metadata = new Metadata { Name = "ai-agent-limits" },
                Spec = new LimitRange.LRSpec
                {
                    Limits = new List<LimitRange.LimitRangeItem>
                    {
                        new LimitRange.LimitRangeItem
                        {
                            Type = "Container",
                            DefaultRequest = "cpu=200m,memory=128Mi", // Min guarantee
                            Default = "cpu=500m,memory=512Mi",        // Max burst default
                            Max = "cpu=2000m,memory=2Gi",             // Hard cap to prevent OOMKill
                            Min = "cpu=100m,memory=64Mi"              // Hard floor
                        }
                    }
                }
            };
            File.WriteAllText("limitrange.yaml", serializer.Serialize(limitRange));

            // 3. Cost-Optimized Deployment
            var deployment = new Deployment
            {
                Metadata = new Metadata { Name = "sentiment-agent-optimized" },
                Spec = new DeploymentSpec
                {
                    Replicas = 3,
                    Selector = new Selector { MatchLabels = new Dictionary<string, string> { { "app", "agent" } } },
                    Template = new PodTemplateSpec
                    {
                        Metadata = new Metadata { /* labels omitted for brevity */ },
                        Spec = new PodSpec
                        {
                            Containers = new List<ContainerSpec> { new ContainerSpec { Name = "agent" } },
                            
                            // Tolerations for Spot Instances
                            Tolerations = new List<Toleration>
                            {
                                new Toleration
                                {
                                    Key = "spot-instance",
                                    Value = "true",
                                    Effect = "NoSchedule"
                                }
                            },

                            // Affinity for High Availability
                            Affinity = new Affinity
                            {
                                PodAntiAffinity = new PodAntiAffinity
                                {
                                    RequiredDuringSchedulingIgnoredDuringExecution = new List<LabelSelectorRequirement>
                                    {
                                        new LabelSelectorRequirement
                                        {
                                            Key = "app",
                                            Operator = "In",
                                            Values = new List<string> { "agent" }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };
            File.WriteAllText("deployment-cost-optimized.yaml", serializer.Serialize(deployment));

            Console.WriteLine("Generated quota.yaml, limitrange.yaml, and deployment-cost-optimized.yaml");
        }
    }
}
