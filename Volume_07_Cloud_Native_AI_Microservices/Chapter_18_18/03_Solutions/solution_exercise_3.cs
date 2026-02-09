
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

# Source File: solution_exercise_3.cs
# Description: Solution for Exercise 3
# ==========================================

using System;
using System.Collections.Generic;
using System.IO;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace IstioCanaryGenerator
{
    // 1. Istio Resource Modeling
    public class VirtualService
    {
        public string ApiVersion { get; set; } = "networking.istio.io/v1beta1";
        public string Kind { get; set; } = "VirtualService";
        public Metadata Metadata { get; set; }
        public VsSpec Spec { get; set; }
    }

    public class VsSpec
    {
        public List<string> Hosts { get; set; }
        public List<HttpRoute> Http { get; set; }
    }

    public class HttpRoute
    {
        public List<RouteDestination> Route { get; set; }
    }

    public class RouteDestination
    {
        public Destination Destination { get; set; }
        public int Weight { get; set; }
    }

    public class Destination
    {
        public string Host { get; set; }
        public string Subset { get; set; }
    }

    public class DestinationRule
    {
        public string ApiVersion { get; set; } = "networking.istio.io/v1beta1";
        public string Kind { get; set; } = "DestinationRule";
        public Metadata Metadata { get; set; }
        public DrSpec Spec { get; set; }
    }

    public class DrSpec
    {
        public string Host { get; set; }
        public List<Subset> Subsets { get; set; }
        public TrafficPolicy TrafficPolicy { get; set; } // For Circuit Breaking
    }

    public class Subset
    {
        public string Name { get; set; }
        public Dictionary<string, string> Labels { get; set; }
    }

    public class TrafficPolicy
    {
        public ConnectionPool ConnectionPool { get; set; }
        public OutlierDetection OutlierDetection { get; set; }
    }

    public class ConnectionPool
    {
        public Tcp Tcp { get; set; }
        public Http Http { get; set; }
    }

    public class Tcp { public int MaxConnections { get; set; } = 100; }
    public class Http { public int Http1MaxPendingRequests { get; set; } = 50; }

    public class OutlierDetection
    {
        public int Consecutive5xxErrors { get; set; } = 5;
        public int Interval { get; set; } = 10;
        public int BaseEjectionTime { get; set; } = 30;
    }

    public class Metadata
    {
        public string Name { get; set; }
    }

    class Program
    {
        // 2. Traffic Shifting Logic
        static void GenerateTrafficShiftYaml(int v1Percentage, int v2Percentage)
        {
            if (v1Percentage + v2Percentage != 100)
                throw new ArgumentException("Weights must sum to 100%");

            var vs = new VirtualService
            {
                Metadata = new Metadata { Name = "sentiment-agent-vs" },
                Spec = new VsSpec
                {
                    Hosts = new List<string> { "sentiment-agent.example.com" },
                    Http = new List<HttpRoute>
                    {
                        new HttpRoute
                        {
                            Route = new List<RouteDestination>
                            {
                                new RouteDestination
                                {
                                    Destination = new Destination { Host = "sentiment-agent", Subset = "v1" },
                                    Weight = v1Percentage
                                },
                                new RouteDestination
                                {
                                    Destination = new Destination { Host = "sentiment-agent", Subset = "v2" },
                                    Weight = v2Percentage
                                }
                            }
                        }
                    }
                }
            };

            var serializer = new SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            string yaml = serializer.Serialize(vs);
            File.WriteAllText($"virtualservice_{v1Percentage}_{v2Percentage}.yaml", yaml);
            Console.WriteLine($"Generated VirtualService with {v1Percentage}% v1 / {v2Percentage}% v2");
        }

        static void Main(string[] args)
        {
            // 3. Simulate Rollout
            GenerateTrafficShiftYaml(100, 0); // Baseline
            GenerateTrafficShiftYaml(90, 10); // Canary
            GenerateTrafficShiftYaml(0, 100); // Full Rollout

            // 4. Generate Destination Rule with Circuit Breaking
            var dr = new DestinationRule
            {
                Metadata = new Metadata { Name = "sentiment-agent-dr" },
                Spec = new DrSpec
                {
                    Host = "sentiment-agent",
                    Subsets = new List<Subset>
                    {
                        new Subset { Name = "v1", Labels = new Dictionary<string, string> { { "version", "v1" } } },
                        new Subset { Name = "v2", Labels = new Dictionary<string, string> { { "version", "v2" } } }
                    },
                    TrafficPolicy = new TrafficPolicy
                    {
                        ConnectionPool = new ConnectionPool
                        {
                            Tcp = new Tcp { MaxConnections = 100 },
                            Http = new Http { Http1MaxPendingRequests = 50 }
                        },
                        OutlierDetection = new OutlierDetection
                        {
                            Consecutive5xxErrors = 5,
                            Interval = 10,
                            BaseEjectionTime = 30
                        }
                    }
                }
            };

            var serializer = new SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();
            File.WriteAllText("destinationrule.yaml", serializer.Serialize(dr));
            Console.WriteLine("Generated DestinationRule with Circuit Breaking");
        }
    }
}
