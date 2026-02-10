
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

# Source File: theory_theoretical_foundations_part2.cs
# Description: Theoretical Foundations
# ==========================================

using System;
using System.Threading.Tasks;
using k8s;
using k8s.Models;

// Example of how a C# agent might interact with the Kubernetes API to discover peers.
// This illustrates the "Self-Awareness" of cloud-native agents.

public class PeerDiscoveryService
{
    private readonly IKubernetes _client;

    public PeerDiscoveryService()
    {
        // In a real pod, this config is loaded automatically via ServiceAccount tokens.
        var config = KubernetesClientConfiguration.InClusterConfig();
        _client = new Kubernetes(config);
    }

    public async Task<List<string>> GetAgentPodsAsync()
    {
        // Query the API for pods labeled with 'app=ai-agent'
        var pods = await _client.ListNamespacedPodAsync(
            namespaceParameter: "default",
            labelSelector: "app=ai-agent"
        );

        var addresses = new List<string>();
        foreach (var pod in pods.Items)
        {
            // Agents use the Pod IP for direct communication
            addresses.Add(pod.Status.PodIP);
        }
        return addresses;
    }
}
