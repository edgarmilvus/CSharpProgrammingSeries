
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

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Channels;

// 1. Define a telemetry interface for tracking
public interface ITelemetryService
{
    void TrackDbContextCreation(string contextId);
    void TrackDbContextDisposal(string contextId);
}

// 2. Refactored DbContext with Async Initialization Support
public class AppDbContext : DbContext
{
    private readonly ITelemetryService _telemetry;
    private readonly Guid _instanceId = Guid.NewGuid();

    public AppDbContext(DbContextOptions<AppDbContext> options, ITelemetryService telemetry) 
        : base(options)
    {
        _telemetry = telemetry;
        _telemetry.TrackDbContextCreation(_instanceId.ToString());
    }

    // Custom factory method for async initialization (e.g., vector embeddings)
    public static async Task<AppDbContext> CreateAsync(
        DbContextOptions<AppDbContext> options, 
        ITelemetryService telemetry,
        ChannelWriter<VectorEmbeddingJob> embeddingChannel)
    {
        var context = new AppDbContext(options, telemetry);
        
        // Perform async initialization logic here
        // Example: Warm up vector connections or seed initial data
        await embeddingChannel.Writer.WriteAsync(new VectorEmbeddingJob { IsWarmup = true });
        
        return context;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _telemetry.TrackDbContextDisposal(_instanceId.ToString());
        }
        base.Dispose(disposing);
    }

    protected override async ValueTask DisposeAsyncCore()
    {
        _telemetry.TrackDbContextDisposal(_instanceId.ToString());
        await base.DisposeAsyncCore();
    }
}

// 3. The Refactored Service (Thread-safe, DI-compliant)
public class UserProfileService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<UserProfileService> _logger;

    public UserProfileService(IServiceProvider serviceProvider, ILogger<UserProfileService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    // Scoped lifetime ensures a new context per request, managed by DI
    public async Task<User> GetUserAsync(int id)
    {
        // Create a scope to resolve the DbContext
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        
        // Async all the way down
        return await context.Users.FindAsync(id);
    }

    public async Task UpdateUserAsync(User user)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        
        context.Users.Update(user);
        
        // Async save to prevent blocking threads
        await context.SaveChangesAsync();
    }
}

// 4. Registration (Startup / Program.cs)
public static class ServiceExtensions
{
    public static void AddApplicationServices(this IServiceCollection services, string connectionString)
    {
        // Thread-safe Singleton for Telemetry
        services.AddSingleton<ITelemetryService, TelemetryService>();

        // Scoped lifetime is the standard for DbContext in web apps
        // It handles concurrency safely by giving each request its own instance
        services.AddDbContext<AppDbContext>((sp, options) =>
        {
            options.UseSqlServer(connectionString);
        });

        services.AddScoped<UserProfileService>();
    }
}

// Mocks for compilation
public class User { public int Id { get; set; } }
public class TelemetryService : ITelemetryService 
{
    public void TrackDbContextCreation(string id) => Console.WriteLine($"Created: {id}");
    public void TrackDbContextDisposal(string id) => Console.WriteLine($"Disposed: {id}");
}
public class VectorEmbeddingJob { public bool IsWarmup { get; set; } }
