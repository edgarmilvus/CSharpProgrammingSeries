
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

// File: metric.proto
syntax = "proto3";
package Metrics.V1;
option csharp_namespace = "Metrics.V1";

message MetricUpdate {
  string node_id = 1;
  float gpu_temp = 2;
  float vram_usage = 3;
  int64 timestamp = 4;
}

message AggregationResult {
  float avg_gpu_temp = 1;
  float max_vram_usage = 2;
  int32 metrics_count = 3;
}

service MetricsAggregator {
  // New method using Client-Side Streaming
  rpc ReportMetrics (stream MetricUpdate) returns (AggregationResult);
}

// File: Client/MetricClient.cs
using Grpc.Core;
using Metrics.V1;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public class MetricClient
{
    public async Task StreamMetricsWithRetry(Channel channel)
    {
        int retryCount = 0;
        const int maxRetries = 3;

        while (retryCount < maxRetries)
        {
            try
            {
                var client = new MetricsAggregator.MetricsAggregatorClient(channel);
                
                // Using C# 8.0 IAsyncEnumerable to stream data
                // This allows us to yield metrics as they are generated without buffering everything in memory
                var streamingCall = client.ReportMetrics(cancellationToken: CancellationToken.None);

                // Start the background task to read the response (which comes only at the end)
                var responseTask = Task.Run(async () =>
                {
                    var response = await streamingCall.ResponseAsync;
                    Console.WriteLine($"\n[Client] Aggregation Received: Avg Temp: {response.AvgGpuTemp}, Count: {response.MetricsCount}");
                });

                // Simulate streaming metrics
                Console.Write("[Client] Streaming metrics");
                for (int i = 0; i < 10; i++)
                {
                    // Simulate network failure on the 5th item for demonstration
                    if (i == 5 && retryCount == 0)
                    {
                        Console.WriteLine("\n[Client] SIMULATING NETWORK FAILURE...");
                        // In a real scenario, the connection drops here.
                        // We throw an exception to trigger the retry logic.
                        throw new RpcException(Status.DefaultCancelled, "Simulated Network Failure");
                    }

                    var metric = new MetricUpdate
                    {
                        NodeId = "Node-A",
                        GpuTemp = 60 + i,
                        VramUsage = 8000 + (i * 100),
                        Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                    };

                    await streamingCall.RequestStream.WriteAsync(metric);
                    Console.Write("."); // Visualize the stream

                    // Small delay to simulate real-time collection
                    await Task.Delay(100);
                }

                // Complete the stream
                await streamingCall.RequestStream.CompleteAsync();

                // Wait for the aggregation response
                await responseTask;
                
                // Success, exit retry loop
                break; 
            }
            catch (RpcException ex)
            {
                retryCount++;
                Console.WriteLine($"\n[Client] Error: {ex.Status.Detail}. Retry {retryCount}/{maxRetries}");
                
                if (retryCount >= maxRetries) 
                {
                    Console.WriteLine("[Client] Max retries reached. Giving up.");
                    throw;
                }

                // Exponential backoff
                await Task.Delay(1000 * retryCount);
            }
        }
    }
}

// File: Server/MetricServer.cs
using Grpc.Core;
using Metrics.V1;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class MetricServerImpl : MetricsAggregator.MetricsAggregatorBase
{
    public override async Task<AggregationResult> ReportMetrics(
        IAsyncStreamReader<MetricUpdate> requestStream, 
        ServerCallContext context)
    {
        var metrics = new List<MetricUpdate>();

        // Read the stream until the client closes it
        await foreach (var metric in requestStream.ReadAllAsync())
        {
            metrics.Add(metric);
        }

        // Perform Aggregation
        if (metrics.Count == 0) return new AggregationResult();

        return new AggregationResult
        {
            AvgGpuTemp = metrics.Average(m => m.GpuTemp),
            MaxVramUsage = metrics.Max(m => m.VramUsage),
            MetricsCount = metrics.Count
        };
    }
}
