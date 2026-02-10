
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

# Source File: basic_basic_code_example.cs
# Description: Basic Code Example
# ==========================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CloudNativeAI.Microservices.Inference
{
    /// <summary>
    /// Represents a simple AI inference agent that processes text inputs.
    /// In a real-world scenario, this would load a machine learning model (e.g., ONNX, TensorFlow).
    /// For this "Hello World" example, we simulate inference logic.
    /// </summary>
    public class InferenceAgent
    {
        private readonly string _agentId;
        private readonly Random _random = new Random();

        public InferenceAgent(string agentId)
        {
            _agentId = agentId;
        }

        /// <summary>
        /// Simulates processing an input text (e.g., sentiment analysis).
        /// </summary>
        /// <param name="input">The text to analyze.</param>
        /// <returns>A task representing the asynchronous operation, with the result being the inference score.</returns>
        public async Task<double> ProcessAsync(string input)
        {
            // Simulate model loading latency (common in cold starts)
            await Task.Delay(100); 

            // Simulate inference computation
            // In reality, this would be: tensor.Run(input);
            double score = _random.NextDouble(); 

            // Simulate post-processing
            await Task.Delay(50);

            return score;
        }
    }

    /// <summary>
    /// Manages a pool of InferenceAgents. 
    /// This acts as a rudimentary "Model Server" or "Agent Pool" to handle concurrent requests.
    /// </summary>
    public class AgentPool
    {
        private readonly Queue<InferenceAgent> _availableAgents = new Queue<InferenceAgent>();
        private readonly int _maxPoolSize;
        private int _currentAgentCount = 0;

        public AgentPool(int maxPoolSize)
        {
            _maxPoolSize = maxPoolSize;
        }

        /// <summary>
        /// Acquires an agent from the pool. If none available and under max size, creates a new one.
        /// </summary>
        public async Task<InferenceAgent> AcquireAgentAsync()
        {
            lock (_availableAgents)
            {
                if (_availableAgents.Count > 0)
                {
                    return _availableAgents.Dequeue();
                }
            }

            // If pool is empty but we haven't reached max capacity, create a new agent
            if (_currentAgentCount < _maxPoolSize)
            {
                Interlocked.Increment(ref _currentAgentCount);
                return new InferenceAgent($"Agent-{_currentAgentCount}");
            }

            // If at capacity, wait (blocking) - in a real system, we'd use async semaphores or backpressure
            // For this simple example, we spin-wait.
            while (true)
            {
                lock (_availableAgents)
                {
                    if (_availableAgents.Count > 0)
                    {
                        return _availableAgents.Dequeue();
                    }
                }
                await Task.Delay(10); // Yield CPU
            }
        }

        /// <summary>
        /// Returns an agent to the pool for reuse.
        /// </summary>
        public void ReleaseAgent(InferenceAgent agent)
        {
            lock (_availableAgents)
            {
                _availableAgents.Enqueue(agent);
            }
        }
    }

    /// <summary>
    /// Simulates the Kubernetes Horizontal Pod Autoscaler (HPA) logic.
    /// It monitors load and decides whether to scale the AgentPool up or down.
    /// </summary>
    public class Autoscaler
    {
        private readonly AgentPool _pool;
        private readonly int _targetRequestsPerSecond;
        private readonly TimeSpan _evaluationInterval = TimeSpan.FromSeconds(5);
        
        // Metrics tracking
        private int _requestsInLastInterval = 0;
        private DateTime _lastEvaluationTime = DateTime.UtcNow;

        public Autoscaler(AgentPool pool, int targetRequestsPerSecond)
        {
            _pool = pool;
            _targetRequestsPerSecond = targetRequestsPerSecond;
        }

        /// <summary>
        /// Records a request to calculate throughput.
        /// </summary>
        public void RecordRequest()
        {
            Interlocked.Increment(ref _requestsInLastInterval);
        }

        /// <summary>
        /// Starts the monitoring loop (simulates the K8s controller manager).
        /// </summary>
        public async Task StartMonitoringAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(_evaluationInterval, cancellationToken);
                await EvaluateAndScaleAsync();
            }
        }

        private async Task EvaluateAndScaleAsync()
        {
            int currentRequests = Interlocked.Exchange(ref _requestsInLastInterval, 0);
            double actualRps = currentRequests / _evaluationInterval.TotalSeconds;

            // Simple logic: if actual RPS > target RPS, we need more capacity.
            // In a real K8s HPA, this is calculated based on CPU/Memory or custom metrics.
            // Here we simulate scaling by adjusting the pool's internal capacity (simplified).
            
            Console.WriteLine($"[Autoscaler] Current RPS: {actualRps:F2} | Target RPS: {_targetRequestsPerSecond}");

            if (actualRps > _targetRequestsPerSecond)
            {
                Console.WriteLine("[Autoscaler] SCALING UP: High load detected.");
                // In a real K8s scenario, this would trigger: kubectl scale deployment inference-agent --replicas=N
                // Here, we just log the action.
            }
            else if (actualRps < _targetRequestsPerSecond * 0.5) // Scale down if load is 50% of target
            {
                Console.WriteLine("[Autoscaler] SCALING DOWN: Low load detected.");
                // Real K8s: kubectl scale deployment inference-agent --replicas=N
            }
            else
            {
                Console.WriteLine("[Autoscaler] STABLE: Load within acceptable range.");
            }
        }
    }

    /// <summary>
    /// Main entry point simulating the Microservice receiving HTTP requests.
    /// </summary>
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Initializing Cloud-Native Inference Service...");

            // 1. Initialize the Agent Pool (Simulating a Kubernetes Deployment)
            // We limit the pool size to simulate resource constraints (CPU/Memory limits).
            var agentPool = new AgentPool(maxPoolSize: 4);

            // 2. Initialize the Autoscaler (Simulating the HPA Controller)
            var autoscaler = new Autoscaler(agentPool, targetRequestsPerSecond: 10);

            // 3. Start the Autoscaler monitoring loop in the background
            var cts = new CancellationTokenSource();
            _ = autoscaler.StartMonitoringAsync(cts.Token);

            // 4. Simulate incoming traffic (Incoming Requests)
            Console.WriteLine("Simulating incoming request traffic...");
            var tasks = new List<Task>();

            // Burst 1: Simulate a sudden spike in traffic
            for (int i = 0; i < 50; i++)
            {
                tasks.Add(ProcessRequest(agentPool, autoscaler));
                // Randomize delay to simulate real-world traffic patterns
                await Task.Delay(new Random().Next(10, 100));
            }

            await Task.WhenAll(tasks);
            Console.WriteLine("Burst 1 complete. Waiting for autoscaler evaluation...");

            // Wait for autoscaler to evaluate the burst
            await Task.Delay(6000); 

            // Burst 2: Simulate low traffic (potential scale down)
            tasks.Clear();
            Console.WriteLine("Simulating low traffic...");
            for (int i = 0; i < 5; i++)
            {
                tasks.Add(ProcessRequest(agentPool, autoscaler));
                await Task.Delay(500);
            }

            await Task.WhenAll(tasks);
            
            // Allow final evaluation
            await Task.Delay(6000);
            
            cts.Cancel();
            Console.WriteLine("Simulation complete.");
        }

        static async Task ProcessRequest(AgentPool pool, Autoscaler autoscaler)
        {
            // Record metric for autoscaler
            autoscaler.RecordRequest();

            // Acquire agent (simulates getting a pod from service)
            var agent = await pool.AcquireAgentAsync();
            
            try
            {
                // Perform inference
                var result = await agent.ProcessAsync("Hello World Input");
                // Console.WriteLine($"Processed with score: {result:F4}");
            }
            finally
            {
                // Return agent to pool (simulates keeping pod alive for reuse)
                pool.ReleaseAgent(agent);
            }
        }
    }
}
