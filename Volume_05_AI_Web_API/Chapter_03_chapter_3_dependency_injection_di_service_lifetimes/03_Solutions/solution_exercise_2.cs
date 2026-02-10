
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

// Source File: solution_exercise_2.cs
// Description: Solution for Exercise 2
// ==========================================

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

// ---------------------------------------------------------
// 1. CORRECT SERVICE LIFETIMES
// ---------------------------------------------------------
public static class ServiceRegistration
{
    public static void RegisterServices(IServiceCollection services)
    {
        // DbContext is registered as Scoped.
        // Why NOT Singleton?
        // 1. Change Tracking: DbContext tracks entity state. If reused across requests, 
        //    entities from User A might appear in User B's query results.
        // 2. Connection Pooling: EF Core manages connections. A Singleton would hold a connection 
        //    open indefinitely, exhausting the connection pool.
        // 3. Memory Leaks: The DbContext cache grows indefinitely, never releasing tracked entities.
        services.AddDbContext<MyDbContext>(options => 
            options.UseSqlServer("..."));

        // Stateful manager must be Scoped to match the request lifecycle.
        services.AddScoped<ConversationStateManager>();
        
        // Register the Queue as Singleton (it's a thread-safe communication channel)
        services.AddSingleton<ConcurrentQueue<PromptRequest>>();
        
        // Register the Background Service as Singleton
        services.AddHostedService<BackgroundInferenceService>();
    }
}

// ---------------------------------------------------------
// 2. & 3. BACKGROUND SERVICE WITH SCOPE FACTORY
// ---------------------------------------------------------
public class BackgroundInferenceService : IHostedService
{
    private readonly ConcurrentQueue<PromptRequest> _queue;
    private readonly IServiceScopeFactory _scopeFactory;
    private Timer _timer;

    public BackgroundInferenceService(
        ConcurrentQueue<PromptRequest> queue, 
        IServiceScopeFactory scopeFactory)
    {
        _queue = queue;
        _scopeFactory = scopeFactory;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        // Poll the queue every 2 seconds
        _timer = new Timer(ProcessQueue, null, TimeSpan.Zero, TimeSpan.FromSeconds(2));
        return Task.CompletedTask;
    }

    private void ProcessQueue(object state)
    {
        if (!_queue.TryDequeue(out var request)) return;

        // CRITICAL: Create a new scope to resolve Scoped services.
        // We cannot inject DbContext directly into this Singleton service.
        using (var scope = _scopeFactory.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<MyDbContext>();
            // Resolve the PredictionEngine (registered as Scoped in Exercise 1)
            var predictionEngine = scope.ServiceProvider.GetRequiredService<PredictionEngine<ModelInput, ModelOutput>>();

            // Perform inference
            var result = predictionEngine.Predict(new ModelInput { Text = request.Prompt });

            // Save to DB
            dbContext.Results.Add(new Result { Output = result.Sentiment.ToString() });
            dbContext.SaveChanges();
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _timer?.Dispose();
        return Task.CompletedTask;
    }
}

// ---------------------------------------------------------
// INTERACTIVE CHALLENGE: SCOPE LEAK FIX
// ---------------------------------------------------------

// BAD: Leaky Singleton
public class LeakyService
{
    private readonly MyDbContext _context;

    public LeakyService(MyDbContext context) // Captures Scoped service in Singleton
    {
        _context = context;
    }

    public void SaveData()
    {
        // This will throw ObjectDisposedException or access wrong data
        _context.SaveChanges(); 
    }
}

// GOOD: Fixed Singleton using Scope Factory
public class FixedLeakyService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public FixedLeakyService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public void SaveData()
    {
        // Create a fresh scope for this operation
        using (var scope = _scopeFactory.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<MyDbContext>();
            dbContext.SaveChanges();
        }
    }
}

// Dummy classes
public class MyDbContext : DbContext { }
public class PromptRequest { public string Prompt { get; set; } }
public class Result { public string Output { get; set; } }
public class ModelInput { public string Text { get; set; } }
public class ModelOutput { public float Sentiment { get; set; } }
