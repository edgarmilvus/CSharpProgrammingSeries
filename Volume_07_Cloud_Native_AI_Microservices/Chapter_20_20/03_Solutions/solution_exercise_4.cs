
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

# Source File: solution_exercise_4.cs
# Description: Solution for Exercise 4
# ==========================================

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;

public class ResilientProducerService : BackgroundService
{
    private readonly IBus _bus;
    private readonly ILogger<ResilientProducerService> _logger;
    
    // Define Polly Policies
    private readonly AsyncRetryPolicy _retryPolicy;
    private readonly AsyncCircuitBreakerPolicy _circuitBreakerPolicy;
    private readonly AsyncPolicyWrap _policyWrap;

    public ResilientProducerService(IBus bus, ILogger<ResilientProducerService> logger)
    {
        _bus = bus;
        _logger = logger;

        // 3. Polly Circuit Breaker
        // Opens if 50% of calls fail within a 30-second sampling duration.
        _circuitBreakerPolicy = Policy
            .Handle<Exception>()
            .CircuitBreakerAsync(
                exceptionsAllowedBeforeBreaking: 2, // 2 failures to open
                durationOfBreak: TimeSpan.FromSeconds(30), 
                onBreak: (ex, breakDelay) => logger.LogWarning("Circuit Opened"),
                onReset: () => logger.LogInformation("Circuit Closed")
            );

        // 4. Polly Retry with Jittered Exponential Backoff
        _retryPolicy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: attempt => 
                {
                    // Exponential: 200ms, 400ms, 800ms
                    var delay = Math.Pow(2, attempt) * 100; 
                    // Jitter: Add random variance to prevent thundering herd
                    var jitter = new Random().Next(0, 100); 
                    return TimeSpan.FromMilliseconds(delay + jitter);
                },
                onRetry: (ex, delay, retryCount, context) => 
                    logger.LogWarning("Retry {RetryCount} after {Delay}ms due to {Exception}", 
                        retryCount, delay.TotalMilliseconds, ex.Message)
            );

        // Combine: Retry inside Circuit Breaker
        // If Circuit is Open, Retry is bypassed immediately.
        _policyWrap = _circuitBreakerPolicy.WrapAsync(_retryPolicy);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var task = new DispatchAgentTask { Payload = "Critical Data", Priority = 10 };

            try
            {
                // Execute the wrapped policy
                await _policyWrap.ExecuteAsync(async () => 
                {
                    // 2. Custom Span Creation (via ActivitySource if using OpenTelemetry)
                    // Note: MassTransit has built-in OpenTelemetry support, 
                    // but here we manually wrap the publish logic if needed.
                    using var activity = Activity.Current?.Source.StartActivity("Publish Agent Task");
                    activity?.SetTag("task.priority", task.Priority);
                    activity?.SetTag("task.id", task.TaskId);

                    await _bus.Publish<IAgentTask>(task, stoppingToken);
                });
            }
            catch (BrokenCircuitException)
            {
                _logger.LogError("Circuit breaker is OPEN. Dropping message or buffering.");
                // Strategy: Buffer to disk or dead-letter queue here
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Unrecoverable error in producer");
            }

            await Task.Delay(2000, stoppingToken);
        }
    }
}

// ---------------------------------------------------------
// Health Checks Implementation
// ---------------------------------------------------------

public class RabbitMqHealthCheck : IHealthCheck
{
    private readonly IBusControl _bus;

    public RabbitMqHealthCheck(IBusControl bus) => _bus = bus;

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken ct = default)
    {
        // Check if the bus is connected (MassTransit exposes the connection status)
        // This is a simplified check; in production, check actual connection topology
        if (_bus != null) // && _bus.IsConnected logic depends on MassTransit version
            return Task.FromResult(HealthCheckResult.Healthy("Connected to RabbitMQ"));

        return Task.FromResult(HealthCheckResult.Unhealthy("Disconnected from RabbitMQ"));
    }
}

// In Program.cs
public static void ConfigureHealthChecks(IServiceCollection services)
{
    services.AddHealthChecks()
        .AddCheck<RabbitMqHealthCheck>("rabbitmq_connection", tags: new[] { "ready" }); // Only check readiness for connection
}

// ---------------------------------------------------------
// OpenTelemetry Configuration (In Program.cs)
// ---------------------------------------------------------

public static void ConfigureOpenTelemetry(IServiceCollection services)
{
    // Define Resource (Application Name)
    var resourceBuilder = ResourceBuilder.CreateDefault()
        .AddService(serviceName: "AgentSwarm", serviceVersion: "1.0.0");

    services.AddOpenTelemetry()
        .WithTracing(builder => builder
            .SetResourceBuilder(resourceBuilder)
            .AddSource("MassTransit") // Capture MassTransit activities
            .AddSource("AgentConsumer") // Our custom source
            .AddJaegerExporter() // Export to Jaeger
        )
        .WithMetrics(builder => builder
            .SetResourceBuilder(resourceBuilder)
            .AddMeter("MassTransit") // Capture MassTransit metrics
            .AddPrometheusExporter() // Export to Prometheus
        );
}
