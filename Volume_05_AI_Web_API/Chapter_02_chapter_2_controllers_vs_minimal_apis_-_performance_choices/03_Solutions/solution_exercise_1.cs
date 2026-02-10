
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

// Source File: solution_exercise_1.cs
// Description: Solution for Exercise 1
// ==========================================

// Program.cs (Unified for both Controller and Minimal API)
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// 1. Setup Services
builder.Services.AddControllers(); // For Controller support
builder.Services.AddSingleton(new AiModelSimulator()); // Mock AI Service

var app = builder.Build();

// 2. Configure Middleware Pipeline
app.UseRouting();
app.MapControllers(); // Map Controller endpoints

// 3. Minimal API Endpoint
app.MapPost("/minimal/v1/sentiment", async (AiModelSimulator model, HttpContext context) =>
{
    // Read request body manually to match Controller binding overhead
    var request = await JsonSerializer.DeserializeAsync<SentimentRequest>(
        context.Request.Body, 
        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

    if (request?.Text is null) return Results.BadRequest();

    // Simulate AI Inference (50ms)
    await model.InferAsync();

    // Return result
    return Results.Json(new SentimentResponse 
    { 
        Sentiment = "Positive", 
        Confidence = 0.98f 
    });
});

// 4. Benchmark Runner Endpoint (Trigger via console app or browser)
app.MapGet("/run-benchmark", async (IHttpClientFactory factory) =>
{
    var client = factory.CreateClient();
    var stopwatch = Stopwatch.StartNew();
    var tasks = new List<Task>();
    
    // Concurrent requests
    for (int i = 0; i < 1000; i++)
    {
        tasks.Add(Task.Run(() => 
            client.PostAsJsonAsync("http://localhost:5000/api/v1/sentiment", new { text = "test" })));
    }
    
    await Task.WhenAll(tasks);
    stopwatch.Stop();
    
    return $"Total Time: {stopwatch.ElapsedMilliseconds}ms | Avg per req: {stopwatch.ElapsedMilliseconds / 1000.0}ms";
});

// Custom Result Type for Interactive Challenge
app.MapPost("/minimal/v1/sentiment/custom", async (AiModelSimulator model, HttpContext context) =>
{
    var request = await JsonSerializer.DeserializeAsync<SentimentRequest>(context.Request.Body);
    if (request?.Text is null) return Results.BadRequest();

    await model.InferAsync();
    
    // Using custom IResult
    return new AiResult(new SentimentResponse { Sentiment = "Positive", Confidence = 0.98f });
});

app.Run();

// --- Supporting Classes ---

public record SentimentRequest(string Text);
public record SentimentResponse(string Sentiment, float Confidence);

// Mock Service
public class AiModelSimulator
{
    public async Task InferAsync() => await Task.Delay(50);
}

// Custom IResult Implementation
public class AiResult : IResult
{
    private readonly object _payload;
    public AiResult(object payload) => _payload = payload;

    public async Task ExecuteAsync(HttpContext httpContext)
    {
        httpContext.Response.ContentType = "application/json";
        await JsonSerializer.SerializeAsync(httpContext.Response.Body, _payload);
    }
}

// Controller Implementation
[ApiController]
[Route("api/v1/sentiment")]
public class SentimentController : ControllerBase
{
    private readonly AiModelSimulator _model;

    public SentimentController(AiModelSimulator model) => _model = model;

    [HttpPost]
    public async Task<IActionResult> Analyze([FromBody] SentimentRequest request)
    {
        if (request?.Text is null) return BadRequest();

        await _model.InferAsync();

        return Ok(new SentimentResponse { Sentiment = "Positive", Confidence = 0.98f });
    }
}
