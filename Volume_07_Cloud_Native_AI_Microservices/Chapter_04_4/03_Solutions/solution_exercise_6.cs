
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

using System.Diagnostics;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

// 1. OpenTelemetry Setup
public static TracerProvider ConfigureTracing()
{
    return Sdk.CreateTracerProviderBuilder()
        .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("InferenceService"))
        .AddSource("InferenceService") // Source name matches the ActivitySource
        .AddHttpClientInstrumentation() // Auto-instrument HTTP clients
        .AddGrpcClientInstrumentation() // Auto-instrument gRPC clients
        .AddConsoleExporter() // For debugging
        // .AddJaegerExporter() // For production
        .Build();
}

public class InferenceService
{
    private static readonly ActivitySource ActivitySource = new("InferenceService");
    private readonly ILogger<InferenceService> _logger;

    public async Task<string> ProcessAsync(string input)
    {
        // 2. Custom Spans
        using var activity = ActivitySource.StartActivity("InferencePipeline");

        // Pre-processing Span
        using (var preProcActivity = ActivitySource.StartActivity("PreProcessingSpan"))
        {
            _logger.LogInformation("Starting pre-processing", 
                new { InputLength = input.Length, ModelVersion = "v2" });
            // Tokenization logic...
        }

        // Inference Span
        using (var inferenceActivity = ActivitySource.StartActivity("InferenceSpan"))
        {
            // Simulate GPU work
            await Task.Delay(100); 
            _logger.LogInformation("GPU Inference complete");
        }

        // Post-processing Span
        using (var postProcActivity = ActivitySource.StartActivity("PostProcessingSpan"))
        {
            // Decoding logic...
        }

        return "Result";
    }
}
