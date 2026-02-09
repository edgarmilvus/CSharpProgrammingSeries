
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

// ============================================================
// REAL-WORLD PROBLEM: Real-time AI Model Inference Pipeline
// ============================================================
// Scenario: A financial analytics platform receives real-time stock price 
// feeds from multiple market data providers. Each price update triggers 
// an AI model inference to predict short-term volatility. The system must:
// 1. Handle high-throughput streaming data (10,000+ updates/sec).
// 2. Minimize latency for time-sensitive trading decisions.
// 3. Efficiently batch requests to the AI model service to maximize GPU utilization.
// 4. Maintain a persistent connection between the API Gateway and the AI Inference Service.
//
// Solution: Use gRPC bi-directional streaming to create a pipeline where 
// the API Gateway streams market data to the AI service, and the AI service 
// streams back predictions in real-time.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Net.Client;

namespace FinancialAiPipeline
{
    // ============================================================
    // 1. DATA MODELS (Simulating Protobuf Messages)
    // ============================================================
    // In a real gRPC app, these are defined in a .proto file and generated.
    // We simulate them here using simple classes to demonstrate the logic flow.
    // These act as the Data Transfer Objects (DTOs) for our binary communication.

    public class MarketData
    {
        public string Symbol { get; set; }      // e.g., "AAPL"
        public double Price { get; set; }       // Current price
        public long Timestamp { get; set; }     // Unix epoch milliseconds
    }

    public class VolatilityPrediction
    {
        public string Symbol { get; set; }
        public double PredictedVolatility { get; set; } // AI model output (0.0 to 1.0)
        public string RiskLevel { get; set; }           // Low, Medium, High
        public long InferenceTimeMs { get; set; }       // Processing latency
    }

    // ============================================================
    // 2. AI INFERENCE SERVICE (Server Side Simulation)
    // ============================================================
    // This class simulates the remote AI model service. In production, 
    // this would be a separate ASP.NET Core application running on a GPU cluster.
    // It implements the bi-directional streaming handler.

    public class AiInferenceService
    {
        // Simulates the AI model (e.g., a neural network).
        // In reality, this would call a library like ML.NET or ONNX Runtime.
        private VolatilityPrediction RunModelInference(MarketData data)
        {
            // Simulate processing delay (e.g., 50ms for GPU computation)
            Thread.Sleep(50); 
            
            // Mock logic: Calculate a pseudo-volatility based on price and time
            double volatility = (data.Price % 100) / 100.0; 
            string risk = volatility > 0.7 ? "High" : (volatility > 0.4 ? "Medium" : "Low");

            return new VolatilityPrediction
            {
                Symbol = data.Symbol,
                PredictedVolatility = volatility,
                RiskLevel = risk,
                InferenceTimeMs = 50 // Mock latency
            };
        }

        // Handles a bi-directional stream of requests and responses.
        // This method processes incoming market data and streams back predictions.
        public async Task HandleStreamAsync(
            IAsyncStreamReader<MarketData> requestStream, 
            IServerStreamWriter<VolatilityPrediction> responseStream,
            ServerCallContext context)
        {
            Console.WriteLine("[AI Service] Client connected. Waiting for data stream...");

            // Read incoming messages from the client (API Gateway) loop
            await foreach (var data in requestStream.ReadAllAsync())
            {
                // Process the data using the AI model
                var prediction = RunModelInference(data);

                // Stream the prediction back to the client immediately
                await responseStream.WriteAsync(prediction);
                
                Console.WriteLine($"[AI Service] Processed {data.Symbol}: {prediction.RiskLevel} Risk");
            }

            Console.WriteLine("[AI Service] Client disconnected.");
        }
    }

    // ============================================================
    // 3. API GATEWAY (Client Side)
    // ============================================================
    // This represents the ASP.NET Core API Gateway that receives HTTP requests
    // from frontend apps and communicates with the backend AI service via gRPC.

    public class ApiGateway
    {
        private readonly AiInferenceService _localService; // For simulation only

        public ApiGateway()
        {
            _localService = new AiInferenceService();
        }

        // Simulates the gRPC client call.
        // In a real scenario, this would use a generated gRPC client stub.
        public async Task StreamDataToAiServiceAsync()
        {
            Console.WriteLine("\n[API Gateway] Starting Market Data Stream...");

            // Simulate a bi-directional stream channel
            // In real gRPC: var channel = GrpcChannel.ForAddress("https://ai-service:5001");
            // In real gRPC: var client = new PredictionService.PredictionServiceClient(channel);
            
            // We simulate the stream logic using local method calls to demonstrate the flow
            // without needing a running server for this console example.
            
            var requestStream = new MockAsyncStreamReader();
            var responseStream = new MockServerStreamWriter();

            // Start the server processing in a background task (simulating network latency)
            var serverTask = Task.Run(() => _localService.HandleStreamAsync(requestStream, responseStream, null));

            // Generate synthetic market data (Simulating a market feed)
            string[] symbols = { "AAPL", "GOOGL", "MSFT", "AMZN" };
            Random rand = new Random();

            for (int i = 0; i < 10; i++) // Send 10 batches of data
            {
                foreach (var sym in symbols)
                {
                    var marketData = new MarketData
                    {
                        Symbol = sym,
                        Price = 150.0 + rand.NextDouble() * 50,
                        Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                    };

                    // Send data to the stream (Client -> Server)
                    await requestStream.AddMessage(marketData);
                    
                    Console.WriteLine($"[API Gateway] Sent data for {sym}");

                    // Wait for the prediction (Server -> Client)
                    // In bi-directional streaming, we can process responses as they arrive
                    // without waiting for the entire request stream to finish.
                }
                
                // Small delay to simulate real-time interval
                await Task.Delay(100);
            }

            // Signal end of stream
            requestStream.Complete();

            // Read all remaining responses
            await foreach (var prediction in responseStream.ReadAllAsync())
            {
                Console.WriteLine($"[API Gateway] RECEIVED PREDICTION: {prediction.Symbol} | Volatility: {prediction.PredictedVolatility:F4} | Risk: {prediction.RiskLevel}");
            }

            await serverTask;
            Console.WriteLine("[API Gateway] Stream processing complete.");
        }
    }

    // ============================================================
    // 4. MOCK STREAM IMPLEMENTATIONS (Simulation Helpers)
    // ============================================================
    // These classes simulate the Grpc.Core interfaces to allow this 
    // console app to run without a physical network connection.

    public interface IAsyncStreamReader<T>
    {
        Task<bool> MoveNext(CancellationToken cancellationToken = default);
        T Current { get; }
    }

    public interface IServerStreamWriter<T>
    {
        Task WriteAsync(T message);
    }

    // Simulates the incoming stream from the Client to the Server
    public class MockAsyncStreamReader : IAsyncStreamReader<MarketData>
    {
        private readonly Queue<MarketData> _queue = new Queue<MarketData>();
        private readonly TaskCompletionSource<bool> _tcs = new TaskCompletionSource<bool>();
        private bool _completed = false;

        public MarketData Current { get; private set; }

        public async Task AddMessage(MarketData data)
        {
            lock (_queue) _queue.Enqueue(data);
            _tcs.TrySetResult(true); // Signal that data is available
        }

        public void Complete()
        {
            _completed = true;
            _tcs.TrySetResult(false); // Signal end of stream
        }

        public async Task<bool> MoveNext(CancellationToken cancellationToken)
        {
            while (true)
            {
                lock (_queue)
                {
                    if (_queue.Count > 0)
                    {
                        Current = _queue.Dequeue();
                        return true;
                    }
                    if (_completed) return false;
                }
                
                // Wait for new data or completion signal
                var result = await _tcs.Task;
                if (!result) return false; // Stream completed
                
                // Reset TCS for next wait cycle
                _tcs.TrySetResult(false); 
            }
        }
    }

    // Simulates the outgoing stream from the Server to the Client
    public class MockServerStreamWriter : IServerStreamWriter<VolatilityPrediction>
    {
        private readonly Queue<VolatilityPrediction> _queue = new Queue<VolatilityPrediction>();
        private readonly TaskCompletionSource<bool> _tcs = new TaskCompletionSource<bool>();

        public async Task WriteAsync(VolatilityPrediction message)
        {
            lock (_queue) _queue.Enqueue(message);
            _tcs.TrySetResult(true);
        }

        public async IAsyncEnumerable<VolatilityPrediction> ReadAllAsync()
        {
            while (true)
            {
                VolatilityPrediction next = null;
                lock (_queue)
                {
                    if (_queue.Count > 0)
                    {
                        next = _queue.Dequeue();
                    }
                }

                if (next != null)
                {
                    yield return next;
                    continue;
                }

                // Wait for data
                await _tcs.Task;
                _tcs.TrySetResult(false); // Reset for next wait
                
                // Check again if queue has data (might have been added while waiting)
                lock (_queue)
                {
                    if (_queue.Count > 0)
                    {
                        next = _queue.Dequeue();
                        if (next != null) yield return next;
                    }
                }
            }
        }
    }

    // ============================================================
    // 5. MAIN EXECUTION ENTRY POINT
    // ============================================================
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("=================================================");
            Console.WriteLine("Financial AI Pipeline - gRPC Bi-Directional Stream");
            Console.WriteLine("=================================================");

            var gateway = new ApiGateway();
            
            // Execute the high-performance streaming pipeline
            var stopwatch = Stopwatch.StartNew();
            
            await gateway.StreamDataToAiServiceAsync();
            
            stopwatch.Stop();
            Console.WriteLine($"\nTotal execution time: {stopwatch.ElapsedMilliseconds}ms");
            Console.WriteLine("Simulation Complete.");
        }
    }
}
