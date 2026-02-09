
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

// File: KernelFactory.cs
using Microsoft.SemanticKernel;

public static class KernelFactory
{
    public static Kernel CreateKernel(string deploymentName, string endpoint, string apiKey)
    {
        // Validate inputs to prevent NullReferenceException during configuration
        if (string.IsNullOrWhiteSpace(deploymentName))
            throw new ArgumentNullException(nameof(deploymentName), "Deployment name cannot be null or empty.");
        
        if (string.IsNullOrWhiteSpace(endpoint))
            throw new ArgumentNullException(nameof(endpoint), "Endpoint cannot be null or empty.");

        if (string.IsNullOrWhiteSpace(apiKey))
            throw new ArgumentNullException(nameof(apiKey), "API Key cannot be null or empty.");

        // Use the modern KernelBuilder pattern
        var builder = Kernel.CreateBuilder();

        // Configure the Azure OpenAI Chat Completion service
        builder.Services.AddAzureOpenAIChatCompletion(
            deploymentName: deploymentName,
            endpoint: endpoint,
            apiKey: apiKey);

        // Build and return the kernel
        return builder.Build();
    }

    // Interactive Challenge: Fallback mechanism
    public static Kernel CreateKernelWithFallback(
        string azureDeployment, string azureEndpoint, string azureApiKey,
        string openAiModel, string openAiApiKey)
    {
        try
        {
            Console.WriteLine("Attempting to initialize with Azure OpenAI...");
            return CreateKernel(azureDeployment, azureEndpoint, azureApiKey);
        }
        catch (Exception ex) when (ex is HttpOperationException || ex is ArgumentNullException)
        {
            Console.WriteLine($"Azure initialization failed: {ex.Message}");
            Console.WriteLine("Falling back to standard OpenAI...");

            // Validate fallback inputs
            if (string.IsNullOrWhiteSpace(openAiModel) || string.IsNullOrWhiteSpace(openAiApiKey))
                throw new InvalidOperationException("Fallback configuration is incomplete.");

            var builder = Kernel.CreateBuilder();
            builder.Services.AddOpenAIChatCompletion(
                modelId: openAiModel,
                apiKey: openAiApiKey);
            
            return builder.Build();
        }
    }
}

// File: Program.cs
using Microsoft.SemanticKernel;

// Top-level statements
try
{
    // Retrieve configuration (simulated for this exercise)
    var deployment = Environment.GetEnvironmentVariable("AZURE_DEPLOYMENT") ?? "gpt-4";
    var endpoint = Environment.GetEnvironmentVariable("AZURE_ENDPOINT") ?? "https://example.openai.azure.com/";
    var apiKey = Environment.GetEnvironmentVariable("AZURE_API_KEY") ?? "fake-key";

    // Initialize the kernel
    var kernel = KernelFactory.CreateKernel(deployment, endpoint, apiKey);

    // Verify the kernel is not null
    if (kernel != null)
    {
        Console.WriteLine("Kernel initialized successfully.");
        Console.WriteLine($"Plugins registered: {kernel.Plugins.Count}");
    }
}
catch (ArgumentNullException argEx)
{
    Console.Error.WriteLine($"Configuration Error: {argEx.Message}");
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Unexpected Error: {ex.Message}");
}
