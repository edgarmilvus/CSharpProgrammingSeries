
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
using Microsoft.ML;
using Microsoft.ML.Transforms.Onnx;
using System;
using System.Collections.Concurrent;
using System.Threading;

// 1. ANTI-PATTERN EXPLANATION:
// The PredictionEngine<TInput, TOutput> in ML.NET is NOT thread-safe.
// It relies on internal buffers and state for tensor operations. 
// Registering it as a Singleton means a single instance is shared across all HTTP requests.
// When multiple threads access it simultaneously, they overwrite each other's input data 
// and intermediate calculations, leading to race conditions, exceptions, or garbage predictions.

// ---------------------------------------------------------
// SOLUTION 1: Scoped Lifetime (Recommended for simplicity)
// ---------------------------------------------------------
public static class ServiceRegistration
{
    public static void RegisterServices(IServiceCollection services)
    {
        // FIX: Change lifetime to Scoped. 
        // A new instance is created for every HTTP request, ensuring isolation.
        // The instance is disposed automatically at the end of the request.
        services.AddScoped<PredictionEngine<ModelInput, ModelOutput>>(sp =>
        {
            var mlContext = new MLContext();
            // Load model (simplified for example)
            ITransformer model = mlContext.Model.Load("model.onnx", out _);
            return mlContext.Model.CreatePredictionEngine<ModelInput, ModelOutput>(model);
        });
    }
}

// ---------------------------------------------------------
// SOLUTION 2: Factory Pattern (Advanced)
// ---------------------------------------------------------

// Interface for the factory
public interface IPredictionEngineFactory
{
    PredictionEngine<ModelInput, ModelOutput> CreatePredictionEngine();
}

// Implementation: Singleton
// We register this as Singleton. It doesn't hold the PredictionEngine itself,
// but rather the logic to create one.
public class PredictionEngineFactory : IPredictionEngineFactory
{
    private readonly IServiceProvider _serviceProvider;

    public PredictionEngineFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public PredictionEngine<ModelInput, ModelOutput> CreatePredictionEngine()
    {
        // We resolve the PredictionEngine from the DI container here.
        // Since PredictionEngine is registered as Scoped (or Transient), 
        // GetService creates a NEW instance isolated to the current request scope.
        // Note: In a pure factory pattern, you might instantiate directly here, 
        // but using DI ensures dependencies are injected correctly.
        return _serviceProvider.GetRequiredService<PredictionEngine<ModelInput, ModelOutput>>();
    }
}

// Usage in Controller
public class SentimentController : ControllerBase
{
    private readonly IPredictionEngineFactory _factory;

    public SentimentController(IPredictionEngineFactory factory)
    {
        _factory = factory;
    }

    [HttpPost("predict")]
    public IActionResult Predict(ModelInput input)
    {
        // Get a fresh, thread-isolated engine instance
        using var engine = _factory.CreatePredictionEngine();
        var result = engine.Predict(input);
        return Ok(result);
    }
}

// ---------------------------------------------------------
// MEMORY MANAGEMENT ANALYSIS (Comment Block)
// ---------------------------------------------------------
/*
 * MEMORY & GC IMPACT ANALYSIS:
 * 
 * 1. SINGLETON (Current/Broken):
 *    - Memory: Low. One instance holds memory for the entire app lifetime.
 *    - GC: Minimal pressure. No allocations per request.
 *    - Risk: CRITICAL. Data corruption and thread safety issues make this unusable.
 * 
 * 2. SCOPED (Recommended Fix):
 *    - Memory: Medium. One instance per request. Memory is held for the duration of the request.
 *    - GC: Moderate pressure. Instances are created and destroyed frequently. 
 *      However, ASP.NET Core's object pooling and efficient scope disposal mitigate this.
 *    - IDisposable: The PredictionEngine implements IDisposable. 
 *      ASP.NET Core automatically disposes Scoped services when the HTTP request ends 
 *      (via the ServiceProvider scope). This releases unmanaged native memory used by ONNX/ML.NET.
 * 
 * 3. TRANSIENT (Interactive Challenge):
 *    - Memory: High. If injected multiple times into the same request (e.g., Controller -> Service -> Service), 
 *      multiple instances are created.
 *    - GC: Highest pressure. Creates the most garbage per unit of time.
 *    - Performance: Creating a PredictionEngine involves loading model weights and initializing 
 *      native resources. Doing this per injection (not just per request) adds significant CPU overhead.
 * 
 * CONCLUSION: Scoped is the balance. It ensures thread safety (isolation) and proper resource cleanup 
 * without the excessive allocation cost of Transient if the engine is only needed once per request.
 */

// Dummy classes for compilation context
public class ModelInput { public string Text { get; set; } }
public class ModelOutput { public float Sentiment { get; set; } }
