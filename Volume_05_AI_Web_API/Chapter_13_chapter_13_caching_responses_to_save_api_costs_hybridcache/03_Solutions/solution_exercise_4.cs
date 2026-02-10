
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

// Source File: solution_exercise_4.cs
// Description: Solution for Exercise 4
// ==========================================

using Microsoft.Extensions.Caching.Hybrid;

// 1. Configuration
public class ModelConfig
{
    public string Version { get; set; } = "v1.0";
}

public interface IAIService
{
    Task<string> PredictAsync(string input, CancellationToken ct = default);
}

public class AIService : IAIService
{
    private readonly HybridCache _cache;
    private readonly ModelConfig _modelConfig;

    public AIService(HybridCache cache, ModelConfig modelConfig)
    {
        _cache = cache;
        _modelConfig = modelConfig;
    }

    public async Task<string> PredictAsync(string input, CancellationToken ct = default)
    {
        // Composite key including version for safety, though tags handle invalidation
        var key = $"predict:{_modelConfig.Version}:{input}";

        return await _cache.GetOrCreateAsync(
            key,
            async (ct) =>
            {
                // Simulate expensive prediction
                await Task.Delay(200, ct);
                return $"Prediction for '{input}' using model {_modelConfig.Version}";
            },
            new HybridCacheEntryOptions
            {
                Expiration = TimeSpan.FromHours(1),
                // 3. Tagging Strategy
                Tags = new HashSet<string> { $"model-{_modelConfig.Version}" }
            },
            ct
        );
    }
}

// Program.cs
var builder = WebApplication.CreateBuilder(args);

// Inject ModelConfig
var modelConfig = new ModelConfig { Version = "v1.0" };
builder.Services.AddSingleton(modelConfig);

builder.Services.AddHybridCache(options =>
{
    options.DefaultEntryOptions = new HybridCacheEntryOptions { Expiration = TimeSpan.FromMinutes(10) };
});

builder.Services.AddSingleton<IAIService, AIService>();

var app = builder.Build();

// 4. Admin Endpoint for Invalidation
app.MapPost("/admin/model/update", async (HybridCache cache, ModelConfig config, HttpContext ctx) =>
{
    var oldVersion = config.Version;
    
    // Increment version logic
    config.Version = config.Version == "v1.0" ? "v1.1" : "v2.0"; // Simple toggle for demo

    // Remove by tag
    // Note: RemoveByTagAsync is available in HybridCache
    await cache.RemoveByTagAsync($"model-{oldVersion}");

    return Results.Ok($"Model updated from {oldVersion} to {config.Version}. Cache cleared for old version.");
});

// 5. User Endpoint
app.MapGet("/predict", async (string input, IAIService service, CancellationToken ct) =>
{
    var result = await service.PredictAsync(input, ct);
    return Results.Ok(result);
});

app.Run();
