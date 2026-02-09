
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

# Source File: solution_exercise_5.cs
# Description: Solution for Exercise 5
# ==========================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace TritonOptimizedPipeline
{
    // 1. Triton Config Generator
    public static class TritonConfigGenerator
    {
        public static void GenerateConfigPbTxt()
        {
            // Triton uses a custom config format, not YAML/JSON.
            // We generate this string manually.
            var sb = new StringBuilder();
            sb.AppendLine("name: \"sentiment_analysis\"");
            sb.AppendLine("platform: \"pytorch_libtorch\""); // Assuming PyTorch backend
            sb.AppendLine("max_batch_size: 8");
            sb.AppendLine("input [");
            sb.AppendLine("  {");
            sb.AppendLine("    name: \"TEXT\"");
            sb.AppendLine("    data_type: TYPE_STRING");
            sb.AppendLine("    dims: [ 1 ]");
            sb.AppendLine("  }");
            sb.AppendLine("]");
            sb.AppendLine("output [");
            sb.AppendLine("  {");
            sb.AppendLine("    name: \"SENTIMENT\"");
            sb.AppendLine("    data_type: TYPE_FP32");
            sb.AppendLine("    dims: [ 1 ]");
            sb.AppendLine("  }");
            sb.AppendLine("]");
            sb.AppendLine("instance_group [");
            sb.AppendLine("  {");
            sb.AppendLine("    count: 1");
            sb.AppendLine("    kind: KIND_GPU");
            sb.AppendLine("  }");
            sb.AppendLine("]");

            File.WriteAllText("models/sentiment_analysis/config.pbtxt", sb.ToString());
            Console.WriteLine("Generated Triton config.pbtxt");
        }
    }

    // 2. Extended Deployment Classes for Triton
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
        public List<Volume> Volumes { get; set; }
    }

    public class ContainerSpec
    {
        public string Name { get; set; }
        public string Image { get; set; }
        public Dictionary<string, object> Resources { get; set; }
        public List<VolumeMount> VolumeMounts { get; set; }
        public Probe LivenessProbe { get; set; }
        public Probe ReadinessProbe { get; set; }
        public Probe StartupProbe { get; set; }
    }

    public class Probe
    {
        public HttpGet HttpGet { get; set; }
        public int InitialDelaySeconds { get; set; }
        public int PeriodSeconds { get; set; }
        public int FailureThreshold { get; set; }
    }

    public class HttpGet
    {
        public string Path { get; set; }
        public int Port { get; set; }
    }

    public class Volume
    {
        public string Name { get; set; }
        public EmptyDir EmptyDir { get; set; }
    }

    public class EmptyDir { }

    public class VolumeMount
    {
        public string Name { get; set; }
        public string MountPath { get; set; }
    }

    public class Metadata { public string Name { get; set; } }
    public class Selector { public Dictionary<string, string> MatchLabels { get; set; } }

    // 3. Advanced HPA Generation
    public class HpaGenerator
    {
        public void GenerateTritonHpa()
        {
            var hpa = new
            {
                ApiVersion = "autoscaling/v2",
                Kind = "HorizontalPodAutoscaler",
                Metadata = new { name = "triton-hpa" },
                Spec = new
                {
                    ScaleTargetRef = new { ApiVersion = "apps/v1", Kind = "Deployment", Name = "triton-server" },
                    MinReplicas = 2,
                    MaxReplicas = 20,
                    Metrics = new[]
                    {
                        new
                        {
                            Type = "Resource",
                            Resource = new { Name = "cpu", Target = new { Type = "Utilization", AverageUtilization = 70 } }
                        }
                    },
                    // 4. Performance Tuning: Behavior
                    Behavior = new
                    {
                        ScaleUp = new
                        {
                            StabilizationWindowSeconds = 60,
                            Policies = new[]
                            {
                                new { Type = "Pods", Value = 2, PeriodSeconds = 15 } // Add 2 pods at a time
                            }
                        }
                    }
                }
            };

            var serializer = new SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();
            File.WriteAllText("triton-hpa.yaml", serializer.Serialize(hpa));
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            // 1. Generate Triton Config
            TritonConfigGenerator.GenerateConfigPbTxt();

            // 2. Generate Optimized Deployment
            var deployment = new Deployment
            {
                Metadata = new Metadata { Name = "triton-server" },
                Spec = new DeploymentSpec
                {
                    Replicas = 1,
                    Selector = new Selector { MatchLabels = new Dictionary<string, string> { { "app", "triton" } } },
                    Template = new PodTemplateSpec
                    {
                        Metadata = new Metadata { /* labels */ },
                        Spec = new PodSpec
                        {
                            Volumes = new List<Volume>
                            {
                                new Volume { Name = "model-store", EmptyDir = new EmptyDir() }
                            },
                            Containers = new List<ContainerSpec>
                            {
                                new ContainerSpec
                                {
                                    Name = "triton",
                                    Image = "nvcr.io/nvidia/tritonserver:23.10-py3",
                                    Resources = new Dictionary<string, object>
                                    {
                                        { "limits", new Dictionary<string, string> { { "nvidia.com/gpu", "1" } } }
                                    },
                                    VolumeMounts = new List<VolumeMount>
                                    {
                                        new VolumeMount { Name = "model-store", MountPath = "/models" }
                                    },
                                    // 4. Startup Probe for Cold Starts
                                    StartupProbe = new Probe
                                    {
                                        HttpGet = new HttpGet { Path = "/v2/health/ready", Port = 8000 },
                                        FailureThreshold = 60, // Allow 60 * 10s = 10 minutes for model loading
                                        PeriodSeconds = 10,
                                        InitialDelaySeconds = 5
                                    },
                                    LivenessProbe = new Probe
                                    {
                                        HttpGet = new HttpGet { Path = "/v2/health/live", Port = 8000 },
                                        PeriodSeconds = 5
                                    },
                                    ReadinessProbe = new Probe
                                    {
                                        HttpGet = new HttpGet { Path = "/v2/health/ready", Port = 8000 },
                                        PeriodSeconds = 5
                                    }
                                }
                            }
                        }
                    }
                }
            };

            var serializer = new SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();
            File.WriteAllText("triton-deployment.yaml", serializer.Serialize(deployment));

            // 3. Generate HPA
            new HpaGenerator().GenerateTritonHpa();

            Console.WriteLine("Generated Triton manifests (Deployment, HPA, config.pbtxt)");
        }
    }
}
