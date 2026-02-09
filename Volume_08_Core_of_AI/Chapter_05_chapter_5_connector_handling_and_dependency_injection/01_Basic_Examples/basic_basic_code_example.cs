
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

# Source File: basic_basic_code_example.cs
# Description: Basic Code Example
# ==========================================

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace SemanticKernelDiDemo
{
    // 1. THE CORE LOGIC (AGNOSTIC OF AI PROVIDER)
    // This class represents a business service that needs AI capabilities.
    // It does not know if the AI comes from Azure, OpenAI, or a local file.
    public class FinancialReportGenerator
    {
        private readonly Kernel _kernel;

        // DEPENDENCY INJECTION: We inject the Kernel (the orchestrator) via the constructor.
        // This adheres to the "Inversion of Control" principle.
        public FinancialReportGenerator(Kernel kernel)
        {
            _kernel = kernel;
        }

        // A simple plugin method that the AI will call.
        // We use [Description] to help the AI understand what this function does.
        [Description("Calculates the total revenue by summing up individual sales figures.")]
        public double CalculateTotalRevenue(double[] salesFigures)
        {
            double total = 0;
            foreach (var figure in salesFigures)
            {
                total += figure;
            }
            return total;
        }

        public async Task<string> GenerateAnalysisAsync(string prompt)
        {
            // We add the plugin dynamically to the kernel instance.
            // In a larger app, plugins are usually registered in the DI container.
            _kernel.Plugins.AddFromObject(this, "FinancialTools");

            // Define execution settings specifying the function calling behavior.
            var executionSettings = new OpenAIPromptExecutionSettings
            {
                FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
            };

            // Invoke the kernel with the prompt.
            var result = await _kernel.InvokePromptAsync(prompt, executionSettings);
            return result.ToString();
        }
    }

    class Program
    {
        static async Task Main(string[] args)
        {
            // 2. CONFIGURING THE SERVICE CONTAINER (DI)
            var services = new ServiceCollection();

            // Configure Logging (Essential for debugging AI interactions)
            services.AddLogging(builder => builder
                .AddConsole()
                .SetMinimumLevel(LogLevel.Warning)); // Suppress verbose Semantic Kernel logs for clarity

            // 3. CONNECTOR HANDLING
            // Here we register the specific AI connector.
            // In a real scenario, API_KEY would come from Azure Key Vault or User Secrets.
            // NOTE: This code will throw an exception if the key is invalid, 
            // but the architecture remains valid.
            string apiKey = Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY") ?? "YOUR_API_KEY_HERE";
            string deploymentName = "gpt-4o-mini"; // Or your specific deployment name
            string endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT") ?? "https://your-endpoint.openai.azure.com/";

            // Register the Kernel with a Singleton lifetime.
            // The Kernel is expensive to create, so we reuse it.
            services.AddSingleton<Kernel>(sp =>
            {
                // Create a builder to configure the Kernel.
                var builder = Kernel.CreateBuilder();

                // Add the Azure OpenAI Connector (Chat Completion Service).
                // This is where the "Connector Pattern" is applied.
                builder.AddAzureOpenAIChatCompletion(
                    deploymentName: deploymentName,
                    endpoint: endpoint,
                    apiKey: apiKey);

                // Build the kernel.
                return builder.Build();
            });

            // Register our business service (FinancialReportGenerator) as a Transient service.
            // A new instance is created every time it is requested (good for stateless logic).
            services.AddTransient<FinancialReportGenerator>();

            // 4. BUILDING THE SERVICE PROVIDER
            // This creates the container that will manage our dependencies.
            var serviceProvider = services.BuildServiceProvider();

            // 5. RESOLVING DEPENDENCIES
            // We request the FinancialReportGenerator from the container.
            // The container automatically resolves its dependency (Kernel) and injects it.
            var generator = serviceProvider.GetRequiredService<FinancialReportGenerator>();

            Console.WriteLine("--- Starting Financial Analysis ---");

            try
            {
                // Prepare a prompt that requires both reasoning and tool usage.
                string prompt = "Analyze the sales data [100, 250, 300]. Calculate the total revenue and provide a brief summary.";

                // Execute the logic.
                string analysis = await generator.GenerateAnalysisAsync(prompt);

                Console.WriteLine($"\nAI Analysis Result:\n{analysis}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nError: {ex.Message}");
                Console.WriteLine("(Note: If you see an authentication error, ensure your API Key is set correctly.)");
            }
            finally
            {
                // In a web app, the container is disposed automatically.
                // In a console app, we ensure disposal to release resources.
                if (serviceProvider is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
        }
    }
}
