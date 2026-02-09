
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

namespace AgenticSupportEngine
{
    class Program
    {
        // Entry point of the application
        static async Task Main(string[] args)
        {
            Console.WriteLine("Initializing Agentic Support Kernel...\n");

            // 1. KERNEL SETUP
            // We initialize the Semantic Kernel to orchestrate the AI interaction.
            // Note: In a real scenario, the API key would be stored securely, not hardcoded.
            // We are simulating the kernel behavior here since we cannot connect to a real endpoint
            // without a key, but the structure follows Semantic Kernel patterns.
            var kernel = Kernel.Builder.Build();

            // 2. FEW-SHOT PROMPTING CONSTRUCTION
            // We manually construct a prompt that includes examples (shots) to guide the model.
            // This reduces hallucinations by providing concrete patterns for the AI to follow.
            string fewShotPrompt = @"
Analyze the following user support ticket and categorize it.

--- FEW-SHOT EXAMPLES ---

[Input]: My monitor is flickering and shows no signal.
[Output]: Category: Hardware | Issue: Display Failure | Action: Check Cables/Driver

[Input]: I cannot log in to the portal, it says 'Invalid Credentials'.
[Output]: Category: Software | Issue: Authentication | Action: Reset Password

[Input]: The application crashes immediately upon opening.
[Output]: Category: Software | Issue: Crash | Action: Reinstall/Logs

[Input]: The printer is jammed and making a grinding noise.
[Output]: Category: Hardware | Issue: Mechanical Failure | Action: Physical Maintenance

--- END EXAMPLES ---

[Input]: {0}
[Output]: 
";

            // 3. CHAIN OF THOUGHT (COT) PROMPT CONSTRUCTION
            // We explicitly ask the model to "think step by step". This forces the LLM
            // to decompose the problem before arriving at a conclusion, improving accuracy.
            string cotPrompt = @"
Analyze the following user support ticket.

Instructions: 
1. First, reason through the problem step-by-step (CoT).
2. Then, provide the final structured output.

Ticket: {0}

Reasoning (Chain of Thought):
";

            // 4. DATA SOURCE (SIMULATED)
            // A list of real-world tickets to process.
            string[] tickets = new string[]
            {
                "My keyboard keys are sticking and double typing.",
                "I forgot my admin password for the dashboard.",
                "The server rack is overheating and the fans are loud."
            };

            // 5. EXECUTION PIPELINE
            foreach (var ticket in tickets)
            {
                Console.WriteLine($"=============================================================");
                Console.WriteLine($"PROCESSING TICKET: \"{ticket}\"");
                Console.WriteLine($"=============================================================");

                // --- Phase A: Few-Shot Execution (Simulated) ---
                Console.WriteLine("\n[Phase 1: Few-Shot Prompting]");
                // In a real kernel, we would invoke a function like: kernel.CreateSemanticFunction(fewShotPrompt);
                // Here we simulate the expected output based on the examples provided.
                string fewShotResult = SimulateFewShotResult(ticket);
                Console.WriteLine("Generated Output:");
                Console.WriteLine(fewShotResult);

                // --- Phase B: CoT Execution (Simulated) ---
                Console.WriteLine("\n[Phase 2: Chain of Thought Prompting]");
                // We construct the full prompt with the specific ticket.
                string finalCotPrompt = string.Format(cotPrompt, ticket);
                
                // Simulate the CoT process. The LLM would normally generate the text inside.
                // We are simulating the "Reasoning" part explicitly here to demonstrate the concept.
                string simulatedReasoning = SimulateCoTReasoning(ticket);
                Console.WriteLine("Reasoning (CoT):");
                Console.WriteLine(simulatedReasoning);

                // Simulate the final output derived from the reasoning
                string finalOutput = SimulateFinalOutput(ticket);
                Console.WriteLine("\nFinal Structured Output:");
                Console.WriteLine(finalOutput);

                Console.WriteLine("\n");
            }

            Console.WriteLine("Agentic Workflow Completed.");
        }

        // --- HELPER SIMULATION METHODS ---
        // NOTE: These methods exist solely to demonstrate the logic flow because 
        // we are not connecting to a live LLM in this console example.

        static string SimulateFewShotResult(string ticket)
        {
            if (ticket.Contains("keyboard") || ticket.Contains("sticking"))
                return "Category: Hardware | Issue: Peripheral Failure | Action: Clean/Replace";
            if (ticket.Contains("password") || ticket.Contains("forgot"))
                return "Category: Software | Issue: Authentication | Action: Reset Password";
            if (ticket.Contains("server") || ticket.Contains("overheating"))
                return "Category: Hardware | Issue: Thermal Throttling | Action: Check Cooling";
            return "Category: Unknown | Issue: Unclassified | Action: Escalate";
        }

        static string SimulateCoTReasoning(string ticket)
        {
            // This simulates the "Step-by-Step" reasoning a CoT prompt elicits.
            string reasoning = "";
            if (ticket.Contains("keyboard"))
            {
                reasoning += "1. The user mentions 'keyboard' and 'sticking'.\n";
                reasoning += "2. This indicates a physical input device.\n";
                reasoning += "3. 'Sticking' suggests debris or mechanical failure.\n";
                reasoning += "4. Conclusion: This is a hardware peripheral issue.";
            }
            else if (ticket.Contains("password"))
            {
                reasoning += "1. The user mentions 'forgot' and 'password'.\n";
                reasoning += "2. This relates to account access.\n";
                reasoning += "3. Access issues are software-related authentication problems.\n";
                reasoning += "4. Conclusion: This requires a credential reset.";
            }
            else if (ticket.Contains("server") || ticket.Contains("overheating"))
            {
                reasoning += "1. The user mentions 'server rack' and 'overheating'.\n";
                reasoning += "2. Servers require specific operating temperatures.\n";
                reasoning += "3. Loud fans indicate the system is trying to cool down.\n";
                reasoning += "4. Conclusion: This is a critical hardware thermal issue.";
            }
            return reasoning;
        }

        static string SimulateFinalOutput(string ticket)
        {
            if (ticket.Contains("keyboard"))
                return "{ \"Category\": \"Hardware\", \"Priority\": \"Low\", \"Assignee\": \"IT-Helpdesk\" }";
            if (ticket.Contains("password"))
                return "{ \"Category\": \"Software\", \"Priority\": \"Medium\", \"Assignee\": \"Identity-Team\" }";
            if (ticket.Contains("server"))
                return "{ \"Category\": \"Hardware\", \"Priority\": \"High\", \"Assignee\": \"Infra-Ops\" }";
            return "{ \"Category\": \"Unknown\", \"Priority\": \"Unknown\", \"Assignee\": \"Manager\" }";
        }
    }
}
