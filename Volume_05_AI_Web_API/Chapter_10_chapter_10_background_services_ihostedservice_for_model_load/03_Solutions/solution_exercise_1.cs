
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

// Source File: solution_exercise_1.cs
// Description: Solution for Exercise 1
// ==========================================

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

// 1. Define the Interface
public interface IModelLoader
{
    Task LoadModelAsync(CancellationToken cancellationToken);
}

// 2. Implementation with Retry Logic
public class ResilientModelLoader : IModelLoader
{
    private readonly ILogger<ResilientModelLoader> _logger;
    private readonly Random _random = new();

    public ResilientModelLoader(ILogger<ResilientModelLoader> logger)
    {
        _logger = logger;
    }

    public async Task LoadModelAsync(CancellationToken cancellationToken)
    {
        int retries = 0;
        int maxRetries = 3;
        int delayMs = 1000;

        while (true)
        {
            try
            {
                // Simulate failure 30% of the time
                if (_random.Next(1, 101) <= 30)
                {
                    throw new InvalidOperationException("Simulated network failure during model load.");
                }

                _logger.LogInformation("Loading model...");
                await Task.Delay(5000, cancellationToken); // Simulate heavy load
                _logger.LogInformation("Model loaded successfully.");
                return;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                retries++;
                if (retries > maxRetries)
                {
                    _logger.LogError(ex, "Model loading failed after {Retries} retries.", maxRetries);
                    // In a real scenario, we might set a flag indicating degraded state
                    throw; // Let the BackgroundService handle the final state
                }

                _logger.LogWarning(ex, "Retry {RetryCount}/{MaxRetries} in {Delay}ms...", retries, maxRetries, delayMs);
                await Task.Delay(delayMs, cancellationToken);
                delayMs *= 2; // Exponential backoff
            }
        }
    }
}

// 3. State Tracker for Health Checks
public class ModelState
{
    public bool IsLoaded { get; set; }
    public bool IsLoadingFailed { get; set; }
    
    // Thread-safe update
    public void SetLoaded() => IsLoaded = true;
    public void SetFailed() => IsLoadingFailed = true;
}

// 4. Background Service
public class ModelBackgroundService : BackgroundService
{
    private readonly IModelLoader _loader;
    private readonly ModelState _state;
    private readonly ILogger<ModelBackgroundService> _logger;

    public ModelBackgroundService(IModelLoader loader, ModelState state, ILogger<ModelBackgroundService> logger)
    {
        _loader = loader;
        _state = state;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            // Respect cancellation immediately
            stoppingToken.ThrowIfCancellationRequested();
            
            await _loader.LoadModelAsync(stoppingToken);
            _state.SetLoaded();
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Model loading cancelled during shutdown.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Critical error in model background service.");
            _state.SetFailed();
        }
    }
}

// 5. Health Check
public class ModelHealthCheck : IHealthCheck
{
    private readonly ModelState _state;

    public ModelHealthCheck(ModelState state)
    {
        _state = state;
    }

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        if (_state.IsLoadingFailed)
        {
            return Task.FromResult(HealthCheckResult.Unhealthy("Model failed to load."));
        }

        if (_state.IsLoaded)
        {
            return Task.FromResult(HealthCheckResult.Healthy("Model is ready."));
        }

        // Returning Degraded (503) is common for "starting up"
        return Task.FromResult(HealthCheckResult.Degraded("Model is loading..."));
    }
}

// 6. Registration (Program.cs equivalent)
public static class ServiceRegistration
{
    public static void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<ModelState>();
        services.AddSingleton<IModelLoader, ResilientModelLoader>();
        services.AddHostedService<ModelBackgroundService>();
        
        services.AddHealthChecks()
            .AddCheck<ModelHealthCheck>("model_health");
    }
}
