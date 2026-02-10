
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

namespace AgenticWorkflowDemo
{
    // Real-World Context: Automated Customer Support Escalation System
    // Problem: A company receives thousands of support tickets daily. 
    // We need an agent that can:
    // 1. Classify the ticket (Billing, Technical, General).
    // 2. Route it to the correct department.
    // 3. Determine urgency based on keywords.
    // 4. Draft an initial response.
    //
    // We will implement this using the "Stepwise Planner" pattern logic:
    // Breaking a complex task into discrete, deterministic steps.

    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("=== Agentic Support System Initialized ===\n");

            // Simulated incoming support tickets
            var tickets = new List<SupportTicket>
            {
                new SupportTicket { Id = 101, Content = "My credit card was charged twice for the subscription.", CustomerEmail = "alice@example.com" },
                new SupportTicket { Id = 102, Content = "The application crashes when I click the 'Export' button.", CustomerEmail = "bob@example.com" },
                new SupportTicket { Id = 103, Content = "I forgot my password and need help resetting it.", CustomerEmail = "charlie@example.com" }
            };

            // Initialize the orchestrator (The "Kernel" equivalent)
            var orchestrator = new SupportOrchestrator();

            foreach (var ticket in tickets)
            {
                Console.WriteLine($"Processing Ticket #{ticket.Id}...");
                
                // Execute the Stepwise Plan
                var result = await orchestrator.ProcessTicketAsync(ticket);
                
                // Output the result
                Console.WriteLine($"  [Step 1] Classification: {result.Department}");
                Console.WriteLine($"  [Step 2] Urgency: {result.Priority}");
                Console.WriteLine($"  [Step 3] Route To: {result.AssignedAgent}");
                Console.WriteLine($"  [Step 4] Draft Response: \"{result.DraftResponse}\"");
                Console.WriteLine(new string('-', 40));
            }
        }
    }

    // ---------------------------------------------------------
    // DATA MODELS (Basic Classes)
    // ---------------------------------------------------------
    public class SupportTicket
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public string CustomerEmail { get; set; }
    }

    public class ProcessingResult
    {
        public string Department { get; set; }
        public string Priority { get; set; }
        public string AssignedAgent { get; set; }
        public string DraftResponse { get; set; }
    }

    // ---------------------------------------------------------
    // CORE LOGIC: The Stepwise Planner Implementation
    // ---------------------------------------------------------
    public class SupportOrchestrator
    {
        // Simulated Knowledge Base (In a real app, this comes from a Vector Database)
        private readonly string[] _billingKeywords = { "charge", "refund", "invoice", "payment", "card" };
        private readonly string[] _techKeywords = { "crash", "bug", "error", "export", "login" };

        public async Task<ProcessingResult> ProcessTicketAsync(SupportTicket ticket)
        {
            // Initialize result container
            var result = new ProcessingResult();

            // -----------------------------------------------------
            // STEP 1: Classification (Deterministic Logic)
            // -----------------------------------------------------
            // Why: We use strict keyword matching for predictable routing.
            // This avoids the randomness of LLM classification for simple tasks.
            result.Department = ClassifyTicket(ticket.Content);

            // -----------------------------------------------------
            // STEP 2: Urgency Assessment (Heuristic Logic)
            // -----------------------------------------------------
            // Why: Prioritization is critical for SLA (Service Level Agreement).
            // We check for high-impact words.
            result.Priority = AssessUrgency(ticket.Content);

            // -----------------------------------------------------
            // STEP 3: Routing (Rule-Based Assignment)
            // -----------------------------------------------------
            // Why: We map departments to specific human agents or queues.
            // This ensures accountability.
            result.AssignedAgent = DetermineAssignment(result.Department, result.Priority);

            // -----------------------------------------------------
            // STEP 4: Response Drafting (Template Filling)
            // -----------------------------------------------------
            // Why: Handlebars Planner logic would be ideal here for dynamic templates.
            // Since we are in a basic C# context, we simulate template filling.
            result.DraftResponse = GenerateResponse(ticket, result.Department, result.Priority);

            // Simulate async work (e.g., database write)
            await Task.Delay(100); 

            return result;
        }

        // --- Sub-Step Logic Implementation ---

        private string ClassifyTicket(string content)
        {
            // Convert to lowercase for case-insensitive matching
            string lowerContent = content.ToLower();

            // Check for Billing Keywords
            foreach (var keyword in _billingKeywords)
            {
                if (lowerContent.Contains(keyword))
                    return "Billing";
            }

            // Check for Technical Keywords
            foreach (var keyword in _techKeywords)
            {
                if (lowerContent.Contains(keyword))
                    return "Technical";
            }

            // Default fallback
            return "General";
        }

        private string AssessUrgency(string content)
        {
            // Simple heuristic: If "crash" or "charge twice" is present, it's High.
            string lowerContent = content.ToLower();
            
            if (lowerContent.Contains("crash") || lowerContent.Contains("twice"))
            {
                return "High";
            }
            
            return "Normal";
        }

        private string DetermineAssignment(string department, string urgency)
        {
            // Complex logic: Urgency overrides standard routing
            if (department == "Billing" && urgency == "High")
            {
                return "Senior Finance Agent (Priority Queue)";
            }
            
            if (department == "Technical")
            {
                return "L2 Technical Support";
            }

            return "General Support Queue";
        }

        private string GenerateResponse(SupportTicket ticket, string department, string urgency)
        {
            // Simulating a Handlebars template: "Hello {{Name}}, we received your {{Department}} request..."
            // In a real Semantic Kernel scenario, we would call a Prompt Function here.
            
            string baseTemplate = "Hello {0}, thank you for contacting us regarding your {1} issue. ";
            
            if (urgency == "High")
            {
                baseTemplate += "We have flagged this as urgent and escalated it to a specialist. ";
            }
            else
            {
                baseTemplate += "Our team will review this shortly. ";
            }

            return string.Format(baseTemplate, ticket.CustomerEmail, department);
        }
    }
}
