
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

namespace AdvancedDeploymentPatterns
{
    // =================================================================================================================================================
    // REAL-WORLD CONTEXT: AI INFERENCE GATEWAY WITH BATCHING & STATE MANAGEMENT
    // =================================================================================================================================================
    // Scenario: We are building a "Smart Home Voice Assistant Gateway". This gateway receives
    // voice-to-text transcription requests from multiple IoT devices (smart speakers, cameras).
    //
    // Problem: Running an AI model for every single request is expensive and slow (high latency).
    // We need to implement:
    // 1. Request Batching: Group multiple incoming requests into a single batch to process them
    //    simultaneously on the GPU, maximizing throughput.
    // 2. State Management: Maintain the state of the conversation (context) for each user device
    //    so the AI remembers previous interactions.
    // 3. Concurrency: Handle multiple users trying to access the gateway at the same time without
    //    corrupting the shared state.
    // =================================================================================================================================================

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== Smart Home AI Gateway Simulation ===");
            Console.WriteLine("Initializing Batching Engine & State Cache...");
            Console.WriteLine("--------------------------------------------------\n");

            // 1. Initialize the Distributed State Cache (Simulating Redis/Memcached)
            // This stores conversation history for specific device IDs.
            var stateCache = new DistributedStateCache();

            // 2. Initialize the Batching Engine
            // This engine collects requests and processes them in chunks.
            var batchingEngine = new BatchingEngine(stateCache);

            // 3. Simulate Incoming Traffic
            // We spawn multiple "Device Agents" that send requests concurrently.
            List<Task> agentTasks = new List<Task>();
            string[] deviceIds = { "kitchen-speaker", "living-room-cam", "bedroom-thermostat" };

            Random rnd = new Random();

            // Simulate 10 incoming requests over time
            for (int i = 0; i < 10; i++)
            {
                string deviceId = deviceIds[i % 3]; // Round-robin device selection
                string voiceInput = $"Command #{i + 1}: Turn on the lights.";
                
                // Asynchronously send request to the gateway
                agentTasks.Add(Task.Run(() => 
                {
                    batchingEngine.ReceiveRequest(deviceId, voiceInput);
                }));

                // Small jitter to simulate real-world network timing
                Thread.Sleep(rnd.Next(50, 200));
            }

            // Wait for all requests to be processed
            Task.WaitAll(agentTasks.ToArray());

            Console.WriteLine("\n=== Simulation Complete ===");
            Console.WriteLine("Final State Cache Status:");
            stateCache.DisplayCacheContents();
        }
    }

    // =================================================================================================================================================
    // ARCHITECTURAL COMPONENT 1: DISTRIBUTED STATE CACHE
    // =================================================================================================================================================
    // Purpose: Decouples the processing logic from the storage of user context.
    // In a real cloud-native environment (Kubernetes), this would be an external Redis cluster.
    // Here, we simulate it with a thread-safe Dictionary.
    // =================================================================================================================================================

    public class DistributedStateCache
    {
        // Simulating a distributed key-value store.
        // Key: Device ID (string)
        // Value: Last processed command context (string)
        private Dictionary<string, string> _cache;
        private object _lock = new object();

        public DistributedStateCache()
        {
            _cache = new Dictionary<string, string>();
        }

        // Retrieves the conversation context for a specific device.
        public string GetContext(string deviceId)
        {
            lock (_lock)
            {
                if (_cache.ContainsKey(deviceId))
                {
                    return _cache[deviceId];
                }
                return "No prior context."; // Default state
            }
        }

        // Updates the context after processing a request.
        public void UpdateContext(string deviceId, string newContext)
        {
            lock (_lock)
            {
                _cache[deviceId] = newContext;
                Console.WriteLine($"    [Cache Update] Device: {deviceId} | Context: {newContext}");
            }
        }

        // Helper to display final state
        public void DisplayCacheContents()
        {
            lock (_lock)
            {
                foreach (var kvp in _cache)
                {
                    Console.WriteLine($"Device: {kvp.Key} -> Last Action: {kvp.Value}");
                }
            }
        }
    }

    // =================================================================================================================================================
    // ARCHITECTURAL COMPONENT 2: BATCHING ENGINE
    // =================================================================================================================================================
    // Purpose: Implements the "Request Batching" pattern.
    // Logic:
    // 1. Accumulates incoming requests in a buffer.
    // 2. Waits for either a specific time window (e.g., 100ms) OR a specific batch size (e.g., 4 requests).
    // 3. Processes the batch (Simulating GPU inference).
    // 4. Updates the state cache with results.
    // =================================================================================================================================================

    public class BatchingEngine
    {
        // Buffer to hold incoming requests before processing
        private List<RequestPayload> _batchBuffer;
        private object _bufferLock = new object();
        
        // Configuration for batching logic
        private const int MaxBatchSize = 4;       // Max requests to hold before forcing a flush
        private const int BatchWindowMs = 200;    // Max time to wait before processing partial batch

        private DistributedStateCache _stateCache;
        private Timer _batchTimer;

        public BatchingEngine(DistributedStateCache cache)
        {
            _stateCache = cache;
            _batchBuffer = new List<RequestPayload>();

            // Initialize a timer that triggers every BatchWindowMs to process whatever we have
            _batchTimer = new Timer(ProcessBatchTimerCallback, null, BatchWindowMs, BatchWindowMs);
        }

        // Called by external agents/clients
        public void ReceiveRequest(string deviceId, string voiceInput)
        {
            Console.WriteLine($"[Ingress] Received request from {deviceId}");

            var payload = new RequestPayload
            {
                DeviceId = deviceId,
                InputText = voiceInput,
                ReceivedAt = DateTime.Now
            };

            lock (_bufferLock)
            {
                _batchBuffer.Add(payload);

                // Check if we hit the max batch size
                // If so, we trigger processing immediately (don't wait for timer)
                if (_batchBuffer.Count >= MaxBatchSize)
                {
                    Console.WriteLine($"    [Trigger] Batch size limit reached ({MaxBatchSize}). Flushing buffer.");
                    // Note: In a real async system, we would use a Task.Run here to avoid blocking the ingress.
                    // For this console simulation, we call directly.
                    ProcessBatch(); 
                }
            }
        }

        // Timer Callback: Triggers when the time window expires
        private void ProcessBatchTimerCallback(object state)
        {
            lock (_bufferLock)
            {
                if (_batchBuffer.Count > 0)
                {
                    Console.WriteLine($"    [Trigger] Time window expired. Flushing buffer.");
                    ProcessBatch();
                }
            }
        }

        // The Core Logic: Simulating AI Inference on a Batch
        private void ProcessBatch()
        {
            // 1. Snapshot the current buffer and clear it immediately so new requests can enter
            List<RequestPayload> currentBatch;
            
            lock (_bufferLock)
            {
                if (_batchBuffer.Count == 0) return;
                currentBatch = new List<RequestPayload>(_batchBuffer);
                _batchBuffer.Clear();
            }

            Console.WriteLine($"\n>>> PROCESSING BATCH: Size={currentBatch.Count} <<<");

            // 2. Simulate GPU Inference Latency
            // In a real scenario, this is where we call TensorFlow.NET or ONNX Runtime
            // passing the batch of tensors.
            Thread.Sleep(150); // Simulate heavy compute time

            // 3. Post-Process Results (Simulated)
            foreach (var req in currentBatch)
            {
                // Simulate AI logic: "If input contains 'lights', action is 'ToggleLights'"
                string action = req.InputText.Contains("lights") ? "ToggleLights" : "Unknown";
                
                // 4. Update State Cache (Distributed State Management)
                // We append the new action to the context history.
                string currentContext = _stateCache.GetContext(req.DeviceId);
                string newContext = $"[Action: {action}] Processed at {DateTime.Now:HH:mm:ss}";
                
                _stateCache.UpdateContext(req.DeviceId, newContext);
            }

            Console.WriteLine("<<< BATCH PROCESSING COMPLETE >>>\n");
        }
    }

    // =================================================================================================================================================
    // DATA STRUCTURES
    // =================================================================================================================================================
    // Simple payload object to hold request data.
    // Note: We avoid Records (C# 9) to stick to basic blocks as per constraints.
    // =================================================================================================================================================

    public class RequestPayload
    {
        public string DeviceId { get; set; }
        public string InputText { get; set; }
        public DateTime ReceivedAt { get; set; }
    }
}
