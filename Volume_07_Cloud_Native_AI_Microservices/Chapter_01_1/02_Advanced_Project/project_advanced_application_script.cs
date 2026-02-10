
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
using System.Threading.Tasks;

namespace CloudNativeAgentInference
{
    // Simulated external dependency: A high-performance GPU-accelerated model runner.
    // In a real cloud-native environment, this would be a separate microservice or a native library call.
    public interface IModelInferenceEngine
    {
        Task<float[]> PredictAsync(float[] input);
    }

    // Mock implementation of the inference engine to demonstrate the logic without actual hardware.
    public class MockGpuEngine : IModelInferenceEngine
    {
        // Simulates the latency and computation of a GPU-bound operation.
        public async Task<float[]> PredictAsync(float[] input)
        {
            // Simulate network latency and GPU processing time (e.g., 100ms - 300ms)
            await Task.Delay(new Random().Next(100, 300));
            
            // Return dummy prediction data (e.g., sentiment score, classification probabilities)
            float[] result = new float[input.Length];
            for (int i = 0; i < input.Length; i++)
            {
                result[i] = (float)new Random().NextDouble(); // Random confidence score between 0.0 and 1.0
            }
            return result;
        }
    }

    // Represents a single unit of work (e.g., a user request, a document to analyze).
    // In a Kubernetes environment, this would be encapsulated in a JSON payload sent via Kafka or HTTP.
    public class InferenceRequest
    {
        public string RequestId { get; set; }
        public float[] InputData { get; set; }
        public DateTime Timestamp { get; set; }
    }

    // The core Agent Logic. 
    // This class encapsulates the "autonomous" decision-making of the microservice.
    // It is designed to be stateless: state is passed in via the request, not stored in class fields.
    public class InferenceAgent
    {
        private readonly IModelInferenceEngine _engine;
        private readonly float _confidenceThreshold;

        public InferenceAgent(IModelInferenceEngine engine, float threshold)
        {
            _engine = engine;
            _confidenceThreshold = threshold;
        }

        // The main processing method. 
        // It orchestrates data transformation, inference, and result evaluation.
        public async Task<InferenceResult> ProcessRequestAsync(InferenceRequest request)
        {
            // 1. Pre-processing: Validate and prepare input
            if (request.InputData == null || request.InputData.Length == 0)
            {
                return new InferenceResult { RequestId = request.RequestId, Status = "Failed", Message = "Empty input data." };
            }

            // 2. Inference: Call the model engine
            // In a real scenario, this is where the container communicates with the GPU driver.
            float[] predictions = await _engine.PredictAsync(request.InputData);

            // 3. Post-processing: Analyze results based on business logic
            bool isHighConfidence = false;
            float maxScore = 0f;

            // Basic loop to find the highest confidence score (avoiding LINQ as per constraints)
            for (int i = 0; i < predictions.Length; i++)
            {
                if (predictions[i] > maxScore)
                {
                    maxScore = predictions[i];
                }
            }

            if (maxScore >= _confidenceThreshold)
            {
                isHighConfidence = true;
            }

            // 4. Return structured result
            return new InferenceResult
            {
                RequestId = request.RequestId,
                Status = "Success",
                Confidence = maxScore,
                IsHighConfidence = isHighConfidence,
                ProcessingTimeMs = (DateTime.Now - request.Timestamp).TotalMilliseconds
            };
        }
    }

    // Data Transfer Object (DTO) for the output
    public class InferenceResult
    {
        public string RequestId { get; set; }
        public string Status { get; set; }
        public float Confidence { get; set; }
        public bool IsHighConfidence { get; set; }
        public double ProcessingTimeMs { get; set; }
        public string Message { get; set; }
    }

    // Simulates the Kubernetes Pod Autoscaler and Event Bus (e.g., Kafka)
    // This class generates load and manages the "scale" of agents.
    public class Orchestrator
    {
        private readonly List<InferenceAgent> _agentPool;
        private readonly int _maxConcurrentRequests;
        private int _activeRequests = 0;

        public Orchestrator(int poolSize, float threshold)
        {
            _agentPool = new List<InferenceAgent>();
            var engine = new MockGpuEngine();
            
            // "Containerizing" agents: Instantiating distinct agent objects to handle load
            for (int i = 0; i < poolSize; i++)
            {
                _agentPool.Add(new InferenceAgent(engine, threshold));
            }
            _maxConcurrentRequests = poolSize * 2; // Simulating over-subscription of CPU/GPU
        }

        public async void StartProcessingSimulation()
        {
            Console.WriteLine($"[Orchestrator] Starting simulation with {_agentPool.Count} agents.");
            Console.WriteLine($"[Orchestrator] Max Concurrent Capacity: {_maxConcurrentRequests}");
            Console.WriteLine("-------------------------------------------------------------");

            // Simulate an incoming event stream
            for (int i = 1; i <= 20; i++)
            {
                // Check for autoscaling trigger
                if (_activeRequests >= _maxConcurrentRequests)
                {
                    Console.WriteLine($"[Autoscaler] CRITICAL: Load {_activeRequests}/{_maxConcurrentRequests}. Scaling blocked (simulated).");
                    // In real K8s, this would trigger a HPA (Horizontal Pod Autoscaler) event
                    await Task.Delay(500); // Backpressure delay
                }

                _activeRequests++;
                
                // Round-robin agent selection (Load Balancing)
                InferenceAgent agent = _agentPool[i % _agentPool.Count];

                // Create a request payload
                var request = new InferenceRequest
                {
                    RequestId = $"REQ-{i:000}",
                    InputData = new float[] { 0.5f, 0.1f, 0.9f }, // Mock feature vector
                    Timestamp = DateTime.Now
                };

                Console.WriteLine($"[EventBus] Received {request.RequestId}. Dispatching to Agent {i % _agentPool.Count}.");

                // Asynchronous processing (Fire and Forget to simulate parallelism, but tracked for load)
                _ = Task.Run(async () => 
                {
                    try 
                    {
                        var result = await agent.ProcessRequestAsync(request);
                        LogResult(result);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[Error] Processing failed for {request.RequestId}: {ex.Message}");
                    }
                    finally
                    {
                        Interlocked.Decrement(ref _activeRequests);
                    }
                });

                // Simulate variable arrival rate of events
                await Task.Delay(new Random().Next(50, 200));
            }

            // Wait for remaining tasks to finish
            while (_activeRequests > 0)
            {
                await Task.Delay(100);
            }
            
            Console.WriteLine("-------------------------------------------------------------");
            Console.WriteLine("[Orchestrator] Simulation complete. All requests processed.");
        }

        private void LogResult(InferenceResult result)
        {
            string colorCode = result.IsHighConfidence ? "\x1b[32m" : "\x1b[33m"; // Green or Yellow
            string reset = "\x1b[0m";
            
            Console.WriteLine($"{colorCode}[Result] ID: {result.RequestId} | " +
                              $"Conf: {result.Confidence:F4} | " +
                              $"Time: {result.ProcessingTimeMs:F0}ms | " +
                              $"Status: {result.Status}{reset}");
        }
    }

    class Program
    {
        static async Task Main(string[] args)
        {
            // Configuration: Simulating K8s ConfigMap/Secrets injection
            float confidenceThreshold = 0.8f;
            int initialAgentPoolSize = 3; // Simulating initial ReplicaSet size

            // Initialize the Cloud-Native Orchestrator
            var orchestrator = new Orchestrator(initialAgentPoolSize, confidenceThreshold);

            // Start the event-driven processing loop
            orchestrator.StartProcessingSimulation();

            // Keep console open to view async output
            await Task.Delay(5000); 
        }
    }
}
