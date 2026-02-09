
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

# Source File: solution_exercise_27.cs
# Description: Solution for Exercise 27
# ==========================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// 1. Energy Efficient Indexing
public class EnergyAwareIndexer
{
    // Use IVF (Inverted File Index) rather than HNSW for lower memory/energy during build
    // HNSW is faster for query but expensive to build.
    
    public async Task BuildIndexAsync(List<Document> documents)
    {
        // 1. Scheduling System (Off-Peak)
        var localTime = DateTime.Now;
        if (localTime.Hour < 6 || localTime.Hour > 22)
        {
            // Perform heavy indexing
            await PerformIVFIndexing(documents);
        }
        else
        {
            // Queue for later
            await QueueForOffPeak(documents);
        }
    }

    private async Task PerformIVFIndexing(List<Document> docs)
    {
        // Simulate energy-efficient clustering (IVF)
        // Fewer distance calculations than exhaustive search
        await Task.Delay(100);
    }

    private async Task QueueForOffPeak(List<Document> docs) { /* Logic */ }
}

// 2. Energy Metrics
public class EnergyMetrics
{
    // Rough estimation based on FLOPs (Floating Point Operations)
    public double EstimateFlops(float[] a, float[] b)
    {
        // Dot product: N multiplications + N additions
        return a.Length * 2; 
    }

    public double EstimateEnergyConsumption(double flops)
    {
        // Approximate Joules (assuming specific hardware efficiency)
        // This is highly hardware dependent
        return flops * 1e-9; 
    }
}

// 3. Pruning Strategy (Low Importance)
public class GreenVectorService
{
    private readonly EnergyMetrics _metrics;

    public async Task ProcessBatchAsync(List<Document> docs)
    {
        // Calculate "Importance Score" (e.g., based on access frequency)
        var prioritized = docs.OrderByDescending(d => d.AccessCount).ToList();

        // Process top 80% (Vital)
        await IndexDocuments(prioritized.Take((int)(docs.Count * 0.8)).ToList());

        // Skip bottom 20% (Low importance) to save energy
        // Or process them with lower precision
    }

    private async Task IndexDocuments(List<Document> docs)
    {
        // Index logic
    }
}
