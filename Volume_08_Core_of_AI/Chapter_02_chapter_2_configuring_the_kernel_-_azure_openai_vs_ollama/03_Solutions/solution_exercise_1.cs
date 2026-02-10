
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

using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Connectors.Ollama;
using System.ClientModel;
using System.Diagnostics;

namespace KernelConfigExercises;

// 1. Configuration Records using 'required' and modern syntax
public record AzureConfig
{
    public required string DeploymentName { get; init; }
    public required string Endpoint { get; init; }
    public required string ApiKey { get; init; }
}

public record OllamaConfig
{
    public required string ModelName { get; init; }
    public required string Endpoint { get; init; }
}

public record FailoverConfig
{
    public required string ProviderType { get; init; }
    public required OllamaConfig Ollama { get; init; }
}

// 2. Interface for Polymorphism
public interface IAiServiceConnector
{
    void ConfigureKernel(KernelBuilder builder, IConfiguration config);
}

// 3. Azure Connector
public class AzureOpenAiConnector : IAiServiceConnector
{
    public void ConfigureKernel(KernelBuilder builder, IConfiguration config)
    {
        var azureConfig = config.GetSection("Azure").Get<AzureConfig>() 
            ?? throw new InvalidOperationException("Azure configuration section is missing.");

        // Validate required fields manually if Get<T> returns nulls
        if (string.IsNullOrEmpty(azureConfig.DeploymentName) || 
            string.IsNullOrEmpty(azureConfig.Endpoint) || 
            string.IsNullOrEmpty(azureConfig.ApiKey))
        {
            throw new InvalidOperationException("Missing required fields in Azure configuration (DeploymentName, Endpoint, or ApiKey).");
        }

        // Simulate potential auth failure for the Interactive Challenge
        if (azureConfig.ApiKey == "FAIL_SIMULATION")
        {
            throw new ClientAuthenticationException("Simulated Azure Authentication Failure.");
        }

        builder.AddAzureOpenAIChatCompletion(
            deploymentName: azureConfig.DeploymentName,
            endpoint: azureConfig.Endpoint,
            apiKey: azureConfig.ApiKey
        );
    }
}

// 4. Ollama Connector
public class OllamaConnector : IAiServiceConnector
{
    public void ConfigureKernel(KernelBuilder builder, IConfiguration config)
    {
        var ollamaConfig = config.GetSection("Ollama").Get<OllamaConfig>() 
            ?? throw new InvalidOperationException("Ollama configuration section is missing.");

        if (string.IsNullOrEmpty(ollamaConfig.ModelName) || string.IsNullOrEmpty(ollamaConfig.Endpoint))
        {
            throw new InvalidOperationException("Missing required fields in Ollama configuration (ModelName or Endpoint).");
        }

        builder.AddOllamaChatCompletion(
            modelId: ollamaConfig.ModelName,
            endpoint: ollamaConfig.Endpoint
        );
    }
}

// 5. Factory with Failover Logic
public class ConnectorFactory
{
    public void ConfigureKernel(KernelBuilder builder, IConfiguration config)
    {
        string providerType = config["ProviderType"] ?? "Ollama";
        
        // Interactive Challenge: Failover Logic
        if (providerType.Equals("Azure", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                Console.WriteLine("Attempting Azure Connection...");
                new AzureOpenAiConnector().ConfigureKernel(builder, config);
                return;
            }
            catch (ClientAuthenticationException ex)
            {
                Console.WriteLine($"Azure Auth Failed: {ex.Message}. Switching to Failover...");
                
                // Fallback to Ollama based on FailoverConfig
                var failoverSection = config.GetSection("FailoverProvider");
                if (failoverSection.Exists())
                {
                    // We treat the failover section as the root for the Ollama connector
                    // or we could create a specific adapter. Here we pass the specific section.
                    var tempConfig = new ConfigurationBuilder()
                        .AddInMemoryCollection(new Dictionary<string, string?>
                        {
                            ["Ollama:ModelName"] = failoverSection["Ollama:ModelName"],
                            ["Ollama:Endpoint"] = failoverSection["Ollama:Endpoint"]
                        })
                        .Build();

                    new OllamaConnector().ConfigureKernel(builder, tempConfig);
                    return;
                }
                throw; // If failover not configured, rethrow
            }
        }

        // Standard Ollama path
        new OllamaConnector().ConfigureKernel(builder, config);
    }
}
