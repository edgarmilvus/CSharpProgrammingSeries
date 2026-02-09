
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

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

public class ModelLifecycleService : IHostedLifecycleService
{
    private readonly ModelService _modelService;
    private readonly ILogger<ModelLifecycleService> _logger;

    public ModelLifecycleService(ModelService modelService, ILogger<ModelLifecycleService> logger)
    {
        _modelService = modelService;
        _logger = logger;
    }

    public Task StartingAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Lifecycle: Checking Environment (CUDA, Disk Space)...");
        // Fast check. If fails, throw exception to halt startup immediately.
        if (!EnvironmentCheck())
        {
            throw new InvalidOperationException("Environment check failed. Aborting startup.");
        }
        return Task.CompletedTask;
    }

    public Task StartedAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Lifecycle: Waiting for Model Warmup...");
        // This BLOCKS the host from signaling "Started" until complete.
        // Requests cannot be processed yet.
        return _modelService.InitializeAsync(cancellationToken);
    }

    public Task StoppingAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Lifecycle: Application is stopping...");
        return Task.CompletedTask;
    }

    public Task StoppedAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Lifecycle: Application has stopped.");
        return Task.CompletedTask;
    }

    private bool EnvironmentCheck() => true; // Simulate check
}

// Composite Pattern for Ordered Execution
public class CompositeHostedService : IHostedService
{
    private readonly IEnumerable<IHostedLifecycleService> _services;
    private readonly ILogger<CompositeHostedService> _logger;

    public CompositeHostedService(IEnumerable<IHostedLifecycleService> services, ILogger<CompositeHostedService> logger)
    {
        _services = services;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        // Execute StartingAsync in order
        foreach (var service in _services)
        {
            await service.StartingAsync(cancellationToken);
        }

        // Execute StartedAsync in order
        foreach (var service in _services)
        {
            await service.StartedAsync(cancellationToken);
        }
        
        _logger.LogInformation("Composite Hosted Services started.");
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        // Reverse order for stopping
        foreach (var service in _services.Reverse())
        {
            await service.StoppedAsync(cancellationToken);
        }
    }
}
