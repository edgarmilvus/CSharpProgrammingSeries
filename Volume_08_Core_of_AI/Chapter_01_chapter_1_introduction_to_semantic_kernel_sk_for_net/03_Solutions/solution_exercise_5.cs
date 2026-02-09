
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

// File: TelemetryMiddleware.cs
using Microsoft.SemanticKernel;
using System.Diagnostics;

public class TelemetryMiddleware : KernelFunctionFilter
{
    private readonly ILogger<TelemetryMiddleware> _logger;

    public TelemetryMiddleware(ILogger<TelemetryMiddleware> logger)
    {
        _logger = logger;
    }

    public override void OnFunctionInvoking(FunctionInvokingContext context)
    {
        _logger.LogInformation("Invoking function: {FunctionName}", context.Function.Name);
        _logger.LogDebug("Arguments: {@Arguments}", context.Arguments);
    }

    public override void OnFunctionInvoked(FunctionInvokedContext context)
    {
        var stopwatch = context.Items["Stopwatch"] as Stopwatch;
        stopwatch?.Stop();
        
        _logger.LogInformation("Function {FunctionName} completed in {Elapsed}ms", 
            context.Function.Name, stopwatch?.ElapsedMilliseconds);

        // Interactive Challenge: Log Token Usage (Simulated via Context Items)
        if (context.Result.Metadata is not null && context.Result.Metadata.TryGetValue("Usage", out var usage))
        {
            _logger.LogInformation("Token Usage: {Usage}", usage);
        }
    }
}

// File: Program.cs
using Microsoft.SemanticKernel;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using System.Diagnostics;

// 1. Configure Logging
var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddConsole()
           .SetMinimumLevel(LogLevel.Information);
});
var logger = loggerFactory.CreateLogger<TelemetryMiddleware>();

// 2. Initialize Kernel with Logging
var kernel = Kernel.CreateBuilder()
    .AddAzureOpenAIChatCompletion("deployment-name", "https://endpoint", "key")
    .Build();
kernel.LoggerFactory = loggerFactory;

// 3. Register Middleware
kernel.FunctionFilters.Add(new TelemetryMiddleware(logger));

// 4. Define Retry Policy (using Polly)
var retryPolicy = Policy
    .Handle<HttpOperationException>()
    .Or<KernelFunctionException>()
    .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
        onRetry: (exception, timeSpan, retryCount, context) =>
        {
            logger.LogWarning($"Retry {retryCount} after {timeSpan.TotalSeconds}s due to: {exception.Message}");
        });

// 5. Execution Wrapper
async Task ExecuteWithResilienceAsync()
{
    // Simulate a complex scenario
    await retryPolicy.ExecuteAsync(async () =>
    {
        var stopwatch = Stopwatch.StartNew();
        
        // Add stopwatch to context for middleware access
        var context = new KernelArguments();
        
        // Execute a function (simulated)
        var result = await kernel.InvokeAsync("SomePlugin", "SomeFunction", context);
        
        // In a real scenario, the middleware would handle timing, 
        // but we ensure the stopwatch is stopped in OnFunctionInvoked.
        // For this wrapper, we just ensure the policy executes.
        
        Console.WriteLine($"Result: {result}");
    });
}

// Interactive Challenge: Custom ChatCompletion Wrapper
// This wrapper aggregates usage statistics. 
// Note: In real SK, this often involves decorating the IChatCompletionService.
// For brevity, we simulate the registration of a custom service.
public class TelemetryChatCompletion : IChatCompletionService
{
    private readonly IChatCompletionService _innerService;
    private readonly ILogger<TelemetryChatCompletion> _logger;
    public TelemetryChatCompletion(IChatCompletionService inner, ILogger<TelemetryChatCompletion> logger)
    {
        _innerService = inner;
        _logger = logger;
    }

    public async Task<ChatMessageContent> GetChatMessageContentAsync(ChatHistory chatHistory, PromptExecutionSettings? executionSettings = null, Kernel? kernel = null, CancellationToken cancellationToken = default)
    {
        var result = await _innerService.GetChatMessageContentAsync(chatHistory, executionSettings, kernel, cancellationToken);
        
        // Log usage if available
        if (result.Metadata.TryGetValue("Usage", out var usage))
        {
            _logger.LogInformation("Total Tokens Used: {Usage}", usage);
        }
        return result;
    }

    // Implement other interface members delegating to _innerService...
    public IReadOnlyDictionary<string, object?> Attributes => _innerService.Attributes;
}

// To use this wrapper, you would replace the standard AddAzureOpenAIChatRegistration 
// with a custom registration that injects this decorator.

// Run the simulation
ExecuteWithResilienceAsync().Wait();
