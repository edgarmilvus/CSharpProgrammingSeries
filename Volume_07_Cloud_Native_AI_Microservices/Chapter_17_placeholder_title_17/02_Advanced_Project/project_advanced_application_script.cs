
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
using System.Threading;
using System.Threading.Tasks;

namespace CloudNativeAgents
{
    // REASONING: In a microservices architecture, specifically when containerizing autonomous agents,
    // we must decouple the "decision" logic from the "execution" logic. This allows us to scale
    // specific heavy-inference components (like the LLM inference engine) independently of the
    // lightweight orchestration logic.
    // 
    // We simulate a "Stateful Agent" using a class that maintains internal state (Memory/Context)
    // and communicates via a message bus pattern (simulated via a static Event Aggregator).
    // This mimics the behavior of a Kubernetes Pod handling a specific workload lifecycle.

    // DEFINITION: Event Aggregator (Simulating a Service Mesh / Message Queue)
    // Handles communication between the Orchestration Layer and the Inference Layer.
    public static class MessageBus
    {
        // Thread-safe queue to simulate a distributed message queue (e.g., RabbitMQ, Kafka)
        private static readonly Queue<string> _messageQueue = new Queue<string>();
        private static readonly object _lock = new object();

        public static void Publish(string message)
        {
            lock (_lock)
            {
                _messageQueue.Enqueue(message);
                Console.WriteLine($"[Bus] Message queued: {message}");
            }
        }

        public static bool TryConsume(out string message)
        {
            lock (_lock)
            {
                if (_messageQueue.Count > 0)
                {
                    message = _messageQueue.Dequeue();
                    return true;
                }
                message = null;
                return false;
            }
        }
    }

    // DEFINITION: Inference Engine (Simulating the GPU-bound Microservice)
    // This represents the heavy-lifting component that can be scaled out horizontally.
    // In a real scenario, this would interface with a TensorFlow/PyTorch runtime or an LLM API.
    public class InferenceEngine
    {
        private readonly string _engineId;
        private readonly int _gpuMemoryLimit; // Simulating resource constraints

        public InferenceEngine(string engineId, int gpuMemoryLimit)
        {
            _engineId = engineId;
            _gpuMemoryLimit = gpuMemoryLimit;
        }

        // Simulates a heavy computation (LLM Inference)
        // Returns a complex decision object based on input context.
        public string ProcessInference(string context)
        {
            Console.WriteLine($"[GPU:{_engineId}] Starting inference job. Context length: {context.Length}. Allocating {_gpuMemoryLimit}MB VRAM.");
            
            // Simulate processing delay (GPU compute time)
            Thread.Sleep(2000); 

            // Simple logic to demonstrate state transition based on input
            string decision;
            if (context.Contains("Error") || context.Contains("Alert"))
            {
                decision = "TRIGGER_CRITICAL_ALERT";
            }
            else if (context.Contains("Optimize"))
            {
                decision = "ADJUST_RESOURCES";
            }
            else
            {
                decision = "MAINTAIN_STATE";
            }

            Console.WriteLine($"[GPU:{_engineId}] Inference Complete. Decision: {decision}");
            return decision;
        }
    }

    // DEFINITION: Autonomous Agent (Stateful Workload)
    // Represents a containerized unit of execution. It maintains local memory (state)
    // and interacts with the environment via the MessageBus.
    public class AutonomousAgent
    {
        public string AgentId { get; }
        private string _internalMemory; // State persistence
        private readonly InferenceEngine _inferenceEngine; // Dependency injection of compute resource

        public AutonomousAgent(string agentId, InferenceEngine engine)
        {
            AgentId = agentId;
            _internalMemory = "Initial State";
            _inferenceEngine = engine;
        }

        // The agent's "Loop" - equivalent to the container entrypoint script
        public void RunLifecycle()
        {
            Console.WriteLine($"\n[Agent:{AgentId}] Waking up. Memory: {_internalMemory}");

            // 1. Observe: Check the message bus for external stimuli
            if (MessageBus.TryConsume(out string stimulus))
            {
                Console.WriteLine($"[Agent:{AgentId}] Received stimulus: {stimulus}");
                
                // Update internal state (Context enrichment)
                _internalMemory = $"Processing: {stimulus}";

                // 2. Act: Offload heavy computation to the Inference Engine
                // In K8s, this might be a sidecar container or a separate service call
                string decision = _inferenceEngine.ProcessInference(_internalMemory);

                // 3. React: Update state based on inference
                UpdateState(decision);
            }
            else
            {
                Console.WriteLine($"[Agent:{AgentId}] No stimuli. Idling...");
            }
        }

        private void UpdateState(string decision)
        {
            // State transition logic
            switch (decision)
            {
                case "TRIGGER_CRITICAL_ALERT":
                    _internalMemory = "ALERT_MODE";
                    // In a real system, this would trigger a scaling event or PagerDuty
                    break;
                case "ADJUST_RESOURCES":
                    _internalMemory = "OPTIMIZED";
                    break;
                default:
                    _internalMemory = "STABLE";
                    break;
            }
            Console.WriteLine($"[Agent:{AgentId}] New State: {_internalMemory}");
        }
    }

    // DEFINITION: Orchestrator (Kubernetes Operator Simulation)
    // Manages the lifecycle of Agents and ensures resource availability.
    public class AgentOrchestrator
    {
        private List<AutonomousAgent> _agents = new List<AutonomousAgent>();
        private InferenceEngine _sharedInferencePool; // Simulating a pool of GPU nodes

        public AgentOrchestrator()
        {
            // Initialize a shared compute resource (simulating a Node with GPUs)
            _sharedInferencePool = new InferenceEngine("Node-01-GPU", 16000);
        }

        public void DeployAgent(string agentId)
        {
            Console.WriteLine($"[Orchestrator] Deploying Agent {agentId}...");
            var newAgent = new AutonomousAgent(agentId, _sharedInferencePool);
            _agents.Add(newAgent);
        }

        public void TriggerEvent(string eventMessage)
        {
            MessageBus.Publish(eventMessage);
        }

        public void RunTick()
        {
            Console.WriteLine("\n=== Orchestrator Tick ===");
            // In a real K8s operator, this is the Reconcile Loop
            foreach (var agent in _agents)
            {
                agent.RunLifecycle();
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Initializing Cloud-Native Agent System...");
            
            // 1. Initialize the Operator
            var orchestrator = new AgentOrchestrator();

            // 2. Deploy Agents (Simulating Pod Creation)
            orchestrator.DeployAgent("Agent-Alpha");
            orchestrator.DeployAgent("Agent-Beta");

            // 3. Simulate Time / Event Loop
            // In K8s, this is driven by the Control Loop or external triggers.
            
            // Tick 1: Normal operation
            orchestrator.RunTick();
            Thread.Sleep(1000);

            // Tick 2: Inject a stimulus requiring inference
            Console.WriteLine("\n[External System] Injecting workload event...");
            orchestrator.TriggerEvent("Optimize database queries");
            orchestrator.RunTick();
            Thread.Sleep(1000);

            // Tick 3: Inject a critical error
            Console.WriteLine("\n[External System] Injecting critical error...");
            orchestrator.TriggerEvent("System Alert: Latency Spike");
            orchestrator.RunTick();

            Console.WriteLine("\nSimulation Complete.");
        }
    }
}
