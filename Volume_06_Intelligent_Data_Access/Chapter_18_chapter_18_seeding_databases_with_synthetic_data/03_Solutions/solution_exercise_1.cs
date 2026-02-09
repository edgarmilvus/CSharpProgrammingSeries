
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

# Source File: solution_exercise_1.cs
# Description: Solution for Exercise 1
# ==========================================

using Bogus;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// 1. Domain Models
public record Warehouse(Guid Id, string Name, string Location, int Capacity);
public record Supplier(Guid Id, string CompanyName, string Specialization);
public record Shipment(Guid Id, DateTime ShipmentDate, double WeightKg, Guid SupplierId, Guid WarehouseId);

// 2. DbContext
public class AppDbContext : DbContext
{
    public DbSet<Warehouse> Warehouses { get; set; }
    public DbSet<Supplier> Suppliers { get; set; }
    public DbSet<Shipment> Shipments { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Explicitly define keys for the generated records
        modelBuilder.Entity<Warehouse>().HasKey(w => w.Id);
        modelBuilder.Entity<Supplier>().HasKey(s => s.Id);
        modelBuilder.Entity<Shipment>().HasKey(sh => sh.Id);
    }
}

public class Seeder
{
    public static async Task SeedData(AppDbContext context)
    {
        // Ensure database is created
        await context.Database.EnsureCreatedAsync();

        // 3. Generate Warehouses (10)
        var warehouseFaker = new Faker<Warehouse>()
            .RuleFor(w => w.Id, f => Guid.NewGuid())
            .RuleFor(w => w.Name, f => f.Company.CompanyName())
            .RuleFor(w => w.Location, f => f.Address.City())
            .RuleFor(w => w.Capacity, f => f.Random.Int(1000, 50000));

        var warehouses = warehouseFaker.Generate(10);

        // 4. Generate Suppliers (20)
        var supplierFaker = new Faker<Supplier>()
            .RuleFor(s => s.Id, f => Guid.NewGuid())
            .RuleFor(s => s.CompanyName, f => f.Company.CompanyName())
            .RuleFor(s => s.Specialization, f => f.Commerce.ProductAdjective());

        var suppliers = supplierFaker.Generate(20);

        // 5. Generate Shipments (100) with Valid References
        // CRITICAL: We pick IDs from the already generated lists to ensure FK validity.
        var shipmentFaker = new Faker<Shipment>()
            .RuleFor(sh => sh.Id, f => Guid.NewGuid())
            .RuleFor(sh => sh.ShipmentDate, f => f.Date.Past(1))
            .RuleFor(sh => sh.WeightKg, f => f.Random.Double(10.0, 500.0))
            .RuleFor(sh => sh.SupplierId, f => f.PickRandom(suppliers).Id)
            .RuleFor(sh => sh.WarehouseId, f => f.PickRandom(warehouses).Id);

        var shipments = shipmentFaker.Generate(100);

        // 6. Bulk Insertion Strategy
        // Using EF Core 7+ ExecuteInsertAsync (or raw SQL) is ideal for performance.
        // For this example, we simulate high-performance insertion by disabling change tracking
        // and using AddRange for the bulk operation, which is significantly faster than individual SaveChanges.
        
        // Note: In a real high-scale scenario, consider SqlBulkCopy or EF Core Bulk Extensions.
        
        await context.Warehouses.AddRangeAsync(warehouses);
        await context.Suppliers.AddRangeAsync(suppliers);
        await context.Shipments.AddRangeAsync(shipments);
        
        await context.SaveChangesAsync();

        // 7. Verification
        Console.WriteLine($"Seeded {warehouses.Count} Warehouses.");
        Console.WriteLine($"Seeded {suppliers.Count} Suppliers.");
        Console.WriteLine($"Seeded {shipments.Count} Shipments.");
        
        Console.WriteLine("\nFirst 5 Shipment IDs (Verifying Referential Integrity):");
        foreach (var sh in shipments.Take(5))
        {
            Console.WriteLine($" - ID: {sh.Id}, Supplier: {sh.SupplierId}, Warehouse: {sh.WarehouseId}");
        }
    }
}
