
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

# Source File: solution_exercise_6.cs
# Description: Solution for Exercise 6
# ==========================================

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Bulkhead;
using Polly.CircuitBreaker;
using Polly.Timeout;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

public class ResilientModelLoader
{
    private readonly ResiliencePipeline _criticalPipeline;
    private readonly ResiliencePipeline _optionalPipeline;
    private readonly ILogger<ResilientModelLoader> _logger;

    public ResilientModelLoader(ILogger<ResilientModelLoader> logger)
    {
        _logger = logger;

        // 1. Critical Pipeline: Timeout -> Retry -> Circuit Breaker
        _criticalPipeline = new ResiliencePipelineBuilder()
            .AddTimeout(TimeSpan.FromSeconds(30))
            .AddRetry(new RetryStrategyOptions 
            { 
                ShouldHandle = new PredicateBuilder().Handle<HttpRequestException>(),
                MaxRetryAttempts = 3,
                BackoffType = DelayBackoffType.Exponential
            })
            .AddCircuitBreaker(new CircuitBreakerStrategyOptions
            {
                ShouldHandle = new PredicateBuilder().Handle<HttpRequestException>(),
                FailureRatio = 1.0, // Open after 5 consecutive failures (default sampling duration)
                MinimumThroughput = 5,
                BreakDuration = TimeSpan.FromSeconds(30)
            })
            .Build();

        // 2. Optional Pipeline: Bulkhead (Limit concurrency)
        _optionalPipeline = new ResiliencePipelineBuilder()
            .AddBulkhead(new BulkheadStrategyOptions
            {
                MaxParallelization = 1, // Only 1 concurrent load
                QueueMaxization = 0     // No queue, fail immediately if busy
            })
            .Build();
    }

    public async Task LoadCriticalModelAsync(CancellationToken token)
    {
        await _criticalPipeline.ExecuteAsync(async ct => 
        {
            await SimulateNetworkLoad(ct);
        }, token);
    }

    public async Task LoadOptionalModelAsync(CancellationToken token)
    {
        try
        {
            await _optionalPipeline.ExecuteAsync(async ct =>
            {
                await SimulateNetworkLoad(ct);
            }, token);
        }
        catch (BulkheadRejectedException)
        {
            _logger.LogWarning("Optional model loading rejected: Bulkhead is full.");
            // Application continues without optional model
        }
    }

    private async Task SimulateNetworkLoad(CancellationToken token)
    {
        // Simulate transient fault
        if (new Random().Next(1, 101) <= 50)
            throw new HttpRequestException("Network unstable");

        await Task.Delay(1000, token);
    }
}
