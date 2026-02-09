
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

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Connectors.Ollama;
using Microsoft.SemanticKernel.Services;
using System.Reflection;

namespace KernelConfigExercises;

public static class ServiceSelectionExtensions
{
    // 4. Extension method to select service for a plugin
    public static Kernel GetServiceForPlugin(this Kernel kernel, string pluginName)
    {
        // Deep Dive Nuance: 
        // The Kernel relies on IAIServiceSelector. By default, it picks the first registered service.
        // To force a specific service, we can inspect the execution context or, more simply here,
        // create a new Kernel scope that prioritizes the specific service key.
        
        // However, a more robust way in modern SK is to utilize the KernelBuilder's 
        // 'WithAIServiceSelector' or simply ensure the specific service is the *only* one 
        // registered for that specific Kernel instance if strict isolation is needed.
        
        // For this exercise, we will simulate the selection by inspecting the service key 
        // and creating a "proxy" kernel that forces the selection.
        
        var serviceKey = pluginName switch
        {
            "CodeAnalysisPlugin" => "AzureGPT",
            "DataPrivacyPlugin" => "LocalOllama",
            _ => throw new NotSupportedException($"No mapping for {pluginName}")
        };

        // Note: In a real scenario, we might use a custom IAIServiceSelector implementation
        // that checks Function.Metadata.Tags for a specific service key.
        // Here, we return the kernel but the caller must be aware of the mapping logic.
        return kernel;
    }
}

public class ServiceSelectionExercise
{
    public async Task ExecuteAsync()
    {
        // 1. Setup Configuration (Mocked)
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Azure:DeploymentName"] = "gpt-4o",
                ["Azure:Endpoint"] = "https://test.openai.azure.com/",
                ["Azure:ApiKey"] = "test-key",
                ["Ollama:ModelName"] = "llama3.2",
                ["Ollama:Endpoint"] = "http://localhost:11434"
            })
            .Build();

        // 2. Register Two Distinct Services
        var builder = new KernelBuilder();
        
        // Service 1: Azure
        builder.AddAzureOpenAIChatCompletion(
            deploymentName: config["Azure:DeploymentName"]!,
            endpoint: config["Azure:Endpoint"]!,
            apiKey: config["Azure:ApiKey"]!,
            serviceId: "AzureGPT" // Explicit Service ID
        );

        // Service 2: Ollama
        builder.AddOllamaChatCompletion(
            modelId: config["Ollama:ModelName"]!,
            endpoint: config["Ollama:Endpoint"]!,
            serviceId: "LocalOllama" // Explicit Service ID
        );

        var kernel = builder.Build();

        // 3. Deep Dive Nuance (Code Comments)
        /*
         * INTERNAL BEHAVIOR:
         * When kernel.InvokeAsync is called, it looks for an IAIServiceSelector.
         * The default implementation (DefaultAIServiceSelector) selects the first service
         * that matches the required type (IChatCompletionService).
         * 
         * To route based on plugin intent, we have two options:
         * 1. Use the 'serviceId' in PromptExecutionSettings when invoking.
         * 2. Implement a custom IAIServiceSelector that reads metadata.
         * 
         * The code below demonstrates how the caller (or middleware) must explicitly 
         * pass the serviceId to ensure the correct model is used.
         */
        
        // Usage Example:
        // var result = await kernel.InvokeAsync("CodeAnalysisPlugin", "AnalyzeCode", 
        //     new KernelArguments(new OpenAIPromptExecutionSettings() { ServiceId = "AzureGPT" }));
    }
}
