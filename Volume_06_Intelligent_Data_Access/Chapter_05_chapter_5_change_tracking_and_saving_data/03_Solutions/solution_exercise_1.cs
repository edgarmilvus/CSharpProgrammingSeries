
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

using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

// 1. Define Entity Classes using modern C# features
public record Customer
{
    public int Id { get; init; } // Init-only setter for immutability
    public required string Name { get; init; }
    public string Email { get; init; } = string.Empty;
}

public record Order
{
    public int Id { get; init; }
    public required string OrderNumber { get; init; }
    
    // Foreign Key
    public int CustomerId { get; init; }
    public Customer? Customer { get; init; } // Navigation property
    
    public List<OrderItem> Items { get; init; } = new();
}

public record OrderItem
{
    public int Id { get; init; }
    public required string ProductName { get; init; }
    public int Quantity { get; init; }
    
    public int OrderId { get; init; }
    public Order? Order { get; init; }
}

public class OrderContext : DbContext
{
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlServer("YourConnectionStringHere"); // Placeholder

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Ensure composite keys or relationships are defined if needed
        modelBuilder.Entity<OrderItem>()
            .HasOne(oi => oi.Order)
            .WithMany(o => o.Items)
            .HasForeignKey(oi => oi.OrderId);
    }
}

public class OrderService
{
    // 3. Write a method ProcessOrderAsync
    public async Task ProcessOrderAsync(OrderContext context, Order orderGraph)
    {
        // 2. Simulate disconnected state (Ids are defaults/new)
        // In a real scenario, this object comes from JSON deserialization.
        
        // 4. Explicitly attach Customer as Unchanged
        // We assume the Customer ID is valid and exists in the DB.
        // If the customer is new, we would need logic to detect that, 
        // but per requirements, we treat it as existing.
        if (orderGraph.Customer != null)
        {
            context.Attach(orderGraph.Customer); // Defaults to Unchanged if PK exists
            Console.WriteLine($"Customer State: {context.Entry(orderGraph.Customer).State}");
        }

        // 4. Explicitly attach Order as Added
        context.Orders.Add(orderGraph); // This marks Order as Added
        Console.WriteLine($"Order State: {context.Entry(orderGraph).State}");

        // 5. OrderItems are tracked as Added automatically via the relationship
        // because the parent Order is Added.
        
        // 6. Use a single SaveChangesAsync call
        // 7. Log EntityState immediately before saving
        Console.WriteLine("\n--- Entity States Before SaveChanges ---");
        foreach (var entry in context.ChangeTracker.Entries())
        {
            Console.WriteLine($"Entity: {entry.Entity.GetType().Name}, State: {entry.State}");
        }

        await context.SaveChangesAsync();
        
        Console.WriteLine("\nData persisted successfully.");
    }
}
