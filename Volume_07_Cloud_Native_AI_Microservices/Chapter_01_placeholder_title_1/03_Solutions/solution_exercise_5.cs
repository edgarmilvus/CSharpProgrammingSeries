
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

// Source File: solution_exercise_5.cs
// Description: Solution for Exercise 5
// ==========================================

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Diagnostics;
using System.Text;

namespace ObservabilityTracing
{
    // 1. Define the ActivitySource
    public static class AgentActivitySource
    {
        public static readonly ActivitySource Activities = new("AgentWorkflow", "1.0.0");
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = Host.CreateDefaultBuilder(args)
                .ConfigureServices((context, services) =>
                {
                    // 2. Configure OpenTelemetry
                    services.AddOpenTelemetry()
                        .ConfigureResource(resource => resource
                            .AddService(serviceName: "agent-service", serviceVersion: "1.0.0"))
                        .WithTracing(tracing => tracing
                            .AddSource(AgentActivitySource.Activities.Name) // Subscribe to our custom source
                            .AddConsoleExporter() // Export to console for debugging
                            .AddJaegerExporter(o => // Export to Jaeger
                            {
                                o.AgentHost = context.Configuration["Jaeger:Host"] ?? "localhost";
                                o.AgentPort = int.Parse(context.Configuration["Jaeger:Port"] ?? "6831");
                            }));
                    
                    // Register other services (Supervisor, Worker, etc.)
                })
                .Build();

            // Example usage of instrumentation
            using var activity = AgentActivitySource.Activities.StartActivity("MainOperation");
            activity?.SetTag("messaging.system", "kafka");
            activity?.SetTag("messaging.destination", "agent-tasks");
            
            // 3. Context Propagation Example (Simulated)
            InjectContextIntoKafkaMessage("some-message-key", "some-payload");

            builder.Run();
        }

        // 3. Propagating Context
        public static void InjectContextIntoKafkaMessage(string key, string value)
        {
            var currentActivity = Activity.Current;
            if (currentActivity == null) return;

            var headers = new Confluent.Kafka.Headers();
            
            // Use W3C Trace Context standard
            var propagator = new W3CTextMapPropagator();
            
            // Inject the trace context into the headers collection
            propagator.Inject(new PropagationContext(currentActivity.Context, Baggage.Current), headers, (h, k, v) =>
            {
                h.Add(k, Encoding.UTF8.GetBytes(v));
            });

            // Log injection for verification
            Console.WriteLine($"Trace Context injected into headers. TraceId: {currentActivity.TraceId}");
            
            // In a real scenario, you would pass these headers to the Kafka producer
            // var result = await producer.ProduceAsync(topic, new Message<string, string> { Key = key, Value = value, Headers = headers });
        }
    }
}
