
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

namespace ScalableInferenceOrchestrator
{
    // REASONING: In a cloud-native microservices environment, we treat "Inference Requests" 
    // as discrete units of work. A Request object encapsulates the state of a single 
    // user interaction, including the input payload, the target model, and the status 
    // of processing.
    public class InferenceRequest
    {
        public string RequestId { get; set; }
        public string InputData { get; set; }
        public string ModelName { get; set; }
        public string Status { get; set; } // Pending, Processing, Completed, Failed
        public DateTime CreatedAt { get; set; }
        public string Result { get; set; }

        public InferenceRequest(string id, string data, string model)
        {
            RequestId = id;
            InputData = data;
            ModelName = model;
            Status = "Pending";
            CreatedAt = DateTime.Now;
        }
    }

    // REASONING: This class simulates a specific AI Agent runtime (e.g., a containerized 
    // Python process or a ONNX runtime). In a real scenario, this would interface with 
    // a GPU driver or an external API. We use Thread.Sleep to simulate the latency 
    // inherent in matrix multiplications and neural network inference.
    public class InferenceWorker
    {
        public string WorkerId { get; private set; }
        public bool IsBusy { get; private set; }
        public string CurrentModel { get; private set; }

        public InferenceWorker(string id)
        {
            WorkerId = id;
            IsBusy = false;
        }

        public async Task<string> ProcessAsync(InferenceRequest request)
        {
            IsBusy = true;
            CurrentModel = request.ModelName;
            
            // Simulate heavy compute load (GPU inference)
            // In production, this is where we call TensorFlow.NET, TorchSharp, or an HTTP client to a model server.
            int processingTimeMs = new Random().Next(1000, 3000); 
            await Task.Delay(processingTimeMs);

            IsBusy = false;
            CurrentModel = null;
            
            return $"Result processed by {WorkerId} for '{request.InputData}' in {processingTimeMs}ms";
        }
    }

    // REASONING: The Load Balancer is the core of the scaling strategy. It implements 
    // a "Round Robin" or "Least Connections" pattern. In this implementation, we iterate 
    // through available workers to find the first available one. This ensures we distribute 
    // load across the cluster of containers rather than overloading a single instance.
    public class LoadBalancer
    {
        private List<InferenceWorker> _workers;
        private int _currentIndex = 0;

        public LoadBalancer(int poolSize)
        {
            _workers = new List<InferenceWorker>();
            for (int i = 0; i < poolSize; i++)
            {
                _workers.Add(new InferenceWorker($"Worker-{i + 1}"));
            }
        }

        public InferenceWorker GetNextAvailableWorker()
        {
            // Simple Round Robin strategy
            for (int i = 0; i < _workers.Count; i++)
            {
                int index = (_currentIndex + i) % _workers.Count;
                if (!_workers[index].IsBusy)
                {
                    _currentIndex = (index + 1) % _workers.Count;
                    return _workers[index];
                }
            }
            return null; // All workers are busy
        }

        public void PrintStatus()
        {
            Console.WriteLine("\n--- Cluster Status ---");
            foreach (var w in _workers)
            {
                string status = w.IsBusy ? "[BUSY]" : "[IDLE]";
                string model = w.IsBusy ? $"Model: {w.CurrentModel}" : "";
                Console.WriteLine($"| {w.WorkerId.PadRight(10)} | {status.PadRight(8)} | {model}");
            }
            Console.WriteLine("----------------------\n");
        }
    }

    // REASONING: The Orchestrator manages the lifecycle of the requests. It decouples 
    // the arrival of requests from their processing. It uses a Queue (simulated by a List) 
    // to handle backpressure. If no workers are available, requests wait in the queue 
    // rather than being rejected immediately.
    public class InferenceOrchestrator
    {
        private LoadBalancer _loadBalancer;
        private Queue<InferenceRequest> _requestQueue;
        private bool _isRunning;
        private object _lock = new object();

        public InferenceOrchestrator(int workerCount)
        {
            _loadBalancer = new LoadBalancer(workerCount);
            _requestQueue = new Queue<InferenceRequest>();
            _isRunning = false;
        }

        public void SubmitRequest(InferenceRequest request)
        {
            lock (_lock)
            {
                _requestQueue.Enqueue(request);
                Console.WriteLine($"[Orchestrator] Request {request.RequestId} received and queued.");
            }
        }

        public void StartProcessing()
        {
            _isRunning = true;
            // Start the processing loop in a background thread (simulating an async message pump)
            Task.Run(() => ProcessingLoop());
        }

        public void StopProcessing()
        {
            _isRunning = false;
        }

        private async void ProcessingLoop()
        {
            while (_isRunning)
            {
                InferenceRequest currentRequest = null;
                InferenceWorker availableWorker = null;

                lock (_lock)
                {
                    if (_requestQueue.Count > 0)
                    {
                        availableWorker = _loadBalancer.GetNextAvailableWorker();
                        if (availableWorker != null)
                        {
                            currentRequest = _requestQueue.Dequeue();
                        }
                    }
                }

                if (currentRequest != null && availableWorker != null)
                {
                    // Update status
                    currentRequest.Status = "Processing";
                    Console.WriteLine($"[Orchestrator] Dispatching {currentRequest.RequestId} to {availableWorker.WorkerId}...");

                    // Asynchronous processing (non-blocking)
                    // We fire and forget the task, but in a real system, we would await a callback or use a TaskCompletionSource.
                    _ = ProcessAndReturnAsync(currentRequest, availableWorker);
                }
                else
                {
                    // Backoff if no workers are available or queue is empty
                    await Task.Delay(500); 
                }
            }
        }

        private async Task ProcessAndReturnAsync(InferenceRequest req, InferenceWorker worker)
        {
            try
            {
                string result = await worker.ProcessAsync(req);
                req.Status = "Completed";
                req.Result = result;
                Console.WriteLine($"[Orchestrator] Request {req.RequestId} Completed. Result: {req.Result}");
            }
            catch (Exception ex)
            {
                req.Status = "Failed";
                Console.WriteLine($"[Orchestrator] Request {req.RequestId} Failed: {ex.Message}");
            }
        }

        public void PrintClusterStatus()
        {
            _loadBalancer.PrintStatus();
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            // SCENARIO: A cloud-native AI service handling multiple user prompts 
            // simultaneously. We have a cluster of 3 containerized workers (simulated).
            // The system must handle high throughput by queuing requests and balancing load.
            
            Console.WriteLine("Initializing Scalable AI Inference Cluster...");
            var orchestrator = new InferenceOrchestrator(workerCount: 3);
            
            // Start the background processing engine
            orchestrator.StartProcessing();

            // Simulate a burst of incoming traffic
            string[] prompts = {
                "Summarize article on quantum computing",
                "Generate image of a cyberpunk city",
                "Translate 'Hello World' to French",
                "Analyze sentiment of review text",
                "Write a python script for data scraping",
                "Explain the theory of relativity",
                "Draft an email to the CEO",
                "Calculate matrix multiplication"
            };

            // Submit requests with a slight delay to simulate network arrival
            foreach (var prompt in prompts)
            {
                var req = new InferenceRequest(Guid.NewGuid().ToString(), prompt, "GPT-4-Turbo");
                orchestrator.SubmitRequest(req);
                
                // Visual feedback on throughput
                Thread.Sleep(300); 
                orchestrator.PrintClusterStatus();
            }

            Console.WriteLine("\nAll requests submitted. Waiting for queue to drain...");
            
            // Keep the application running to allow background tasks to complete
            // In a real web API, this would be the HTTP request lifecycle or a hosted service.
            while (true)
            {
                Thread.Sleep(2000);
                // In a real scenario, we would check if the queue is empty and break.
                // For this console demo, we'll just break after a timeout.
                Console.WriteLine("Press 'q' to stop monitoring or wait 10s...");
                // Simulating a wait for tasks to finish
                break; 
            }
            
            orchestrator.StopProcessing();
            Console.WriteLine("System Shutdown.");
        }
    }
}
