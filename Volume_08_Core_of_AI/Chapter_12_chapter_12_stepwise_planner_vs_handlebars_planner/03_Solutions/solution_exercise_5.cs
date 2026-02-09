
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

# Source File: solution_exercise_5.cs
# Description: Solution for Exercise 5
# ==========================================

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Planning;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;
using System;

public class PlannerVisualizer
{
    public static async Task ExecuteAsync()
    {
        var kernel = Kernel.CreateBuilder()
            .AddAzureOpenAIChatCompletion(
                deploymentName: "gpt-4o-mini",
                endpoint: Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT"),
                apiKey: Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY"))
            .Build();

        kernel.ImportPluginFromObject(new MockOrderPlugin(), "Order");

        // Goal that might induce loops or complex branching
        var goal = "Order a pizza, track delivery, and notify the user.";

        var planner = new StepwisePlanner(kernel, new StepwisePlannerConfig 
        { 
            MaxIterations = 10,
            ExecutionSettings = new OpenAIPromptExecutionSettings { FunctionChoiceBehavior = FunctionChoiceBehavior.Auto() }
        });

        Console.WriteLine("Generating Plan...");
        var plan = planner.CreatePlan(goal);

        // 1. Execution Tracing & Data Structure
        // Since StepwisePlanner doesn't expose a direct "Trace" event in older versions,
        // we simulate the trace by analyzing the generated plan steps.
        // In a real runtime, we would hook into Kernel.Log or FunctionInvoked events.
        
        var steps = new List<ExecutionStep>();
        for (int i = 0; i < plan.Steps.Count; i++)
        {
            var step = plan.Steps[i];
            steps.Add(new ExecutionStep
            {
                StepId = i,
                FunctionName = $"{step.PluginName}.{step.Name}",
                InputParameters = step.Parameters,
                // Simulating "NextStepId" based on linear execution order
                NextStepId = i < plan.Steps.Count - 1 ? i + 1 : -1 
            });
        }

        // 2. Graph Generation (DOT String)
        var dotGraph = GenerateDotGraph(steps);
        Console.WriteLine("\n--- Graphviz DOT Output ---");
        Console.WriteLine(dotGraph);

        // 3. Interactive Challenge: Cycle Detection
        // We simulate a loop scenario for visualization purposes
        Console.WriteLine("\n--- Cycle Detection Simulation ---");
        var cyclicSteps = new List<ExecutionStep>(steps);
        // Artificially create a cycle for demonstration: Last step points to first
        if (cyclicSteps.Count > 0)
        {
            cyclicSteps[cyclicSteps.Count - 1].NextStepId = 0; 
        }

        var cyclicDotGraph = GenerateDotGraphWithCycleDetection(cyclicSteps);
        Console.WriteLine(cyclicDotGraph);
    }

    private static string GenerateDotGraph(List<ExecutionStep> steps)
    {
        var sb = new StringBuilder();
        sb.AppendLine("digraph ExecutionPlan {");
        sb.AppendLine("  rankdir=LR;"); // Left to Right layout
        sb.AppendLine("  node [shape=box, style=rounded];");

        foreach (var step in steps)
        {
            // Sanitize label
            var label = step.FunctionName.Replace("\"", "\\\"");
            sb.AppendLine($"  Node{step.StepId} [label=\"{step.StepId}: {label}\"];");
            
            if (step.NextStepId != -1)
            {
                sb.AppendLine($"  Node{step.StepId} -> Node{step.NextStepId};");
            }
        }

        sb.AppendLine("}");
        return sb.ToString();
    }

    private static string GenerateDotGraphWithCycleDetection(List<ExecutionStep> steps)
    {
        // DFS Cycle Detection
        var visited = new HashSet<int>();
        var recursionStack = new HashSet<int>();
        var cyclicNodes = new HashSet<int>();

        void DetectCycles(int nodeId)
        {
            visited.Add(nodeId);
            recursionStack.Add(nodeId);

            var nextNode = steps.Find(s => s.StepId == nodeId)?.NextStepId;
            if (nextNode != null && nextNode != -1)
            {
                if (!visited.Contains(nextNode.Value))
                {
                    DetectCycles(nextNode.Value);
                }
                else if (recursionStack.Contains(nextNode.Value))
                {
                    // Cycle detected
                    cyclicNodes.Add(nodeId);
                    cyclicNodes.Add(nextNode.Value);
                }
            }

            recursionStack.Remove(nodeId);
        }

        // Run detection for all nodes
        foreach (var step in steps)
        {
            if (!visited.Contains(step.StepId))
            {
                DetectCycles(step.StepId);
            }
        }

        // Build DOT Graph with Highlighting
        var sb = new StringBuilder();
        sb.AppendLine("digraph ExecutionPlanWithCycles {");
        sb.AppendLine("  rankdir=LR;");

        foreach (var step in steps)
        {
            var label = step.FunctionName.Replace("\"", "\\\"");
            string style;
            
            if (cyclicNodes.Contains(step.StepId))
            {
                style = "color=red, style=filled, fillcolor=lightcoral";
            }
            else
            {
                style = "color=black, style=rounded";
            }

            sb.AppendLine($"  Node{step.StepId} [label=\"{step.StepId}: {label}\", {style}];");
            
            if (step.NextStepId != -1)
            {
                var edgeColor = cyclicNodes.Contains(step.StepId) && cyclicNodes.Contains(step.NextStepId) ? "color=red" : "color=black";
                sb.AppendLine($"  Node{step.StepId} -> Node{step.NextStepId} [{edgeColor}];");
            }
        }

        sb.AppendLine("}");
        return sb.ToString();
    }
}

public class ExecutionStep
{
    public int StepId { get; set; }
    public string FunctionName { get; set; } = "";
    public string InputParameters { get; set; } = "";
    public int NextStepId { get; set; }
}

public class MockOrderPlugin
{
    [Description("Orders a pizza.")]
    public string OrderPizza(string type) => $"Ordered {type} pizza.";

    [Description("Tracks delivery status.")]
    public string TrackDelivery(string orderId) => $"Status for {orderId}: On the way.";

    [Description("Notifies the user.")]
    public string NotifyUser(string message) => $"Notification sent: {message}";
}
