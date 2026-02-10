
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

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Microsoft.EntityFrameworkCore;

// Reuse Entities from Exercise 1 (Order, OrderItem, Product, Customer)

public class CategorySalesReport
{
    public int Year { get; set; }
    public string Category { get; set; }
    public decimal TotalRevenue { get; set; }
    public int UniqueCustomers { get; set; }
    public decimal AverageOrderValue { get; set; }
}

public class SalesAnalyticsService
{
    private readonly EcommerceContext _context;

    public SalesAnalyticsService(EcommerceContext context)
    {
        _context = context;
    }

    public List<CategorySalesReport> GetSalesReport()
    {
        // Step 1: Join the necessary tables
        var query = from order in _context.Orders
                    join item in _context.OrderItems on order.Id equals item.OrderId
                    join product in _context.Products on item.ProductId equals product.Id
                    select new
                    {
                        order.Id,
                        order.CustomerId,
                        Year = order.OrderDate.Year, // Computed Property
                        product.Category,
                        ItemValue = item.Quantity * item.UnitPrice
                    };

        // Step 2: Group by Year and Category
        var grouped = query.GroupBy(x => new { x.Year, x.Category });

        // Step 3: Calculate Aggregates
        var report = grouped.Select(g => new CategorySalesReport
        {
            Year = g.Key.Year,
            Category = g.Key.Category,
            
            // Total Revenue for this group
            TotalRevenue = g.Sum(x => x.ItemValue),
            
            // Distinct Customer Count
            UniqueCustomers = g.Select(x => x.CustomerId).Distinct().Count(),
            
            // Average Order Value (AOV)
            // Note: AOV is usually TotalRevenue / NumberOfOrders.
            // Since we are grouped by Category/Year, we need to count distinct OrderIds in this group.
            AverageOrderValue = g.Sum(x => x.ItemValue) / (g.Select(x => x.Id).Distinct().Count())
        })
        .OrderByDescending(r => r.Year)
        .ThenBy(r => r.Category)
        .ToList();

        return report;
    }

    public List<CategorySalesReport> GetSalesReportRawSql()
    {
        // Raw SQL Fallback: Useful if EF Core translation is inefficient or if using specific DB features (e.g. Cube/Rollup)
        // Note: Column aliasing in SQL must match the DTO property names or be mapped via [Column] attributes.
        
        string sql = @"
            SELECT 
                YEAR(o.OrderDate) AS Year,
                p.Category,
                SUM(oi.Quantity * oi.UnitPrice) AS TotalRevenue,
                COUNT(DISTINCT o.CustomerId) AS UniqueCustomers,
                AVG(oi.Quantity * oi.UnitPrice) AS AverageOrderValue -- Simplified AOV for demo
            FROM Orders o
            JOIN OrderItems oi ON o.Id = oi.OrderId
            JOIN Products p ON oi.ProductId = p.Id
            GROUP BY YEAR(o.OrderDate), p.Category
            ORDER BY Year DESC, p.Category";

        // Using FromSqlRaw for EF Core < 7, or Database.SqlQueryRaw for EF Core 7+
        // Assuming EF Core 7+ for modern syntax:
        return _context.Database.SqlQueryRaw<CategorySalesReport>(sql).ToList();
    }
}
