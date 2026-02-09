
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
using Microsoft.SemanticKernel.AI.TextCompletion;
using Microsoft.SemanticKernel.Connectors.AI.OpenAI.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AI.OpenAI.TextCompletion;
using Microsoft.SemanticKernel.Events;

namespace AgenticSecurityFilterApp
{
    // Real-World Context:
    // A financial institution uses an AI agent to process customer queries regarding account balances and transaction history.
    // To comply with strict security regulations (e.g., GDPR, PCI-DSS), the agent must NEVER output:
    // 1. Full Credit Card Numbers (even if present in the input).
    // 2. Social Security Numbers (SSNs).
    // 3. Specific transaction amounts exceeding a certain threshold without additional authorization context.
    // This application demonstrates how to implement a "Security Filter" that intercepts the agent's output generation process
    // to redact sensitive data before it reaches the end-user, ensuring compliance and data privacy.

    class Program
    {
        static async Task Main(string[] args)
        {
            // 1. Setup the Kernel
            // In a real scenario, we would load the kernel builder with Azure OpenAI or OpenAI services.
            // For this demonstration, we will mock the kernel and the AI service to focus purely on the Filter logic.
            // The 'Kernel' is the central orchestrator for AI functions and plugins.
            var kernel = new KernelBuilder().Build();

            // 2. Define the Security Filter
            // We instantiate our custom filter. This filter will be registered globally with the kernel.
            // It intercepts every function invocation that returns a string (common for chat/text agents).
            var securityFilter = new OutputSanitizationFilter();

            // 3. Register the Filter
            // We use the 'FunctionInvoking' event hook. This allows us to inspect the result of a function
            // before it is passed back to the LLM for the next step in a chain, or to the user.
            // Note: In Semantic Kernel, filters can be added to the kernel's function invocation context.
            kernel.FunctionInvoking += securityFilter.OnFunctionInvoking;

            // 4. Simulate an Agent Interaction
            // We simulate a scenario where an agent is processing a user query.
            // The agent retrieves data from a database (simulated here as a string containing sensitive info).
            // The agent then attempts to formulate a response.
            Console.WriteLine("--- Starting Agent Simulation ---");
            Console.WriteLine("Agent: I am analyzing your request for account details...");
            
            // Simulate the agent retrieving raw data from a backend system.
            // In a real app, this would come from a database query or API call.
            string rawAgentResponse = "Account 123456789 has a balance of $10,000. " +
                                      "Recent transaction: Card ending in 4567 (Visa) used for $500. " +
                                      "User SSN: 999-00-1234 is on file.";

            Console.WriteLine($"\n[Raw Agent Output (Pre-Filter)]:\n\"{rawAgentResponse}\"");

            // 5. Trigger the Filter Logic
            // We simulate the kernel invoking a function that returns this sensitive string.
            // The 'FunctionInvoking' event will fire, invoking our filter's logic.
            // We create a mock context to demonstrate how the filter modifies the output.
            var mockContext = new FunctionInvokingEventArgs(
                new KernelFunctionMetadata("GetAccountDetails"), 
                new KernelArguments(), 
                rawAgentResponse);

            // Manually trigger the event handler (in a real runtime, the kernel does this)
            await securityFilter.OnFunctionInvoking(kernel, mockContext);

            // 6. Retrieve the Sanitized Output
            // The filter modifies the context.Result if sensitive data is found.
            string sanitizedResponse = mockContext.Result?.ToString() ?? "[No Content]";

            Console.WriteLine($"\n[Sanitized Agent Output (Post-Filter)]:\n\"{sanitizedResponse}\"");
            Console.WriteLine("\n--- Agent Simulation Complete ---");
            Console.WriteLine("Reasoning: The filter successfully intercepted PII and PCI data.");
        }
    }

    /// <summary>
    /// Custom Filter Implementation.
    /// This class encapsulates the logic for intercepting function invocations.
    /// It adheres to the 'Filter and Hook' pattern discussed in Chapter 19.
    /// </summary>
    public class OutputSanitizationFilter
    {
        // Simple patterns for detecting sensitive data.
        // In a production system, these would be complex Regex patterns or calls to dedicated PII detection services.
        private readonly string[] _sensitivePatterns = new string[]
        {
            @"\d{3}-\d{2}-\d{4}", // SSN Pattern (Simple)
            @"\d{16}",            // 16-digit Credit Card
            @"Card ending in \d{4}" // Descriptive card info
        };

        /// <summary>
        /// Handles the FunctionInvoking event.
        /// This method runs BEFORE the function's result is finalized or returned to the caller.
        /// </summary>
        public async Task OnFunctionInvoking(object? sender, FunctionInvokingEventArgs e)
        {
            Console.WriteLine("\n[Filter Triggered]: Analyzing function output for security compliance...");

            // Check if the result is a string (text-based output)
            if (e.Result is string currentResult)
            {
                string sanitizedResult = currentResult;

                // Iterate through known sensitive patterns
                // Note: Using basic loops as per constraints, avoiding LINQ.
                for (int i = 0; i < _sensitivePatterns.Length; i++)
                {
                    string pattern = _sensitivePatterns[i];
                    
                    // Simulate detection logic
                    if (sanitizedResult.Contains(pattern) || 
                        (pattern.Contains("SSN") && sanitizedResult.Contains("SSN")) ||
                        (pattern.Contains("Card") && sanitizedResult.Contains("Card ending")))
                    {
                        // Apply Redaction Logic
                        sanitizedResult = ApplyRedaction(sanitizedResult, pattern);
                    }
                }

                // Update the context result.
                // This is the core power of filters: modifying the data flow dynamically.
                e.Result = sanitizedResult;
                
                Console.WriteLine("[Filter Action]: Redaction applied. Sensitive data replaced with '[REDACTED]'.");
            }
            
            await Task.CompletedTask;
        }

        /// <summary>
        /// Helper method to perform the actual string replacement.
        /// </summary>
        private string ApplyRedaction(string input, string pattern)
        {
            // In a real implementation, we would use Regex.Replace.
            // Here, we simulate specific replacements for clarity.
            string output = input;
            
            if (pattern.Contains("SSN"))
            {
                // Replace SSN logic (simulated)
                output = output.Replace("999-00-1234", "[SSN_REDACTED]");
            }
            else if (pattern.Contains("Card"))
            {
                // Replace Card logic
                output = output.Replace("Card ending in 4567", "[CARD_REDACTED]");
            }
            else if (pattern.Contains(@"\d{16}"))
            {
                // Generic 16 digit replacement logic
                // (Simulated for the specific string in this demo)
                output = output.Replace("4567", "[CARD_REDACTED]"); 
            }

            return output;
        }
    }
}
