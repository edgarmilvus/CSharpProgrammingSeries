
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

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.EntityFrameworkCore;

// Entity Definitions
public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
}

public class Order
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public DateTime OrderDate { get; set; }
    public string Status { get; set; }
    
    public Customer Customer { get; set; }
    public ICollection<OrderItem> OrderItems { get; set; }
}

public class OrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    
    public Order Order { get; set; }
    public Product Product { get; set; }
}

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Category { get; set; }
    public decimal Price { get; set; }
}

public class EcommerceContext : DbContext
{
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<Product> Products { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer("YourConnectionString");
    }
}

// DTO for Projection
public class OrderSummaryDto
{
    public int OrderId { get; set; }
    public string CustomerName { get; set; }
    public decimal TotalCategoryValue { get; set; }
    public string ItemDetails { get; set; }
}

public class OrderReporter
{
    public IQueryable<OrderSummaryDto> GetFilteredOrders(
        EcommerceContext context, 
        string category, 
        decimal minTotalValue, 
        DateTime startDate)
    {
        // 1. Base Query (IQueryable) - No execution yet
        var query = context.Orders
            .Where(o => o.OrderDate >= startDate)
            .AsQueryable();

        // 2. Handle Edge Case: Category Filtering
        // If category is null/empty, we might want to return all categories or none.
        // Here, we assume filtering by specific category if provided.
        if (!string.IsNullOrEmpty(category))
        {
            query = query.Where(o => o.OrderItems.Any(oi => oi.Product.Category == category));
        }

        // 3. Grouping and Aggregation Logic
        // We project early to shape the data, but the aggregation (Sum) happens in the final Select
        // Note: Complex aggregations over navigation properties often require SelectMany or GroupJoin.
        // However, to filter by the Sum of items *matching* the category, we need a subquery approach 
        // or a GroupBy on the client side if the database provider doesn't support complex GroupBy translation well.
        // EF Core 6+ translates GroupBy on client properties well, but here we need to filter the Order based on 
        // the sum of specific items.
        
        // Strategy: Join Orders with their specific OrderItems (filtered by category if applicable)
        // Group by Order, calculate sum, then filter.
        
        var resultQuery = context.Orders
            .Where(o => o.OrderDate >= startDate)
            .Select(o => new 
            {
                Order = o,
                // Filter items for the specific category (or all items if category is null/empty)
                RelevantItems = o.OrderItems.Where(oi => 
                    string.IsNullOrEmpty(category) || oi.Product.Category == category)
            })
            .Where(x => x.RelevantItems.Sum(oi => oi.Quantity * oi.UnitPrice) >= minTotalValue)
            .Select(x => new OrderSummaryDto
            {
                OrderId = x.Order.Id,
                CustomerName = x.Order.Customer.Name,
                TotalCategoryValue = x.RelevantItems.Sum(oi => oi.Quantity * oi.UnitPrice),
                // String concatenation in LINQ to Entities is tricky; often translated to SQL STRING_AGG
                // If translation fails, this part might execute in memory after data retrieval.
                // For SQL Server, EF Core 6+ translates string concatenation.
                ItemDetails = string.Join(", ", x.RelevantItems.Select(oi => $"{oi.Product.Name} x{oi.Quantity}"))
            });

        return resultQuery;
    }
}

/* 
Instructor's Note on Edge Cases (Comments in Code Structure):
1. Null/Empty Category: 
   - The code uses `string.IsNullOrEmpty(category)` to conditionally apply the filter. 
   - If null, `oi.Product.Category == category` is ignored (evaluates to true via OR logic), effectively including all categories.
   - Alternatively, you might want to throw an exception or return an empty set depending on business rules.

2. MinTotalValue = 0:
   - The filter `Sum(...) >= 0` will naturally include all orders with non-negative totals. 
   - If the requirement is to strictly find orders exceeding a specific value, 0 might be a valid lower bound.
   - Performance impact is negligible as it's a simple scalar comparison.

3. Null Prices/Quantities:
   - Assuming database constraints prevent nulls for UnitPrice/Quantity. 
   - If nullable, `oi.Quantity ?? 0` or `oi.UnitPrice.GetValueOrDefault()` should be used to prevent runtime exceptions during Sum calculation.
*/
