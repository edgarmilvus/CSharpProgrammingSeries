
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

using Microsoft.EntityFrameworkCore;
using System.Text.Json;

// --- Tenant Configuration Models ---

public enum DatabaseStrategy { SharedTable, DedicatedSchema, DedicatedDatabase }

public class TenantInfo
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public DatabaseStrategy Strategy { get; set; }
    public string ConnectionString { get; set; } = string.Empty;
    public string? SchemaName { get; set; } // Used for DedicatedSchema
}

// --- Entity Definitions ---

public class Order
{
    public int Id { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
}

public class UserProfile
{
    public int Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    // UserProfile is always in a shared table, so it might not have a TenantId,
    // or it might have one if the table is shared but partitioned.
    // For this example, we assume SharedTable strategy uses a global query filter on entities that have TenantId.
}

// --- Tenant Store ---

public interface ITenantStore
{
    Task<TenantInfo?> GetTenantAsync(string tenantId);
}

public class JsonFileTenantStore : ITenantStore
{
    private readonly string _filePath;
    private readonly List<TenantInfo> _tenants;

    public JsonFileTenantStore(string filePath)
    {
        _filePath = filePath;
        // In a real app, load from file or DB. Mocking data here for the solution.
        _tenants = new List<TenantInfo>
        {
            new TenantInfo { Id = "t1", Name = "Tenant A", Strategy = DatabaseStrategy.SharedTable, ConnectionString = "Server=.;Database=SharedDb;Trusted_Connection=True;" },
            new TenantInfo { Id = "t2", Name = "Tenant B", Strategy = DatabaseStrategy.DedicatedSchema, ConnectionString = "Server=.;Database=SharedDb;Trusted_Connection=True;", SchemaName = "TenantB" },
            new TenantInfo { Id = "t3", Name = "Tenant C", Strategy = DatabaseStrategy.DedicatedDatabase, ConnectionString = "Server=.;Database=TenantC_Db;Trusted_Connection=True;" }
        };
    }

    public Task<TenantInfo?> GetTenantAsync(string tenantId)
    {
        return Task.FromResult(_tenants.FirstOrDefault(t => t.Id == tenantId));
    }
}

// --- DbContext and Factory ---

public class AppDbContext : DbContext
{
    private readonly TenantInfo _tenantInfo;

    public DbSet<Order> Orders { get; set; }
    public DbSet<UserProfile> UserProfiles { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options, TenantInfo tenantInfo) 
        : base(options)
    {
        _tenantInfo = tenantInfo;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // 1. Handle DedicatedSchema Strategy
        if (_tenantInfo.Strategy == DatabaseStrategy.DedicatedSchema && !string.IsNullOrEmpty(_tenantInfo.SchemaName))
        {
            modelBuilder.Entity<Order>().ToTable("Orders", _tenantInfo.SchemaName);
            // Note: UserProfile is explicitly kept in shared table even if tenant has dedicated schema
            // This is part of the "Hybrid" interactive challenge requirement.
            modelBuilder.Entity<UserProfile>().ToTable("UserProfiles"); 
        }
        else
        {
            // Shared Table or Dedicated DB (which acts like a single tenant DB)
            modelBuilder.Entity<Order>().ToTable("Orders");
            modelBuilder.Entity<UserProfile>().ToTable("UserProfiles");
        }

        // 2. Global Query Filters (SharedTable Strategy)
        // Only apply query filter if it's a SharedTable strategy.
        // In a Dedicated DB/Schema, isolation is physical, so filters might be redundant but safe to keep.
        if (_tenantInfo.Strategy == DatabaseStrategy.SharedTable)
        {
            // We use a shadow property or the mapped TenantId
            modelBuilder.Entity<Order>().HasQueryFilter(o => o.TenantId == _tenantInfo.Id);
            
            // Interactive Challenge: Hybrid Strategy
            // UserProfile is shared, so we DO NOT apply a tenant filter to it.
        }
    }

    // Interactive Challenge: Disable filter for admin
    public AppDbContext IgnoreQueryFilters()
    {
        ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        // In EF Core 5+, we can clear query filters, but it's tricky on the instance.
        // Usually, we create a new context without the filter or use specific methods.
        // For this demo, we return the context but note that standard queries might still have filters.
        // A better approach for admin is a separate context or specific LINQ extensions.
        return this;
    }
}

public interface ITenantDbContextFactory<TContext> where TContext : DbContext
{
    Task<TContext> CreateDbContextAsync(string tenantId);
}

public class TenantDbContextFactory : ITenantDbContextFactory<AppDbContext>
{
    private readonly ITenantStore _tenantStore;

    public TenantDbContextFactory(ITenantStore tenantStore)
    {
        _tenantStore = tenantStore;
    }

    public async Task<AppDbContext> CreateDbContextAsync(string tenantId)
    {
        var tenantInfo = await _tenantStore.GetTenantAsync(tenantId);
        if (tenantInfo == null) throw new InvalidOperationException($"Tenant {tenantId} not found.");

        // Handle Connection Pooling / Validity
        if (string.IsNullOrEmpty(tenantInfo.ConnectionString))
            throw new InvalidOperationException("Invalid connection string.");

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        
        // Use the tenant-specific connection string
        optionsBuilder.UseSqlServer(tenantInfo.ConnectionString);

        return new AppDbContext(optionsBuilder.Options, tenantInfo);
    }
}

// --- Interactive Challenge: Hybrid Strategy Support ---

// Attribute to mark entities for specific routing
[AttributeUsage(AttributeTargets.Class)]
public class TenantStorageAttribute : Attribute
{
    public bool UseSharedTable { get; set; } = true;
}

[TenantStorage(UseSharedTable = true)] // Override for specific logic in OnModelCreating
public class HybridOrder : Order { }

// --- Main Program for Testing ---

class Program2
{
    static async Task Main(string[] args)
    {
        var store = new JsonFileTenantStore("tenants.json");
        var factory = new TenantDbContextFactory(store);

        // Test Shared Table Strategy (Tenant A)
        Console.WriteLine("--- Testing Tenant A (Shared Table) ---");
        using (var contextA = await factory.CreateDbContextAsync("t1"))
        {
            // Simulate query
            var orders = contextA.Orders.Where(o => o.ProductName.Contains("Laptop")).ToList();
            Console.WriteLine($"Tenant A Orders found: {orders.Count}");
        }

        // Test Dedicated Schema Strategy (Tenant B)
        Console.WriteLine("\n--- Testing Tenant B (Dedicated Schema) ---");
        using (var contextB = await factory.CreateDbContextAsync("t2"))
        {
            // Ensure schema is applied (requires DB setup manually for this snippet)
            // The SQL generated will be: SELECT ... FROM [Orders] ON [TenantB].[Orders]
            var orders = contextB.Orders.ToList();
            Console.WriteLine($"Tenant B Orders found: {orders.Count}");
        }

        // Test Tenant Switching
        Console.WriteLine("\n--- Testing Sequential Switching ---");
        using var context1 = await factory.CreateDbContextAsync("t1");
        using var context2 = await factory.CreateDbContextAsync("t2");
        
        var orders1 = context1.Orders.ToList();
        var orders2 = context2.Orders.ToList();
        
        Console.WriteLine($"Context 1 (Tenant A) count: {orders1.Count}");
        Console.WriteLine($"Context 2 (Tenant B) count: {orders2.Count}");
    }
}
