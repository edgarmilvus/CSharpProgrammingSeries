
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

# Source File: solution_exercise_2.cs
# Description: Solution for Exercise 2
# ==========================================

using System.Diagnostics;
using System.Diagnostics.Metrics;
using Microsoft.Extensions.Hosting;

public class MetricsService : IHostedService
{
    private readonly Meter _meter;
    private readonly ObservableGauge<double> _queueGauge;
    private double _currentQueueLength = 0;

    public MetricsService()
    {
        // 1. Create a Meter
        _meter = new Meter("InferenceService", "1.0.0");

        // 2. Create a Counter for total requests
        var counter = _meter.CreateCounter<int>("inference_requests_total");

        // 3. Create an ObservableGauge for queue length
        _queueGauge = _meter.CreateObservableGauge<double>("inference_queue_length", 
            () => _currentQueueLength, 
            "Current number of items waiting for inference");

        // Simulate queue activity
        Task.Run(async () => 
        {
            while(true) 
            {
                // In a real app, this would be updated by the API controller
                _currentQueueLength = new Random().NextDouble() * 15; 
                await Task.Delay(1000);
            }
        });
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        // OpenTelemetry SDK configuration would typically be in Program.cs
        // This exposes the metrics via an endpoint (e.g., /metrics) if OTel exporter is configured
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
