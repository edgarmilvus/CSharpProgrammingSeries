
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

// Source File: solution_exercise_5.cs
// Description: Solution for Exercise 5
// ==========================================

// Models/Interfaces (Reused from Ex 1)
public interface IAgentInference { Task<string> GenerateResponseAsync(string prompt); }
public record TrafficSplitConfig(int V1Percentage, int V2Percentage);

// Implementations/VersionedEngines.cs
public class V1InferenceEngine : IAgentInference
{
    public Task<string> GenerateResponseAsync(string prompt) 
        => Task.FromResult($"V1 Response: {prompt}");
}

public class V2InferenceEngine : IAgentInference
{
    public Task<string> GenerateResponseAsync(string prompt) 
        => Task.FromResult($"V2 Response: {prompt} (Optimized)");
}

// Implementations/TrafficSplitter.cs
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;

public class TrafficSplitter : IAgentInference
{
    private readonly V1InferenceEngine _v1;
    private readonly V2InferenceEngine _v2;
    private readonly TrafficSplitConfig _config;
    private readonly ILogger<TrafficSplitter> _logger;
    private readonly Random _random = new();
    
    // Interactive Challenge: Canary Beta Testers
    private readonly ConcurrentBag<string> _betaTesters;

    public TrafficSplitter(
        V1InferenceEngine v1, 
        V2InferenceEngine v2, 
        IOptions<TrafficSplitConfig> config,
        ILogger<TrafficSplitter> logger)
    {
        _v1 = v1;
        _v2 = v2;
        _config = config.Value;
        _logger = logger;
        _betaTesters = new ConcurrentBag<string> { "user-123", "user-456" }; // Mock config
    }

    public async Task<string> GenerateResponseAsync(string prompt)
    {
        // In a real app, we would pass HttpContext here or inject IHttpContextAccessor
        // For this exercise, we simulate retrieving the header.
        // Note: In a real implementation, we'd need to inject IHttpContextAccessor 
        // to read headers inside this method.
        
        // Simulating header retrieval for the logic
        string? userId = null; // Simulate: HttpContextAccessor.HttpContext?.Request.Headers["X-User-Id"];

        // Canary Logic: Check for Beta User
        if (userId != null && _betaTesters.Contains(userId))
        {
            _logger.LogInformation($"Request routed to V2 (Canary) for User: {userId}");
            return await _v2.GenerateResponseAsync(prompt);
        }

        // Percentage Split Logic
        int roll = _random.Next(100);
        if (roll < _config.V1Percentage)
        {
            _logger.LogInformation("Request routed to V1");
            return await _v1.GenerateResponseAsync(prompt);
        }
        else
        {
            _logger.LogInformation("Request routed to V2");
            return await _v2.GenerateResponseAsync(prompt);
        }
    }
}

// Program.cs (Registration)
var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<TrafficSplitConfig>(options => 
{
    options.V1Percentage = 90;
    options.V2Percentage = 10;
});

builder.Services.AddSingleton<V1InferenceEngine>();
builder.Services.AddSingleton<V2InferenceEngine>();
builder.Services.AddScoped<TrafficSplitter>(); // Scoped to handle request-specific logic
builder.Services.AddScoped<IAgentInference>(sp => sp.GetRequiredService<TrafficSplitter>());

// ... rest of setup (Controllers, etc)
