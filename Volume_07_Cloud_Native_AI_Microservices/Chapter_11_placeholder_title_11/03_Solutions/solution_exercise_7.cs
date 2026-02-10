
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

// Source File: solution_exercise_7.cs
// Description: Solution for Exercise 7
// ==========================================

using System.Diagnostics;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Observability.Tracing
{
    // 1. Custom Activity Source
    public static class InferenceActivitySource
    {
        public static readonly ActivitySource Source = new("AI.InferenceEngine", "1.0.0");
    }

    public class TracingSetup
    {
        public static void ConfigurePipeline(WebApplication app)
        {
            // 2. OpenTelemetry Configuration
            app.UseOpenTelemetryTracing(tracerProviderBuilder =>
            {
                tracerProviderBuilder
                    .AddSource("AI.InferenceEngine") // Subscribe to our custom source
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddJaegerExporter(); // Or Zipkin/OTLP
            });
        }
    }

    public class InferenceService
    {
        public async Task<byte[]> RunInferenceAsync(byte[] input)
        {
            // 3. Instrumentation: Create a Custom Activity
            using var activity = InferenceActivitySource.Source.StartActivity("GPU: Model Inference");

            if (activity != null)
            {
                // Add Tags (Key-Value pairs for filtering)
                activity.SetTag("model.name", "StableDiffusion-v1.5");
                activity.SetTag("batch.size", 1);
                activity.SetTag("gpu.device.id", "cuda:0");

                // Simulate Cache Hit/Miss logic
                bool isCacheHit = true; 
                if (isCacheHit)
                {
                    // Add Event (Timeline annotation)
                    activity.AddEvent(new ActivityEvent("ModelWeightsLoadedFromCache"));
                }
                else
                {
                    activity.AddEvent(new ActivityEvent("ModelWeightsDownloaded"));
                }
            }

            // Simulate GPU Kernel execution
            await Task.Delay(200); 

            return input;
        }
    }
}
