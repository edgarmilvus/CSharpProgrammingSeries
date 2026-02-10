
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

# Source File: solution_exercise_1.cs
# Description: Solution for Exercise 1
# ==========================================

using System;
using System.Threading;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

// ---------------------------------------------------------
// 1. Shared Contracts
// ---------------------------------------------------------

// The task contract shared between producer and consumer
public interface IAgentTask
{
    Guid TaskId { get; }
    string Payload { get; }
    int Priority { get; }
}

// The completion event contract
public interface IAgentTaskCompleted
{
    Guid TaskId { get; }
    DateTime CompletedAt { get; }
}

// Concrete implementation for dispatching
public class DispatchAgentTask : IAgentTask
{
    public Guid TaskId { get; init; } = Guid.NewGuid();
    public string Payload { get; init; } = string.Empty;
    public int Priority { get; init; }
}

// Concrete implementation for completion
public class AgentTaskCompleted : IAgentTaskCompleted
{
    public Guid TaskId { get; init; }
    public DateTime CompletedAt { get; init; } = DateTime.UtcNow;
}

// ---------------------------------------------------------
// 2. Producer Service (The Orchestrator)
// ---------------------------------------------------------

public class TaskProducerService : BackgroundService
{
    private readonly IBus _bus;
    private readonly ILogger<TaskProducerService> _logger;

    public TaskProducerService(IBus bus, ILogger<TaskProducerService> logger)
    {
        _bus = bus;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Simulate high-throughput task generation
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Randomize priority to demonstrate routing later
                var priority = Random.Shared.Next(1, 11);
                var task = new DispatchAgentTask
                {
                    Payload = $"Sync data for user {Random.Shared.Next(1, 1000)}",
                    Priority = priority
                };

                // Publish to the exchange named "agent-tasks"
                await _bus.Publish<IAgentTask>(task, stoppingToken);

                _logger.LogInformation("Published Task: {TaskId} (Priority: {Priority})", 
                    task.TaskId, task.Priority);

                // Throttle production slightly
                await Task.Delay(1000, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error publishing task to RabbitMQ");
                // In a real scenario, we might buffer locally or retry
                await Task.Delay(5000, stoppingToken);
            }
        }
    }
}

// ---------------------------------------------------------
// 3. Consumer Agent (The Worker)
// ---------------------------------------------------------

// Consumer implementation for IAgentTask
public class AgentTaskConsumer : IConsumer<IAgentTask>
{
    private readonly ILogger<AgentTaskConsumer> _logger;
    private readonly IBus _bus;

    public AgentTaskConsumer(ILogger<AgentTaskConsumer> logger, IBus bus)
    {
        _logger = logger;
        _bus = bus;
    }

    public async Task Consume(ConsumeContext<IAgentTask> context)
    {
        var task = context.Message;

        try
        {
            // 1. Log receipt
            _logger.LogInformation(
                "Processing Task: {TaskId} | Payload: {Payload} | Priority: {Priority}",
                task.TaskId, task.Payload, task.Priority);

            // 2. Simulate processing time based on priority (Higher priority = faster processing)
            var processingTime = task.Priority > 7 ? 200 : 1000;
            await Task.Delay(processingTime, context.CancellationToken);

            // 3. Publish completion event
            var completedEvent = new AgentTaskCompleted
            {
                TaskId = task.TaskId
            };

            await _bus.Publish<IAgentTaskCompleted>(completedEvent, context.CancellationToken);

            _logger.LogInformation("Completed Task: {TaskId}", task.TaskId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process Task: {TaskId}", task.TaskId);
            // MassTransit automatically moves to _error queue if retry fails
            throw; 
        }
    }
}

// Background service to host the consumer (if running in the same process as producer, 
// usually we separate these, but for the exercise we define the consumer service)
public class TaskConsumerAgent : BackgroundService
{
    private readonly IHostApplicationLifetime _lifetime;
    private readonly IBusControl _busControl;
    private readonly ILogger<TaskConsumerAgent> _logger;

    public TaskConsumerAgent(IHostApplicationLifetime lifetime, IBusControl busControl, ILogger<TaskConsumerAgent> logger)
    {
        _lifetime = lifetime;
        _busControl = busControl;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Start the bus (connects to RabbitMQ)
        await _busControl.StartAsync(stoppingToken);
        _logger.LogInformation("Consumer Agent started and connected to RabbitMQ.");

        // Wait until the application is stopping to shutdown the bus
        using var registration = _lifetime.ApplicationStopping.Register(async () => 
        {
            await _busControl.StopAsync(TimeSpan.FromSeconds(10));
        });

        // Keep the service alive
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }
}

// ---------------------------------------------------------
// 4. Configuration (Program.cs style)
// ---------------------------------------------------------

public static class Program
{
    public static void Main(string[] args)
    {
        var host = Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                // Get RabbitMQ connection string from Env Vars
                var rabbitMqHost = context.Configuration["RABBITMQ_HOST"] ?? "localhost";
                var rabbitMqUser = context.Configuration["RABBITMQ_USER"] ?? "guest";
                var rabbitMqPass = context.Configuration["RABBITMQ_PASS"] ?? "guest";

                // Configure MassTransit with Resilience
                services.AddMassTransit(x =>
                {
                    // Register the consumer
                    x.AddConsumer<AgentTaskConsumer>();

                    x.UsingRabbitMq((cfg, rmq) =>
                    {
                        rmq.Host(rabbitMqHost, "/", h =>
                        {
                            h.Username(rabbitMqUser);
                            h.Password(rabbitMqPass);
                        });

                        // 5. Resilience: Configure Retry and Error Handling
                        // This handles transient connection failures automatically
                        rmq.ReceiveEndpoint("agent-tasks", e =>
                        {
                            // Retry policy: 3 retries with exponential backoff
                            e.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(1)));
                            
                            // Configure the consumer
                            e.ConfigureConsumer<AgentTaskConsumer>(cfg);
                        });

                        // Configure the results endpoint for the completion events
                        rmq.Publish<IAgentTaskCompleted>(x => x.ExchangeType = "fanout");
                    });
                });

                // Register the background services
                services.AddHostedService<TaskProducerService>();
                services.AddHostedService<TaskConsumerAgent>();
            })
            .Build();

        host.Run();
    }
}
