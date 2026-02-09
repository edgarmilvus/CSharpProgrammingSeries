
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

// System namespaces for core functionality
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Channels;

// Microsoft.Extensions namespaces for Dependency Injection and Hosting
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace SentinelAgent
{
    // 1. Domain Model: Represents a sensor reading
    public record SensorData(string SensorId, double Value, DateTime Timestamp);

    // 2. Domain Model: Represents an anomaly alert
    public record AnomalyAlert(string SensorId, double Value, string Reason);

    // 3. The Core Agent Logic: Inference Engine
    // In a real scenario, this would load a ONNX or ML.NET model.
    public interface IInferenceEngine
    {
        bool IsAnomaly(SensorData data);
    }

    public class SimpleThresholdEngine : IInferenceEngine
    {
        // Simulating a model threshold. In production, this comes from a config map or model file.
        private const double Threshold = 90.0;

        public bool IsAnomaly(SensorData data)
        {
            // Simulate inference latency (e.g., matrix multiplication)
            Thread.Sleep(10); 
            return data.Value > Threshold;
        }
    }

    // 4. The Alerting Service (Output)
    public interface IAlertDispatcher
    {
        Task SendAlertAsync(AnomalyAlert alert, CancellationToken cancellationToken);
    }

    public class ConsoleAlertDispatcher : IAlertDispatcher
    {
        private readonly ILogger<ConsoleAlertDispatcher> _logger;

        public ConsoleAlertDispatcher(ILogger<ConsoleAlertDispatcher> logger)
        {
            _logger = logger;
        }

        public Task SendAlertAsync(AnomalyAlert alert, CancellationToken cancellationToken)
        {
            // In production, this would push to RabbitMQ, Azure Service Bus, or Kafka
            _logger.LogWarning("ALERT TRIGGERED: Sensor {Id} reported {Value}. Reason: {Reason}", 
                alert.SensorId, alert.Value, alert.Reason);
            return Task.CompletedTask;
        }
    }

    // 5. The Data Ingestion Service (Input)
    // Uses Channels for high-throughput, non-blocking data streaming
    public class SensorIngestionService
    {
        private readonly Channel<SensorData> _channel;

        public SensorIngestionService()
        {
            // Bounded channel prevents memory overflows if ingestion outpaces processing
            var options = new BoundedChannelOptions(1000)
            {
                FullMode = BoundedChannelFullMode.Wait
            };
            _channel = Channel.CreateBounded<SensorData>(options);
        }

        public ChannelWriter<SensorData> Writer => _channel.Writer;
        public ChannelReader<SensorData> Reader => _channel.Reader;
    }

    // 6. The Background Worker (The Agent Host)
    public class AgentWorker : BackgroundService
    {
        private readonly SensorIngestionService _ingestionService;
        private readonly IInferenceEngine _inferenceEngine;
        private readonly IAlertDispatcher _dispatcher;
        private readonly ILogger<AgentWorker> _logger;

        public AgentWorker(
            SensorIngestionService ingestionService,
            IInferenceEngine inferenceEngine,
            IAlertDispatcher dispatcher,
            ILogger<AgentWorker> logger)
        {
            _ingestionService = ingestionService;
            _inferenceEngine = inferenceEngine;
            _dispatcher = dispatcher;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Agent Worker started. Listening for sensor data...");

            // Read from the channel continuously
            await foreach (var data in _ingestionService.Reader.ReadAllAsync(stoppingToken))
            {
                // Run inference
                if (_inferenceEngine.IsAnomaly(data))
                {
                    var alert = new AnomalyAlert(data.SensorId, data.Value, "Threshold Exceeded");
                    await _dispatcher.SendAlertAsync(alert, stoppingToken);
                }
                else
                {
                    _logger.LogDebug("Sensor {Id} reading {Value} is normal.", data.SensorId, data.Value);
                }
            }
        }
    }

    // 7. Simulated Data Generator (To make the example runnable)
    public class DataGenerator : BackgroundService
    {
        private readonly SensorIngestionService _ingestionService;
        private readonly Random _random = new();
        private readonly ILogger<DataGenerator> _logger;

        public DataGenerator(SensorIngestionService ingestionService, ILogger<DataGenerator> logger)
        {
            _ingestionService = ingestionService;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            int iteration = 0;
            while (!stoppingToken.IsCancellationRequested)
            {
                iteration++;
                
                // Simulate varying sensor values. 
                // Occasionally generate a high value (>90) to trigger an anomaly.
                double value = _random.NextDouble() * 100; 
                if (iteration % 20 == 0) value = 95.0; // Force anomaly every 20 ticks

                var data = new SensorData($"Sensor-{_random.Next(1, 5)}", value, DateTime.UtcNow);

                // Write to the channel (non-blocking)
                await _ingestionService.Writer.WriteAsync(data, stoppingToken);
                
                _logger.LogDebug("Generated data: {Id} = {Value}", data.SensorId, data.Value);

                // Simulate sensor polling rate
                await Task.Delay(500, stoppingToken);
            }
        }
    }

    // 8. Main Entry Point (DI Configuration)
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var host = Host.CreateDefaultBuilder(args)
                .ConfigureServices((context, services) =>
                {
                    // Register Singleton for the channel wrapper
                    services.AddSingleton<SensorIngestionService>();

                    // Register Transient/Scoped implementations
                    services.AddSingleton<IInferenceEngine, SimpleThresholdEngine>();
                    services.AddSingleton<IAlertDispatcher, ConsoleAlertDispatcher>();

                    // Register Hosted Services (Background Tasks)
                    services.AddHostedService<AgentWorker>();
                    services.AddHostedService<DataGenerator>(); // Simulates external input
                })
                .ConfigureLogging(logging =>
                {
                    // Clear default providers to simplify console output for Docker logs
                    logging.ClearProviders();
                    // Add simple console logging
                    logging.AddConsole();
                    // Set minimum level to Information to see alerts, Debug to see data flow
                    logging.SetMinimumLevel(LogLevel.Information);
                })
                .Build();

            await host.RunAsync();
        }
    }
}
