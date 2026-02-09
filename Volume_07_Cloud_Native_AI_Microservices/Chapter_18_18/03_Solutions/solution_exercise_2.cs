
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

# Source File: solution_exercise_2.cs
# Description: Solution for Exercise 2
# ==========================================

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace HpaMetricAdapter
{
    // 1. Metric Data Structure (K8s Custom Metrics API format)
    public class MetricValue
    {
        public DescribedObject DescribedObject { get; set; }
        public string MetricName { get; set; }
        public DateTime Timestamp { get; set; }
        public string Value { get; set; } // String representation of integer
    }

    public class DescribedObject
    {
        public string ApiVersion { get; set; }
        public string Kind { get; set; }
        public string Name { get; set; }
    }

    // 2. Mock Metric Collector (Background Service)
    public class MetricCollectorService : BackgroundService
    {
        private readonly ILogger<MetricCollectorService> _logger;
        private readonly Random _random = new Random();
        private int _currentQueueDepth = 0;

        public MetricCollectorService(ILogger<MetricCollectorService> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Simulate polling an agent endpoint
                    // In reality, this would be HttpClient.GetAsync("http://agent:8000/metrics")
                    await Task.Delay(5000, stoppingToken); // Poll every 5 seconds

                    // Simulate fluctuating load
                    _currentQueueDepth = _random.Next(0, 50);
                    
                    // 3. Edge Case Handling: Exponential Backoff Logic (Simplified)
                    if (_currentQueueDepth > 45) // Simulate high load causing potential timeouts
                    {
                        _logger.LogWarning("High queue depth detected, potential endpoint lag.");
                        // In a real scenario, we would implement a retry policy here
                    }

                    _logger.LogInformation($"Polled metric: InferenceQueueDepth = {_currentQueueDepth}");
                    
                    // Here we would push this metric to the Prometheus Adapter or Custom Metrics API
                }
                catch (HttpRequestException ex)
                {
                    _logger.LogError(ex, "Unreachable agent endpoint. Backing off...");
                    // Exponential backoff logic would go here
                    await Task.Delay(30000, stoppingToken); // Wait longer on error
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }
    }

    // 4. HPA Configuration Logic
    public class HpaConfigGenerator
    {
        public void GenerateHpaYaml()
        {
            // Note: While HPA is a K8s resource, it is usually applied via kubectl or manifests.
            // We model it here to show the structure.
            var hpaDefinition = new
            {
                ApiVersion = "autoscaling/v2",
                Kind = "HorizontalPodAutoscaler",
                Metadata = new { name = "agent-hpa" },
                Spec = new
                {
                    ScaleTargetRef = new
                    {
                        ApiVersion = "apps/v1",
                        Kind = "Deployment",
                        Name = "sentiment-agent"
                    },
                    MinReplicas = 2,
                    MaxReplicas = 10,
                    Metrics = new[]
                    {
                        new
                        {
                            Type = "Pods",
                            Pods = new
                            {
                                Metric = new { Name = "InferenceQueueDepth" },
                                Target = new { Type = "AverageValue", AverageValue = "20" }
                            }
                        }
                    },
                    // Behavioral constraints for stability
                    Behavior = new
                    {
                        ScaleDown = new { StabilizationWindowSeconds = 300 }, // Wait 5m before scaling down
                        ScaleUp = new { StabilizationWindowSeconds = 60 }
                    }
                }
            };

            var serializer = new SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            string yaml = serializer.Serialize(hpaDefinition);
            System.IO.File.WriteAllText("hpa.yaml", yaml);
            Console.WriteLine("Generated hpa.yaml");
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            // Simulate running the collector
            var host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    services.AddHostedService<MetricCollectorService>();
                })
                .Build();

            // Run generator
            var generator = new HpaConfigGenerator();
            generator.GenerateHpaYaml();

            // In a real app, we would await host.RunAsync();
            // For this exercise, we just keep the console open briefly to see logs
            Console.WriteLine("Metric Adapter Running. Press Ctrl+C to exit.");
        }
    }
}
