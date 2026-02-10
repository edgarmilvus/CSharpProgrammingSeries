
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

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Connectors.Ollama;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace KernelConfigExercises;

// 4. Structured Configuration Object
public record ExecutionSettingsConfig
{
    [JsonPropertyName("maxTokens")]
    public int MaxTokens { get; set; }

    [JsonPropertyName("temperature")]
    public double Temperature { get; set; }
}

public class DynamicExecutionMiddleware : KernelMiddleware
{
    private readonly ILogger<DynamicExecutionMiddleware> _logger;
    private const int HardLimit = 4096;

    public DynamicExecutionMiddleware(ILogger<DynamicExecutionMiddleware> logger)
    {
        _logger = logger;
    }

    public override async Task InvokeAsync(KernelInvocationContext context, Func<KernelInvocationContext, Task> next)
    {
        if (context.Arguments is null) 
        {
            await next(context);
            return;
        }

        // 2. Analyze Input
        string prompt = context.Arguments.TryGetValue("prompt", out object? p) ? p?.ToString() ?? string.Empty : string.Empty;
        int charCount = prompt.Length;

        // 3. Logic for Settings
        PromptExecutionSettings settings;

        if (charCount < 500)
        {
            // Azure Configuration
            settings = new OpenAIPromptExecutionSettings
            {
                Temperature = 0.1,
                MaxTokens = ClampToken(500)
            };
            // Explicitly setting ServiceId to ensure routing works if multiple services exist
            context.Arguments["serviceId"] = "AzureGPT"; 
            _logger.LogInformation("Short prompt detected. Routing to Azure (High Determinism).");
        }
        else
        {
            // Ollama Configuration
            settings = new OllamaPromptExecutionSettings
            {
                Temperature = 0.7,
                MaxTokens = ClampToken(2000)
            };
            context.Arguments["serviceId"] = "LocalOllama";
            _logger.LogInformation("Long prompt detected. Routing to Ollama (Cost Saving).");
        }

        // Update context with new settings
        context.Arguments.ExecutionSettings = new Dictionary<string, PromptExecutionSettings>
        {
            { settings.GetType().Name, settings }
        };

        await next(context);
    }

    private int ClampToken(int requested)
    {
        // 5. Edge Case: Clamp to hard limit
        return Math.Min(requested, HardLimit);
    }
}

// Interactive Challenge: Streaming Middleware
public class StreamingMonitorMiddleware : KernelMiddleware
{
    private readonly ILogger _logger;
    private int _tokenCount = 0;

    public StreamingMonitorMiddleware(ILogger logger) => _logger = logger;

    public override async Task InvokeAsync(KernelInvocationContext context, Func<KernelInvocationContext, Task> next)
    {
        // Hook into the streaming response
        var originalStream = context.KernelInvocationResult as IAsyncEnumerable<StreamingKernelContent>;
        
        if (originalStream != null)
        {
            // Wrap the stream to count tokens
            context.KernelInvocationResult = WrapStream(originalStream);
        }

        await next(context);
    }

    private async IAsyncEnumerable<StreamingKernelContent> WrapStream(IAsyncEnumerable<StreamingKernelContent> stream)
    {
        await foreach (var content in stream)
        {
            _tokenCount++;
            yield return content;
        }

        if (_tokenCount > 100)
        {
            _logger.LogWarning("Verbose Output Detected: Generated {TokenCount} tokens. Prompt may be vague.", _tokenCount);
        }
    }
}
