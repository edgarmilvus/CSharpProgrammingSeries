
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

// Source File: solution_exercise_7.cs
// Description: Solution for Exercise 7
// ==========================================

using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using Microsoft.Extensions.Hosting;

namespace WorkerService
{
    public class Worker : BackgroundService
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly ILogger<Worker> _logger;
        private readonly string _queueName = "document-processing-queue";

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
            // In production, inject connection factory via DI
            var factory = new ConnectionFactory() { HostName = "rabbitmq-service" };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.QueueDeclare(queue: _queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Worker started. Waiting for messages.");

            // KEDA scales based on queue depth. 
            // When scaling down to 0, KEDA sends a SIGTERM signal.
            
            while (!stoppingToken.IsCancellationRequested)
            {
                // BasicGet is used here for demonstration. 
                // In a real scenario, you might use EventingBasicConsumer for push-based processing.
                var result = _channel.BasicGet(_queueName, autoAck: false);
                
                if (result != null)
                {
                    try 
                    {
                        var body = result.Body.ToArray();
                        var message = Encoding.UTF8.GetString(body);
                        _logger.LogInformation($"Processing: {message}");

                        // Simulate work
                        await Task.Delay(1000, stoppingToken);

                        // Acknowledge message only after successful processing
                        _channel.BasicAck(result.DeliveryTag, multiple: false);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing message");
                        // Negative acknowledgement to requeue or move to dead letter
                        _channel.BasicNack(result.DeliveryTag, multiple: false, requeue: true);
                    }
                }
                else
                {
                    // No message in queue. Wait a bit before polling again to save CPU
                    // This is important when scaling down to 0 to avoid tight loops during shutdown
                    await Task.Delay(100, stoppingToken);
                }
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Worker is stopping. Completing in-flight work...");
            
            // Close connections gracefully
            _channel.Close();
            _connection.Close();
            
            await base.StopAsync(cancellationToken);
        }
    }
}
