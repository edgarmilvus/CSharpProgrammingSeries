
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
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CloudNativeAiMicroservices.Example
{
    /// <summary>
    /// Represents the core data structure for an AI inference request.
    /// In a real-world scenario, this might contain complex tensors, 
    /// image byte arrays, or structured text prompts.
    /// </summary>
    public record InferenceRequest(
        string RequestId,
        string InputData,
        Dictionary<string, object> Parameters
    );

    /// <summary>
    /// Represents the response from the AI model inference.
    /// </summary>
    public record InferenceResponse(
        string RequestId,
        string Result,
        double InferenceTimeMs,
        string ModelVersion
    );

    /// <summary>
    /// Defines the contract for an AI inference service.
    /// This abstraction allows swapping implementations (e.g., local CPU vs. GPU-accelerated).
    /// </summary>
    public interface IInferenceService
    {
        Task<InferenceResponse> PredictAsync(InferenceRequest request, CancellationToken cancellationToken);
    }

    /// <summary>
    /// A mock implementation of an AI inference service.
    /// Simulates the delay and computation of a real model (like BERT or GPT) 
    /// without requiring actual GPU hardware or large model files.
    /// </summary>
    public class MockInferenceService : IInferenceService
    {
        private readonly ILogger<MockInferenceService> _logger;
        private readonly Random _random = new();

        public MockInferenceService(ILogger<MockInferenceService> logger)
        {
            _logger = logger;
        }

        public async Task<InferenceResponse> PredictAsync(InferenceRequest request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Processing request {RequestId} for input: {Input}", request.RequestId, request.InputData);

            // Simulate GPU inference latency (e.g., 50ms to 200ms)
            var delay = _random.Next(50, 200);
            await Task.Delay(delay, cancellationToken);

            // Simulate processing logic
            var result = $"Processed: {request.InputData.ToUpperInvariant()}";
            
            _logger.LogInformation("Completed request {RequestId} in {Time}ms", request.RequestId, delay);

            return new InferenceResponse(
                RequestId: request.RequestId,
                Result: result,
                InferenceTimeMs: delay,
                ModelVersion: "v1.0-mock"
            );
        }
    }

    /// <summary>
    /// A background service that simulates an incoming request queue.
    /// In a real Kubernetes environment, this would be replaced by an HTTP endpoint 
    /// (e.g., ASP.NET Core Minimal API) receiving traffic from an Ingress controller.
    /// </summary>
    public class RequestSimulatorService : BackgroundService
    {
        private readonly IInferenceService _inferenceService;
        private readonly ILogger<RequestSimulatorService> _logger;

        public RequestSimulatorService(IInferenceService inferenceService, ILogger<RequestSimulatorService> logger)
        {
            _inferenceService = inferenceService;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Request Simulator started. Waiting 3 seconds before first request...");

            // Allow time for the application to stabilize
            await Task.Delay(3000, stoppingToken);

            int requestCounter = 0;

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var requestId = $"req-{++requestCounter:D4}";
                    var request = new InferenceRequest(
                        RequestId: requestId,
                        InputData: $"cloud native ai request {requestCounter}",
                        Parameters: new Dictionary<string, object> { { "temperature", 0.7 } }
                    );

                    // Simulate an HTTP POST request to the inference endpoint
                    _ = await _inferenceService.PredictAsync(request, stoppingToken);

                    // Simulate incoming traffic rate (e.g., 1 request every 2 seconds)
                    await Task.Delay(2000, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error simulating request");
                    await Task.Delay(5000, stoppingToken);
                }
            }
        }
    }

    /// <summary>
    /// The main entry point and dependency injection composition root.
    /// </summary>
    public class Program
    {
        public static async Task Main(string[] args)
        {
            // Create the host builder using .NET Generic Host
            // This pattern is standard for microservices, providing lifecycle management,
            // logging, and dependency injection out of the box.
            var host = Host.CreateDefaultBuilder(args)
                .ConfigureServices((context, services) =>
                {
                    // Register the inference service as a Singleton.
                    // Why Singleton? In real scenarios, this service might hold 
                    // a loaded ML model in memory (which is expensive to load/unload).
                    // For HTTP controllers, we usually use Scoped, but for the service logic itself, 
                    // Singleton is efficient if thread-safe.
                    services.AddSingleton<IInferenceService, MockInferenceService>();

                    // Register the background service to simulate traffic.
                    // In a real deployment, this is removed, and the HTTP server handles requests.
                    services.AddHostedService<RequestSimulatorService>();
                })
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddConsole();
                    logging.SetMinimumLevel(LogLevel.Information);
                })
                .Build();

            await host.RunAsync();
        }
    }
}
