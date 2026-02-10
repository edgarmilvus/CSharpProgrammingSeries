
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

// Source File: basic_basic_code_example.cs
// Description: Basic Code Example
// ==========================================

using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

// 1. Domain Model
// We add a 'TenantId' property to every data entity.
// This is the "Multi-Tenant Contract".
public abstract class BaseEntity
{
    public int Id { get; set; }
    public string TenantId { get; set; } = string.Empty; // The Discriminator
}

public class Note : BaseEntity
{
    public string Content { get; set; } = string.Empty;
}

// 2. DbContext Configuration
// This is where the "Magic" happens. We intercept the SQL generation.
public class AppDbContext : DbContext
{
    private readonly string _currentTenantId;

    // We inject the Tenant ID via the constructor.
    public AppDbContext(DbContextOptions<AppDbContext> options, ITenantContext tenantContext) 
        : base(options)
    {
        _currentTenantId = tenantContext.TenantId;
    }

    public DbSet<Note> Notes => Set<Note>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // CRITICAL: Global Query Filter
        // This ensures that EVERY query against the 'Notes' table 
        // automatically appends "WHERE TenantId == 'Acme'".
        // It is impossible to accidentally query all tenants.
        modelBuilder.Entity<Note>().HasQueryFilter(n => n.TenantId == _currentTenantId);
        
        // Optional: Index optimization for performance
        modelBuilder.Entity<Note>().HasIndex(n => n.TenantId);
    }

    // CRITICAL: Automatic Tenant Tagging
    // This ensures that when we INSERT data, we don't have to manually
    // remember to set the TenantId. It happens automatically.
    public override int SaveChanges()
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Property(e => e.TenantId).CurrentValue = _currentTenantId;
            }
        }
        return base.SaveChanges();
    }
}

// 3. Tenant Context
// A simple service to hold the "Current User's Tenant".
public interface ITenantContext
{
    string TenantId { get; }
}

public class TenantContext : ITenantContext
{
    public string TenantId { get; set; } = "Acme"; // Defaults to Acme for this demo
}

// 4. Main Execution Logic
public class Program
{
    public static async Task Main()
    {
        Console.WriteLine("--- Multi-Tenancy Isolation Demo ---\n");

        // SETUP: We use InMemory DB for the demo.
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "MultiTenantDb")
            .Options;

        // SCENARIO 1: User from "Acme Corp" logs in
        Console.WriteLine("1. Processing ACME CORP user...");
        var acmeContext = new TenantContext { TenantId = "Acme" };
        
        // Create DB and Seed Data for Acme
        using (var dbAcme = new AppDbContext(options, acmeContext))
        {
            await dbAcme.Database.EnsureCreatedAsync(); // Initialize DB
            dbAcme.Notes.Add(new Note { Content = "Acme's Secret Strategy" });
            await dbAcme.SaveChangesAsync();
            Console.WriteLine("   -> Acme saved: 'Acme's Secret Strategy'");
        }

        // SCENARIO 2: User from "Beta Inc" logs in
        Console.WriteLine("\n2. Processing BETA INC user...");
        var betaContext = new TenantContext { TenantId = "Beta" };

        // Create DB and Seed Data for Beta
        using (var dbBeta = new AppDbContext(options, betaContext))
        {
            dbBeta.Notes.Add(new Note { Content = "Beta's Marketing Plan" });
            await dbBeta.SaveChangesAsync();
            Console.WriteLine("   -> Beta saved: 'Beta's Marketing Plan'");
        }

        // SCENARIO 3: The "Security Check"
        // Let's pretend we are a developer debugging the system.
        // We create a NEW DbContext instance. We don't know which tenant we are in yet.
        
        // A. Check as Acme
        Console.WriteLine("\n3. Querying as ACME (Should see only Acme data):");
        using (var dbQueryAcme = new AppDbContext(options, acmeContext))
        {
            var notes = dbQueryAcme.Notes.ToList(); // No 'Where' clause needed!
            foreach (var note in notes)
            {
                Console.WriteLine($"   - Found: {note.Content}");
            }
        }

        // B. Check as Beta
        Console.WriteLine("\n4. Querying as BETA (Should see only Beta data):");
        using (var dbQueryBeta = new AppDbContext(options, betaContext))
        {
            var notes = dbQueryBeta.Notes.ToList(); // No 'Where' clause needed!
            foreach (var note in notes)
            {
                Console.WriteLine($"   - Found: {note.Content}");
            }
        }

        // C. Check as "Super Admin" (Simulating a bug where TenantId is null)
        // This simulates a developer creating a context without setting a tenant.
        Console.WriteLine("\n5. Attempting to query as 'Super Admin' (TenantId = null):");
        var adminContext = new TenantContext { TenantId = null! };
        using (var dbAdmin = new AppDbContext(options, adminContext))
        {
            // With Global Query Filters active, this will likely return 0 results
            // because 'null == "Acme"' is false.
            var notes = dbAdmin.Notes.ToList();
            Console.WriteLine($"   - Result count: {notes.Count}");
            Console.WriteLine("   -> Notice: The Query Filter protects data even if the tenant context is broken.");
        }
    }
}
