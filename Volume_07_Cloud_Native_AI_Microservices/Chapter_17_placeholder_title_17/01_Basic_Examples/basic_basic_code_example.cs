
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

// Source File: basic_basic_code_example.cs
// Description: Basic Code Example
// ==========================================

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

// Represents the data payload for a task. In a real system, this could be complex analysis results.
// Using a record for immutability and value-equality semantics.
public record AgentTask(string TaskType, string Payload);

// Represents a request sent from an Orchestrator to a Worker.
public record TaskRequest(Guid RequestId, string TaskType, string Payload);

// Represents the response from a Worker back to the Orchestrator.
public record TaskResponse(Guid RequestId, bool Success, string Result);

// Abstract base class for all agents, providing a common interface for registration and execution.
public abstract class AgentBase
{
    public string AgentName { get; }
    protected AgentBase(string agentName) => AgentName = agentName;

    // The core logic an agent performs. Returns a result string.
    public abstract Task<string> ExecuteAsync(string payload);

    // Registers this agent's capabilities with the central service registry.
    public void Register(IServiceRegistry registry)
    {
        Console.WriteLine($"[System] Agent '{AgentName}' is registering for task type '{GetSupportedTaskType()}'.");
        registry.Register(GetSupportedTaskType(), this);
    }

    // Each agent must declare what task type it can handle.
    public abstract string GetSupportedTaskType();
}

// A specialized agent that simulates analyzing soil moisture data.
public class SoilAnalyzerAgent : AgentBase
{
    public SoilAnalyzerAgent() : base("Soil-Analyzer-01") { }

    public override string GetSupportedTaskType() => "AnalyzeSoil";

    public override async Task<string> ExecuteAsync(string payload)
    {
        // Simulate a time-consuming I/O or computation operation.
        await Task.Delay(500); 
        // In a real-world scenario, this would involve complex ML models or database lookups.
        // For this example, we just process the payload.
        var moistureLevel = new Random().Next(20, 80);
        return $"Analysis complete for '{payload}'. Moisture Level: {moistureLevel}%. Status: {(moistureLevel > 50 ? "Optimal" : "Needs Irrigation")}";
    }
}

// A specialized agent that simulates detecting pests from image data.
public class PestDetectorAgent : AgentBase
{
    public PestDetectorAgent() : base("Pest-Detector-01") { }

    public override string GetSupportedTaskType() => "DetectPests";

    public override async Task<string> ExecuteAsync(string payload)
    {
        await Task.Delay(800); // Simulate heavy image processing.
        var pestsFound = new Random().Next(0, 5);
        return $"Scan complete for '{payload}'. Pests Detected: {pestsFound}. Action: {(pestsFound > 0 ? "Dispatch Bio-Drones" : "All Clear")}";
    }
}

// The central nervous system of our microservices architecture.
// In a real Kubernetes environment, this would be replaced by a service mesh (like Istio) 
// or a service discovery tool (like Consul).
public interface IServiceRegistry
{
    void Register(string taskType, AgentBase agent);
    AgentBase? Resolve(string taskType);
}

public class InMemoryServiceRegistry : IServiceRegistry
{
    // Thread-safe dictionary to store agent registrations.
    private readonly ConcurrentDictionary<string, AgentBase> _registry = new();

    public void Register(string taskType, AgentBase agent)
    {
        // In a real system, this would handle multiple agents for the same task (load balancing).
        // Here, we just overwrite for simplicity.
        _registry.AddOrUpdate(taskType, agent, (key, existing) => agent);
    }

    public AgentBase? Resolve(string taskType)
    {
        _registry.TryGetValue(taskType, out var agent);
        return agent;
    }
}

// The Orchestrator is the entry point for complex workflows.
// It doesn't know *how* to do the work, only *who* to ask.
public class OrchestratorAgent
{
    private readonly IServiceRegistry _serviceRegistry;

    public OrchestratorAgent(IServiceRegistry serviceRegistry)
    {
        _serviceRegistry = serviceRegistry;
    }

    public async Task<string> CoordinateAnalysisAsync(string fieldId)
    {
        Console.WriteLine($"\n--- Starting Analysis for '{fieldId}' ---");
        
        // 1. Delegate Soil Analysis
        var soilTask = new TaskRequest(Guid.NewGuid(), "AnalyzeSoil", fieldId);
        Console.WriteLine($"[Orchestrator] Delegating soil analysis (ID: {soilTask.RequestId})...");
        string soilResult = await DelegateTaskAsync(soilTask);

        // 2. Delegate Pest Detection
        var pestTask = new TaskRequest(Guid.NewGuid(), "DetectPests", fieldId);
        Console.WriteLine($"[Orchestrator] Delegating pest detection (ID: {pestTask.RequestId})...");
        string pestResult = await DelegateTaskAsync(pestTask);

        // 3. Consolidate Report
        Console.WriteLine("\n--- Consolidating Final Report ---");
        return $"FINAL REPORT FOR {fieldId}:\n- Soil Status: {soilResult}\n- Pest Status: {pestResult}";
    }

    private async Task<string> DelegateTaskAsync(TaskRequest request)
    {
        // This is the core service lookup logic.
        var worker = _serviceRegistry.Resolve(request.TaskType);

        if (worker == null)
        {
            return $"ERROR: No agent found for task type '{request.TaskType}'.";
        }

        // Execute the remote (simulated) task.
        var result = await worker.ExecuteAsync(request.Payload);
        
        // Return the formatted response.
        return $"[Response from {worker.AgentName}]: {result}";
    }
}

// Main program entry point.
public class Program
{
    public static async Task Main(string[] args)
    {
        // 1. Setup the environment
        var registry = new InMemoryServiceRegistry();
        var orchestrator = new OrchestratorAgent(registry);

        // 2. Instantiate and register our specialized agents (our microservices)
        var soilAgent = new SoilAnalyzerAgent();
        soilAgent.Register(registry);

        var pestAgent = new PestDetectorAgent();
        pestAgent.Register(registry);

        // 3. Kick off the workflow
        string finalReport = await orchestrator.CoordinateAnalysisAsync("Field-7A");

        // 4. Output the final result
        Console.WriteLine("\n" + finalReport);
    }
}
