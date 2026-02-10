
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

# Source File: solution_exercise_4.cs
# Description: Solution for Exercise 4
# ==========================================

using System;
using System.Collections.Generic;
using System.Linq;

public class InferenceAutoscaler
{
    private DateTime _lastScaleUpTime = DateTime.MinValue;
    private readonly TimeSpan _stabilizationWindow = TimeSpan.FromMinutes(5);
    
    // For Predictive Scaling
    private Queue<double> _metricHistory = new Queue<double>();

    public int CalculateDesiredReplicas(int currentReplicas, double gpuUsagePercent, double requestLatencyMs, Queue<double>? historicalMetrics = null)
    {
        // 1. Scale to Zero Logic
        if (requestLatencyMs == 0 && currentReplicas > 0)
        {
            Console.WriteLine("Scaling to 0 (No traffic)");
            return 0;
        }

        // 2. Cold Start Logic
        if (currentReplicas == 0 && requestLatencyMs > 0)
        {
            Console.WriteLine("Cold Start detected: Scaling immediately to 1");
            return 1;
        }

        // 3. Predictive Scaling Extension
        int predictiveAdjustment = 0;
        if (historicalMetrics != null && historicalMetrics.Count >= 10)
        {
            predictiveAdjustment = CalculatePredictiveAdjustment(historicalMetrics);
        }

        // 4. Standard Scaling Logic
        int desiredReplicas = currentReplicas;

        // Scale Up Condition
        if (gpuUsagePercent > 80 && requestLatencyMs > 100)
        {
            // Check Stabilization Window
            if (DateTime.UtcNow - _lastScaleUpTime > _stabilizationWindow)
            {
                desiredReplicas = (int)Math.Ceiling(currentReplicas * 1.2); // +20%
                _lastScaleUpTime = DateTime.UtcNow;
                Console.WriteLine($"Scaling UP: {currentReplicas} -> {desiredReplicas} (High GPU/Latency)");
            }
            else
            {
                Console.WriteLine("Scale Up suppressed due to stabilization window.");
            }
        }
        // Scale Down Condition
        else if (gpuUsagePercent < 30 && requestLatencyMs < 50)
        {
            desiredReplicas = (int)Math.Floor(currentReplicas * 0.9); // -10%
            if (desiredReplicas < 1) desiredReplicas = 1;
            Console.WriteLine($"Scaling DOWN: {currentReplicas} -> {desiredReplicas} (Low Load)");
        }

        // Apply Predictive Adjustment (if calculated)
        if (predictiveAdjustment > 0)
        {
            // Ensure we don't exceed a max capacity logic (simulated as 2x current or similar)
            int predictiveTarget = desiredReplicas + predictiveAdjustment;
            if (predictiveTarget > desiredReplicas)
            {
                Console.WriteLine($"Predictive Scaling Override: Increasing replicas to {predictiveTarget} due to volatility.");
                return predictiveTarget;
            }
        }

        return desiredReplicas;
    }

    private int CalculatePredictiveAdjustment(Queue<double> metrics)
    {
        // Calculate Standard Deviation of last 10 points
        double[] last10 = metrics.ToArray().TakeLast(10).ToArray();
        double average = last10.Average();
        double sumOfSquares = last10.Sum(val => Math.Pow(val - average, 2));
        double stdDev = Math.Sqrt(sumOfSquares / last10.Length);

        // If standard deviation is high (e.g., > 15), traffic is bursty
        if (stdDev > 15)
        {
            // Proactively increase by 10% (rounded up)
            return (int)Math.Ceiling(last10.Average() * 0.10); 
        }
        return 0;
    }

    // Helper to feed metrics for the simulation
    public void RecordMetric(double value)
    {
        _metricHistory.Enqueue(value);
        if (_metricHistory.Count > 60) _metricHistory.Dequeue(); // Keep last 60 points
    }
}
