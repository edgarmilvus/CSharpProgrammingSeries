
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

using System.Diagnostics;
using OpenTelemetry;
using OpenTelemetry.Trace;

public static class AiTelemetry
{
    // 1. Define a custom ActivitySource
    private static readonly ActivitySource Source = new ActivitySource("AiAgent.Inference");

    public static async Task RunInferenceWithTracing()
    {
        // 2. Start the Root Activity (Represents the full pipeline)
        using var rootActivity = Source.StartActivity("InferencePipeline");
        
        if (rootActivity != null)
        {
            // Add global tags (Attributes)
            rootActivity.SetTag("agent.id", "agent-01");
            rootActivity.SetTag("request.type", "image_classification");
        }

        try
        {
            // 3. Simulate Batching (Child Span)
            using (var batchSpan = Source.StartActivity("Batch.Buffer"))
            {
                batchSpan?.SetTag("batch.size", 32);
                batchSpan?.SetTag("buffer.latency_ms", 10);
                
                // Simulate I/O wait
                await Task.Delay(10); 
            }

            // 4. Simulate Model Execution (Child Span)
            using (var modelSpan = Source.StartActivity("Model.Inference"))
            {
                modelSpan?.SetTag("model.id", "resnet50");
                modelSpan?.SetTag("gpu.utilization", "85%");
                
                // Simulate Compute
                await Task.Delay(50);

                // 5. Error Handling Simulation
                // 10% chance of failure
                if (new Random().Next(0, 10) == 0)
                {
                    // Record exception event
                    modelSpan?.RecordException(new InvalidOperationException("GPU OOM"));
                    
                    // Set status to Error
                    modelSpan?.SetStatus(ActivityStatusCode.Error, "Inference failed due to OOM");
                    
                    throw new InvalidOperationException("Inference failed");
                }
                
                modelSpan?.SetStatus(ActivityStatusCode.Ok);
            }
        }
        catch (Exception ex)
        {
            // Record error on root span if not already recorded
            rootActivity?.RecordException(ex);
            rootActivity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            throw;
        }
    }

    // 6. Visualization Helper
    public static void PrintTraceTree(Activity root)
    {
        Console.WriteLine($"Trace: {root.TraceId}");
        PrintNode(root, 0);
    }

    private static void PrintNode(Activity activity, int depth)
    {
        var indent = new string(' ', depth * 2);
        var status = activity.Status == ActivityStatusCode.Error ? " [ERROR]" : "";
        Console.WriteLine($"{indent}{activity.DisplayName} ({activity.Duration.TotalMilliseconds}ms){status}");
        
        // Tags
        foreach (var tag in activity.TagObjects)
        {
            Console.WriteLine($"{indent}  - {tag.Key}: {tag.Value}");
        }

        // Events (Exceptions)
        foreach (var ev in activity.Events)
        {
            Console.WriteLine($"{indent}  ! Event: {ev.Name}");
        }

        // Recursively print children (Note: Activity.Current is thread static, 
        // so we rely on the stored relationship in a real app, 
        // but here we simulate hierarchy via the object model if available)
        // In a real distributed trace, we would query a backend like Jaeger.
    }
}

// SETUP AND USAGE
public class Program
{
    public static async Task Main()
    {
        // Configure OpenTelemetry (Console Exporter)
        using var tracerProvider = Sdk.CreateTracerProviderBuilder()
            .AddSource("AiAgent.Inference")
            .AddConsoleExporter() // Outputs to Debug/Console
            .Build();

        try
        {
            await AiTelemetry.RunInferenceWithTracing();
        }
        catch (Exception)
        {
            // Expected simulated error
        }
    }
}
