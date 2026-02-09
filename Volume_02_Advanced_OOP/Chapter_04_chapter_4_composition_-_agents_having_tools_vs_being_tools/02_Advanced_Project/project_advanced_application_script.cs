
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

// --- TOOL INTERFACES AND IMPLEMENTATIONS ---

// The Adapter Pattern: We define a common contract for all tools.
// Even if the underlying systems (APIs, databases) are different,
// our Agent will only ever talk to this interface.
public interface ILogisticsTool
{
    string Name { get; }
    void Execute(string context);
}

// Concrete Tool 1: Checks the inventory status.
// This tool is "dumb". It just performs its specific task.
public class InventoryChecker : ILogisticsTool
{
    public string Name => "Inventory Checker";

    public void Execute(string context)
    {
        // Simulate database lookup
        Console.WriteLine($"[Tool: {Name}] Checking inventory for Order #{context}...");
        
        // Simulate a random result for demonstration
        Random rnd = new Random();
        int stockLevel = rnd.Next(0, 10);

        if (stockLevel > 0)
        {
            Console.WriteLine($"[Tool: {Name}] SUCCESS: Item is in stock (Qty: {stockLevel}).");
            // We use a static flag to communicate state back to the agent
            // because we cannot return values from void methods (a constraint of this simplified model).
            LogisticsAgent.LastCheckResult = "IN_STOCK";
        }
        else
        {
            Console.WriteLine($"[Tool: {Name}] FAILURE: Item is out of stock.");
            LogisticsAgent.LastCheckResult = "OUT_OF_STOCK";
        }
    }
}

// Concrete Tool 2: Schedules a shipment.
public class ShippingScheduler : ILogisticsTool
{
    public string Name => "Shipping Scheduler";

    public void Execute(string context)
    {
        Console.WriteLine($"[Tool: {Name}] Preparing shipping label for Order #{context}...");
        Console.WriteLine($"[Tool: {Name}] Booking carrier pickup. Process Complete.");
    }
}

// Concrete Tool 3: Connects to a supplier to reorder.
public class SupplierConnector : ILogisticsTool
{
    public string Name => "Supplier Connector";

    public void Execute(string context)
    {
        Console.WriteLine($"[Tool: {Name}] Connecting to external supplier API...");
        Console.WriteLine($"[Tool: {Name}] Sending reorder request for Order #{context}...");
        Console.WriteLine($"[Tool: {Name}] Supplier confirmed shipment in 5 days.");
    }
}

// --- AGENT ARCHITECTURE ---

// The ToolAgent class.
// This represents the "Active" part of the composition.
// It holds a list of tools and decides when to use them.
public class LogisticsAgent
{
    // State Management: A static variable to simulate passing data between tools.
    // In a real system, this would be a state object or tensor.
    public static string LastCheckResult;

    // Composition: The agent HAS tools. This is a list of dependencies.
    // We are using an array here because lists are forbidden in this scope.
    // We must manually track the count.
    private ILogisticsTool[] _tools;
    private int _toolCount;

    public LogisticsAgent(int maxTools)
    {
        _tools = new ILogisticsTool[maxTools];
        _toolCount = 0;
    }

    // Method to dynamically compose the agent's capabilities
    public void AddTool(ILogisticsTool tool)
    {
        if (_toolCount < _tools.Length)
        {
            _tools[_toolCount] = tool;
            _toolCount++;
            Console.WriteLine($"[Agent] Acquired new capability: {tool.Name}");
        }
    }

    // The Core Logic: The Agent's "Brain"
    public void ProcessOrder(string orderId)
    {
        Console.WriteLine($"\n--- Starting Processing for Order: {orderId} ---");
        
        // 1. The Agent decides to use the Inventory Checker
        // It searches its internal toolset for the right one.
        ILogisticsTool checker = FindTool("Inventory Checker");
        if (checker != null)
        {
            checker.Execute(orderId);
        }

        // 2. The Agent analyzes the result (using the shared state)
        // This is where the "Decision Making" happens.
        if (LastCheckResult == "IN_STOCK")
        {
            Console.WriteLine("[Agent] Decision: Item is available. Initiating Shipping Protocol.");
            
            ILogisticsTool shipper = FindTool("Shipping Scheduler");
            if (shipper != null)
            {
                shipper.Execute(orderId);
            }
        }
        else if (LastCheckResult == "OUT_OF_STOCK")
        {
            Console.WriteLine("[Agent] Decision: Item is unavailable. Initiating Reorder Protocol.");
            
            ILogisticsTool supplier = FindTool("Supplier Connector");
            if (supplier != null)
            {
                supplier.Execute(orderId);
            }
        }
        else
        {
            Console.WriteLine("[Agent] Error: Could not determine inventory status.");
        }

        Console.WriteLine($"--- Finished Processing for Order: {orderId} ---\n");
    }

    // Helper method to simulate searching a collection
    private ILogisticsTool FindTool(string name)
    {
        // Manual loop simulation (since 'foreach' is forbidden)
        int index = 0;
        while (index < _toolCount)
        {
            if (_tools[index].Name == name)
            {
                return _tools[index];
            }
            index++;
        }
        return null;
    }
}

// --- MAIN EXECUTION ---

public class Program
{
    public static void Main(string[] args)
    {
        // 1. Instantiate the Agent
        LogisticsAgent agent = new LogisticsAgent(3);

        // 2. Compose the Agent with Tools (Dependency Injection)
        // Notice the agent is not born knowing how to ship; it is given the ability.
        agent.AddTool(new InventoryChecker());
        agent.AddTool(new ShippingScheduler());
        agent.AddTool(new SupplierConnector());

        // 3. Run Simulation
        // Scenario A: Order 101 (Likely In Stock)
        agent.ProcessOrder("101");

        // Scenario B: Order 102 (Likely Out of Stock)
        agent.ProcessOrder("102");
    }
}
