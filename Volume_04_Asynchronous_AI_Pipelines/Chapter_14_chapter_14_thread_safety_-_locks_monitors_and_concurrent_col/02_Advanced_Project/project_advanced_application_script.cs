
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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AI_Pipeline_Thread_Safety
{
    // Represents a data packet moving through the AI pipeline.
    // Using a simple class (reference type) to simulate shared mutable state.
    public class DataPacket
    {
        public string Id { get; set; }
        public string Content { get; set; }
        public bool IsProcessed { get; set; }
    }

    // Simulates a shared resource (e.g., a database connection or a file handle)
    // that requires exclusive access to prevent data corruption.
    public class SharedResource
    {
        private readonly object _lock = new object();
        private int _activeConnections = 0;

        // Using 'lock' for exclusive access to a critical section.
        public void AccessResource(string packetId)
        {
            lock (_lock)
            {
                _activeConnections++;
                Console.WriteLine($"[Resource] Access granted to Packet {packetId}. Active connections: {_activeConnections}");
                
                // Simulate work (e.g., writing to a shared stream)
                Thread.Sleep(50); 
                
                _activeConnections--;
            }
        }
    }

    // The AI Engine simulates processing tasks. 
    // We use a Monitor for more complex synchronization logic (e.g., signaling).
    public class AIEngine
    {
        private readonly object _monitorLock = new object();
        private readonly Queue<DataPacket> _processingQueue = new Queue<DataPacket>();
        private bool _isBusy = false;

        // Producer method: Adds a packet to the internal queue and signals waiting consumers.
        public void SubmitPacket(DataPacket packet)
        {
            lock (_monitorLock)
            {
                _processingQueue.Enqueue(packet);
                Console.WriteLine($"[Engine] Packet {packet.Id} queued.");
                
                // Pulse signals one waiting thread that the state has changed.
                Monitor.Pulse(_monitorLock);
            }
        }

        // Consumer method: Waits for packets to arrive.
        public void StartProcessing()
        {
            while (true)
            {
                lock (_monitorLock)
                {
                    // Wait if the queue is empty. 
                    // Monitor.Wait releases the lock and blocks until Pulse is received.
                    while (_processingQueue.Count == 0)
                    {
                        Monitor.Wait(_monitorLock);
                    }

                    DataPacket packet = _processingQueue.Dequeue();
                    _isBusy = true;
                    
                    // Simulate AI Processing time
                    Thread.Sleep(100);
                    packet.IsProcessed = true;
                    _isBusy = false;

                    Console.WriteLine($"[Engine] Processed Packet {packet.Id}.");
                    
                    // Pulse all to notify any monitors (e.g., status checkers) that processing finished.
                    Monitor.PulseAll(_monitorLock);
                }
            }
        }
    }

    // Handles the aggregation of results from multiple AI workers.
    // Uses ConcurrentDictionary for thread-safe aggregation without explicit locks.
    public class ResultAggregator
    {
        // Key: Packet ID, Value: Processing Duration (ms)
        private readonly ConcurrentDictionary<string, int> _results = new ConcurrentDictionary<string, int>();

        public void AddResult(string packetId, int duration)
        {
            // TryAdd is thread-safe. No lock needed here.
            if (_results.TryAdd(packetId, duration))
            {
                Console.WriteLine($"[Aggregator] Added result for Packet {packetId}.");
            }
        }

        public void PrintSummary()
        {
            Console.WriteLine("\n--- Pipeline Summary ---");
            Console.WriteLine($"Total Processed Packets: {_results.Count}");
            foreach (var kvp in _results)
            {
                Console.WriteLine($"  Packet {kvp.Key}: {kvp.Value}ms");
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting Asynchronous AI Pipeline Simulation...\n");

            // 1. Initialize Shared Components
            var sharedResource = new SharedResource();
            var aiEngine = new AIEngine();
            var aggregator = new ResultAggregator();

            // 2. Start the AI Engine Thread (Consumer)
            // This runs independently, waiting for data.
            Thread engineThread = new Thread(() => aiEngine.StartProcessing());
            engineThread.IsBackground = true;
            engineThread.Start();

            // 3. Simulate Data Ingestion (Producers)
            // We use Parallel.For to simulate multiple concurrent inputs (e.g., user requests).
            Parallel.For(0, 5, i =>
            {
                string id = Guid.NewGuid().ToString().Substring(0, 4);
                var packet = new DataPacket { Id = id, Content = $"Raw Data {i}" };

                // Accessing Shared Resource with Lock
                sharedResource.AccessResource(packet.Id);

                // Submit to the Engine
                aiEngine.SubmitPacket(packet);

                // Simulate a delay between submissions
                Thread.Sleep(20);
            });

            // 4. Wait for Queue to clear (Simple synchronization)
            // In a real app, we'd use CountdownEvent or Task completion sources.
            Thread.Sleep(1000);

            // 5. Collect Metrics
            // Simulating a background monitor reading the ConcurrentDictionary
            aggregator.AddResult("Batch-01", 120);
            aggregator.AddResult("Batch-02", 95);
            aggregator.PrintSummary();

            Console.WriteLine("\nPipeline simulation finished.");
            Console.ReadKey();
        }
    }
}
