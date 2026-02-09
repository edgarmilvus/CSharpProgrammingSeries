
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

// Program.cs
using Microsoft.Extensions.Hosting;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;

// 1. Inference Service Implementation
public class InferenceService : BackgroundService
{
    private readonly InferenceSession _session;
    private readonly ILogger<InferenceService> _logger;

    public InferenceService(ILogger<InferenceService> logger)
    {
        _logger = logger;
        
        // 2. Configure Session Options for GPU
        // Note: In a real scenario, ensure CUDA libraries are accessible in the container path
        var sessionOptions = new SessionOptions();
        
        // Use the static helper to configure for the default CUDA provider
        // This requires the Microsoft.ML.OnnxRuntime.Gpu package
        SessionOptions.MakeSessionOptionWithCudaProvider(sessionOptions);
        
        // Load the model from the container filesystem
        // Assuming the model is copied to /app/models/model.onnx
        _session = new InferenceSession("/app/models/model.onnx", sessionOptions);
        
        _logger.LogInformation("Inference session initialized with CUDA provider.");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Simulating a service loop or keeping the session alive
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(5000, stoppingToken);
            _logger.LogDebug("Inference service heartbeat.");
        }
    }

    // 3. Inference Method
    public string AnalyzeSentiment(string inputText)
    {
        // Tokenization logic would go here (simplified for demo)
        // Creating a dummy tensor for demonstration
        var inputTensor = new DenseTensor<long>(new long[] { 1, inputText.Length }, new[] { 1, inputTensorLength });
        
        // Bind inputs
        var inputs = new List<NamedOnnxValue>
        {
            NamedOnnxValue.CreateFromTensor("input_ids", inputTensor)
        };

        // Run inference
        using var results = _session.Run(inputs);
        var output = results.First().AsTensor<long>().ToArray();
        
        return $"Sentiment Score: {output[0]}";
    }

    public override void Dispose()
    {
        _session.Dispose();
        base.Dispose();
    }
}

// 4. Main Entry Point
public class Program
{
    public static void Main(string[] args)
    {
        // Handle command line arguments
        if (args.Length > 0)
        {
            // Minimal standalone execution for testing
            var service = new InferenceService(LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<InferenceService>());
            var result = service.AnalyzeSentiment(args[0]);
            Console.WriteLine(result);
            return;
        }

        // Standard Host Execution
        var builder = Host.CreateApplicationBuilder(args);
        builder.Services.AddHostedService<InferenceService>();
        var host = builder.Build();
        host.Run();
    }
}
