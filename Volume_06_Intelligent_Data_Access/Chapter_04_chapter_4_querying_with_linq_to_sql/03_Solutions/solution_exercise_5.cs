
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

// Source File: solution_exercise_5.cs
// Description: Solution for Exercise 5
// ==========================================

using System;
using System.Collections.Generic;
using System.Data.SqlClient; // Or Microsoft.Data.SqlClient
using System.Linq;
using Microsoft.EntityFrameworkCore;

// Reuse Entities from Exercise 1

public class OrderItemWithRunningTotal
{
    public int OrderItemId { get; set; }
    public int OrderId { get; set; }
    public string ProductName { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal RunningTotal { get; set; }
}

public class AdvancedQueryService
{
    private readonly EcommerceContext _context;

    public AdvancedQueryService(EcommerceContext context)
    {
        _context = context;
    }

    public List<OrderItemWithRunningTotal> GetRunningTotals(int orderId)
    {
        // 1. Parameterized SQL Query
        // Using FromSqlRaw requires careful handling of parameters to prevent SQL Injection.
        // We use standard SQL placeholders (@OrderId) and pass SqlParameter objects.
        
        string sql = @"
            SELECT 
                oi.Id AS OrderItemId,
                oi.OrderId,
                p.Name AS ProductName,
                oi.UnitPrice,
                SUM(oi.Quantity * oi.UnitPrice) OVER (
                    PARTITION BY oi.OrderId 
                    ORDER BY oi.Id 
                    ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW
                ) AS RunningTotal
            FROM OrderItems oi
            JOIN Products p ON oi.ProductId = p.Id
            WHERE oi.OrderId = @OrderId
            ORDER BY oi.Id";

        // 2. Execution using EF Core 7+ Database.SqlQueryRaw
        // This method is safer as it maps directly to the DTO and handles the connection.
        var parameters = new[]
        {
            new SqlParameter("@OrderId", System.Data.SqlDbType.Int) { Value = orderId }
        };

        // Note: In EF Core 7+, Database.SqlQueryRaw<T> is the preferred way for unmapped types.
        // For older versions, you might need to register the DTO in OnModelCreating or use a raw connection.
        var results = _context.Database.SqlQueryRaw<OrderItemWithRunningTotal>(sql, parameters).ToList();

        return results;
    }

    /* 
    Error Handling & Schema Changes:
    
    1. Column Mapping:
       - EF Core maps result columns to DTO properties by name (case-insensitive usually).
       - If the database column is 'UnitPrice' and DTO is 'UnitPrice', it maps.
       - If the database column changes (e.g., to 'PricePerUnit'), the query breaks at runtime 
         (InvalidOperationException: "The column 'PricePerUnit' was not found").
    
    2. Mitigation:
       - Use SQL Aliases (AS) in the raw query to match the DTO property names explicitly.
       - Unit Tests: Create integration tests that run against a test database to catch schema/query mismatches.
       - Strongly Typed Wrappers: Avoid magic strings for column names in the SQL string if possible, though hard with raw SQL.
    
    3. Security:
       - NEVER use string interpolation for SQL values (e.g., $"... WHERE Id = {id}").
       - Always use SqlParameter or the parameterization provided by FromSqlRaw/SqlQueryRaw.
       - The code above uses SqlParameter, ensuring the input is treated as data, not executable code.
    */
}
