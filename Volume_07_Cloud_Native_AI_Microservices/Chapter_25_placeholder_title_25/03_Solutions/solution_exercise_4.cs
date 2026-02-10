
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

using System;
using System.Collections.Concurrent;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;

namespace MetricsExporter
{
    // 2. Mock Shared Queue (In production, this would be Redis or SQL)
    public static class SharedTaskQueue
    {
        public static ConcurrentQueue<string> Tasks = new ConcurrentQueue<string>();
    }

    // Custom Metrics Exporter Background Service
    public class MetricsExporterService : BackgroundService
    {
        private readonly ILogger<MetricsExporterService> _logger;

        public MetricsExporterService(ILogger<MetricsExporterService> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Metrics Exporter Service started.");

            // In a real scenario, this service would also populate the queue or 
            // simply read from an external store. 
            // Here we just monitor the static queue.
            
            while (!stoppingToken.IsCancellationRequested)
            {
                // Calculate metric
                int queueDepth = SharedTaskQueue.Tasks.Count;
                
                // Expose metric (Logic handled in the HTTP Endpoint below)
                // In a full implementation, we might push this to Prometheus PushGateway 
                // or simply let Prometheus scrape this service.
                
                await Task.Delay(5000, stoppingToken); // Update frequency
            }
        }
    }

    public class Startup
    {
        public void Configure(IApplicationBuilder app)
        {
            // 2. Expose Prometheus-compatible metrics endpoint
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/metrics", async context =>
                {
                    var depth = SharedTaskQueue.Tasks.Count;
                    
                    // Prometheus text format
                    var sb = new StringBuilder();
                    sb.AppendLine("# HELP agent_queue_depth The number of pending tasks for the agent swarm");
                    sb.AppendLine("# TYPE agent_queue_depth gauge");
                    sb.AppendLine($"agent_queue_depth {depth}");

                    context.Response.ContentType = "text/plain; version=0.0.4";
                    await context.Response.WriteAsync(sb.ToString());
                });
            });
        }
    }
}
