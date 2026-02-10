
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

// C# Kubernetes Client: AgentMonitor.cs
// Uses the KubernetesClient NuGet package to interact with the cluster.
// Prerequisites: Kubeconfig file must be present in ~/.kube/config or accessible via in-cluster config.

using System;
using System.Linq;
using System.Threading.Tasks;
using k8s;
using k8s.Models;

public class AgentMonitor
{
    public static async Task Main(string[] args)
    {
        try
        {
            // 1. Initialize Kubernetes client configuration
            // This automatically detects if running inside a cluster or uses local kubeconfig
            var config = KubernetesClientConfiguration.BuildDefaultConfig();
            var client = new Kubernetes(config);

            Console.WriteLine($"Connected to cluster: {config.Host}");

            // 2. List V1Pod objects with label selector "app=ai-agent"
            var podList = await client.ListNamespacedPodAsync(
                namespaceParameter: "default", 
                labelSelector: "app=ai-agent");

            Console.WriteLine($"Found {podList.Items.Count} pods with label 'app=ai-agent'.");

            foreach (var pod in podList.Items)
            {
                Console.WriteLine($"\nPod: {pod.Metadata.Name}");
                Console.WriteLine($"Phase: {pod.Status.Phase}");
                Console.WriteLine($"Pod IP: {pod.Status.PodIP}");

                // 3. Filter pods where Status.Phase == "Running" but Conditions indicate NotReady
                // Check for 'Ready' condition status being 'False'
                var readyCondition = pod.Status.Conditions?.FirstOrDefault(c => c.Type == "Ready");
                if (readyCondition != null && readyCondition.Status == "False")
                {
                    Console.WriteLine($"  [!] WARNING: Pod is Running but NOT Ready. Reason: {readyCondition.Reason}");
                    
                    // 4. Stream logs for the first failing container
                    // We check container statuses to find which container is not ready
                    var failingContainer = pod.Status.ContainerStatuses?.FirstOrDefault(c => !c.Ready);
                    if (failingContainer != null)
                    {
                        Console.WriteLine($"  Streaming logs for container: {failingContainer.Name}...");
                        
                        var logStream = await client.ReadNamespacedPodLogAsync(
                            name: pod.Metadata.Name,
                            namespaceParameter: "default",
                            container: failingContainer.Name,
                            follow: false, // Set to true to stream continuously
                            tailLines: 20);

                        Console.WriteLine("--- LOG SNIPPET ---");
                        Console.WriteLine(logStream);
                        Console.WriteLine("-------------------");
                    }
                }
                else if (readyCondition?.Status == "True")
                {
                    Console.WriteLine("  Status: Healthy and Ready.");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }
}
