
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

using System.Collections.Concurrent;
using System.Threading.Channels;

// --- Domain Model: The AI Agent's Payload ---
public record SentimentRequest(Guid Id, string Text);
public record SentimentResult(Guid Id, string Sentiment, double Confidence);

// --- The AI Inference Engine ---
// Simulates a heavy computation (e.g., ONNX Runtime inference)
public class InferenceEngine
{
    private static readonly Random _rng = new();

    public async Task<SentimentResult> PredictAsync(SentimentRequest request)
    {
        // Simulate GPU inference latency (100ms - 500ms)
        await Task.Delay(_rng.Next(100, 500));

        // Simulate simple logic based on text length to vary results
        var sentiment = request.Text.Length > 50 ? "Positive" : "Neutral";
        var confidence = 0.5 + (_rng.NextDouble() * 0.5); // 0.5 to 1.0

        return new SentimentResult(request.Id, sentiment, confidence);
    }
}

// --- The AI Agent (Containerized Service) ---
// Represents a single pod running the AI workload
public class AiAgent
{
    private readonly InferenceEngine _engine = new();
    private readonly Channel<SentimentRequest> _queue;
    private readonly string _agentId;
    private int _processedCount = 0;

    public AiAgent(string agentId, int capacity = 10)
    {
        _agentId = agentId;
        // Bounded channel prevents memory overflow if the agent is overwhelmed
        _queue = Channel.CreateBounded<SentimentRequest>(new BoundedChannelOptions(capacity)
        {
            FullMode = BoundedChannelFullMode.Wait
        });
    }

    public string AgentId => _agentId;
    public int QueueDepth => _queue.Reader.Count;
    public int ProcessedCount => _processedCount;

    // Simulates the Kubernetes container entrypoint
    public async Task StartProcessingAsync(CancellationToken cancellationToken)
    {
        await foreach (var request in _queue.Reader.ReadAllAsync(cancellationToken))
        {
            var result = await _engine.PredictAsync(request);
            Interlocked.Increment(ref _processedCount);
            // In a real app, we would send 'result' to an output sink
        }
    }

    public bool TryAcceptRequest(SentimentRequest request)
    {
        return _queue.Writer.TryWrite(request);
    }

    public async Task StopAsync()
    {
        _queue.Writer.Complete();
    }
}

// --- The Orchestrator (Simulates Kubernetes HPA Controller) ---
// Monitors metrics and scales agents up/down
public class HpaOrchestrator
{
    private readonly ConcurrentDictionary<string, AiAgent> _agents = new();
    private readonly int _maxAgents;
    private readonly int _targetQueueDepthPerAgent;

    public HpaOrchestrator(int maxAgents = 10, int targetQueueDepthPerAgent = 5)
    {
        _maxAgents = maxAgents;
        _targetQueueDepthPerAgent = targetQueueDepthPerAgent;
    }

    public int CurrentAgentCount => _agents.Count;

    // Simulates the Kubernetes Metrics Server
    private int GetTotalQueueDepth()
    {
        return _agents.Values.Sum(a => a.QueueDepth);
    }

    // The Core Logic: Calculate desired replicas based on custom metric
    private int CalculateDesiredReplicas()
    {
        int totalDepth = GetTotalQueueDepth();
        
        // Formula: Desired Replicas = ceil(Total Queue Depth / Target Depth per Agent)
        // This is the standard HPA algorithm for custom metrics.
        int desired = (int)Math.Ceiling((double)totalDepth / _targetQueueDepthPerAgent);

        // Clamp to min/max replicas (Kubernetes behavior)
        if (desired < 1) desired = 1;
        if (desired > _maxAgents) desired = _maxAgents;

        return desired;
    }

    public async Task ManageScalingAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            // Check metrics every 2 seconds (like HPA sync period)
            await Task.Delay(2000, cancellationToken);

            int desired = CalculateDesiredReplicas();
            int current = CurrentAgentCount;

            if (desired > current)
            {
                // Scale Out
                int scaleOutCount = desired - current;
                for (int i = 0; i < scaleOutCount; i++)
                {
                    var newAgent = new AiAgent($"agent-{Guid.NewGuid().ToString()[..8]}");
                    _agents.TryAdd(newAgent.AgentId, newAgent);
                    
                    // Start the container (background task)
                    _ = newAgent.StartProcessingAsync(cancellationToken);
                    
                    Console.WriteLine($"[HPA] Scaling OUT: Started {newAgent.AgentId}. Total: {_agents.Count}");
                }
            }
            else if (desired < current)
            {
                // Scale In (Graceful Shutdown)
                // In Kubernetes, we would mark pod for termination (SIGTERM) and wait for active connections to finish.
                // Here, we pick the agent with the shortest queue to drain.
                int scaleInCount = current - desired;
                
                var agentsToScaleIn = _agents.Values
                    .OrderBy(a => a.QueueDepth)
                    .ThenBy(a => a.ProcessedCount)
                    .Take(scaleInCount)
                    .ToList();

                foreach (var agent in agentsToScaleIn)
                {
                    if (_agents.TryRemove(agent.AgentId, out var removedAgent))
                    {
                        await removedAgent.StopAsync(); // Stop accepting new requests
                        Console.WriteLine($"[HPA] Scaling IN: Stopped {removedAgent.AgentId}. Remaining: {_agents.Count}");
                    }
                }
            }
        }
    }

    public void RouteRequest(SentimentRequest request)
    {
        // Simple Round Robin or Least-Connection strategy
        // We pick the agent with the shortest queue to balance load
        var targetAgent = _agents.Values
            .OrderBy(a => a.QueueDepth)
            .FirstOrDefault();

        if (targetAgent != null)
        {
            if (!targetAgent.TryAcceptRequest(request))
            {
                Console.WriteLine($"[Warning] Agent {targetAgent.AgentId} queue full. Request {request.Id} rejected.");
            }
        }
    }
}

// --- Main Program: Simulation Driver ---
public class Program
{
    public static async Task Main()
    {
        Console.WriteLine("--- Starting AI Agent Autoscaling Simulation ---");
        
        // 1. Initialize Orchestrator (HPA Controller)
        // Max 5 pods, Target 3 requests per pod
        var hpa = new HpaOrchestrator(maxAgents: 5, targetQueueDepthPerAgent: 3);
        
        using var cts = new CancellationTokenSource();

        // 2. Start the HPA Control Loop in background
        var scalingTask = hpa.ManageScalingAsync(cts.Token);

        // 3. Simulate Incoming Traffic (Flash Sale)
        var trafficGenerator = Task.Run(async () =>
        {
            for (int i = 1; i <= 20; i++)
            {
                // Burst of 5 requests every second
                for (int j = 0; j < 5; j++)
                {
                    var req = new SentimentRequest(Guid.NewGuid(), $"Review text number {i}-{j}. This is a pretty long review to simulate processing time.");
                    hpa.RouteRequest(req);
                    Console.WriteLine($"[Traffic] Generated Request {req.Id}");
                }
                await Task.Delay(1000);
            }
        });

        // 4. Monitor and Report Status
        var monitorTask = Task.Run(async () =>
        {
            while (!cts.Token.IsCancellationRequested)
            {
                await Task.Delay(3000, cts.Token);
                Console.WriteLine($"[Status] Agents: {hpa.CurrentAgentCount} | Total Processed: {hpa.CurrentAgentCount}"); 
                // Note: In a real app, we'd aggregate ProcessedCount from agents
            }
        });

        await trafficGenerator;
        
        // Let the system drain for a bit
        await Task.Delay(5000);
        cts.Cancel();

        await scalingTask;
        Console.WriteLine("--- Simulation Complete ---");
    }
}
