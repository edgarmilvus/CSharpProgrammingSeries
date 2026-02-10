
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

// Source File: solution_exercise_3.cs
// Description: Solution for Exercise 3
// ==========================================

using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// Assuming Customer entity exists and is linked to Order
public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; }
    public ICollection<Order> Orders { get; set; }
}

public class ReportingService
{
    private readonly AppDbContext _context;

    public ReportingService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<object>> GetTopCustomersReportAsync()
    {
        DateTime thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);

        // Single Query Strategy
        var query = _context.Customers
            .Select(c => new
            {
                CustomerName = c.Name,
                // Filter Orders by Date within the projection
                RecentOrders = c.Orders.Where(o => o.OrderDate >= thirtyDaysAgo),
                
                // Aggregation 1: Total Spending
                TotalSpending = c.Orders
                    .Where(o => o.OrderDate >= thirtyDaysAgo)
                    .Sum(o => o.TotalAmount),

                // Aggregation 2: Distinct Product Count
                // Flatten items from recent orders, select ProductId, count distinct
                DistinctProductCount = c.Orders
                    .Where(o => o.OrderDate >= thirtyDaysAgo)
                    .SelectMany(o => o.Items)
                    .Select(i => i.ProductName) // Assuming ProductName is the identifier or ID exists
                    .Distinct()
                    .Count()
            })
            .Where(x => x.TotalSpending > 0) // Filter out customers with no recent orders
            .OrderByDescending(x => x.TotalSpending)
            .Take(10);

        return await query.Cast<object>().ToListAsync();
    }
}
