
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

using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class ModelHealthCheck : IHealthCheck
{
    private readonly ModelService _modelService;
    private readonly ILogger<ModelHealthCheck> _logger;

    public ModelHealthCheck(ModelService modelService, ILogger<ModelHealthCheck> logger)
    {
        _modelService = modelService;
        _logger = logger;
    }

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        // 1. Check if model is loaded
        if (!_modelService.IsLoaded) 
        {
            return Task.FromResult(HealthCheckResult.Degraded("Model is currently loading..."));
        }

        // 2. Check for failure state
        if (_modelService.LoadFailed)
        {
            return Task.FromResult(HealthCheckResult.Unhealthy("Model failed to initialize"));
        }

        // 3. Deep validation (Edge Case: Loaded but invalid output capability)
        if (_modelService.InternalState == null)
        {
            // Distinct from not loaded: it loaded, but failed internal checks
            return Task.FromResult(HealthCheckResult.Unhealthy("Model loaded but internal state is corrupt"));
        }

        return Task.FromResult(HealthCheckResult.Healthy("Model loaded successfully"));
    }
}

// Publisher for Logging/Monitoring
public class ModelHealthPublisher : IHealthCheckPublisher
{
    private readonly ILogger<ModelHealthPublisher> _logger;

    public ModelHealthPublisher(ILogger<ModelHealthPublisher> logger)
    {
        _logger = logger;
    }

    public Task PublishAsync(HealthReport report, CancellationToken cancellationToken)
    {
        // Log the detailed status
        foreach (var entry in report.Entries)
        {
            _logger.LogInformation("Health Check {Key}: {Status} - {Description}", 
                entry.Key, entry.Value.Status, entry.Value.Description);
        }
        
        // In a real scenario, push to Application Insights, Datadog, etc.
        return Task.CompletedTask;
    }
}

// Program.cs Configuration (Conceptual)
public static class HealthCheckConfig
{
    public static void Configure(IServiceCollection services)
    {
        services.AddHealthChecks()
            .AddCheck<ModelHealthCheck>("model_deep_check", tags: new[] { "readiness", "model-dependency" })
            .AddCheck("live_check", () => HealthCheckResult.Healthy("Process is alive"), tags: new[] { "liveness" });

        services.AddSingleton<IHealthCheckPublisher, ModelHealthPublisher>();
        
        // Configure endpoints
        // app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = (check) => check.Tags.Contains("liveness") });
        // app.MapHealthChecks("/health/ready", new HealthCheckOptions { Predicate = (check) => check.Tags.Contains("readiness") });
    }
}
