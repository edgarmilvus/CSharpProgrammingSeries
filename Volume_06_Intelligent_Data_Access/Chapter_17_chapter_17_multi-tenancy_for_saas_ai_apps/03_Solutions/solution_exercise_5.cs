
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

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using System.Threading.Channels;

// --- Tenant Onboarding Service ---

public class TenantOnboardingService
{
    private readonly ITenantStore _tenantStore;
    private readonly IServiceProvider _serviceProvider;

    public TenantOnboardingService(ITenantStore tenantStore, IServiceProvider serviceProvider)
    {
        _tenantStore = tenantStore;
        _serviceProvider = serviceProvider;
    }

    public async Task OnboardTenantAsync(TenantInfo newTenant)
    {
        // 1. Register Tenant
        // In a real app, this would write to a persistent tenant store (e.g., Master DB)
        // For this example, we assume the store supports adding.
        
        // 2. Initialize Data Structures
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        switch (newTenant.Strategy)
        {
            case DatabaseStrategy.DedicatedSchema:
                await CreateSchemaAsync(dbContext, newTenant.SchemaName!);
                break;
            case DatabaseStrategy.DedicatedDatabase:
                await CreateDatabaseAsync(dbContext);
                break;
            case DatabaseStrategy.SharedTable:
                // No schema creation needed, just ensure TenantId column exists (handled by migrations)
                break;
        }

        // Apply Migrations
        await dbContext.Database.MigrateAsync();

        // 3. Trigger Background Re-indexing
        // We use a Channel to communicate with a Background Service
        var channel = scope.ServiceProvider.GetRequiredService<Channel<TenantOnboardingJob>>();
        await channel.Writer.WriteAsync(new TenantOnboardingJob 
        { 
            TenantId = newTenant.Id, 
            Strategy = newTenant.Strategy 
        });
    }

    private async Task CreateSchemaAsync(AppDbContext context, string schemaName)
    {
        // SQL to create schema if not exists
        await context.Database.ExecuteSqlRawAsync($"CREATE SCHEMA IF NOT EXISTS {schemaName};");
    }

    private async Task CreateDatabaseAsync(AppDbContext context)
    {
        // Note: Creating a database usually requires a connection to 'master' or a system DB.
        // This is a simplified simulation.
        var dbName = context.Database.GetDbConnection().Database;
        await context.Database.ExecuteSqlRawAsync($"CREATE DATABASE IF NOT EXISTS {dbName};");
    }
}

public class TenantOnboardingJob
{
    public string TenantId { get; set; } = string.Empty;
    public DatabaseStrategy Strategy { get; set; }
}

// --- Background Service for Processing Onboarding ---

public class OnboardingBackgroundService : BackgroundService
{
    private readonly Channel<TenantOnboardingJob> _channel;
    private readonly IServiceProvider _serviceProvider;

    public OnboardingBackgroundService(Channel<TenantOnboardingJob> channel, IServiceProvider serviceProvider)
    {
        _channel = channel;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var job in _channel.Reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                // Process Re-indexing
                using var scope = _serviceProvider.CreateScope();
                var orchestrator = scope.ServiceProvider.GetRequiredService<RAGOrchestrator>();
                
                await orchestrator.ReIndexTenantAsync(job.TenantId);
                
                Console.WriteLine($"Onboarding complete for tenant {job.TenantId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing onboarding for {job.TenantId}: {ex.Message}");
                // Implement retry logic here (e.g., put back in queue with delay)
            }
        }
    }
}

// --- RAG Orchestrator Modification ---

public class RAGOrchestrator
{
    // Existing dependencies...
    
    public async Task ReIndexTenantAsync(string tenantId)
    {
        Console.WriteLine($"Starting re-indexing for Tenant: {tenantId}");
        
        // 1. Fetch documents for the tenant (from source or existing shared storage)
        // 2. Generate Embeddings
        // 3. Store in Vector DB with new TenantId
        
        // Simulate work
        await Task.Delay(2000); 
    }
}

// --- Main Program Simulation ---

class Program5
{
    static async Task Main(string[] args)
    {
        // Setup DI container (simplified)
        var channel = Channel.CreateUnbounded<TenantOnboardingJob>();
        var tenantStore = new JsonFileTenantStore("tenants.json");
        
        // Mock Service Provider
        var serviceProvider = new MockServiceProvider(); 
        
        var onboardingService = new TenantOnboardingService(tenantStore, serviceProvider);

        // Start Background Service
        var bgService = new OnboardingBackgroundService(channel, serviceProvider);
        var task = bgService.StartAsync(CancellationToken.None);

        // Simulate Onboarding Request
        var newTenant = new TenantInfo 
        { 
            Id = "t_new", 
            Name = "New Tenant", 
            Strategy = DatabaseStrategy.DedicatedSchema,
            SchemaName = "NewTenantSchema"
        };

        Console.WriteLine("Triggering onboarding...");
        await onboardingService.OnboardTenantAsync(newTenant);

        // Wait for background processing
        await Task.Delay(3000);
        
        await bgService.StopAsync(CancellationToken.None);
    }
}

// Mocks for compilation
public class MockServiceProvider : IServiceProvider 
{
    public object? GetService(Type serviceType) => null;
}
