
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

using System.Text.Json;
using System.Threading.Channels;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

// ==================================================================
// 1. Domain Models: Defines the structure of communication.
// ==================================================================
public record AgentMessage(string AgentId, string Input, DateTime Timestamp);
public record AgentResult(string AgentId, string Response, DateTime Timestamp);

// ==================================================================
// 2. The Agent Logic: Simulates an AI Inference Task.
// ==================================================================
public class AiInferenceEngine
{
    private readonly ILogger<AiInferenceEngine> _logger;

    public AiInferenceEngine(ILogger<AiInferenceEngine> logger)
    {
        _logger = logger;
    }

    // Simulates a CPU/GPU intensive inference call (e.g., LLM prompt processing)
    public async Task<AgentResult> ProcessPromptAsync(AgentMessage message, CancellationToken ct)
    {
        _logger.LogInformation("Agent {Id}: Received input '{Input}'", message.AgentId, message.Input);

        // Simulate network latency and model processing time
        await Task.Delay(new Random().Next(500, 1500), ct);

        // Simple mock logic for the "AI" response
        var response = $"Processed '{message.Input}' -> Logical Conclusion generated.";
        
        _logger.LogInformation("Agent {Id}: Inference complete.", message.AgentId);

        return new AgentResult(message.AgentId, response, DateTime.UtcNow);
    }
}

// ==================================================================
// 3. The Microservice Host: Orchestrates the Agent's lifecycle.
// ==================================================================
public class AgentWorkerService : BackgroundService
{
    private readonly ILogger<AgentWorkerService> _logger;
    private readonly AiInferenceEngine _engine;
    
    // Channel<T> provides efficient, thread-safe producer/consumer queues.
    // This decouples message ingestion from message processing.
    private readonly Channel<AgentMessage> _inbox;

    public AgentWorkerService(ILogger<AgentWorkerService> logger, AiInferenceEngine engine)
    {
        _logger = logger;
        _engine = engine;
        
        // Bounded channel prevents memory overflow if traffic spikes.
        // FullMode.Wait blocks the sender when capacity is reached (backpressure).
        _inbox = Channel.CreateBounded<AgentMessage>(new BoundedChannelOptions(capacity: 10)
        {
            FullMode = BoundedChannelFullMode.Wait
        });
    }

    // ------------------------------------------------------------------
    // Ingestion Point: Simulates an external event (e.g., HTTP Request or Queue Message)
    // ------------------------------------------------------------------
    public async Task EnqueueAsync(AgentMessage message)
    {
        // WriteAsync respects the cancellation token and handles backpressure automatically
        await _inbox.Writer.WriteAsync(message);
        _logger.LogDebug("Message queued for Agent {Id}", message.AgentId);
    }

    // ------------------------------------------------------------------
    // Processing Loop: The heart of the containerized agent
    // ------------------------------------------------------------------
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Agent Worker Service started. Waiting for messages...");

        // We consume from the channel using 'await foreach'
        // This allows the loop to pause efficiently when no messages exist.
        await foreach (var message in _inbox.Reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                // Process the message using the injected engine
                var result = await _engine.ProcessPromptAsync(message, stoppingToken);
                
                // In a real scenario, this would publish to an Event Bus (e.g., RabbitMQ, Azure Service Bus)
                // or update a database.
                _logger.LogInformation("Result published: {Response}", result.Response);
            }
            catch (OperationCanceledException)
            {
                // Graceful shutdown requested
                _logger.LogWarning("Processing interrupted due to shutdown signal.");
                break;
            }
            catch (Exception ex)
            {
                // CRITICAL: Never let the worker loop die due to a single bad message.
                // Log the error and move on (or move to a Dead Letter Queue).
                _logger.LogError(ex, "Error processing message from Agent {Id}", message.AgentId);
            }
        }
    }
}

// ==================================================================
// 4. Main Entry Point: Wiring up Dependency Injection and Execution
// ==================================================================
public class Program
{
    public static async Task Main(string[] args)
    {
        var host = Host.CreateDefaultBuilder(args)
            .ConfigureServices(services =>
            {
                // Register the Engine as a Singleton (stateless logic)
                services.AddSingleton<AiInferenceEngine>();
                
                // Register the Worker as a Hosted Service (runs continuously)
                services.AddHostedService<AgentWorkerService>();
            })
            .ConfigureLogging(logging => 
            {
                logging.ClearProviders();
                logging.AddConsole();
            })
            .Build();

        // Start the background service
        await host.StartAsync();
        
        // SIMULATION: Inject traffic into the agent to demonstrate the flow
        var agentService = host.Services.GetRequiredService<AgentWorkerService>();
        
        Console.WriteLine("--- Injecting Simulation Traffic ---");
        
        // Fire and forget 5 messages to simulate concurrent requests
        var tasks = new List<Task>();
        for (int i = 1; i <= 5; i++)
        {
            var msg = new AgentMessage($"Agent-{i}", $"Query #{i}", DateTime.UtcNow);
            tasks.Add(agentService.EnqueueAsync(msg));
        }

        // Wait for ingestion to complete
        await Task.WhenAll(tasks);
        
        // Keep the app running long enough to process the queue
        await Task.Delay(5000); 
        
        // Graceful shutdown
        await host.StopAsync();
    }
}
