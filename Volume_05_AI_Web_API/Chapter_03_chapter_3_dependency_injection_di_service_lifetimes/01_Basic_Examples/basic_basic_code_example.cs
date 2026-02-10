
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

// Source File: basic_basic_code_example.cs
// Description: Basic Code Example
// ==========================================

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

// --- 1. Domain Models ---

// Represents the input data for our AI model.
public record TextInput(string Text);

// Represents the output prediction from our AI model.
public record TextPrediction(string Label, float Confidence);

// --- 2. AI Inference Service Interfaces & Implementations ---

// Interface defining the contract for our AI inference engine.
public interface IInferenceEngine
{
    Task<TextPrediction> PredictAsync(TextInput input);
}

// SCOPED IMPLEMENTATION:
// Simulates an inference engine that holds state (like a DB context or a non-thread-safe ML.NET pipeline).
// In a real scenario, this might wrap an ML.NET PredictionEngine<TData, TPrediction>.
// WARNING: PredictionEngine<T, T> is NOT thread-safe. It must be instantiated per scope.
public class ScopedInferenceEngine : IInferenceEngine
{
    private readonly Guid _instanceId = Guid.NewGuid(); // Simulating state/identity
    private readonly Random _random = new Random(); // Simulating internal state

    public ScopedInferenceEngine()
    {
        Console.WriteLine($"[Scoped] InferenceEngine created. Instance ID: {_instanceId}");
    }

    public Task<TextPrediction> PredictAsync(TextInput input)
    {
        // Simulate AI processing delay
        Thread.Sleep(50);
        
        // Simulate a prediction result
        string label = input.Text.Contains("error", StringComparison.OrdinalIgnoreCase) ? "Negative" : "Positive";
        float confidence = (float)_random.NextDouble() * (1.0f - 0.5f) + 0.5f; // 0.5 to 1.0

        return Task.FromResult(new TextPrediction(label, confidence));
    }
}

// SINGLETON IMPLEMENTATION:
// Simulates a heavy, thread-safe ONNX Runtime inference session.
// This loads the model into memory once and serves all requests.
public class SingletonInferenceEngine : IInferenceEngine
{
    private readonly Guid _instanceId = Guid.NewGuid();

    public SingletonInferenceEngine()
    {
        Console.WriteLine($"[Singleton] InferenceEngine created. Instance ID: {_instanceId}");
        // In a real app, heavy model loading (e.g., OnnxRuntime.InferenceSession) happens here.
    }

    public Task<TextPrediction> PredictAsync(TextInput input)
    {
        // Simulate AI processing delay
        Thread.Sleep(50);

        // Simulate a prediction result
        string label = input.Text.Contains("error", StringComparison.OrdinalIgnoreCase) ? "Negative" : "Positive";
        float confidence = 0.99f; // High confidence for singleton demo

        return Task.FromResult(new TextPrediction(label, confidence));
    }
}

// --- 3. Application Logic (Simulating Request Handling) ---

public class RequestSimulator
{
    private readonly IInferenceEngine _engine;

    // The dependency is injected here. The lifetime of 'engine' depends on how it was registered.
    public RequestSimulator(IInferenceEngine engine)
    {
        _engine = engine;
    }

    public async Task ProcessRequestAsync(string requestText)
    {
        Console.WriteLine($"  -> Processing request: '{requestText}'");
        
        var input = new TextInput(requestText);
        var prediction = await _engine.PredictAsync(input);
        
        Console.WriteLine($"  -> Prediction: {prediction.Label} (Confidence: {prediction.Confidence:F2})");
    }
}

// --- 4. Main Program Execution ---

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("=== DEMONSTRATING SERVICE LIFETIMES ===\n");

        // --- SCENARIO A: SCOPED LIFETIME ---
        // Services are created once per client request (scope).
        Console.WriteLine("--- SCENARIO 1: SCOPED LIFETIME (Simulating Web Request) ---");
        var scopedProvider = new ServiceCollection()
            .AddScoped<IInferenceEngine, ScopedInferenceEngine>()
            .BuildServiceProvider();

        // Simulate Request 1
        using (var scope1 = scopedProvider.CreateScope())
        {
            var processor1 = scope1.ServiceProvider.GetRequiredService<RequestSimulator>();
            await processor1.ProcessRequestAsync("Hello AI");
        }

        // Simulate Request 2 (New Scope = New Instance)
        using (var scope2 = scopedProvider.CreateScope())
        {
            var processor2 = scope2.ServiceProvider.GetRequiredService<RequestSimulator>();
            await processor2.ProcessRequestAsync("Another Request");
        }
        
        Console.WriteLine("Notice: Two different InferenceEngine instances were created.\n");


        // --- SCENARIO B: SINGLETON LIFETIME ---
        // Service is created once and shared throughout the application lifetime.
        Console.WriteLine("--- SCENARIO 2: SINGLETON LIFETIME (Simulating Shared Resource) ---");
        var singletonProvider = new ServiceCollection()
            // Note: We register RequestSimulator as Transient here so we can resolve it multiple times
            // in the main flow, but the IInferenceEngine it consumes is Singleton.
            .AddSingleton<IInferenceEngine, SingletonInferenceEngine>()
            .AddTransient<RequestSimulator>() 
            .BuildServiceProvider();

        // Simulate Request 1
        var processor1 = singletonProvider.GetRequiredService<RequestSimulator>();
        await processor1.ProcessRequestAsync("Request A");

        // Simulate Request 2
        var processor2 = singletonProvider.GetRequiredService<RequestSimulator>();
        await processor2.ProcessRequestAsync("Request B");

        Console.WriteLine("Notice: The SAME InferenceEngine instance was reused for both requests.\n");
        
        // Keep console open
        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }
}
