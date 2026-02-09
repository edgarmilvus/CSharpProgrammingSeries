
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

# Source File: basic_basic_code_example.cs
# Description: Basic Code Example
# ==========================================

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BackgroundModelLoading
{
    // 1. Define the service interface for the AI Model
    public interface IModelService
    {
        Task<string> PredictAsync(string input);
        bool IsReady { get; }
    }

    // 2. Implementation of the AI Model Service (Singleton)
    // This simulates a heavy service that requires initialization.
    public class AiModelService : IModelService
    {
        private readonly ILogger<AiModelService> _logger;
        private readonly TaskCompletionSource _initializationTcs = new();

        public AiModelService(ILogger<AiModelService> logger)
        {
            _logger = logger;
            _logger.LogInformation("AiModelService instance created. Waiting for initialization...");
        }

        public bool IsReady => _initializationTcs.Task.IsCompleted;

        // Called by the Background Service to load the model
        public async Task InitializeAsync()
        {
            try
            {
                _logger.LogInformation("Starting model load (simulated 5s delay)...");
                
                // Simulate loading a large file (e.g., 500MB) from disk or network
                await Task.Delay(TimeSpan.FromSeconds(5));

                _logger.LogInformation("Model loaded successfully into memory.");
                
                // Signal that initialization is complete
                _initializationTcs.TrySetResult();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load model.");
                _initializationTcs.TrySetException(ex);
            }
        }

        // Called by Controllers/Endpoints
        public async Task<string> PredictAsync(string input)
        {
            // Wait for initialization to complete before processing
            await _initializationTcs.Task;
            
            // Simulate inference time
            await Task.Delay(100);
            return $"Processed '{input}' using loaded model.";
        }
    }

    // 3. The Background Service responsible for initialization
    // This runs as soon as the application starts.
    public class ModelLoaderService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ModelLoaderService> _logger;
        private readonly IHostApplicationLifetime _lifetime;

        public ModelLoaderService(
            IServiceProvider serviceProvider,
            ILogger<ModelLoaderService> logger,
            IHostApplicationLifetime lifetime)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _lifetime = lifetime;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // We create a scope because we are resolving a scoped service (or singleton) 
            // from a singleton background service.
            using var scope = _serviceProvider.CreateScope();
            var modelService = scope.ServiceProvider.GetRequiredService<IModelService>();

            try
            {
                _logger.LogInformation("Background Service: Starting model initialization...");

                // Perform the heavy lifting
                await modelService.InitializeAsync();

                _logger.LogInformation("Background Service: Model initialization complete.");
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Background Service: Critical failure during model loading.");
                
                // In a real scenario, you might want to stop the application if the model is essential
                // _lifetime.StopApplication(); 
            }
        }
    }

    // 4. Program.cs (Setup)
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Register the Model Service as a Singleton
            // It must be Singleton because the background service initializes it once,
            // and controllers need to access the same initialized instance.
            builder.Services.AddSingleton<IModelService, AiModelService>();

            // Register the Background Service
            builder.Services.AddHostedService<ModelLoaderService>();

            var app = builder.Build();

            // 5. Minimal API Endpoint
            // This endpoint will wait for the model to be ready before responding.
            app.MapGet("/predict", async (IModelService model, string input) =>
            {
                if (!model.IsReady)
                {
                    return Results.StatusCode(503); // Service Unavailable
                }

                var result = await model.PredictAsync(input);
                return Results.Ok(result);
            });

            app.Run();
        }
    }
}
