
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

// Source File: solution_exercise_3.cs
// Description: Solution for Exercise 3
// ==========================================

using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace MultiAgentKafka
{
    public class TaskMessage
    {
        public string TaskId { get; set; }
        public string Payload { get; set; }
    }

    public class ResultMessage
    {
        public string TaskId { get; set; }
        public string Result { get; set; }
        public bool Success { get; set; }
    }

    public class WorkerAgent : BackgroundService
    {
        private readonly ILogger<WorkerAgent> _logger;
        private readonly IConsumer<Null, string> _consumer;
        private readonly IProducer<Null, string> _producer;
        private const string ResultTopic = "agent-results";

        public WorkerAgent(ILogger<WorkerAgent> logger, IConsumer<Null, string> consumer, IProducer<Null, string> producer)
        {
            _logger = logger;
            _consumer = consumer;
            _producer = producer;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Subscribe to the task topic
            _consumer.Subscribe("agent-tasks");

            _logger.LogInformation("Worker Agent started and listening for tasks...");

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        // Poll for messages
                        var consumeResult = _consumer.Consume(stoppingToken);

                        if (consumeResult == null) continue;

                        _logger.LogInformation($"Task received: {consumeResult.Message.Value}");

                        // Deserialize the payload
                        TaskMessage task;
                        try
                        {
                            task = JsonSerializer.Deserialize<TaskMessage>(consumeResult.Message.Value);
                        }
                        catch (JsonException ex)
                        {
                            _logger.LogError(ex, "Failed to deserialize task message. Skipping.");
                            // Commit offset to avoid reprocessing bad message
                            _consumer.Commit(consumeResult);
                            continue;
                        }

                        // Simulate processing work
                        _logger.LogInformation($"Processing task {task.TaskId}...");
                        await Task.Delay(1000, stoppingToken); // Simulate heavy work

                        // Prepare result
                        var result = new ResultMessage
                        {
                            TaskId = task.TaskId,
                            Result = $"Processed: {task.Payload}",
                            Success = true
                        };

                        // Produce result to response topic
                        var jsonResult = JsonSerializer.Serialize(result);
                        await _producer.ProduceAsync(ResultTopic, new Message<Null, string> { Value = jsonResult }, stoppingToken);

                        _logger.LogInformation($"Task {task.TaskId} completed and result sent.");

                        // Commit offset manually (or rely on auto-commit if configured)
                        _consumer.Commit(consumeResult);
                    }
                    catch (ConsumeException ex) when (ex.Error.IsFatal)
                    {
                        _logger.LogCritical(ex, "Fatal Kafka consumer error. Stopping agent.");
                        break;
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Unexpected error in consumer loop.");
                        // Optional: Wait before retrying to prevent tight loop on persistent errors
                        await Task.Delay(1000, stoppingToken);
                    }
                }
            }
            finally
            {
                _consumer.Close();
                _consumer.Dispose();
            }
        }
    }
}
