
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
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;

namespace AIServiceTelemetryDemo
{
    // Real-world context: An AI service hosting multiple versions of a chat model.
    // We need to monitor inference latency, track errors, and correlate them
    // with specific model versions to identify performance regressions.
    class Program
    {
        // Configuration constants for the telemetry setup.
        private const string ServiceName = "AI-Chat-API";
        private const string ServiceVersion = "1.0.0";

        static async Task Main(string[] args)
        {
            // 1. Configure OpenTelemetry Tracing
            // We use a "TracerProvider" to manage the lifecycle of traces.
            // Using the ConsoleExporter allows us to see trace data directly in the terminal
            // for this demonstration, simulating a connection to Jaeger/Zipkin.
            using var tracerProvider = Sdk.CreateTracerProviderBuilder()
                .AddSource(ServiceName) // Listen to sources with this name
                .SetResourceBuilder(ResourceBuilder.CreateDefault()
                    .AddService(serviceName: ServiceName, serviceVersion: ServiceVersion))
                .AddConsoleExporter() // Export traces to console
                .Build();

            // 2. Configure OpenTelemetry Logging
            // We replace the default console logger with the OpenTelemetry logger
            // to enable structured logging and correlation with traces.
            using var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddOpenTelemetry(options =>
                {
                    options.SetResourceBuilder(ResourceBuilder.CreateDefault()
                        .AddService(serviceName: ServiceName, serviceVersion: ServiceVersion));
                    options.AddConsoleExporter();
                });
            });

            var logger = loggerFactory.CreateLogger<Program>();

            // 3. Configure OpenTelemetry Metrics
            // We create a meter to track custom business metrics like inference count and latency.
            var meter = new Meter(ServiceName);
            var inferenceCounter = meter.CreateCounter<long>("ai.inferences.total", "inferences", "Total number of model inferences");
            var latencyHistogram = meter.CreateHistogram<double>("ai.inference.latency", "ms", "Inference latency in milliseconds");

            using var meterProvider = Sdk.CreateMeterProviderBuilder()
                .AddMeter(ServiceName)
                .AddConsoleExporter()
                .Build();

            // Simulate the application lifecycle
            Console.WriteLine($"Starting {ServiceName}...");
            SimulateTraffic(inferenceCounter, latencyHistogram, logger);
            Console.WriteLine("Traffic simulation complete. Flushing telemetry...");
            
            // Ensure all telemetry is exported before exiting
            await Task.Delay(2000);
        }

        static void SimulateTraffic(Counter<long> counter, Histogram<double> histogram, ILogger logger)
        {
            // Simulate 10 incoming chat requests
            for (int i = 1; i <= 10; i++)
            {
                ProcessRequest(i, counter, histogram, logger);
                // Simulate delay between requests
                Thread.Sleep(500);
            }
        }

        static void ProcessRequest(int requestId, Counter<long> counter, Histogram<double> histogram, ILogger logger)
        {
            // 1. Start a new Trace Span
            // The ActivitySource is the entry point for creating distributed traces.
            using var activity = ActivitySource.StartActivity("ChatCompletion");
            
            if (activity != null)
            {
                // Add tags (metadata) to the span for filtering in observability tools
                activity.SetTag("request.id", requestId);
                activity.SetTag("user.tier", "premium");
            }

            // 2. Simulate Model Selection
            // In a real scenario, this might be dynamic based on load or configuration.
            string modelVersion = (requestId % 3 == 0) ? "v2-beta" : "v1-stable";
            
            // Log the start of inference using Structured Logging
            // This log entry is automatically correlated with the current Activity (Trace ID)
            logger.LogInformation("Starting inference for Request {RequestId} using Model {ModelVersion}", requestId, modelVersion);

            var stopwatch = Stopwatch.StartNew();
            bool isError = false;

            try
            {
                // Simulate AI Model Inference Logic
                // ---------------------------------------------------------
                // We intentionally introduce a failure for the "v2-beta" model
                // to demonstrate error logging and exception handling in traces.
                if (modelVersion == "v2-beta")
                {
                    // Simulate a random failure rate for the beta model
                    if (new Random().Next(0, 2) == 0) 
                    {
                        throw new InvalidOperationException("Model v2-beta inference timeout");
                    }
                }
                
                // Simulate processing time (random between 200ms and 800ms)
                Thread.Sleep(new Random().Next(200, 800));
                
                // Record successful metric
                counter.Add(1, new KeyValuePair<string, object>("model.version", modelVersion));
            }
            catch (Exception ex)
            {
                isError = true;
                
                // 1. Log the error with exception details
                // Structured logging captures the stack trace automatically.
                logger.LogError(ex, "Inference failed for Request {RequestId} on Model {ModelVersion}", requestId, modelVersion);

                // 2. Record Trace Exception Event
                // This marks the span as failed in the trace visualizer.
                if (activity != null)
                {
                    activity.SetStatus(ActivityStatusCode.Error, ex.Message);
                    activity.RecordException(ex);
                }

                // 3. Record Error Metric
                counter.Add(1, 
                    new KeyValuePair<string, object>("model.version", modelVersion),
                    new KeyValuePair<string, object>("status", "error"));
            }
            finally
            {
                stopwatch.Stop();

                // Record Latency Metric
                // We attach the model version as a tag (dimension) to allow slicing latency by model.
                histogram.Record(stopwatch.ElapsedMilliseconds, 
                    new KeyValuePair<string, object>("model.version", modelVersion),
                    new KeyValuePair<string, object>("status", isError ? "error" : "success"));

                // Log completion with latency context
                logger.LogInformation("Inference finished for Request {RequestId} in {Latency}ms", requestId, stopwatch.ElapsedMilliseconds);
            }
        }
    }
}
