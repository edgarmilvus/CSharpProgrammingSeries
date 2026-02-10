
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

using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public class ShardedMessageConsumer : BackgroundService
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly string _queueName;
    private readonly ILogger<ShardedMessageConsumer> _logger;
    private readonly string _instanceId; // Unique ID for this pod/agent

    public ShardedMessageConsumer(ILogger<ShardedMessageConsumer> logger)
    {
        _logger = logger;
        _instanceId = Environment.GetEnvironmentVariable("HOSTNAME") ?? Guid.NewGuid().ToString();
        
        // Setup Connection
        var factory = new ConnectionFactory { HostName = "rabbitmq" };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        // 1. Define Exchange (Consistent Hash for sharding)
        _channel.ExchangeDeclare(exchange: "sharded_tasks", type: "x-consistent-hash", durable: true);
        
        // 2. Create Queue bound to the exchange
        // The queue name includes the instance ID for uniqueness
        _queueName = $"tasks_queue_{_instanceId}";
        
        // Bind the queue to the exchange. 
        // In a real scenario, you might bind multiple queues to the same exchange 
        // with different routing keys (shards) to be consumed by different agents.
        _channel.QueueDeclare(queue: _queueName, durable: true, exclusive: false, autoDelete: false);
        _channel.QueueBind(queue: _queueName, exchange: "sharded_tasks", routingKey: "1"); // Hash bucket 1
    }

    public void SetupTopology(IModel channel)
    {
        // This method demonstrates the topology setup logic
        channel.ExchangeDeclare(exchange: "sharded_tasks", type: "x-consistent-hash", durable: true);
        
        // Simulating multiple queues for different shards
        for (int i = 1; i <= 3; i++)
        {
            var qName = $"tasks_shard_{i}";
            channel.QueueDeclare(queue: qName, durable: true, exclusive: false, autoDelete: false);
            // Binding with weight '1' means this queue gets roughly 1/3rd of the traffic
            channel.QueueBind(queue: qName, exchange: "sharded_tasks", routingKey: i.ToString());
        }
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        stoppingToken.Register(() => 
        {
            _channel.Close();
            _connection.Close();
            _logger.LogInformation("Queue connection closed gracefully.");
        });

        var consumer = new EventingBasicConsumer(_channel);
        
        consumer.Received += (model, ea) =>
        {
            try
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                
                _logger.LogInformation($"Processing message: {message} on {_queueName}");

                // Simulate processing
                Thread.Sleep(1000); 

                // Acknowledge the message
                _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message");
                // Negative acknowledgement (requeue logic depends on strategy)
                _channel.BasicNack(deliveryTag: ea.DeliveryTag, multiple: false, requeue: false);
            }
        };

        _channel.BasicConsume(queue: _queueName, autoAck: false, consumer: consumer);

        return Task.CompletedTask;
    }
}
