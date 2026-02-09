
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

# Source File: solution_exercise_1.cs
# Description: Solution for Exercise 1
# ==========================================

using System;
using System.Collections.Generic;
using System.IO;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace K8sManifestGenerator
{
    // 1. Define Kubernetes Resource Classes
    public class Metadata
    {
        public string Name { get; set; }
        public Dictionary<string, string> Labels { get; set; }
    }

    public class ContainerSpec
    {
        public string Name { get; set; }
        public string Image { get; set; }
        public List<Port> Ports { get; set; }
        public Dictionary<string, string> Resources { get; set; } // Simplified for exercise
    }

    public class Port
    {
        public int ContainerPort { get; set; }
    }

    public class PodTemplateSpec
    {
        public Metadata Metadata { get; set; }
        public Spec Spec { get; set; }
    }

    public class Spec
    {
        public List<ContainerSpec> Containers { get; set; }
    }

    public class DeploymentSpec
    {
        public int Replicas { get; set; }
        public Selector Selector { get; set; }
        public PodTemplateSpec Template { get; set; }
    }

    public class Selector
    {
        public Dictionary<string, string> MatchLabels { get; set; }
    }

    public class KubernetesDeployment
    {
        public string ApiVersion { get; set; } = "apps/v1";
        public string Kind { get; set; } = "Deployment";
        public Metadata Metadata { get; set; }
        public DeploymentSpec Spec { get; set; }
    }

    public class ServicePort
    {
        public int Port { get; set; }
        public int TargetPort { get; set; }
        public string Protocol { get; set; } = "TCP";
    }

    public class ServiceSpec
    {
        public Dictionary<string, string> Selector { get; set; }
        public List<ServicePort> Ports { get; set; }
        public string Type { get; set; } = "ClusterIP";
    }

    public class KubernetesService
    {
        public string ApiVersion { get; set; } = "v1";
        public string Kind { get; set; } = "Service";
        public Metadata Metadata { get; set; }
        public ServiceSpec Spec { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            // 2. Configuration: Image Name via Command Line or Env Var
            string imageName = args.Length > 0 ? args[0] : Environment.GetEnvironmentVariable("AGENT_IMAGE");
            if (string.IsNullOrEmpty(imageName))
            {
                // Default for simulation if not provided
                imageName = "my-registry/sentiment-agent:latest";
            }

            Console.WriteLine($"Generating manifests for image: {imageName}");

            // 3. Create Deployment Object
            var deployment = new KubernetesDeployment
            {
                Metadata = new Metadata { Name = "sentiment-agent" },
                Spec = new DeploymentSpec
                {
                    Replicas = 3,
                    Selector = new Selector { MatchLabels = new Dictionary<string, string> { { "app", "sentiment-agent" } } },
                    Template = new PodTemplateSpec
                    {
                        Metadata = new Metadata { Labels = new Dictionary<string, string> { { "app", "sentiment-agent" } } },
                        Spec = new Spec
                        {
                            Containers = new List<ContainerSpec>
                            {
                                new ContainerSpec
                                {
                                    Name = "agent",
                                    Image = imageName,
                                    Ports = new List<Port> { new Port { ContainerPort = 8000 } },
                                    // Architectural Nuance: Resource Management
                                    // Setting requests guarantees the pod gets at least 0.5 CPU and 512Mi RAM.
                                    // Setting limits prevents the pod from consuming excess resources (noisy neighbor).
                                    // Too low: OOMKill (Out of Memory) if the model spikes.
                                    // Too high: Resource fragmentation (scheduling fails despite apparent capacity).
                                    Resources = new Dictionary<string, string>
                                    {
                                        { "requests", "cpu=500m,memory=512Mi" },
                                        { "limits", "cpu=1000m,memory=1Gi" }
                                    }
                                }
                            }
                        }
                    }
                }
            };

            // 4. Create Service Object
            var service = new KubernetesService
            {
                Metadata = new Metadata { Name = "sentiment-agent-svc" },
                Spec = new ServiceSpec
                {
                    Selector = new Dictionary<string, string> { { "app", "sentiment-agent" } },
                    Ports = new List<ServicePort>
                    {
                        new ServicePort { Port = 80, TargetPort = 8000 }
                    }
                }
            };

            // 5. Serialize to YAML using YamlDotNet
            var serializer = new SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            string deploymentYaml = serializer.Serialize(deployment);
            string serviceYaml = serializer.Serialize(service);

            // 6. Output Files
            File.WriteAllText("deployment.yaml", deploymentYaml);
            File.WriteAllText("service.yaml", serviceYaml);

            Console.WriteLine("Manifests generated: deployment.yaml, service.yaml");
        }
    }
}
