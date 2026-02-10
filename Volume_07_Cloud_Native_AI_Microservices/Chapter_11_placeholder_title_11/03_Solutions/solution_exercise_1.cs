
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

// Source File: solution_exercise_1.cs
// Description: Solution for Exercise 1
// ==========================================

using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace SentimentAnalysis.Metrics
{
    /// <summary>
    /// Exports custom metrics for the inference service using System.Diagnostics.Metrics (.NET 6+).
    /// This is compatible with the .NET Prometheus exporter and OpenTelemetry.
    /// </summary>
    public sealed class MetricsExporter : IDisposable
    {
        private readonly Meter _meter;
        private readonly ObservableGauge<int> _gpuStreamsGauge;
        private int _activeStreams;

        public MetricsExporter()
        {
            // Initialize the Meter with a unique name
            _meter = new Meter("SentimentAnalysis.Inference", "1.0.0");

            // Define a Gauge instrument. 
            // The value is observed via a callback function for thread safety.
            _gpuStreamsGauge = _meter.CreateObservableGauge<int>(
                "active_gpu_streams",
                description: "Number of active GPU streams processing inference requests",
                observeValue: () => new Measurement<int>(Interlocked.Read(ref _activeStreams))
            );
        }

        /// <summary>
        /// Increments the active GPU stream count atomically.
        /// Call this when a request starts processing.
        /// </summary>
        public void IncrementActiveStreams()
        {
            Interlocked.Increment(ref _activeStreams);
        }

        /// <summary>
        /// Decrements the active GPU stream count atomically.
        /// Call this when a request finishes processing.
        /// </summary>
        public void DecrementActiveStreams()
        {
            Interlocked.Decrement(ref _activeStreams);
        }

        public void Dispose()
        {
            _meter?.Dispose();
        }
    }
}
