
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

# Source File: solution_exercise_4.cs
# Description: Solution for Exercise 4
# ==========================================

using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

// --- Command Side (Write) ---

public class PlaceOrderCommandService
{
    private readonly AppDbContext _context;

    public PlaceOrderCommandService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<int> PlaceOrder(int customerId, decimal totalAmount, List<OrderItem> items)
    {
        // 1. Business Logic / Validation
        if (items == null || !items.Any()) throw new ArgumentException("Order must have items.");
        
        // 2. Create Entity
        var order = new Order
        {
            CustomerId = customerId, // Assuming FK exists
            OrderDate = DateTime.UtcNow,
            Status = "Pending",
            TotalAmount = totalAmount,
            Items = items
        };

        // 3. Persist
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        return order.Id;
    }
}

// --- Query Side (Read) ---

public class OrderReadModelService
{
    private readonly AppDbContext _context;

    public OrderReadModelService(AppDbContext context)
    {
        _context = context;
    }

    // Returns IQueryable to allow further composition
    public IQueryable<OrderSummaryDto> GetOrderSummary()
    {
        // Direct projection using Join
        var query = from o in _context.Orders
                    join c in _context.Customers on o.CustomerId equals c.Id
                    select new OrderSummaryDto
                    {
                        OrderId = o.Id,
                        CustomerName = c.Name,
                        TotalAmount = o.TotalAmount,
                        ItemCount = o.Items.Count
                    };

        return query;
    }
}

// --- Usage Example (Consumer) ---

public class ReportController
{
    private readonly OrderReadModelService _readService;

    public ReportController(OrderReadModelService readService)
    {
        _readService = readService;
    }

    public async Task<object> GetHighValueOrders()
    {
        // Demonstrate filtering/sorting on the returned IQueryable
        var highValueOrders = _readService.GetOrderSummary()
                                          .Where(o => o.TotalAmount > 1000)
                                          .OrderByDescending(o => o.TotalAmount)
                                          .Take(5);

        return await highValueOrders.ToListAsync();
    }
}
