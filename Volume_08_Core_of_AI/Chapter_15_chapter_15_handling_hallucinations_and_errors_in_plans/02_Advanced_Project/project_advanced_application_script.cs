
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
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AgenticErrorHandling
{
    // REAL-WORLD CONTEXT:
    // We are building an "Agentic Financial Transaction Planner" for a banking system.
    // The AI Agent receives natural language requests to transfer money.
    // Because AI models can hallucinate (inventing amounts or accounts) or make execution errors (invalid logic),
    // we must implement a robust validation pipeline before any real transaction occurs.
    // This code demonstrates the "Validation Step" and "Self-Correcting Loop" patterns from Chapter 15.

    public class Program
    {
        // Main entry point simulating the orchestration of the agent.
        public static async Task Main(string[] args)
        {
            Console.WriteLine("--- Agentic Financial Planner Initializing ---\n");

            // 1. Simulate a list of incoming natural language requests (some valid, some hallucinated).
            string[] userRequests = new string[]
            {
                "Transfer $500 from Savings to Checking for rent.",
                "Transfer $10,000,000 from Savings to Offshore Account (Urgent!)", // Hallucination: Insufficient funds
                "Move $50 from Checking to Savings", // Valid
                "Send $200 to unknown account" // Error: Missing account details
            };

            // 2. Iterate through requests using the Agentic Loop.
            foreach (var request in userRequests)
            {
                Console.WriteLine($"\n[INPUT]: {request}");

                // Instantiate the agent for this request.
                var agent = new FinancialAgent();

                // Execute the plan with error handling.
                try
                {
                    await agent.ExecutePlanAsync(request);
                }
                catch (Exception ex)
                {
                    // Final fallback if the self-correcting loop fails.
                    Console.WriteLine($"[CRITICAL FAILURE]: {ex.Message}");
                }
            }
        }
    }

    // DATA STRUCTURES (Basic Blocks)
    // We use simple classes to represent the structured output enforced by the agent.
    // This prevents the AI from returning unstructured text which is hard to validate.
    public class TransactionPlan
    {
        public string SourceAccount { get; set; }
        public string DestinationAccount { get; set; }
        public decimal Amount { get; set; }
        public bool IsValid { get; set; } = false;
        public string ValidationMessage { get; set; }
    }

    // CORE AGENT LOGIC
    public class FinancialAgent
    {
        // Dependency: Mock LLM Service (Simulating Semantic Kernel's AI Service)
        private readonly MockLLMService _llmService = new MockLLMService();

        // Dependency: Validation Service (Simulating Kernel Functions for validation)
        private readonly ValidationService _validator = new ValidationService();

        // Dependency: Retry Policy (Simulating Semantic Kernel's Retry Mechanisms)
        private readonly RetryPolicy _retryPolicy = new RetryPolicy(maxRetries: 2);

        public async Task ExecutePlanAsync(string userRequest)
        {
            TransactionPlan finalPlan = null;

            // PATTERN: Self-Correcting Loop
            // The agent attempts to generate a plan, validate it, and correct it if necessary.
            // We allow a maximum of 3 correction cycles to prevent infinite loops.
            int maxCorrectionCycles = 3;
            
            for (int cycle = 1; cycle <= maxCorrectionCycles; cycle++)
            {
                Console.WriteLine($"  [Cycle {cycle}] Generating Plan...");

                // 1. GENERATE PLAN (Simulating LLM Call)
                // We wrap this in a retry policy to handle transient errors (e.g., network timeouts).
                TransactionPlan currentPlan = await _retryPolicy.ExecuteAsync(
                    () => _llmService.GeneratePlanAsync(userRequest)
                );

                // 2. VALIDATE PLAN
                // Check for hallucinations (e.g., impossible amounts) and errors (missing data).
                bool isValid = _validator.Validate(currentPlan, out string validationMessage);
                
                currentPlan.IsValid = isValid;
                currentPlan.ValidationMessage = validationMessage;

                if (isValid)
                {
                    finalPlan = currentPlan;
                    break; // Plan is valid, exit loop.
                }
                else
                {
                    Console.WriteLine($"  [Validation Failed]: {validationMessage}");

                    // 3. REFLECTION PATTERN (Self-Correction)
                    // If validation fails, we feed the error back into the LLM to generate a new plan.
                    // We append the error context to the original request.
                    userRequest = $"Previous plan failed: {validationMessage}. Please correct the plan for: {userRequest}";
                }
            }

            // 4. EXECUTION GUARD
            if (finalPlan == null || !finalPlan.IsValid)
            {
                throw new InvalidOperationException("Agent failed to produce a valid plan after maximum correction cycles.");
            }

            // 5. FINAL EXECUTION
            // Only execute if the plan passed all validation gates.
            Console.WriteLine($"  [SUCCESS] Executing Plan: Transfer ${finalPlan.Amount} from {finalPlan.SourceAccount} to {finalPlan.DestinationAccount}");
            await Task.Delay(500); // Simulate processing time
            Console.WriteLine($"  [COMPLETE] Transaction recorded.");
        }
    }

    // MOCK LLM SERVICE
    // Simulates the AI model's behavior. In a real app, this would call Azure OpenAI or similar.
    public class MockLLMService
    {
        private Random _random = new Random();

        public async Task<TransactionPlan> GeneratePlanAsync(string request)
        {
            // Simulate network latency
            await Task.Delay(200);

            // Parse the request (Simulating Function Calling / Structured Output)
            // We use basic string parsing here to avoid complex libraries.
            var plan = new TransactionPlan();

            // Hallucination Logic: Sometimes the AI invents huge numbers or missing accounts.
            if (request.Contains("10,000,000") || request.Contains("million"))
            {
                plan.SourceAccount = "Savings";
                plan.DestinationAccount = "Offshore Account";
                plan.Amount = 10000000m; // Hallucination: Insufficient funds
            }
            else if (request.Contains("unknown"))
            {
                plan.SourceAccount = "Checking";
                plan.DestinationAccount = ""; // Error: Empty destination
                plan.Amount = 200m;
            }
            else if (request.Contains("$500"))
            {
                plan.SourceAccount = "Savings";
                plan.DestinationAccount = "Checking";
                plan.Amount = 500m;
            }
            else if (request.Contains("$50"))
            {
                plan.SourceAccount = "Checking";
                plan.DestinationAccount = "Savings";
                plan.Amount = 50m;
            }
            else
            {
                // Fallback for corrected requests
                if (request.Contains("Insufficient funds"))
                {
                    plan.SourceAccount = "Savings";
                    plan.DestinationAccount = "Checking";
                    plan.Amount = 500m; // Corrected amount
                }
                else if (request.Contains("Missing destination"))
                {
                    plan.SourceAccount = "Checking";
                    plan.DestinationAccount = "Checking"; // Defaulting to self to avoid loss
                    plan.Amount = 200m;
                }
            }

            return plan;
        }
    }

    // VALIDATION SERVICE
    // Contains the business logic to detect hallucinations and errors.
    public class ValidationService
    {
        // Simulated account balances
        private readonly Dictionary<string, decimal> _balances = new Dictionary<string, decimal>
        {
            { "Savings", 1000m },
            { "Checking", 500m }
        };

        public bool Validate(TransactionPlan plan, out string message)
        {
            // Check 1: Null or Empty Data (Execution Error)
            if (string.IsNullOrEmpty(plan.SourceAccount) || string.IsNullOrEmpty(plan.DestinationAccount))
            {
                message = "Missing source or destination account.";
                return false;
            }

            // Check 2: Hallucination - Account does not exist
            if (!_balances.ContainsKey(plan.SourceAccount) || !_balances.ContainsKey(plan.DestinationAccount))
            {
                message = "Invalid account name detected (Hallucination).";
                return false;
            }

            // Check 3: Hallucination - Insufficient Funds
            if (_balances[plan.SourceAccount] < plan.Amount)
            {
                message = $"Insufficient funds in {plan.SourceAccount}. Hallucination detected.";
                return false;
            }

            // Check 4: Logical Error - Self Transfer (Optional check)
            if (plan.SourceAccount == plan.DestinationAccount)
            {
                message = "Source and Destination are the same.";
                return false;
            }

            message = "Plan is valid.";
            return true;
        }
    }

    // RETRY POLICY
    // Implements the retry mechanism for transient failures.
    public class RetryPolicy
    {
        private readonly int _maxRetries;

        public RetryPolicy(int maxRetries)
        {
            _maxRetries = maxRetries;
        }

        public async Task<T> ExecuteAsync<T>(Func<Task<T>> action) where T : class
        {
            int attempt = 0;
            while (true)
            {
                try
                {
                    attempt++;
                    return await action();
                }
                catch (Exception ex)
                {
                    if (attempt >= _maxRetries)
                    {
                        Console.WriteLine($"  [Retry Policy] Max retries ({_maxRetries}) reached. Throwing exception.");
                        throw; // Re-throw if retries exhausted
                    }
                    Console.WriteLine($"  [Retry Policy] Attempt {attempt} failed: {ex.Message}. Retrying...");
                    await Task.Delay(100); // Backoff
                }
            }
        }
    }
}
