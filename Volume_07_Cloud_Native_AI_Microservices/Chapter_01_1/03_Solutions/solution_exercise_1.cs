
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

# Source File: solution_exercise_1.cs
# Description: Solution for Exercise 1
# ==========================================

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenAI; // Assuming OpenAI package is used
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace AgentContainerization
{
    public class AgentService : IHostedService
    {
        private readonly ILogger<AgentService> _logger;
        private readonly OpenAIClient _client;
        private readonly string _heartbeatPath = "/tmp/agent.heartbeat";

        public AgentService(ILogger<AgentService> logger, OpenAIClient client)
        {
            _logger = logger;
            _client = client;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Agent Service starting...");
            
            // Ensure the directory exists (important for non-root user permissions)
            var dir = Path.GetDirectoryName(_heartbeatPath);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    // Simulate agent processing
                    _logger.LogInformation("Processing cycle...");
                    
                    // Write heartbeat for Docker HEALTHCHECK
                    await File.WriteAllTextAsync(_heartbeatPath, DateTime.UtcNow.ToString("O"), cancellationToken);
                    
                    await Task.Delay(5000, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in agent loop");
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Agent Service stopping...");
            return Task.CompletedTask;
        }
    }

    public class Program
    {
        public static async Task Main(string[] args)
        {
            var host = Host.CreateDefaultBuilder(args)
                .ConfigureServices((context, services) =>
                {
                    // Configuration for OpenAI
                    var apiKey = context.Configuration["OpenAI:ApiKey"];
                    if (!string.IsNullOrEmpty(apiKey))
                    {
                        services.AddSingleton(new OpenAIClient(apiKey));
                    }
                    else
                    {
                        // Fallback for local dev if secret is missing
                        services.AddSingleton(new OpenAIClient("fake-key-for-dev"));
                    }
                    
                    services.AddHostedService<AgentService>();
                })
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddConsole();
                })
                .Build();

            await host.RunAsync();
        }
    }
}
