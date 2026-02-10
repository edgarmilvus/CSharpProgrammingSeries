
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

using System.Collections.Concurrent;
using System.Diagnostics;

// 1. METRICS AGGREGATOR
public class MetricsAggregator
{
    private readonly ConcurrentQueue<(DateTime Time, double Latency, int QueueDepth)> _metrics = new();
    private readonly TimeSpan _window = TimeSpan.FromSeconds(10);

    public void AddMetric(double latency, int queueDepth)
    {
        _metrics.Enqueue((DateTime.UtcNow, latency, queueDepth));
        
        // Clean up old data
        while (_metrics.TryPeek(out var oldest) && DateTime.UtcNow - oldest.Time > _window)
        {
            _metrics.TryDequeue(out _);
        }
    }

    public (double AvgLatency, int AvgQueueDepth) GetAverages()
    {
        if (_metrics.IsEmpty) return (0, 0);
        
        var items = _metrics.ToArray();
        double avgLatency = items.Average(i => i.Latency);
        int avgQueue = (int)items.Average(i => i.QueueDepth);
        return (avgLatency, avgQueue);
    }
}

// 2. STRATEGY PATTERN FOR STATES
public interface IScalingState
{
    IScalingState CheckState(AutoScaler scaler, MetricsAggregator metrics);
}

public class StabilizingState : IScalingState
{
    private int _stableChecks = 0;

    public IScalingState CheckState(AutoScaler scaler, MetricsAggregator metrics)
    {
        var (latency, queue) = metrics.GetAverages();

        if (queue > 10 || latency > 200)
        {
            _stableChecks = 0;
            return new ScalingOutState();
        }

        if (queue < 2 && latency < 50)
        {
            _stableChecks++;
            if (_stableChecks >= 6) // 6 checks * 5 seconds = 30 seconds
            {
                return new ScalingInState();
            }
        }
        else
        {
            _stableChecks = 0;
        }

        return this;
    }
}

public class ScalingOutState : IScalingState
{
    public IScalingState CheckState(AutoScaler scaler, MetricsAggregator metrics)
    {
        if (scaler.CurrentReplicas < scaler.MaxReplicas)
        {
            scaler.CurrentReplicas++;
            Console.WriteLine($"[SCALE OUT] Replicas increased to {scaler.CurrentReplicas}");
        }
        return new StabilizingState();
    }
}

public class ScalingInState : IScalingState
{
    public IScalingState CheckState(AutoScaler scaler, MetricsAggregator metrics)
    {
        if (scaler.CurrentReplicas > scaler.MinReplicas)
        {
            scaler.CurrentReplicas--;
            Console.WriteLine($"[SCALE IN] Replicas decreased to {scaler.CurrentReplicas}");
        }
        return new StabilizingState();
    }
}

// 3. AUTO SCALER CONTROLLER
public class AutoScaler
{
    private readonly MetricsAggregator _metrics;
    private IScalingState _currentState = new StabilizingState();
    
    public int CurrentReplicas { get; set; } = 1;
    public int MaxReplicas { get; } = 10;
    public int MinReplicas { get; } = 1;

    public AutoScaler(MetricsAggregator metrics)
    {
        _metrics = metrics;
    }

    public void CheckScaling()
    {
        // Delegate decision to current state
        _currentState = _currentState.CheckState(this, _metrics);
    }

    public IEnumerable<string> GetReplicaEndpoints()
    {
        // Return dynamic list of endpoints based on replica count
        for (int i = 0; i < CurrentReplicas; i++)
        {
            yield return $"http://agent-pod-{i}.local";
        }
    }
}

// 4. LOAD BALANCER INTEGRATION (Simulated)
public class LoadBalancer
{
    private readonly AutoScaler _autoScaler;
    private int _roundRobinIndex = 0;

    public LoadBalancer(AutoScaler autoScaler)
    {
        _autoScaler = autoScaler;
    }

    public string RouteRequest()
    {
        var endpoints = _autoScaler.GetReplicaEndpoints().ToList();
        if (!endpoints.Any()) return "No replicas available";

        // Simple Round Robin
        var endpoint = endpoints[_roundRobinIndex % endpoints.Count];
        Interlocked.Increment(ref _roundRobinIndex);
        return endpoint;
    }
}
