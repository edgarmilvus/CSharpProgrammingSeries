
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

# Source File: solution_exercise_7.cs
# Description: Solution for Exercise 7
# ==========================================

// InferenceService/Consumer/InferenceConsumerService.cs
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shared.Contracts;
using System.Text;
using System.Text.Json;

namespace InferenceService.Consumer;

public class InferenceConsumerService : BackgroundService
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly ILogger<InferenceConsumerService> _logger;
    private readonly AsyncCircuitBreakerPolicy _circuitBreaker;
    private readonly AsyncRetryPolicy _retryPolicy;
    private const string QueueName = "inference_queue";
    private const string DlqName = "inference_dlq";
    private int _consecutiveFailures = 0;

    public InferenceConsumerService(ILogger<InferenceConsumerService> logger)
    {
        _logger = logger;
        
        // 1. Setup RabbitMQ
        var factory = new ConnectionFactory { HostName = "localhost" };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
        
        _channel.ExchangeDeclare(exchange: "inference.direct", type: ExchangeType.Direct);
        _channel.QueueDeclare(QueueName, durable: true, exclusive: false, autoDelete: false);
        _channel.QueueBind(QueueName, "inference.direct", "inference.request");
        
        // DLQ Setup
        _channel.QueueDeclare(DlqName, durable: true, exclusive: false, autoDelete: false);

        // 2. Resilience Policies
        // Retry policy for transient network errors
        _retryPolicy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (ex, time) => _logger.LogWarning(ex, "Retry {Time} due to {Ex}", time, ex.Message));

        // Circuit Breaker: Open after 5 failures, wait 30s
        _circuitBreaker = Policy
            .Handle<Exception>()
            .CircuitBreakerAsync(
                exceptionsAllowedBeforeBreaking: 5,
                durationOfBreak: TimeSpan.FromSeconds(30),
                onBreak: (ex, breakDelay) => 
                {
                    _logger.LogCritical("Circuit opened for {Delay}s due to {Ex}", breakDelay.TotalSeconds, ex.Message);
                },
                onReset: () => _logger.LogInformation("Circuit closed. Resuming normal operation.")
            );
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Configure QoS to limit concurrency to 5
        _channel.BasicQos(prefetchSize: 0, prefetchCount: 5, global: false);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        
        consumer.Received += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var messageJson = Encoding.UTF8.GetString(body);
            
            try
            {
                // Combine Retry and Circuit Breaker
                await _circuitBreaker.ExecuteAsync(async () =>
                {
                    await _retryPolicy.ExecuteAsync(async () =>
                    {
                        var message = JsonSerializer.Deserialize<InferenceRequestMessage>(messageJson);
                        
                        // Simulate processing logic
                        if (message == null) throw new InvalidDataException("Invalid message format");
                        await ProcessInference(message);
                        
                        _consecutiveFailures = 0; // Reset on success
                    });
                });

                // Acknowledge only on success
                _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                _consecutiveFailures++;
                _logger.LogError(ex, "Failed to process message. Consecutive failures: {Count}", _consecutiveFailures);

                // If circuit is open or we hit max retries, move to DLQ
                if (_circuitBreaker.CircuitState == CircuitState.Open || _consecutiveFailures >= 5)
                {
                    // Publish to DLQ
                    _channel.BasicPublish(exchange: "", routingKey: DlqName, basicProperties: null, body: body);
                    _channel.BasicAck(ea.DeliveryTag, false); // Ack original to remove from main queue
                    _logger.LogWarning("Message moved to DLQ: {MessageId}", ea.BasicProperties.MessageId);
                }
                else
                {
                    // Negative Ack (requeue)
                    _channel.BasicNack(ea.DeliveryTag, false, requeue: true);
                }
            }
        };

        _channel.BasicConsume(queue: QueueName, autoAck: false, consumer: consumer);
        
        return Task.CompletedTask;
    }

    private async Task ProcessInference(InferenceRequestMessage message)
    {
        // Simulate inference work
        _logger.LogInformation("Processing inference for Message ID: {Id}", message.MessageId);
        await Task.Delay(100); 
    }
    
    public override void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
        base.Dispose();
    }
}
