
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

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

// Reuse Entities from Exercise 1 (Order, OrderItem, Customer)

public class ClvDto
{
    public int CustomerId { get; set; }
    public string CustomerName { get; set; }
    public decimal TotalSpend { get; set; }
    public DateTime? LastOrderDate { get; set; }
}

public class ClvService
{
    private readonly EcommerceContext _context;

    public ClvService(EcommerceContext context)
    {
        _context = context;
    }

    // Refactored Server-Side Query
    public List<ClvDto> CalculateClv(List<int> customerIds = null, TimeSpan? lastNDays = null)
    {
        // 1. Base Query (IQueryable)
        var query = _context.Orders
            .Include(o => o.Customer)
            .AsQueryable();

        // 2. Apply Time Filter (Interactive Extension)
        if (lastNDays.HasValue)
        {
            var cutoffDate = DateTime.UtcNow - lastNDays.Value;
            query = query.Where(o => o.OrderDate >= cutoffDate);
        }

        // 3. Apply Customer Filter (Optional)
        if (customerIds != null && customerIds.Any())
        {
            query = query.Where(o => customerIds.Contains(o.CustomerId));
        }

        // 4. Grouping and Aggregation
        // We need to flatten OrderItems first, then group by Customer
        var clvResults = query
            .SelectMany(o => o.OrderItems, (order, item) => new { order.Customer, order.OrderDate, item })
            .GroupBy(x => new { x.Customer.Id, x.Customer.Name })
            .Select(g => new ClvDto
            {
                CustomerId = g.Key.Id,
                CustomerName = g.Key.Name,
                
                // 5. Handling Nulls (Null Coalescing)
                // If UnitPrice or Quantity is null, treat as 0 to prevent exceptions.
                TotalSpend = g.Sum(x => (x.item.Quantity ?? 0) * (x.item.UnitPrice ?? 0m)),
                
                LastOrderDate = g.Max(x => (DateTime?)x.OrderDate) // Cast to Nullable to handle Max on empty sets
            })
            .OrderByDescending(c => c.TotalSpend)
            .ToList();

        return clvResults;
    }
}

/*
Visualization (Graphviz DOT Format):

digraph LinqExecution {
    rankdir=LR;
    node [shape=box, style=rounded];
    
    Application [label="C# Application\n(ClvService.CalculateClv)"];
    ExpressionTree [label="IQueryable Expression Tree\n(Filter -> SelectMany -> GroupBy -> Select)"];
    SqlGenerator [label="EF Core SQL Generator"];
    Database [label="SQL Server\n(Execution Engine)"];
    Result [label="Materialized Results\n(List<ClvDto>)"];

    Application -> ExpressionTree [label="Constructs Query"];
    ExpressionTree -> SqlGenerator [label="Translates to SQL AST"];
    SqlGenerator -> Database [label="SELECT ... WHERE ... GROUP BY"];
    Database -> Result [label="Data Stream"];
}

Key Flow:
1.  The C# code builds an expression tree (not executed yet).
2.  EF Core analyzes the tree, optimizing the GroupBy to happen in the database.
3.  The SQL Generator creates a parameterized query.
4.  The Database executes the query and streams results back.
5.  EF Core hydrates the DTOs.
*/
