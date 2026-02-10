
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

// Source File: solution_exercise_4.cs
// Description: Solution for Exercise 4
// ==========================================

// Telemetry Configuration (Program.cs for Router or Worker)
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Diagnostics;

public class Program
{
    public static void Main(string[] args)
    {
        // 1. Define a ResourceBuilder with service name
        var serviceName = "AiAgentRouter";
        var resourceBuilder = ResourceBuilder.CreateDefault()
            .AddService(serviceName: serviceName, serviceVersion: "1.0.0");

        // 2. Configure TracerProviderBuilder
        using var tracerProvider = Sdk.CreateTracerProviderBuilder()
            .SetResourceBuilder(resourceBuilder)
            // Add automatic instrumentation
            .AddAspNetCoreInstrumentation() // Captures incoming HTTP requests
            .AddHttpClientInstrumentation() // Captures outgoing HTTP calls
            .AddGrpcClientInstrumentation() // Specific for gRPC clients
            // Add Exporter (Jaeger)
            .AddJaegerExporter(o =>
            {
                o.AgentHost = "localhost"; // or Jaeger service name in K8s
                o.AgentPort = 6831;
            })
            .Build();

        // 3. Set the default text map propagator for context propagation
        // This ensures W3C TraceContext headers (traceparent) are used
        Sdk.SetDefaultTextMapPropagator(new OpenTelemetry.Extensions.Propagators.CompositeTextMapPropagator(
            new[] { new OpenTelemetry.Context.Propagation.TraceContextPropagator() }));

        // Start the Host (e.g., ASP.NET Core or Worker Service)
        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostContext, services) =>
            {
                // Register Load Generator Background Service
                services.AddHostedService<LoadGeneratorService>();
            });
}

// Load Generator Background Service
public class LoadGeneratorService : BackgroundService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<LoadGeneratorService> _logger;

    public LoadGeneratorService(IHttpClientFactory httpClientFactory, ILogger<LoadGeneratorService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var client = _httpClientFactory.CreateClient();
            
            // Simulate a request to the Router
            var stopwatch = Stopwatch.StartNew();
            try 
            {
                // Assuming Router is listening on localhost:5000
                var response = await client.GetAsync("http://localhost:5000/api/route", stoppingToken);
                stopwatch.Stop();
                
                // Extract TraceId from the current Activity (automatically managed by OpenTelemetry)
                var traceId = Activity.Current?.TraceId.ToString() ?? "Unknown";

                _logger.LogInformation($"Request sent. TraceId: {traceId}. Latency: {stopwatch.ElapsedMilliseconds}ms");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Load generation request failed.");
            }

            await Task.Delay(2000, stoppingToken); // Wait 2 seconds before next request
        }
    }
}
