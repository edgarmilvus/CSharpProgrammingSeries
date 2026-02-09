
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
using System.Collections.Generic;
using System.Threading;

namespace CloudNativeInferenceAgent
{
    // 1. Model Management & Resource Loading
    // Simulates loading heavy model weights from persistent storage (e.g., PVC in K8s).
    // In a real scenario, this would involve reading gigabytes of binary data.
    public class ModelLoader
    {
        private readonly string _modelPath;
        private bool _isLoaded;

        public ModelLoader(string path)
        {
            _modelPath = path;
            _isLoaded = false;
        }

        public void LoadWeights()
        {
            Console.WriteLine($"[ModelLoader] Loading weights from {_modelPath}...");
            // Simulate I/O latency for loading large model files
            Thread.Sleep(1000); 
            _isLoaded = true;
            Console.WriteLine("[ModelLoader] Weights loaded into memory. Ready for inference.");
        }

        public bool IsReady() => _isLoaded;
    }

    // 2. Inference Engine
    // Represents the containerized runtime performing the actual prediction.
    public class InferenceEngine
    {
        private readonly ModelLoader _modelLoader;

        public InferenceEngine(ModelLoader loader)
        {
            _modelLoader = loader;
        }

        // Simulates a prediction calculation based on input telemetry.
        // Uses basic loops and math to avoid advanced features like LINQ.
        public string Predict(float[] telemetry)
        {
            if (!_modelLoader.IsReady())
                return "Error: Model not loaded";

            // Simulate complex tensor operations
            float sum = 0;
            for (int i = 0; i < telemetry.Length; i++)
            {
                sum += telemetry[i];
            }
            
            float average = sum / telemetry.Length;

            // Simple threshold logic for failure prediction
            if (average > 75.0f)
            {
                return $"CRITICAL: Failure predicted (Avg Temp: {average:F2})";
            }
            else if (average > 60.0f)
            {
                return $"WARNING: High stress detected (Avg Temp: {average:F2})";
            }
            
            return $"NORMAL: Operating within limits (Avg Temp: {average:F2})";
        }
    }

    // 3. Telemetry Queue (Workload Buffer)
    // Simulates an event-driven source like Kafka or RabbitMQ.
    public class TelemetryQueue
    {
        private readonly Queue<float[]> _queue = new Queue<float[]>();
        private readonly object _lock = new object();

        public void Enqueue(float[] data)
        {
            lock (_lock)
            {
                _queue.Enqueue(data);
            }
        }

        public float[] Dequeue()
        {
            lock (_lock)
            {
                if (_queue.Count > 0)
                    return _queue.Dequeue();
                return null;
            }
        }

        public int GetCount()
        {
            lock (_lock)
            {
                return _queue.Count;
            }
        }
    }

    // 4. Autoscaler Controller (Simulating KEDA)
    // Monitors queue depth and scales the number of worker agents.
    public class Autoscaler
    {
        private readonly TelemetryQueue _queue;
        private readonly List<InferenceAgent> _agents;
        private readonly int _maxAgents;
        private readonly int _threshold; // Messages per agent to trigger scaling

        public Autoscaler(TelemetryQueue queue, int maxAgents, int threshold)
        {
            _queue = queue;
            _agents = new List<InferenceAgent>();
            _maxAgents = maxAgents;
            _threshold = threshold;
        }

        public void StartScalingLoop()
        {
            Console.WriteLine("\n[Autoscaler] Starting scaling monitor...");
            while (true)
            {
                int currentLoad = _queue.GetCount();
                int currentAgents = _agents.Count;

                // Calculate desired replicas based on load
                // Formula: Ceiling(QueueDepth / Threshold)
                int desiredAgents = (int)Math.Ceiling((double)currentLoad / _threshold);

                // Clamp to max agents
                if (desiredAgents > _maxAgents) desiredAgents = _maxAgents;
                if (desiredAgents < 1 && currentLoad > 0) desiredAgents = 1; // Always have at least 1 if work exists
                if (desiredAgents < 1 && currentLoad == 0) desiredAgents = 0; // Scale to zero if idle

                if (desiredAgents != currentAgents)
                {
                    Console.WriteLine($"[Autoscaler] Load: {currentLoad} | Current Agents: {currentAgents} | Desired: {desiredAgents}");
                    Scale(desiredAgents);
                }

                // Polling interval (simulating KEDA refresh rate)
                Thread.Sleep(2000); 
            }
        }

        private void Scale(int targetCount)
        {
            // Scale Up
            while (_agents.Count < targetCount)
            {
                var agent = new InferenceAgent(_queue);
                _agents.Add(agent);
                // Start agent on a new thread (simulating a Pod)
                Thread t = new Thread(agent.ProcessWork);
                t.Start();
                Console.WriteLine($"  -> Scaled UP: Agent #{_agents.Count} started.");
            }

            // Scale Down (Graceful termination)
            while (_agents.Count > targetCount)
            {
                var agent = _agents[_agents.Count - 1];
                _agents.RemoveAt(_agents.Count - 1);
                agent.RequestStop(); // Signal graceful shutdown
                Console.WriteLine($"  -> Scaled DOWN: Agent removed. Draining remaining work.");
            }
        }
    }

    // 5. Worker Agent (The Pod)
    // Represents a single container instance processing the queue.
    public class InferenceAgent
    {
        private readonly TelemetryQueue _queue;
        private readonly InferenceEngine _engine;
        private volatile bool _shouldStop = false;

        public InferenceAgent(TelemetryQueue queue)
        {
            _queue = queue;
            // In a real K8s pod, the model loader would be shared via volume or init container
            var loader = new ModelLoader("s3://models/v1/failure_detection.bin");
            loader.LoadWeights(); 
            _engine = new InferenceEngine(loader);
        }

        public void RequestStop()
        {
            _shouldStop = true;
        }

        public void ProcessWork()
        {
            while (!_shouldStop)
            {
                var data = _queue.Dequeue();
                if (data != null)
                {
                    string result = _engine.Predict(data);
                    Console.WriteLine($"[Agent] Processed: {result}");
                }
                else
                {
                    // Idle wait
                    Thread.Sleep(500);
                }
            }
        }
    }

    // 6. Main Program (Orchestrator)
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== Cloud-Native AI Inference System ===");
            Console.WriteLine("Simulating KEDA-based Autoscaling for GPU Workloads\n");

            // Initialize the event bus (Queue)
            var telemetryQueue = new TelemetryQueue();

            // Initialize the Autoscaler (KEDA Logic)
            // Config: Max 3 agents, 5 messages per agent before scaling up
            var scaler = new Autoscaler(telemetryQueue, maxAgents: 3, threshold: 5);

            // Start the scaler in a background thread
            Thread scalerThread = new Thread(scaler.StartScalingLoop);
            scalerThread.Start();

            // Simulate Incoming Telemetry Data (The Producer)
            // Generates random temperature readings to trigger load spikes
            Random rnd = new Random();
            Console.WriteLine("Generating telemetry data...\n");

            for (int i = 0; i < 30; i++)
            {
                // Generate a batch of data points
                float[] batch = new float[5];
                for (int j = 0; j < 5; j++)
                {
                    // Random temp between 40 and 90
                    batch[j] = (float)(40 + rnd.NextDouble() * 50);
                }

                telemetryQueue.Enqueue(batch);
                Console.WriteLine($"[Producer] Enqueued batch {i + 1}. Queue Depth: {telemetryQueue.GetCount()}");
                
                // Variable interval to simulate bursty traffic
                Thread.Sleep(rnd.Next(200, 800));
            }

            Console.WriteLine("\nData generation complete. Waiting for agents to finish processing...");
            // Keep main thread alive to observe scaling behavior
            Thread.Sleep(5000);
        }
    }
}
