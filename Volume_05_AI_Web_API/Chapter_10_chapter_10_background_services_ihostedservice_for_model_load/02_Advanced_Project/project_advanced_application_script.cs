
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

# Source File: project_advanced_application_script.cs
# Description: Advanced Application Script
# ==========================================

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

// PROBLEM SOLVED:
// A high-throughput AI inference API needs to load multiple large language models (LLMs) into memory.
// Loading these models takes significant time (seconds to minutes). If we load them synchronously during
// startup, the web server (Kestrel) won't start listening for requests until *all* models are loaded,
// leading to "Service Unavailable" errors and poor DevOps experience during deployments.
// SOLUTION:
// We implement a BackgroundService that loads models asynchronously on a separate thread.
// We use TaskCompletionSource to signal when a specific model is ready, allowing the application
// to serve traffic for Model A while Model B is still initializing.

namespace AIModelLoadingDemo
{
    // 1. The Model Wrapper
    // Represents a heavy AI model. In a real scenario, this would hold the ONNX runtime session or TorchSharp tensors.
    // We simulate the "heaviness" with a Thread.Sleep in the LoadAsync method.
    public class AiModel
    {
        public string Name { get; }
        public bool IsLoaded { get; private set; }
        private readonly TaskCompletionSource<bool> _loadingTcs = new TaskCompletionSource<bool>();

        public AiModel(string name)
        {
            Name = name;
        }

        // Simulates a blocking, I/O bound model loading operation
        public async Task LoadAsync()
        {
            Console.WriteLine($"[Model {Name}] Starting load sequence...");
            // Simulate heavy work (e.g., reading 4GB file from disk, initializing tensors)
            await Task.Delay(2000); 
            
            IsLoaded = true;
            _loadingTcs.TrySetResult(true);
            Console.WriteLine($"[Model {Name}] Loaded and ready.");
        }

        // Allows consumers to await the model being ready without polling
        public Task WaitForReadyAsync() => _loadingTcs.Task;
    }

    // 2. The Background Service
    // This runs independently of the HTTP request pipeline.
    // It inherits from BackgroundService (which implements IHostedService).
    public class ModelLoaderService : BackgroundService
    {
        private readonly List<AiModel> _models;
        private readonly IHostApplicationLifetime _lifetime;

        public ModelLoaderService(IHostApplicationLifetime lifetime)
        {
            _lifetime = lifetime;
            // Initialize registry with available models
            _models = new List<AiModel>
            {
                new AiModel("SentimentAnalysis-v1"),
                new AiModel("CodeGeneration-v2"),
                new AiModel("ImageCaptioning-v3")
            };
        }

        // ExecuteAsync is called by the host when the application starts.
        // It runs on a background thread and does not block startup.
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Console.WriteLine(">>> Background Service: Model Loader started.");

            // We loop through models to load them sequentially (or you could load in parallel).
            foreach (var model in _models)
            {
                // Check for cancellation (graceful shutdown)
                if (stoppingToken.IsCancellationRequested) break;

                try
                {
                    // We offload the heavy loading to the model itself.
                    // In a real scenario, we might wrap this in a Polly retry policy.
                    await model.LoadAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"!!! Critical Error loading {model.Name}: {ex.Message}");
                    // In a real app, we might decide to stop the application if critical models fail:
                    // _lifetime.StopApplication();
                }
            }

            Console.WriteLine(">>> Background Service: All models loaded.");
        }

        // Helper method for the HTTP layer to retrieve a loaded model safely
        public AiModel? GetModel(string name)
        {
            return _models.Find(m => m.Name == name && m.IsLoaded);
        }

        // Helper to check if a specific model is ready (for Health Checks)
        public bool IsModelReady(string name)
        {
            var model = _models.Find(m => m.Name == name);
            return model != null && model.IsLoaded;
        }
    }

    // 3. The "API" Simulation (Simulating an ASP.NET Core Controller)
    // This represents the entry point for a web request.
    public class InferenceController
    {
        private readonly ModelLoaderService _modelService;

        public InferenceController(ModelLoaderService modelService)
        {
            _modelService = modelService;
        }

        public async Task<string> PredictAsync(string modelName, string input)
        {
            // 1. Get the model
            var model = _modelService.GetModel(modelName);

            // 2. Handle the "Loading..." state gracefully
            if (model == null)
            {
                // If the model isn't loaded yet, we could:
                // A. Return 503 Service Unavailable
                // B. Wait for it (if the user is willing to wait)
                // C. Return a "Please try again later" message.
                
                // Let's try to wait for it briefly (simulating a resilience strategy)
                Console.WriteLine($"[API] Model {modelName} not ready. Waiting...");
                
                // We can't easily await the specific model here without exposing the internal TCS, 
                // so we rely on the service status.
                // For this demo, we will just fail fast if not loaded.
                return $"Error: Model '{modelName}' is currently loading. Please retry in a moment.";
            }

            // 3. Perform Inference
            // Simulate processing time
            await Task.Delay(100); 
            return $"Result for '{input}' using {model.Name}: [Positive Sentiment]";
        }
    }

    // 4. Health Check Implementation
    // Essential for Kubernetes/LB to know when the pod is truly ready.
    public class ModelHealthCheck : IHealthCheck
    {
        private readonly ModelLoaderService _modelService;

        public ModelHealthCheck(ModelLoaderService modelService)
        {
            _modelService = modelService;
        }

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            // Check if critical models are loaded
            bool criticalModelReady = _modelService.IsModelReady("SentimentAnalysis-v1");

            if (criticalModelReady)
            {
                return Task.FromResult(HealthCheckResult.Healthy("Critical model is ready."));
            }
            
            // Return Unhealthy or Degraded depending on logic
            return Task.FromResult(HealthCheckResult.Degraded("SentimentAnalysis-v1 is still loading..."));
        }
    }

    // 5. Main Entry Point (Simulating Program.cs)
    // Since we cannot use Dependency Injection containers (like IServiceCollection) explicitly 
    // as per "basic blocks" constraint, we manually wire up our services.
    public class Program
    {
        public static async Task Main(string[] args)
        {
            Console.WriteLine("System Boot: Initializing Host...");

            // Simulate Host Creation
            var lifetime = new HostApplicationLifetime();
            
            // Instantiate our services
            var modelLoader = new ModelLoaderService(lifetime);
            var healthCheck = new ModelHealthCheck(modelLoader);
            var controller = new InferenceController(modelLoader);

            // Start the Background Service manually (mimicking IHostedService startup)
            // In a real ASP.NET Core app, the Host does this automatically.
            // We use a Task.Run to simulate the non-blocking nature of IHostedService.ExecuteAsync
            var loaderTask = Task.Run(() => modelLoader.StartAsync(lifetime.ApplicationStopping));

            // --- SIMULATION OF APPLICATION RUNNING ---

            // Phase 1: Immediate Requests (App is up, but models are loading)
            Console.WriteLine("\n--- Phase 1: App Started, Models Loading ---");
            
            // Try to hit the API immediately
            var result1 = await controller.PredictAsync("SentimentAnalysis-v1", "I love this product!");
            Console.WriteLine($"[Client Request 1]: {result1}");

            // Check Health
            var healthResult = await healthCheck.CheckHealthAsync(new HealthCheckContext());
            Console.WriteLine($"[Health Check 1]: {healthResult.Status}");

            // Wait a bit (simulating time passing)
            await Task.Delay(1000);

            // Phase 2: Partial Load
            Console.WriteLine("\n--- Phase 2: Partial Load ---");
            var result2 = await controller.PredictAsync("SentimentAnalysis-v1", "This is okay.");
            Console.WriteLine($"[Client Request 2]: {result2}"); // Might still fail if < 2 seconds

            // Phase 3: Full Load (Wait for background task to finish)
            // We wait for the loader task to complete (or just sleep enough for the 2s delay)
            Console.WriteLine("\n--- Phase 3: Waiting for full load ---");
            await loaderTask; // Wait for the background service to finish its loop

            // Phase 4: Serving Traffic
            Console.WriteLine("\n--- Phase 4: Full Capacity ---");
            var result3 = await controller.PredictAsync("SentimentAnalysis-v1", "I hate waiting.");
            Console.WriteLine($"[Client Request 3]: {result3}");

            var healthResult2 = await healthCheck.CheckHealthAsync(new HealthCheckContext());
            Console.WriteLine($"[Health Check 2]: {healthResult2.Status}");
        }
    }

    // --- MOCK INFRASTRUCTURE (To make the code runnable without full ASP.NET Core references) ---
    // These classes simulate the interfaces usually found in Microsoft.Extensions namespaces.
    
    public class HostApplicationLifetime
    {
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        public CancellationToken ApplicationStopping => _cts.Token;
        public void StopApplication() => _cts.Cancel();
    }

    public abstract class BackgroundService
    {
        public Task? _executingTask;
        public CancellationTokenSource? _stoppingCts;

        public async Task StartAsync(CancellationToken stoppingToken)
        {
            _stoppingCts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
            _executingTask = ExecuteAsync(_stoppingCts.Token);

            if (_executingTask.IsCompleted)
            {
                await _executingTask;
            }
        }

        protected abstract Task ExecuteAsync(CancellationToken stoppingToken);
    }

    public enum HealthStatus { Unhealthy, Degraded, Healthy }
    public class HealthCheckResult
    {
        public HealthStatus Status { get; }
        public string Description { get; }
        private HealthCheckResult(HealthStatus status, string description) { Status = status; Description = description; }
        public static HealthCheckResult Healthy(string description) => new HealthCheckResult(HealthStatus.Healthy, description);
        public static HealthCheckResult Degraded(string description) => new HealthCheckResult(HealthStatus.Degraded, description);
        public static HealthCheckResult Unhealthy(string description) => new HealthCheckResult(HealthStatus.Unhealthy, description);
    }
    public class HealthCheckContext { }
    public interface IHealthCheck { Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default); }
    public interface IHostApplicationLifetime { CancellationToken ApplicationStopping { get; } ; void StopApplication(); }
}
