
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

namespace MultiAgentPersonaPattern
{
    // ---------------------------------------------------------
    // AGENT 1: The Project Manager (Orchestrator)
    // ---------------------------------------------------------
    // Context: This agent acts as the team lead. It receives the high-level goal,
    // breaks it down into actionable tasks, and delegates them to specialized agents.
    // It uses the Persona Pattern to enforce a structured, directive communication style.
    public class ProjectManagerAgent
    {
        public string Name { get; } = "Sarah (Project Manager)";
        
        // System Prompt: Defines the persona, tone, and decision-making logic.
        private readonly string _systemPrompt = 
            "You are Sarah, a Senior Project Manager. " +
            "Your role is to analyze requirements, break them into concrete steps, " +
            "and assign tasks to the appropriate team members (Developer or QA). " +
            "You must ensure all tasks are covered before declaring the project complete.";

        public List<string> Tasks { get; private set; } = new List<string>();

        public ProjectManagerAgent()
        {
            Console.WriteLine($"[{Name}]: System initialized. {_systemPrompt}");
        }

        // Method: Analyze the requirement and generate tasks.
        // This simulates the "brainstorming" phase of the persona.
        public void DefineScope(string requirement)
        {
            Console.WriteLine($"\n[{Name}]: Received requirement: '{requirement}'");
            
            // Simulating LLM logic with deterministic rules for this example.
            // In a real scenario, this would be a kernel invocation.
            if (requirement.Contains("website"))
            {
                Tasks.Add("Design UI Layout");
                Tasks.Add("Implement Frontend Logic");
                Tasks.Add("Perform Cross-Browser Testing");
            }
            else if (requirement.Contains("database"))
            {
                Tasks.Add("Design Schema");
                Tasks.Add("Write SQL Scripts");
                Tasks.Add("Optimize Query Performance");
            }
            else
            {
                Tasks.Add("Analyze Feasibility");
            }

            Console.WriteLine($"[{Name}]: Scope defined. Created {Tasks.Count} tasks.");
        }

        // Method: Delegate tasks based on agent capabilities.
        // This demonstrates the Orchestration Strategy.
        public void DelegateTasks(DeveloperAgent dev, QaAgent qa)
        {
            Console.WriteLine($"\n[{Name}]: Delegating tasks...");
            
            foreach (var task in Tasks)
            {
                if (task.Contains("Design") || task.Contains("Implement") || task.Contains("Write"))
                {
                    // Direct communication to the Developer persona.
                    dev.AssignTask(task);
                }
                else if (task.Contains("Test") || task.Contains("Optimize") || task.Contains("Analyze"))
                {
                    // Direct communication to the QA persona.
                    qa.AssignTask(task);
                }
            }
        }
    }

    // ---------------------------------------------------------
    // AGENT 2: The Developer (Specialist)
    // ---------------------------------------------------------
    // Context: This agent focuses on creation and implementation.
    // It has a specific persona that reacts to "implementation" tasks.
    public class DeveloperAgent
    {
        public string Name { get; } = "Alex (Developer)";
        private readonly List<string> _completedWork = new List<string>();

        public DeveloperAgent()
        {
            Console.WriteLine($"[{Name}]: System initialized. I build things.");
        }

        // Method: Receive and process a task.
        public void AssignTask(string task)
        {
            Console.WriteLine($"[{Name}]: Received task -> '{task}'");
            
            // Simulate work execution
            string result = ExecuteTask(task);
            _completedWork.Add(result);
        }

        // Internal logic simulating the "action" phase of the persona.
        private string ExecuteTask(string task)
        {
            // Simulating processing time
            System.Threading.Thread.Sleep(100); 
            
            string output = $"[DELIVERABLE]: {task} - Code committed.";
            Console.WriteLine($"    -> {Name} finished: {task}");
            return output;
        }

        public List<string> GetWorkLog() => _completedWork;
    }

    // ---------------------------------------------------------
    // AGENT 3: The QA Engineer (Validator)
    // ---------------------------------------------------------
    // Context: This agent focuses on verification and quality assurance.
    // It operates with a critical persona, often requiring clarification.
    public class QaAgent
    {
        public string Name { get; } = "Morgan (QA Engineer)";
        private readonly List<string> _testReports = new List<string>();

        public QaAgent()
        {
            Console.WriteLine($"[{Name}]: System initialized. I verify quality.");
        }

        // Method: Receive and process a task.
        public void AssignTask(string task)
        {
            Console.WriteLine($"[{Name}]: Received task -> '{task}'");
            
            // Simulate work execution
            string result = ExecuteTask(task);
            _testReports.Add(result);
        }

        // Internal logic simulating the "action" phase of the persona.
        private string ExecuteTask(string task)
        {
            // Simulating processing time
            System.Threading.Thread.Sleep(100);

            // QA Persona Logic: If a task is vague, they might flag it (Edge Case Handling).
            if (task.Contains("Analyze"))
            {
                Console.WriteLine($"    -> {Name} flagged: '{task}' is too vague. Requesting details.");
                return $"[FLAGGED]: {task} - Needs specification.";
            }

            string output = $"[REPORT]: {task} - Tests passed.";
            Console.WriteLine($"    -> {Name} finished: {task}");
            return output;
        }

        public List<string> GetTestReports() => _testReports;
    }

    // ---------------------------------------------------------
    // MAIN SIMULATION RUNNER
    // ---------------------------------------------------------
    // Context: This class simulates the Semantic Kernel Runtime environment.
    // It manages the lifecycle of the agents and facilitates the message exchange.
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("=== MULTI-AGENT PERSONA PATTERN SIMULATION ===\n");

            // 1. Instantiate Agents (Persona Pattern)
            // We create distinct instances, each with their own internal state and system prompts.
            var pm = new ProjectManagerAgent();
            var dev = new DeveloperAgent();
            var qa = new QaAgent();

            // 2. Define the Real-World Problem
            // A client requests a complex system: a "Customer Portal Website".
            string clientRequest = "Build a Customer Portal Website";

            // 3. Orchestration: Project Manager takes the lead
            // The PM analyzes the request and breaks it down.
            pm.DefineScope(clientRequest);

            // 4. Inter-Agent Communication & Delegation
            // The PM delegates to specialized agents based on the persona capabilities.
            pm.DelegateTasks(dev, qa);

            // 5. Simulating Asynchronous Execution
            // In a real kernel, agents would run in parallel. Here we await completion.
            await Task.Delay(500);

            // 6. Aggregation & Reporting
            // The PM (or a separate reporting agent) collects outputs to verify completion.
            Console.WriteLine("\n=== PROJECT STATUS REPORT ===");
            Console.WriteLine($"Total Developer Tasks: {dev.GetWorkLog().Count}");
            Console.WriteLine($"Total QA Tasks: {qa.GetTestReports().Count}");
            
            // 7. Conflict Resolution / Feedback Loop
            // Check for flagged items from the QA agent.
            foreach(var report in qa.GetTestReports())
            {
                if(report.Contains("FLAGGED"))
                {
                    Console.WriteLine($"\n[CRITICAL]: Conflict detected by QA. Rerouting to PM.");
                    // In a full loop, the PM would re-define scope here.
                }
            }

            Console.WriteLine("\n=== SIMULATION COMPLETE ===");
        }
    }
}
