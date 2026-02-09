
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

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class ModelInstance : IDisposable
{
    private readonly string _name;
    private readonly ILogger _logger;

    public ModelInstance(string name, ILogger logger)
    {
        _name = name;
        _logger = logger;
    }

    public void Dispose()
    {
        _logger.LogInformation("Disposing ModelInstance: {Name} (Releasing GPU Memory)", _name);
        // Simulate unmanaged resource cleanup
        Thread.Sleep(500); 
    }
}

public class ResourceManager : IHostedService, IDisposable
{
    private readonly IHostApplicationLifetime _lifetime;
    private readonly ILogger<ResourceManager> _logger;
    private readonly List<ModelInstance> _instances = new();
    private readonly CancellationTokenSource _shutdownCts = new();

    public ResourceManager(IHostApplicationLifetime lifetime, ILogger<ResourceManager> logger)
    {
        _lifetime = lifetime;
        _logger = logger;
        
        // Register shutdown hook
        _lifetime.ApplicationStopping.Register(OnShutdown);
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        // Load models (simulated)
        _instances.Add(new ModelInstance("ImageModel (High VRAM)", _logger));
        _instances.Add(new ModelInstance("TextModel (Low VRAM)", _logger));
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        // Background service stop logic
        _shutdownCts.Cancel();
        return Task.CompletedTask;
    }

    private void OnShutdown()
    {
        _logger.LogInformation("Shutdown initiated. Releasing resources...");

        // Priority Order: Image Models (High VRAM) first
        // Using LINQ to order by a hypothetical priority property if available
        // Here we hardcode order for demonstration
        
        try
        {
            // Staggered shutdown with timeout
            using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            
            // Simulate ordered disposal
            foreach (var instance in _instances)
            {
                if (timeoutCts.Token.IsCancellationRequested)
                {
                    _logger.LogWarning("Shutdown timeout reached. Skipping remaining resources.");
                    break;
                }
                instance.Dispose();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during resource cleanup.");
        }
    }

    public void Dispose()
    {
        _shutdownCts.Dispose();
    }
}

// Program.cs Hook
public static class ShutdownConfig
{
    public static void Configure(IApplicationBuilder app, IHostApplicationLifetime lifetime)
    {
        // The ResourceManager is registered as a HostedService, which is automatically stopped by the host.
        // The ApplicationStopping event is used here to coordinate the disposal logic.
    }
}
