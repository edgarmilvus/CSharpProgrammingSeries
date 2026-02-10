
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

// Source File: solution_exercise_1.cs
// Description: Solution for Exercise 1
// ==========================================

// Using RabbitMQ.Client library
public class PreprocessingAgent
{
    public void ProcessAndPublish(string data)
    {
        var factory = new ConnectionFactory() { HostName = "rabbitmq-service" };
        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();
        
        channel.QueueDeclare(queue: "preprocessing-output", durable: true, exclusive: false, autoDelete: false, arguments: null);
        
        var processedData = HeavyProcessing(data);
        var body = Encoding.UTF8.GetBytes(processedData);
        
        channel.BasicPublish(exchange: "", routingKey: "preprocessing-output", basicProperties: null, body: body);
    }
}

public class InferenceAgent
{
    public void ConsumeAndInfer()
    {
        var factory = new ConnectionFactory() { HostName = "rabbitmq-service" };
        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();

        var consumer = new EventingBasicConsumer(channel);
        consumer.Received += (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            
            // Perform Inference
            var result = RunInference(message);
            
            // Publish to next queue
            channel.BasicPublish(exchange: "", routingKey: "inference-output", body: Encoding.UTF8.GetBytes(result));
            
            channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
        };
        
        channel.BasicConsume(queue: "preprocessing-output", autoAck: false, consumer: consumer);
    }
}
