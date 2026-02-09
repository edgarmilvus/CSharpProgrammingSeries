
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
using System.ComponentModel;
using System.Text;

// Reusing BankingPlugin from Exercise 4 for demonstration
public class BankingPlugin
{
    [KernelFunction]
    public string CheckBalance(string accountNumber) => $"Balance for {accountNumber}: $1,500";

    [KernelFunction]
    public string Transfer(decimal amount, string fromAccount, string toAccount) => $"Transferred ${amount} from {fromAccount} to {toAccount}.";
}

public class ExecutionTracker
{
    public List<string> Events { get; } = new();
    private readonly object _lock = new();

    public void LogEvent(string eventName, string? details = null)
    {
        lock (_lock)
        {
            Events.Add($"[{DateTime.Now:HH:mm:ss.fff}] {eventName} - {details}");
        }
    }

    public string GenerateExecutionGraph()
    {
        var sb = new StringBuilder();
        sb.AppendLine("digraph G {");
        sb.AppendLine("  rankdir=TB;");
        sb.AppendLine("  node [shape=box];");

        // Simple visualization logic based on event logs
        // In a real complex scenario, we would build a node/edge graph structure
        // Here we map specific events to graph nodes
        
        string lastFunction = "";
        foreach (var evt in Events)
        {
            if (evt.Contains("Invoking") && !evt.Contains("LLM"))
            {
                // Native Function Start
                var funcName = evt.Split('-').Last().Trim();
                sb.AppendLine($"  {funcName} [label=\"Native: {funcName}\", shape=box, style=filled, color=lightblue];");
                lastFunction = funcName;
            }
            else if (evt.Contains("PromptRendered"))
            {
                sb.AppendLine($"  LLM_Decision [label=\"LLM Decides\", shape=diamond];");
            }
            else if (evt.Contains("Invoked") && !evt.Contains("LLM"))
            {
                // Native Function End (Logic to connect nodes)
                if (!string.IsNullOrEmpty(lastFunction))
                {
                     sb.AppendLine($"  LLM_Decision -> {lastFunction};");
                     sb.AppendLine($"  {lastFunction} -> LLM_Response;");
                }
            }
        }
        
        sb.AppendLine("  LLM_Response [label=\"Final LLM Response\", shape=ellipse];");
        sb.AppendLine("}");
        return sb.ToString();
    }
}

class Program
{
    static async Task Main(string[] args)
    {
        var builder = Kernel.CreateBuilder();
        // builder.AddOpenAIChatCompletion(...);
        var kernel = builder.Build();
        
        var tracker = new ExecutionTracker();
        var plugin = kernel.ImportPluginFromObject(new BankingPlugin(), "Banking");

        // 1. Subscribe to Events
        kernel.FunctionInvoking += (s, e) => 
            tracker.LogEvent("FunctionInvoking", e.Function.Name);
        
        kernel.FunctionInvoked += (s, e) => 
            tracker.LogEvent("FunctionInvoked", e.Function.Name);
        
        kernel.PromptRendered += (s, e) => 
            tracker.LogEvent("PromptRendered");

        // 2. Simulate a Planner Scenario (Interactive Challenge)
        // We will manually simulate what a Planner does to ensure we capture the flow
        Console.WriteLine("Executing simulated planner flow...");
        
        // Step A: LLM decides to Check Balance (Simulated)
        tracker.LogEvent("PromptRendered", "LLM decides to check balance first");
        
        // Step B: Invoke CheckBalance
        var balanceFunc = plugin["CheckBalance"];
        var balanceResult = await kernel.InvokeAsync(balanceFunc, new() { ["accountNumber"] = "123" });
        
        // Step C: LLM decides to Transfer
        tracker.LogEvent("PromptRendered", "LLM decides to transfer based on balance");
        
        // Step D: Invoke Transfer
        var transferFunc = plugin["Transfer"];
        var transferResult = await kernel.InvokeAsync(transferFunc, new() 
            { ["amount"] = 200m, ["fromAccount"] = "123", ["toAccount"] = "456" });

        // 3. Generate Graph
        var dotGraph = tracker.GenerateExecutionGraph();
        
        Console.WriteLine("\n--- Generated Graphviz DOT ---");
        Console.WriteLine(dotGraph);
        
        Console.WriteLine("\n--- Raw Event Logs ---");
        foreach(var log in tracker.Events) Console.WriteLine(log);
    }
}
