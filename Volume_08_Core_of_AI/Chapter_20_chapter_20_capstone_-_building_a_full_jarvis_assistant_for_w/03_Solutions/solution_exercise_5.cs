
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

# Source File: solution_exercise_5.cs
# Description: Solution for Exercise 5
# ==========================================

// Project: Jarvis.ProactiveService.csproj
// Add reference to System.Diagnostics.PerformanceCounter

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Jarvis.ProactiveService
{
    // 1. Proactive Plugin
    public class ProactivePlugin
    {
        private readonly NotificationService _notificationService;

        public ProactivePlugin(NotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [KernelFunction("offer_assistance")]
        public async Task<string> OfferAssistance(string context)
        {
            // In a real scenario, we would call an LLM here to generate a witty or helpful message.
            // For this exercise, we simulate the LLM response.
            string message = $"System Alert: {context}. Would you like me to close some applications?";
            
            _notificationService.ShowToast("Jarvis Monitor", message);
            return message;
        }
    }

    // Mock Notification Service (from Exercise 1)
    public class NotificationService
    {
        public void ShowToast(string title, string message) 
            => Console.WriteLine($"[TOAST] {title}: {message}");
    }

    public class ProactiveWorker : BackgroundService
    {
        private readonly ILogger<ProactiveWorker> _logger;
        private readonly Kernel _kernel;
        private readonly NotificationService _notificationService;
        
        // State Management
        private DateTime _lastNotificationTime = DateTime.MinValue;
        private readonly TimeSpan _cooldown = TimeSpan.FromMinutes(15);
        private readonly object _lock = new object();

        public ProactiveWorker(ILogger<ProactiveWorker> logger)
        {
            _logger = logger;
            _notificationService = new NotificationService();

            var builder = Kernel.CreateBuilder();
            // builder.AddAzureOpenAIChatCompletion(...);
            _kernel = builder.Build();
            
            // Inject service into plugin
            _kernel.ImportPluginFromObject(new ProactivePlugin(_notificationService), "Monitor");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Use PeriodicTimer for modern .NET async waiting
            using var timer = new PeriodicTimer(TimeSpan.FromSeconds(10)); // Check every 10 seconds

            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                await CheckSystemMetrics();
            }
        }

        private async Task CheckSystemMetrics()
        {
            float cpuUsage = GetCpuUsage();

            if (cpuUsage > 80) // Threshold
            {
                bool shouldNotify = false;
                
                // Thread-safe check for throttle
                lock (_lock)
                {
                    if (DateTime.Now - _lastNotificationTime > _cooldown)
                    {
                        shouldNotify = true;
                        _lastNotificationTime = DateTime.Now;
                    }
                }

                if (shouldNotify)
                {
                    _logger.LogInformation("High CPU detected ({CPU}%). Triggering proactive assistance.", cpuUsage);
                    
                    try
                    {
                        // Invoke Kernel to handle the logic
                        var result = await _kernel.InvokeAsync("Monitor", "offer_assistance", 
                            new KernelArguments($"High CPU Usage: {cpuUsage}%"));
                            
                        _logger.LogInformation("Proactive action completed: {Result}", result);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to execute proactive assistance.");
                    }
                }
                else
                {
                    _logger.LogDebug("High CPU detected but throttled. Cooldown active.");
                }
            }
        }

        private float GetCpuUsage()
        {
            // Using PerformanceCounter to get CPU load
            // Note: This requires initialization time to get a valid reading.
            try
            {
                using var pc = new PerformanceCounter("Processor", "% Processor Time", "_Total");
                pc.NextValue(); // Call once to initialize
                Thread.Sleep(1000); // Wait for accurate reading
                return pc.NextValue();
            }
            catch
            {
                return 0; // Fallback if counter unavailable
            }
        }
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            IHost host = Host.CreateDefaultBuilder(args)
                .UseWindowsService(options => options.ServiceName = "Jarvis Proactive Monitor")
                .ConfigureServices(services => services.AddHostedService<ProactiveWorker>())
                .Build();

            host.Run();
        }
    }
}
