
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

// Source File: theory_theoretical_foundations_part1.cs
// Description: Theoretical Foundations
// ==========================================

using System.Diagnostics;
using System.Diagnostics.Metrics;

// The meter is the source of truth for the agent's health.
public class InferenceMetrics
{
    private static readonly Meter _meter = new("AI.Agent.Inference");
    
    // Tracks the latency of generating a response.
    private static readonly Histogram<double> _generationLatency = _meter.CreateHistogram<double>(
        "agent.generation.latency.ms", 
        "ms", 
        "Time taken to generate a response");

    // Tracks the queue depth (how many requests are waiting for the GPU).
    private static readonly ObservableGauge<int> _queueDepth = _meter.CreateObservableGauge<int>(
        "agent.queue.depth",
        () => RequestQueue.Count, // Callback to read current queue size
        "requests",
        "Number of requests waiting for inference");

    public void RecordLatency(double latencyMs)
    {
        _generationLatency.Record(latencyMs);
    }
}
