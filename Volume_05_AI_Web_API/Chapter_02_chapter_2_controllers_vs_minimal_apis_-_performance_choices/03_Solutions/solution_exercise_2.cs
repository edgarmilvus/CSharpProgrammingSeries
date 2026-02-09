
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

# Source File: solution_exercise_2.cs
# Description: Solution for Exercise 2
# ==========================================

using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

// 1. Service Definition & Registration
builder.Services.AddControllers();

// Registering with different lifetimes for comparison
builder.Services.AddSingleton<IHeavyComputationService, HeavyComputationService>(); 
// Note: In a real load test, you would run separate tests for Scoped/Transient.

var app = builder.Build();

app.UseRouting();
app.MapControllers();

// 2. Controller Endpoint (Constructor Injection)
[ApiController]
[Route("api/heavy")]
public class HeavyController : ControllerBase
{
    private readonly IHeavyComputationService _service;
    public HeavyController(IHeavyComputationService service) => _service = service;

    [HttpPost]
    public IActionResult Compute() => Ok(_service.PerformComputation());
}

// 3. Minimal API Endpoint (Lambda Injection)
app.MapPost("/minimal/heavy", (IHeavyComputationService svc) => 
{
    return Results.Ok(svc.PerformComputation());
});

// 4. Benchmarking Endpoint
app.MapGet("/benchmark-di", async (IHttpClientFactory factory) =>
{
    var client = factory.CreateClient();
    long initialMemory = GC.GetTotalMemory(true);
    var stopwatch = Stopwatch.StartNew();
    
    var tasks = new List<Task>();
    for (int i = 0; i < 500; i++)
    {
        // Toggle between controller and minimal paths for testing
        tasks.Add(client.PostAsJsonAsync("http://localhost:5000/api/heavy", {}));
    }
    
    await Task.WhenAll(tasks);
    
    stopwatch.Stop();
    long finalMemory = GC.GetTotalMemory(false);
    
    return new 
    { 
        TimeMs = stopwatch.ElapsedMilliseconds, 
        MemoryAllocatedBytes = finalMemory - initialMemory,
        GCollections = GC.CollectionCount(0) + GC.CollectionCount(1) + GC.CollectionCount(2)
    };
});

app.Run();

// --- Supporting Classes ---

public interface IHeavyComputationService 
{
    byte[] PerformComputation();
}

public class HeavyComputationService : IHeavyComputationService
{
    // Simulates memory pressure
    private readonly byte[] _data = new byte[1024 * 10]; // 10KB

    public byte[] PerformComputation()
    {
        // Simulate CPU work
        Array.Copy(_data, _data, _data.Length);
        return _data;
    }
}
