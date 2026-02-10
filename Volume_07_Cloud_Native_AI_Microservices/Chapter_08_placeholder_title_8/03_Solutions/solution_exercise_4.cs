
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

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Prometheus;
using StackExchange.Redis; // Assuming Redis client

public class QueueMetricsPublisher : BackgroundService
{
    private static readonly Gauge WeightedLoad = Metrics.CreateGauge("inference_queue_weighted_load", "Weighted load of the inference queue");
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<QueueMetricsPublisher> _logger;

    public QueueMetricsPublisher(IConnectionMultiplexer redis, ILogger<QueueMetricsPublisher> logger)
    {
        _redis = redis;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var db = _redis.GetDatabase();
                // Get all items in the inference_queue (assuming list structure)
                // In production, use SCAN to avoid blocking, or Lua script for efficiency
                var items = await db.ListRangeAsync("inference_queue");

                double weightedLoad = 0;
                
                // Calculate weighted load: 1 point per 100 characters
                foreach (var item in items)
                {
                    // Assuming item is a JSON string with a 'text' property
                    // Simplified parsing for example:
                    var textLength = item.HasValue ? item.ToString().Length : 0;
                    weightedLoad += (textLength / 100.0);
                }

                // Publish metric
                WeightedLoad.Set(weightedLoad);
                _logger.LogInformation($"Current weighted load: {weightedLoad}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating queue metrics");
            }

            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
        }
    }
}
