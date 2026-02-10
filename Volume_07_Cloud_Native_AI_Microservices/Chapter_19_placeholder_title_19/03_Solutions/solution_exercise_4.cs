
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

using Confluent.Kafka;
using System;
using System.Threading;

public class KafkaInferenceConsumer
{
    public void Consume()
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = "localhost:9092",
            GroupId = "inference-group",
            AutoOffsetReset = AutoOffsetReset.Latest,
            EnableAutoCommit = false // Manual commit for reliability
        };

        using (var consumer = new ConsumerBuilder<Null, string>(config).Build())
        {
            consumer.Subscribe("inference-requests");

            try
            {
                while (true)
                {
                    // Consume message
                    var consumeResult = consumer.Consume(CancellationToken.None);
                    
                    // Simulate processing
                    Console.WriteLine($"Processing: {consumeResult.Message.Value}");
                    Thread.Sleep(1000); // Simulate expensive inference

                    // Commit offset manually after processing
                    consumer.Commit(consumeResult);
                }
            }
            catch (OperationCanceledException)
            {
                // Close consumer
                consumer.Close();
            }
        }
    }
}
