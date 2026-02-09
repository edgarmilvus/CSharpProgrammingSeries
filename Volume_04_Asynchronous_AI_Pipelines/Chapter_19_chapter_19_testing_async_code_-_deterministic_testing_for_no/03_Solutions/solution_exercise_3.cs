
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

# Source File: solution_exercise_3.cs
# Description: Solution for Exercise 3
# ==========================================

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Xunit;

namespace AsyncTestingSolutions
{
    public record Metric(string Name, double Value, DateTime Timestamp);

    public class MetricsStreamService
    {
        private readonly ChannelReader<Metric> _reader;

        public MetricsStreamService(ChannelReader<Metric> reader)
        {
            _reader = reader;
        }

        public IAsyncEnumerable<Metric> GetMetricsStream(CancellationToken token = default)
        {
            return _reader.ReadAllAsync(token);
        }
    }

    public class MetricsAnalyzer
    {
        public async IAsyncEnumerable<string> AnalyzeStream(IAsyncEnumerable<Metric> metrics, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken token)
        {
            int consecutiveHighLatency = 0;
            var startTime = DateTime.MinValue;

            await foreach (var metric in metrics.WithCancellation(token))
            {
                if (metric.Name == "Latency" && metric.Value > 500)
                {
                    if (consecutiveHighLatency == 0) startTime = metric.Timestamp;
                    
                    consecutiveHighLatency++;
                    
                    // Check if 5 seconds have passed
                    if (consecutiveHighLatency >= 5 && (metric.Timestamp - startTime).TotalSeconds >= 5)
                    {
                        yield return "ALERT: High latency sustained!";
                        consecutiveHighLatency = 0; // Reset for next alert
                    }
                }
                else
                {
                    consecutiveHighLatency = 0;
                }
            }
        }
    }

    public class MetricsIntegrationTests
    {
        [Fact]
        public async Task AnalyzeStream_SustainedHighLatency_TriggersAlert()
        {
            // 4. Simulate stream using Channel
            var channel = Channel.CreateUnbounded<Metric>();
            var writer = channel.Writer;
            var reader = channel.Reader;

            var service = new MetricsStreamService(reader);
            var analyzer = new MetricsAnalyzer();

            // 5. Use TaskCompletionSource to synchronize
            var alertTriggered = new TaskCompletionSource<bool>();

            // Start the consumer
            var consumeTask = Task.Run(async () =>
            {
                var stream = service.GetMetricsStream();
                var alerts = analyzer.AnalyzeStream(stream, CancellationToken.None);

                await foreach (var alert in alerts)
                {
                    if (alert.Contains("ALERT"))
                    {
                        alertTriggered.TrySetResult(true);
                    }
                }
            });

            // Start the producer (injecting data)
            var produceTask = Task.Run(async () =>
            {
                var baseTime = DateTime.UtcNow;
                
                // Inject 5 consecutive high latency metrics
                for (int i = 0; i < 5; i++)
                {
                    // 7. Handle backpressure: WriteAsync respects backpressure if channel is bounded
                    await writer.WriteAsync(new Metric("Latency", 600, baseTime.AddSeconds(i)));
                    await Task.Delay(100); // Small delay to simulate real time
                }
                
                // Inject a low latency to stop the stream eventually
                await writer.WriteAsync(new Metric("Latency", 100, baseTime.AddSeconds(6)));
                writer.Complete();
            });

            // 9. Implement timeout for assertion
            var timeoutTask = Task.Delay(TimeSpan.FromSeconds(10));
            var completedTask = await Task.WhenAny(alertTriggered.Task, timeoutTask);

            Assert.Same(alertTriggered.Task, completedTask); // Verify alert triggered before timeout
            Assert.True(alertTriggered.Task.Result); // Verify the alert was actually set

            await consumeTask; // Clean up
            await produceTask;
        }
    }
}
