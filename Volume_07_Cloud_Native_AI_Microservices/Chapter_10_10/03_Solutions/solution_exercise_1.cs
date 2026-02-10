
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

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Prometheus;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

// Placeholder for the Inference Request model
public record InferenceRequest(string Id, string InputData);

public class MetricsPublisher : BackgroundService
{
    // Create a Gauge metric. In a real scenario, this might be static or injected via a factory.
    private static readonly Gauge _queueDepth = Metrics.CreateGauge("inference_queue_depth_total", "Current depth of the inference queue");
    
    // We use a ConcurrentQueue as requested by the prompt context
    private readonly ConcurrentQueue<InferenceRequest> _queue;

    public MetricsPublisher(ConcurrentQueue<InferenceRequest> queue)
    {
        _queue = queue;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            // SOLUTION: 
            // ConcurrentQueue.Count is not an atomic operation in older .NET versions, 
            // but in modern .NET it is generally safe for observation. However, to strictly 
            // satisfy the "Thread Safety" requirement and avoid locking the producer:
            // 1. We can use Interlocked.Read if we track count separately, OR
            // 2. Since we cannot lock the ConcurrentQueue (which would block producers),
            //    we rely on the atomic nature of the Count property in modern .NET 
            //    or simply accept the slight drift. 
            //    However, the BEST approach for high-frequency updates without locking 
            //    is to use a counter variable updated via Interlocked, but the prompt 
            //    explicitly asks to scrape the Queue.
            
            // To strictly follow the requirement "scrape an internal ConcurrentQueue" safely:
            // We read the count. ConcurrentQueue.Count is O(N) and involves a snapshot.
            // It is thread-safe (won't crash), but the number might change immediately after reading.
            // This is acceptable for metrics.
            
            int currentDepth = _queue.Count;
            
            // Update the Prometheus gauge
            _queueDepth.Set(currentDepth);

            // Wait for the next scrape interval
            await Task.Delay(5000, stoppingToken);
        }
    }
}

// KUBERNETES HPA MANIFEST (YAML)
// This is the configuration that would utilize the metric above.
/*
apiVersion: autoscaling/v2
kind: HorizontalPodAutoscaler
metadata:
  name: inference-service-hpa
spec:
  scaleTargetRef:
    apiVersion: apps/v1
    kind: Deployment
    name: inference-service
  minReplicas: 2
  maxReplicas: 10
  metrics:
  - type: Pods
    pods:
      metric:
        name: inference_queue_depth_total
      target:
        type: AverageValue
        averageValue: "10" # Target average of 10 queue items per pod
*/
