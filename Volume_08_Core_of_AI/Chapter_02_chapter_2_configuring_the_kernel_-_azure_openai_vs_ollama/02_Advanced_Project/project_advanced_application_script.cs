
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

# Source File: project_advanced_application_script.cs
# Description: Advanced Application Script
# ==========================================

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Connectors.Ollama;
using System;
using System.Threading.Tasks;

namespace AgenticConfigDemo
{
    // Real-world context: A "Smart Home Energy Optimizer" service.
    // This application analyzes energy usage patterns and suggests optimization strategies.
    // It must support two deployment modes:
    // 1. Cloud Mode (Azure OpenAI): For high-availability, complex analysis, and scalability.
    // 2. Local Mode (Ollama): For on-premise deployment in secure environments or for cost-efficient testing.
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("--- Smart Home Energy Optimizer ---");
            Console.WriteLine("Select AI Provider: 1. Azure OpenAI (Cloud)  2. Ollama (Local)");
            Console.Write("Choice: ");
            var input = Console.ReadLine();

            // 1. CONFIGURATION LOGIC
            // We use a simple conditional to determine which Kernel configuration to apply.
            // This mirrors the architectural decision of choosing a provider based on environment constraints.
            bool useAzure = input?.Trim() == "1";

            // 2. KERNEL INITIALIZATION
            // We initialize the Kernel based on the selected provider.
            // This encapsulates the complexity of service registration.
            IKernelBuilder kernelBuilder = Kernel.CreateBuilder();
            Kernel kernel;

            if (useAzure)
            {
                // AZURE OPENAI CONFIGURATION
                // Requires: Deployment Name, Endpoint, and API Key.
                // We assume these are stored in environment variables for security (not hardcoded).
                string deployment = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT") ?? "gpt-4";
                string endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT") ?? "https://your-resource.openai.azure.com/";
                string apiKey = Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY") ?? "YOUR_KEY_HERE";

                kernelBuilder.AddAzureOpenAIChatCompletion(deployment, endpoint, apiKey);
                Console.WriteLine(">> Configured for Azure OpenAI (Scalable Cloud Mode)");
            }
            else
            {
                // OLLAMA CONFIGURATION
                // Requires: Model ID and Endpoint (default is localhost).
                // Ollama runs locally, offering privacy and no egress costs.
                string modelId = "llama3.2"; // Or any locally installed model
                string endpoint = "http://localhost:11434";

                kernelBuilder.AddOllamaChatCompletion(modelId, endpoint);
                Console.WriteLine(">> Configured for Ollama (Local Private Mode)");
            }

            kernel = kernelBuilder.Build();

            // 3. AGENTIC LOGIC (SIMULATED)
            // In a real app, we would define plugins and planners.
            // Here, we simulate the interaction to demonstrate how the configured kernel
            // is passed to the execution logic, abstracting the provider details.
            string prompt = "Analyze this usage data: [High usage 6PM-9PM, Solar generation peaks at 1PM]. Suggest an optimization.";
            
            Console.WriteLine("\nProcessing Request...");
            
            // Execution
            var result = await kernel.InvokePromptAsync(prompt);

            // 4. OUTPUT HANDLING
            // The result is a string, regardless of the underlying provider.
            // This demonstrates the abstraction layer working.
            Console.WriteLine("\n--- Analysis Result ---");
            Console.WriteLine(result.ToString());
            Console.WriteLine("\n--- End of Session ---");
        }
    }
}
