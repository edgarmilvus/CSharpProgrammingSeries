
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

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.AI.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AI.OpenAI.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AI.OpenAI.TextEmbedding;
using Microsoft.SemanticKernel.Memory;
using Microsoft.SemanticKernel.SkillDefinition;
using Microsoft.SemanticKernel.Orchestration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace SemanticKernelAdvancedApp
{
    // 1. DEFINING THE CONNECTOR INTERFACE
    // Real-world context: We need to standardize how we fetch data from external sources.
    // This interface represents a contract for any data provider (e.g., SQL Database, API, Vector Store).
    public interface ICustomerDataConnector
    {
        Task<string> GetCustomerFeedbackAsync(string customerId);
    }

    // 2. IMPLEMENTING A CUSTOM CONNECTOR
    // Real-world context: Simulating a legacy SQL database or a slow API call.
    // This connector adheres to the Dependency Injection pattern by implementing the interface.
    public class LegacyDatabaseConnector : ICustomerDataConnector
    {
        public async Task<string> GetCustomerFeedbackAsync(string customerId)
        {
            // Simulating network latency and I/O operations
            await Task.Delay(100); 
            
            // Mock data retrieval logic
            if (customerId == "CUST_001")
            {
                return "The product quality is excellent, but the delivery was delayed by 2 days.";
            }
            else if (customerId == "CUST_002")
            {
                return "Customer support was very helpful and resolved my issue quickly.";
            }
            
            return "No feedback available for this customer.";
        }
    }

    // 3. DEFINING AN AGENTIC TOOL (SKILL)
    // Real-world context: A specialized module that performs a specific business logic.
    // In Semantic Kernel, these are often called Skills or Plugins.
    public class FeedbackAnalysisSkill
    {
        private readonly ICustomerDataConnector _dataConnector;

        // Constructor Injection: The DI container will provide the implementation
        public FeedbackAnalysisSkill(ICustomerDataConnector dataConnector)
        {
            _dataConnector = dataConnector;
        }

        [SKFunction]
        public async Task<string> AnalyzeSentimentAsync(SKContext context)
        {
            string customerId = context.Variables["customerId"];
            
            // 1. Fetch raw data using the injected connector
            string feedback = await _dataConnector.GetCustomerFeedbackAsync(customerId);

            // 2. Basic logic to analyze sentiment (Simulating AI processing)
            // In a real scenario, this would call an LLM via the Kernel.
            bool isPositive = feedback.Contains("excellent") || feedback.Contains("helpful");
            bool isNegative = feedback.Contains("delayed") || feedback.Contains("issue");

            string sentiment = "Neutral";
            if (isPositive) sentiment = "Positive";
            if (isNegative) sentiment = "Negative";

            // 3. Construct the result
            string result = $"Customer: {customerId}\nFeedback: {feedback}\nSentiment: {sentiment}\n";
            
            // 4. Store result in context for downstream processes
            context.Variables["analysisResult"] = result;
            
            return result;
        }
    }

    // 4. THE CORE APPLICATION ORCHESTRATOR
    // Real-world context: The main entry point that configures the DI container and orchestrates the Kernel.
    class Program
    {
        static async Task Main(string[] args)
        {
            // --- STEP 1: CONFIGURING DEPENDENCY INJECTION ---
            // We use the standard Microsoft.Extensions.DependencyInjection container.
            // This decouples our logic from concrete implementations.
            var services = new ServiceCollection();

            // Register the Connector with a Scoped lifetime.
            // Why Scoped? In a web app, this ensures one DB connection per request.
            // In a console app, it means one instance per scope (lifetime of the scope).
            services.AddScoped<ICustomerDataConnector, LegacyDatabaseConnector>();

            // Register the Skill.
            // It depends on ICustomerDataConnector, so the container resolves that automatically.
            services.AddScoped<FeedbackAnalysisSkill>();

            // Register the Semantic Kernel instance.
            // We need to configure the Kernel with an AI backend (Azure OpenAI) to function.
            // NOTE: For this example to run without real keys, we will mock the Kernel setup 
            // or handle configuration carefully. 
            services.AddKernel()
                .AddAzureOpenAIChatCompletion("gpt-35-turbo", "https://fake-endpoint.openai.azure.com/", "fake-key");

            // Build the ServiceProvider
            IServiceProvider serviceProvider = services.BuildServiceProvider();

            // --- STEP 2: ORCHESTRATING THE AGENTIC WORKFLOW ---
            Console.WriteLine("--- Starting Advanced Application Script ---");
            Console.WriteLine("Initializing Kernel and Dependencies...\n");

            // Create a scope for the application logic
            using (var scope = serviceProvider.CreateScope())
            {
                // Resolve the Skill from the DI container
                var analysisSkill = scope.ServiceProvider.GetRequiredService<FeedbackAnalysisSkill>();

                // Create a context for the operation
                var context = new SKContext();
                
                // Simulate processing multiple customers
                string[] customerIds = { "CUST_001", "CUST_002", "CUST_003" };

                foreach (var id in customerIds)
                {
                    Console.WriteLine($"Processing Customer ID: {id}...");
                    
                    // Set input variable for the skill
                    context.Variables.Set("customerId", id);

                    // Execute the skill
                    // The skill internally uses the injected connector.
                    string result = await analysisSkill.AnalyzeSentimentAsync(context);

                    Console.WriteLine(result);
                    Console.WriteLine("-----------------------------");
                }
            }

            Console.WriteLine("--- Application Script Completed ---");
        }
    }
}
