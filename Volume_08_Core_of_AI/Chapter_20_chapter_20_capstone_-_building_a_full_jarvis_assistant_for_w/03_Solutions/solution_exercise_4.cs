
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

# Source File: solution_exercise_4.cs
# Description: Solution for Exercise 4
# ==========================================

// Project: Jarvis.BackgroundService.csproj
// Template: dotnet new worker
// Dependencies: Microsoft.Extensions.Hosting, Microsoft.Extensions.Logging, Microsoft.SemanticKernel, Microsoft.Extensions.Hosting.WindowsServices

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Jarvis.BackgroundService
{
    public class EchoPlugin
    {
        [Microsoft.SemanticKernel.KernelFunction("echo")]
        public string Echo(string message) => $"Assistant received: {message}";
    }

    public class JarvisWorker : BackgroundService
    {
        private readonly ILogger<JarvisWorker> _logger;
        private readonly Kernel _kernel;
        private readonly FileSystemWatcher _watcher;
        private readonly string _triggerPath = @"C:\JarvisTriggers";

        public JarvisWorker(ILogger<JarvisWorker> logger)
        {
            _logger = logger;
            
            // Initialize Kernel
            var builder = Kernel.CreateBuilder();
            // builder.AddAzureOpenAIChatCompletion(...); 
            _kernel = builder.Build();
            _kernel.ImportPluginFromObject(new EchoPlugin(), "Assistant");

            // Ensure trigger directory exists
            Directory.CreateDirectory(_triggerPath);

            // Setup File Watcher
            _watcher = new FileSystemWatcher(_triggerPath);
            _watcher.Filter = "*.txt";
            _watcher.Created += OnFileCreated;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Jarvis Background Service started at: {Time}", DateTimeOffset.Now);
            _logger.LogInformation("Listening for triggers in: {Path}", _triggerPath);

            _watcher.EnableRaisingEvents = true;

            // Keep the service alive
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
            }
        }

        private async void OnFileCreated(object sender, FileSystemEventArgs e)
        {
            _logger.LogInformation("Trigger file detected: {File}", e.Name);
            
            // Wait briefly to ensure file is fully written
            await Task.Delay(500);

            try
            {
                string command = await File.ReadAllTextAsync(e.FullPath);
                _logger.LogInformation("Processing command: {Command}", command);

                // Execute non-blocking
                await Task.Run(async () =>
                {
                    try
                    {
                        // Use the Kernel to process the command
                        var result = await _kernel.InvokeAsync("Assistant", "echo", new KernelArguments(command));
                        _logger.LogInformation("Assistant Response: {Result}", result);
                        
                        // Clean up
                        File.Delete(e.FullPath);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing command.");
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading trigger file.");
            }
        }

        public override void Dispose()
        {
            _watcher?.Dispose();
            base.Dispose();
        }
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            IHost host = Host.CreateDefaultBuilder(args)
                .UseWindowsService(options => 
                {
                    options.ServiceName = "Jarvis AI Assistant";
                })
                .ConfigureServices(services =>
                {
                    services.AddHostedService<JarvisWorker>();
                })
                .Build();

            host.Run();
        }
    }
}
