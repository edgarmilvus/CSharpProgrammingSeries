
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

// Add NuGet package: prometheus-net.AspNetCore

using Prometheus;

var builder = WebApplication.CreateBuilder(args);

// ... existing setup ...

var app = builder.Build();

// Define a Gauge metric for queue length
var queueLength = Metrics.CreateGauge("inference_queue_length", "Number of requests currently being processed");

// Expose /metrics endpoint on a separate port (9090)
var metricServer = new MetricServer(port: 9090);
metricServer.Start();

app.MapPost("/analyze", async (HttpContext context) =>
{
    // Increment queue length when request starts
    queueLength.Inc();

    try
    {
        var request = await context.Request.ReadFromJsonAsync<AnalysisRequest>();
        // Simulate processing time
        await Task.Delay(100); 
        
        var result = AnalyzeSentiment(request.Text);
        return Results.Ok(result);
    }
    finally
    {
        // Decrement queue length when request finishes (even on error)
        queueLength.Dec();
    }
});

app.Run();
