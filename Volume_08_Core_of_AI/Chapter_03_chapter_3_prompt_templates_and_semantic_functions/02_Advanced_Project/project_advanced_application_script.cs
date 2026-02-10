
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

// Source File: project_advanced_application_script.cs
// Description: Advanced Application Script
// ==========================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace AgenticSemanticKernelDemo
{
    // REAL-WORLD PROBLEM CONTEXT:
    // A customer support manager needs to triage incoming support tickets.
    // The goal is to automatically categorize tickets (e.g., "Billing", "Technical", "General")
    // and generate a polite, context-aware initial response draft for an agent to review.
    // This reduces manual workload and ensures consistent tone.
    //
    // SOLUTION OVERVIEW:
    // We will build a C# Console Application using Microsoft Semantic Kernel.
    // We will use:
    // 1. PROMPT TEMPLATES: To define the structure of the AI prompt (System Message, User Message).
    // 2. HANDLEBARS SYNTAX: To inject dynamic variables (Ticket Content, Customer Name) into the prompt.
    // 3. SEMANTIC FUNCTIONS: To encapsulate the logic for categorization and response generation.
    // 4. FUNCTION CALLING: The Kernel will route these functions to the LLM automatically.

    class Program
    {
        // Entry point of the application.
        static async Task Main(string[] args)
        {
            Console.WriteLine("Initializing Agentic Support Triage System...\n");

            // 1. KERNEL CONFIGURATION
            // We initialize the Kernel. In a production environment, we would load the API Key
            // and Endpoint from environment variables or Azure Key Vault.
            // For this demo, we simulate the configuration setup.
            var builder = Kernel.CreateBuilder();
            
            // NOTE: In a real scenario, you would add your specific AI service here.
            // e.g., builder.AddAzureOpenAIChatCompletion(...)
            // Since we cannot connect to a real LLM in this static code block, 
            // we will mock the execution logic to demonstrate the architectural flow.
            // We will focus on the STRUCTURE of the Semantic Functions.
            
            // 2. PROMPT TEMPLATE DEFINITION (Categorization)
            // We define a prompt template string using Handlebars syntax.
            // {{ticket_content}} is a placeholder that will be replaced at runtime.
            // We use a "System Message" to instruct the AI on its role and constraints.
            string categorizationPrompt = """
                <system_message>
                You are an AI Support Triage Assistant. Your task is to analyze the incoming support ticket content.
                You must classify the ticket into exactly one of these categories: 
                - "Billing" (if it involves payments, invoices, or subscriptions)
                - "Technical" (if it involves bugs, errors, or software functionality)
                - "General" (for all other inquiries)
                
                Output ONLY the category name. Do not add explanations.
                </system_message>
                
                <ticket_content>
                {{ticket_content}}
                </ticket_content>
                """;

            // 3. PROMPT TEMPLATE DEFINITION (Response Generation)
            // This prompt is more complex. It uses multiple variables: {{category}}, {{customer_name}}, and {{ticket_content}}.
            // It instructs the AI to generate a draft response based on the determined category.
            string responseGenerationPrompt = """
                <system_message>
                You are a customer support agent drafting an initial response.
                You have already determined the ticket category is: {{category}}.
                
                Guidelines:
                1. Address the customer by name: {{customer_name}}.
                2. Acknowledge their issue based on the content provided.
                3. If the category is "Billing", mention payment processing times.
                4. If "Technical", assure them an engineer is reviewing the logs.
                5. If "General", provide a standard reassurance message.
                6. Keep the tone professional and empathetic.
                </system_message>
                
                <original_ticket>
                {{ticket_content}}
                </original_ticket>
                """;

            // 4. SEMANTIC FUNCTION CREATION
            // We create "Semantic Functions" by associating the prompt templates with the Kernel.
            // In a full implementation, we would use kernel.CreateFunctionFromPrompt().
            // Here, we simulate the function objects to demonstrate the architecture.
            var categorizeFunction = new MockSemanticFunction("CategorizeTicket", categorizationPrompt);
            var generateResponseFunction = new MockSemanticFunction("GenerateDraftResponse", responseGenerationPrompt);

            Console.WriteLine("Semantic Functions created successfully.\n");

            // 5. SIMULATING REAL-WORLD DATA INPUT
            // We create a list of support tickets to process.
            // This represents the data flowing into our system.
            var tickets = new List<SupportTicket>
            {
                new SupportTicket { Id = 1, CustomerName = "Alice", Content = "My invoice for March is incorrect. I was charged twice." },
                new SupportTicket { Id = 2, CustomerName = "Bob", Content = "The application crashes when I click the 'Export' button." },
                new SupportTicket { Id = 3, CustomerName = "Charlie", Content = "How do I reset my password?" }
            };

            // 6. EXECUTION LOOP (The Agentic Flow)
            // We iterate through each ticket. This mimics a background worker or an API endpoint processing requests.
            foreach (var ticket in tickets)
            {
                Console.WriteLine($"--- Processing Ticket #{ticket.Id} ---");
                Console.WriteLine($"Customer: {ticket.CustomerName}");
                Console.WriteLine($"Issue: {ticket.Content}");

                try
                {
                    // STEP A: CATEGORIZATION
                    // We invoke the CategorizeTicket function.
                    // The Kernel replaces {{ticket_content}} with ticket.Content.
                    // The LLM returns the category string.
                    string category = await categorizeFunction.InvokeAsync(ticket.Content);
                    
                    Console.WriteLine($"[AI Analysis] Category: {category}");

                    // STEP B: RESPONSE GENERATION
                    // We invoke the GenerateDraftResponse function.
                    // We pass multiple arguments: category (determined in Step A), customer name, and content.
                    // The Kernel handles the variable substitution in the prompt template.
                    string draftResponse = await generateResponseFunction.InvokeAsync(
                        category, 
                        ticket.CustomerName, 
                        ticket.Content
                    );

                    // STEP C: OUTPUT
                    // Display the generated draft for the human agent to review.
                    Console.WriteLine($"[AI Draft]: \"{draftResponse}\"");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing ticket: {ex.Message}");
                }
                
                Console.WriteLine(); // Empty line for readability
            }

            Console.WriteLine("Batch processing complete.");
        }
    }

    // ---------------------------------------------------------
    // SUPPORTING CLASSES & MOCK IMPLEMENTATIONS
    // ---------------------------------------------------------
    // These classes simulate the data structures and kernel behavior 
    // required to make the code runnable and understandable.

    /// <summary>
    /// Represents a raw support ticket from the intake system.
    /// </summary>
    public class SupportTicket
    {
        public int Id { get; set; }
        public string CustomerName { get; set; }
        public string Content { get; set; }
    }

    /// <summary>
    /// A mock representation of a Semantic Kernel Function.
    /// In the real SDK, this is handled by KernelFunction and KernelArguments.
    /// We use this to demonstrate how the Prompt Template is processed.
    /// </summary>
    public class MockSemanticFunction
    {
        private readonly string _name;
        private readonly string _promptTemplate;

        public MockSemanticFunction(string name, string promptTemplate)
        {
            _name = name;
            _promptTemplate = promptTemplate;
        }

        /// <summary>
        /// Simulates the LLM invocation process.
        /// 1. Replaces Handlebars variables {{var}} with provided arguments.
        /// 2. Simulates network latency.
        /// 3. Returns a deterministic mock response based on the function name.
        /// </summary>
        public async Task<string> InvokeAsync(params string[] args)
        {
            string processedPrompt = _promptTemplate;

            // Basic Handlebars-style variable substitution logic
            // In a real scenario, the Handlebars engine handles this robustly.
            if (_name == "CategorizeTicket")
            {
                // Expecting 1 arg: ticket_content
                processedPrompt = processedPrompt.Replace("{{ticket_content}}", args[0]);
                
                // Simulate LLM logic: Determine category based on keywords
                if (processedPrompt.Contains("invoice") || processedPrompt.Contains("charged"))
                    return "Billing";
                if (processedPrompt.Contains("crashes") || processedPrompt.Contains("button"))
                    return "Technical";
                return "General";
            }
            else if (_name == "GenerateDraftResponse")
            {
                // Expecting 3 args: category, customer_name, ticket_content
                processedPrompt = processedPrompt.Replace("{{category}}", args[0]);
                processedPrompt = processedPrompt.Replace("{{customer_name}}", args[1]);
                processedPrompt = processedPrompt.Replace("{{ticket_content}}", args[2]);

                // Simulate LLM logic: Generate response based on category
                string response = "";
                switch (args[0]) // args[0] is the category
                {
                    case "Billing":
                        response = $"Dear {args[1]}, thank you for reaching out. We have reviewed your invoice discrepancy and are processing a refund immediately.";
                        break;
                    case "Technical":
                        response = $"Dear {args[1]}, thank you for reporting the crash. Our engineering team is currently analyzing the logs for the 'Export' button error.";
                        break;
                    case "General":
                        response = $"Dear {args[1]}, thank you for your inquiry. Please follow these steps to reset your password securely.";
                        break;
                    default:
                        response = $"Dear {args[1]}, we have received your request and are looking into it.";
                        break;
                }
                
                // Simulate async network call
                await Task.Delay(100); 
                return response;
            }

            return "Unknown function.";
        }
    }
}
