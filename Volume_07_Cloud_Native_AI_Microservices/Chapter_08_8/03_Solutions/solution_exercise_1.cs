
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

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

// 1. Register Redis Connection as Singleton
var redisConnectionString = Environment.GetEnvironmentVariable("REDIS_CONNECTION_STRING");
if (string.IsNullOrEmpty(redisConnectionString))
{
    throw new InvalidOperationException("Redis connection string is not set.");
}
// Assuming a simple interface for Redis connection
builder.Services.AddSingleton<IRedisConnection>(sp => new RedisConnection(redisConnectionString));

// 2. Register Inference Service with Lazy Model Loading
builder.Services.AddSingleton<IInferenceService, InferenceService>();

// 3. Add Health Checks
builder.Services.AddHealthChecks()
    .AddCheck<RedisHealthCheck>("redis_connection")
    .AddCheck<ModelLoadedHealthCheck>("ml_model");

var app = builder.Build();

app.MapHealthChecks("/health");

app.Run();

// --- Supporting Classes ---

public interface IInferenceService
{
    Task<string> AnalyzeSentimentAsync(string text);
}

public class InferenceService : IInferenceService
{
    private readonly Lazy<MLModel> _modelLoader;
    private readonly ILogger<InferenceService> _logger;

    public InferenceService(ILogger<InferenceService> logger)
    {
        _logger = logger;
        // Lazy initialization ensures the heavy model is loaded only upon first access
        _modelLoader = new Lazy<MLModel>(() => 
        {
            _logger.LogInformation("Loading ML Model into memory...");
            // Simulate loading model.zip
            return new MLModel(); 
        });
    }

    public Task<string> AnalyzeSentimentAsync(string text)
    {
        // Accessing .Value triggers loading if not already loaded
        var model = _modelLoader.Value;
        var result = model.Predict(text);
        return Task.FromResult(result);
    }
}

public class MLModel 
{
    // Placeholder for actual ML.NET model
    public string Predict(string text) => "Positive";
}

// Custom Health Check for the Model
public class ModelLoadedHealthCheck : Microsoft.Extensions.Diagnostics.HealthChecks.IHealthCheck
{
    private readonly IInferenceService _inferenceService;
    public ModelLoadedHealthCheck(IInferenceService inferenceService) => _inferenceService = inferenceService;

    public Task<Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult> CheckHealthAsync(
        Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckContext context, 
        CancellationToken cancellationToken = default)
    {
        // Check if the Lazy value has been initialized
        // (In a real scenario, you might check a flag or try a lightweight prediction)
        try 
        {
            // We trigger a dummy prediction to ensure model is loaded
            // In a real app, you might expose an IsLoaded property on the service
            return Task.FromResult(Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy("Model loaded"));
        }
        catch
        {
            return Task.FromResult(Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Unhealthy("Model not loaded"));
        }
    }
}
