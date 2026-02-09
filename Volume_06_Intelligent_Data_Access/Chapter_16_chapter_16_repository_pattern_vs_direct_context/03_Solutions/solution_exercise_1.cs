
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// --- Existing Interfaces and Classes (from context) ---
public interface IRepository<T> where T : class
{
    IEnumerable<T> GetAll();
    T GetById(int id);
    void Add(T entity);
}

public class Order
{
    public int Id { get; set; }
    public DateTime OrderDate { get; set; }
    public string Status { get; set; }
    public decimal TotalAmount { get; set; }
    public ICollection<OrderItem> Items { get; set; }
}

public class OrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public string ProductName { get; set; }
    public int Quantity { get; set; }
}

public class AppDbContext : DbContext
{
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
}

// --- Refactored Service ---

// DTO for Projection
public class OrderSummaryDto
{
    public int Id { get; set; }
    public decimal TotalAmount { get; set; }
}

public class OrderService
{
    private readonly AppDbContext _context;

    // Requirement 2: Accept DbContext directly
    public OrderService(AppDbContext context)
    {
        _context = context;
    }

    // Requirement 3: Optimized Query (Server-side filtering)
    public async Task<IEnumerable<Order>> GetOrdersByDateRangeAsync(DateTime start, DateTime end)
    {
        // Before: _orderRepo.GetAll().Where(...) -> Filters in memory
        // After: _context.Orders.Where(...) -> Translates to SQL WHERE clause
        return await _context.Orders
            .Where(o => o.OrderDate >= start && o.OrderDate <= end)
            .ToListAsync();
    }

    // Requirement 4: Projection
    public async Task<IEnumerable<OrderSummaryDto>> GetOrderSummariesAsync()
    {
        // Select only necessary columns to reduce network payload
        return await _context.Orders
            .Select(o => new OrderSummaryDto
            {
                Id = o.Id,
                TotalAmount = o.TotalAmount
            })
            .ToListAsync();
    }

    // Requirement 5: Eager Loading (Fixing N+1)
    public async Task<Order> GetOrderWithItemsAsync(int orderId)
    {
        // Before: Lazy loading would trigger separate queries for Items
        // After: Single query with JOIN using Include
        return await _context.Orders
            .Include(o => o.Items) // Eager load related entities
            .FirstOrDefaultAsync(o => o.Id == orderId);
    }
}
