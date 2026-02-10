
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
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace AgentSwarmInference
{
    // ============================================================================
    // 1. CORE DATA MODELS
    // ============================================================================

    /// <summary>
    /// Represents an inference task dispatched to the agent swarm.
    /// In a real-world scenario (e.g., LLM inference), this might contain
    /// tokenized prompts, image tensors, or structured JSON payloads.
    /// </summary>
    public record InferenceTask(Guid Id, string Payload, DateTime CreatedAt);

    /// <summary>
    /// Represents the result of an inference operation.
    /// </summary>
    public record InferenceResult(Guid TaskId, string Output, TimeSpan ProcessingTime);

    // ============================================================================
    // 2. DISTRIBUTED TASK QUEUE (SIMULATED)
    // ============================================================================

    /// <summary>
    /// Simulates a distributed message broker (like RabbitMQ, Kafka, or Azure Service Bus).
    /// In a Kubernetes environment, this would be an external dependency, but for this
    /// self-contained example, we use a thread-safe Channel<T>.
    /// </summary>
    public class DistributedTaskQueue
    {
        // Channel<T> is a modern, high-performance concurrency primitive for passing data
        // between producers and consumers. It handles buffering and backpressure automatically.
        private readonly Channel<InferenceTask> _channel;

        public DistributedTaskQueue()
        {
            // Bounded capacity prevents memory exhaustion during traffic spikes.
            // In a real K8s deployment, this bound would be dictated by the queue's retention policy.
            var options = new BoundedChannelOptions(capacity: 1000)
            {
                FullMode = BoundedChannelFullMode.Wait // Blocks producer if queue is full
            };
            _channel = Channel.CreateBounded<InferenceTask>(options);
        }

        /// <summary>
        /// Dispatches a task to the swarm.
        /// </summary>
        public async Task EnqueueAsync(InferenceTask task, CancellationToken ct = default)
        {
            await _channel.Writer.WriteAsync(task, ct);
        }

        /// <summary>
        /// Reads a task from the swarm (simulating a worker pulling from a queue).
        /// </summary>
        public async Task<InferenceTask> DequeueAsync(CancellationToken ct = default)
        {
            return await _channel.Reader.ReadAsync(ct);
        }
    }

    // ============================================================================
    // 3. AUTONOMOUS AGENT IMPLEMENTATION
    // ============================================================================

    /// <summary>
    /// Represents a single "Pod" or container in the Kubernetes cluster.
    /// It encapsulates the logic to process tasks independently.
    /// </summary>
    public class AutonomousAgent
    {
        private readonly string _agentId;
        private readonly Random _random = new();

        public AutonomousAgent(string agentId)
        {
            _agentId = agentId;
        }

        /// <summary>
        /// Performs the actual inference work. 
        /// In a real scenario, this would call an ONNX runtime or an LLM API.
        /// </summary>
        public async Task<InferenceResult> ProcessTaskAsync(InferenceTask task)
        {
            var startTime = DateTime.UtcNow;

            // Simulate compute-intensive work (e.g., matrix multiplication).
            // We use Task.Delay to simulate network latency or GPU processing time.
            var processingDelay = _random.Next(50, 200); 
            await Task.Delay(processingDelay);

            var duration = DateTime.UtcNow - startTime;

            // Simulate a transformation of the input payload.
            var output = $"[Agent {_agentId}] Processed: {task.Payload.ToUpperInvariant()} (Latency: {processingDelay}ms)";

            return new InferenceResult(task.Id, output, duration);
        }
    }

    // ============================================================================
    // 4. KEDA-STYLE SCALING LOGIC (SIMULATED)
    // ============================================================================

    /// <summary>
    /// Simulates the logic of KEDA (Kubernetes Event-driven Autoscaling).
    /// KEDA calculates the 'Desired Replica Count' based on queue metrics.
    /// Formula: DesiredReplicas = ceil( QueueLength / TargetQueueLength )
    /// </summary>
    public class ScalingController
    {
        private readonly int _targetQueueLengthPerAgent;

        public ScalingController(int targetQueueLengthPerAgent)
        {
            _targetQueueLengthPerAgent = targetQueueLengthPerAgent;
        }

        /// <summary>
        /// Calculates how many agents (pods) should be running.
        /// </summary>
        /// <param name="currentQueueLength">Current messages in the queue.</param>
        /// <param name="currentReplicas">Currently running agents.</param>
        /// <returns>The new target replica count.</returns>
        public int CalculateDesiredReplicas(int currentQueueLength, int currentReplicas)
        {
            // Avoid division by zero
            if (_targetQueueLengthPerAgent <= 0) return currentReplicas;

            // KEDA Formula Implementation
            int desired = (int)Math.Ceiling((double)currentQueueLength / _targetQueueLengthPerAgent);

            // Safety Clamps (Standard in production autoscalers)
            // 1. Minimum replicas (keep the service alive).
            if (desired < 1) desired = 1;
            
            // 2. Maximum replicas (prevent cost explosion).
            if (desired > 50) desired = 50; 

            return desired;
        }
    }

    // ============================================================================
    // 5. ORCHESTRATOR (THE SWARM MANAGER)
    // ============================================================================

    /// <summary>
    /// Manages the lifecycle of the agent pool.
    /// In Kubernetes, this logic is split:
    /// - KEDA ScaledObject: Calculates the desired count.
    /// - Kubernetes HPA: Adjusts the Deployment replica count.
    /// - K8s Scheduler: Places Pods on Nodes.
    /// </summary>
    public class SwarmOrchestrator
    {
        private readonly DistributedTaskQueue _queue;
        private readonly ScalingController _controller;
        private readonly List<AutonomousAgent> _activeAgents;
        private readonly ConcurrentDictionary<Guid, Task<InferenceResult>> _processingTasks;
        
        // Cancellation token to stop the swarm
        private CancellationTokenSource _cts;

        public SwarmOrchestrator(DistributedTaskQueue queue, ScalingController controller)
        {
            _queue = queue;
            _controller = controller;
            _activeAgents = new List<AutonomousAgent>();
            _processingTasks = new ConcurrentDictionary<Guid, Task<InferenceResult>>();
            _cts = new CancellationTokenSource();
        }

        /// <summary>
        /// Starts the control loop.
        /// In K8s, this is analogous to the KEDA operator reconciling state.
        /// </summary>
        public async Task StartControlLoopAsync()
        {
            Console.WriteLine("🚀 Starting Swarm Control Loop...");
            
            // Start the scaling monitor (simulates KEDA metrics server)
            var scalingTask = Task.Run(async () =>
            {
                while (!_cts.Token.IsCancellationRequested)
                {
                    await Task.Delay(2000, _cts.Token); // Check metrics every 2 seconds
                    
                    // Get current metrics
                    int queueLength = 0; // In reality, this is fetched from the queue API
                    // For this simulation, we estimate based on internal tracking or a shared counter
                    // We'll rely on the processing tasks count to simulate load for this demo
                    
                    int currentReplicas = _activeAgents.Count;
                    int desiredReplicas = _controller.CalculateDesiredReplicas(_processingTasks.Count, currentReplicas);

                    if (desiredReplicas != currentReplicas)
                    {
                        Console.WriteLine($"[KEDA Simulator] Queue Load: {_processingTasks.Count} tasks. " +
                                          $"Current Replicas: {currentReplicas}. " +
                                          $"Scaling to: {desiredReplicas}");
                        
                        AdjustAgentPool(desiredReplicas);
                    }
                }
            }, _cts.Token);

            // Start the task dispatcher (simulates K8s Service Mesh routing)
            var dispatchTask = Task.Run(async () =>
            {
                while (!_cts.Token.IsCancellationRequested)
                {
                    try
                    {
                        // Simulate incoming traffic
                        await Task.Delay(500, _cts.Token);
                        
                        // Only dispatch if we have agents
                        if (_activeAgents.Count > 0)
                        {
                            var task = new InferenceTask(Guid.NewGuid(), $"Request_{DateTime.Now.Ticks}", DateTime.UtcNow);
                            await _queue.EnqueueAsync(task, _cts.Token);
                            Console.WriteLine($"[Dispatcher] Enqueued task {task.Id}");
                        }
                    }
                    catch (OperationCanceledException) { break; }
                }
            }, _cts.Token);

            // Start the worker loop (simulates Pod execution)
            // In a real K8s setup, every Pod runs this loop independently.
            // Here, we simulate multiple agents running on the shared orchestrator context.
            var workerTask = Task.Run(async () =>
            {
                while (!_cts.Token.IsCancellationRequested)
                {
                    // If no agents are available, wait
                    if (_activeAgents.Count == 0)
                    {
                        await Task.Delay(100, _cts.Token);
                        continue;
                    }

                    try
                    {
                        // Pull task from queue
                        var task = await _queue.DequeueAsync(_cts.Token);
                        
                        // Round-robin selection of an agent (simulates Load Balancer)
                        var agent = _activeAgents.OrderBy(a => Guid.NewGuid()).First();
                        
                        // Start processing asynchronously (parallel execution)
                        var processingTask = agent.ProcessTaskAsync(task);
                        
                        // Track the task
                        _processingTasks.TryAdd(task.Id, processingTask);

                        // Fire and forget cleanup
                        _ = processingTask.ContinueWith(t =>
                        {
                            _processingTasks.TryRemove(task.Id, out _);
                            if (t.IsCompletedSuccessfully)
                            {
                                Console.WriteLine($"[Result] {t.Result.Output}");
                            }
                        });
                    }
                    catch (OperationCanceledException) { break; }
                }
            }, _cts.Token);

            await Task.WhenAll(scalingTask, dispatchTask, workerTask);
        }

        /// <summary>
        /// Adjusts the number of active agents.
        /// In K8s, this triggers a Deployment scale event.
        /// </summary>
        private void AdjustAgentPool(int targetCount)
        {
            int currentCount = _activeAgents.Count;

            if (targetCount > currentCount)
            {
                // Scale Up
                for (int i = currentCount; i < targetCount; i++)
                {
                    var newAgent = new AutonomousAgent($"agent-pod-{i + 1}");
                    _activeAgents.Add(newAgent);
                    Console.WriteLine($"  + Added Agent: agent-pod-{i + 1}");
                }
            }
            else if (targetCount < currentCount)
            {
                // Scale Down (Graceful Shutdown)
                // In K8s, this corresponds to receiving a SIGTERM signal.
                for (int i = currentCount - 1; i >= targetCount; i--)
                {
                    var agent = _activeAgents[i];
                    Console.WriteLine($"  - Removing Agent: {agent.GetHashCode()} (Graceful Shutdown)");
                    _activeAgents.RemoveAt(i);
                }
            }
        }

        public void Stop()
        {
            Console.WriteLine("🛑 Stopping Swarm...");
            _cts.Cancel();
        }
    }

    // ============================================================================
    // 6. MAIN PROGRAM EXECUTION
    // ============================================================================

    class Program
    {
        static async Task Main(string[] args)
        {
            // 1. Initialize Infrastructure
            var queue = new DistributedTaskQueue();
            
            // KEDA Configuration: Target 5 concurrent tasks per Agent Pod
            var kedaConfig = new ScalingController(targetQueueLengthPerAgent: 5);
            
            var orchestrator = new SwarmOrchestrator(queue, kedaConfig);

            // 2. Run the Swarm
            // We run for a limited time to demonstrate the scaling behavior
            var runTask = orchestrator.StartControlLoopAsync();
            
            // Let it run for 10 seconds to observe scaling
            await Task.Delay(10000);
            
            orchestrator.Stop();
            
            // Allow time for graceful shutdown
            await Task.Delay(2000);
            
            Console.WriteLine("Simulation Complete.");
        }
    }
}
