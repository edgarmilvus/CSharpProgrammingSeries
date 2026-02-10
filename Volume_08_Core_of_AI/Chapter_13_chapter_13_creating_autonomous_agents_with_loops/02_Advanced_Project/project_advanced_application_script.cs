
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

namespace AutonomousInventoryAgent
{
    // ==========================================
    // REAL-WORLD PROBLEM CONTEXT
    // ==========================================
    // Scenario: A warehouse uses a legacy system where inventory counts are updated 
    // via manual daily logs. Stock levels are often inaccurate due to human error, 
    // misplaced items, or delayed data entry.
    // 
    // The Goal: Create an Autonomous Agent that continuously monitors inventory data.
    // Instead of just reporting errors, the agent must:
    // 1. Detect discrepancies (e.g., physical count vs. system count).
    // 2. Investigate potential causes by checking recent logs.
    // 3. Attempt to correct the data autonomously if confidence is high.
    // 4. Loop until the inventory status is "Verified" or a manual review is flagged.
    // ==========================================

    // ==========================================
    // CORE CONCEPT: AGENT STATE & MEMORY
    // ==========================================
    // In Semantic Kernel, Agents often need to maintain state across iterations.
    // We simulate this using a simple class to hold the "Memory" of the agent.
    // This represents the "Context" in a loop execution.
    public class InventoryContext
    {
        public string ItemId { get; set; }
        public int SystemCount { get; set; }
        public int PhysicalCount { get; set; }
        public List<string> RecentLogs { get; set; } = new List<string>();
        public string Status { get; set; } = "Pending";
        public int LoopIteration { get; set; } = 0;
        public bool RequiresManualReview { get; set; } = false;
    }

    // ==========================================
    // MOCK EXTERNAL SYSTEM (Simulating Kernel Skills/Plugins)
    // ==========================================
    // Before building the loop, we need tools the agent can use.
    // In Semantic Kernel, these would be C# methods decorated with [KernelFunction].
    // Here, we simulate them as simple methods that interact with "External Data".
    public static class WarehouseSystem
    {
        // Simulates fetching data from a database
        public static InventoryContext GetInventoryData(string itemId)
        {
            // Simulating a slight discrepancy in the system
            return new InventoryContext
            {
                ItemId = itemId,
                SystemCount = 100, // System thinks we have 100
                PhysicalCount = 95, // Physical count shows 95
                Status = "DiscrepancyDetected"
            };
        }

        // Simulates reading sensor logs or recent transaction logs
        public static List<string> FetchRecentLogs(string itemId)
        {
            // These logs provide context for the loop to analyze
            return new List<string>
            {
                "10:00 AM: Item shipped (Qty: -5)",
                "10:15 AM: System update failed (Network Timeout)",
                "10:30 AM: Manual scan registered (Qty: +0)"
            };
        }

        // Simulates an action to correct the database
        public static bool UpdateInventory(string itemId, int newCount)
        {
            // In a real scenario, this writes to the DB.
            // For this simulation, we assume success.
            Console.WriteLine($"   -> ACTION: Database updated for {itemId}. New count: {newCount}");
            return true;
        }
    }

    // ==========================================
    // THE AUTONOMOUS AGENT
    // ==========================================
    // This class encapsulates the logic for the loop.
    // It mimics the behavior of a Semantic Kernel Agent executing a plan.
    public class InventoryCorrectionAgent
    {
        // Maximum iterations to prevent infinite loops (Safety Guardrail)
        private const int MAX_LOOPS = 5;

        // The "Kernel" equivalent: Entry point for execution
        public async Task ExecuteAutonomousLoopAsync(string itemId)
        {
            Console.WriteLine($"[Agent] Initializing for Item: {itemId}");
            
            // 1. Initialize Context (Memory)
            InventoryContext context = WarehouseSystem.GetInventoryData(itemId);
            context.RecentLogs = WarehouseSystem.FetchRecentLogs(itemId);

            // 2. The Autonomous Loop
            // While the status is not "Verified" and we haven't hit the safety limit
            while (context.Status != "Verified" && context.LoopIteration < MAX_LOOPS)
            {
                context.LoopIteration++;
                Console.WriteLine($"\n--- Loop Iteration {context.LoopIteration} ---");

                // 3. Analyze Phase (Reasoning)
                // The agent evaluates the current state against goals
                await AnalyzeDiscrepancyAsync(context);

                // 4. Act Phase (Execution)
                // The agent performs actions based on analysis
                await TakeActionAsync(context);

                // 5. Evaluate Phase (Feedback)
                // The agent checks if the action resolved the issue
                await EvaluateResultAsync(context);

                // Artificial delay to simulate processing time
                await Task.Delay(500);
            }

            // 6. Loop Termination
            // Determine why the loop ended and report
            await FinalizeReportAsync(context);
        }

        // Step 1: Analysis Logic
        private async Task AnalyzeDiscrepancyAsync(InventoryContext context)
        {
            Console.WriteLine("   [Analysis] Checking discrepancy...");

            int difference = context.SystemCount - context.PhysicalCount;
            
            // Logic: If difference is small, check logs for recent shipments
            if (Math.Abs(difference) <= 5)
            {
                Console.WriteLine("   [Analysis] Discrepancy is low (<= 5). Checking logs for unprocessed shipments...");
                
                // Simulate reading logs to find a match
                bool foundShipmentLog = context.RecentLogs.Exists(log => log.Contains("shipped"));
                
                if (foundShipmentLog)
                {
                    Console.WriteLine("   [Analysis] Root cause found: Unprocessed shipment log.");
                    context.Status = "CorrectionNeeded";
                }
                else
                {
                    Console.WriteLine("   [Analysis] No log match found. Flagging for manual review.");
                    context.Status = "ManualReview";
                    context.RequiresManualReview = true;
                }
            }
            else
            {
                // Large discrepancy requires immediate manual review
                Console.WriteLine("   [Analysis] Discrepancy too large for auto-correction.");
                context.Status = "ManualReview";
                context.RequiresManualReview = true;
            }

            await Task.CompletedTask; // Async stub
        }

        // Step 2: Action Logic
        private async Task TakeActionAsync(InventoryContext context)
        {
            if (context.Status == "CorrectionNeeded")
            {
                Console.WriteLine("   [Action] Attempting to correct inventory count...");
                
                // The agent acts on the system
                bool success = WarehouseSystem.UpdateInventory(context.ItemId, context.PhysicalCount);
                
                if (success)
                {
                    // Update local context to reflect the change (feedback loop)
                    context.SystemCount = context.PhysicalCount; 
                }
            }
            else if (context.Status == "ManualReview")
            {
                Console.WriteLine("   [Action] Cannot resolve automatically. Flagging ticket.");
                // In a real app, this would trigger an email or ticket system
            }

            await Task.CompletedTask;
        }

        // Step 3: Evaluation Logic (Feedback)
        private async Task EvaluateResultAsync(InventoryContext context)
        {
            if (context.Status == "CorrectionNeeded")
            {
                // Re-check the condition after action
                if (context.SystemCount == context.PhysicalCount)
                {
                    Console.WriteLine("   [Evaluation] Count matches. Marking as Verified.");
                    context.Status = "Verified";
                }
                else
                {
                    Console.WriteLine("   [Evaluation] Correction failed. Retrying logic...");
                    // Loop continues automatically
                }
            }
            else if (context.Status == "ManualReview")
            {
                Console.WriteLine("   [Evaluation] Process halted for human intervention.");
                // Loop will break due to status check
            }

            await Task.CompletedTask;
        }

        // Step 4: Termination & Reporting
        private async Task FinalizeReportAsync(InventoryContext context)
        {
            Console.WriteLine("\n========================================");
            Console.WriteLine("AGENT EXECUTION SUMMARY");
            Console.WriteLine($"Item: {context.ItemId}");
            Console.WriteLine($"Iterations: {context.LoopIteration}");
            Console.WriteLine($"Final Status: {context.Status}");
            
            if (context.RequiresManualReview)
            {
                Console.WriteLine("!! ALERT: Manual intervention required. Ticket #1234 created.");
            }
            else
            {
                Console.WriteLine("SUCCESS: Inventory corrected autonomously.");
            }
            Console.WriteLine("========================================");

            await Task.CompletedTask;
        }
    }

    // ==========================================
    // MAIN PROGRAM (Simulation Entry Point)
    // ==========================================
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Initializing Autonomous Inventory Agent System...\n");

            // Instantiate the agent
            var agent = new InventoryCorrectionAgent();

            // Execute the autonomous loop for a specific item ID
            // This mimics starting a long-running process or a chat interaction
            await agent.ExecuteAutonomousLoopAsync("ITEM-550-Alpha");

            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }
    }
}
