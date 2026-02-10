
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
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;

public class MetricsService : IHostedService
{
    // Thread-safe queue to track active requests
    private static readonly ConcurrentQueue<string> _requestQueue = new();
    private readonly IApplicationBuilder _app;
    private readonly IHostApplicationLifetime _lifetime;

    public MetricsService(IApplicationBuilder app, IHostApplicationLifetime lifetime)
    {
        _app = app;
        _lifetime = lifetime;
    }

    // Helper method to simulate adding requests
    public static void AddRequest(string requestId) => _requestQueue.Enqueue(requestId);
    public static void RemoveRequest() 
    {
        if (_requestQueue.TryDequeue(out _)) { /* Removed */ }
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        // Use the built-in Kestrel server to host the metrics endpoint
        // In a real app, you might use a separate port or middleware pipeline
        var server = new Microsoft.AspNetCore.Hosting.WebHostBuilder()
            .UseKestrel()
            .Configure(appBuilder => 
            {
                appBuilder.Use(async (context, next) =>
                {
                    if (context.Request.Path == "/metrics")
                    {
                        var queueLength = _requestQueue.Count;
                        var sb = new StringBuilder();
                        sb.AppendLine("# HELP inference_queue_length Current number of items in the inference queue");
                        sb.AppendLine("# TYPE inference_queue_length gauge");
                        sb.AppendLine($"inference_queue_length {queueLength}");
                        
                        context.Response.ContentType = "text/plain";
                        await context.Response.WriteAsync(sb.ToString());
                    }
                    else
                    {
                        await next();
                    }
                });
            })
            .UseUrls("http://*:8080") // Expose metrics on port 8080
            .Build();

        // Start the server in a background task
        Task.Run(() => server.RunAsync(cancellationToken));
        
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
