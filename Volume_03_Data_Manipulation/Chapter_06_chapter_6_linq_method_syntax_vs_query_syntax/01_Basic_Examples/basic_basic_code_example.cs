
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

using System;
using System.Collections.Generic;
using System.Linq;

public class Transaction
{
    public string Id { get; set; }
    public double Amount { get; set; }
    public bool IsValid { get; set; }
}

public class LinqSyntaxComparison
{
    public static void Main()
    {
        // 1. The Data Source (Simulating raw input data)
        var transactions = new List<Transaction>
        {
            new Transaction { Id = "T001", Amount = 150.50, IsValid = true },
            new Transaction { Id = "T002", Amount = 0.00, IsValid = false }, // Invalid (zero amount)
            new Transaction { Id = "T003", Amount = 230.00, IsValid = true },
            new Transaction { Id = "T004", Amount = -50.00, IsValid = true }  // Invalid (negative)
        };

        // 2. Method Syntax (Fluent API using Lambda Expressions)
        // We filter for valid transactions and project the Amount.
        // This defines the query but does not execute it yet (Deferred Execution).
        var methodQuery = transactions
            .Where(t => t.IsValid && t.Amount > 0)
            .Select(t => new { OriginalId = t.Id, NormalizedAmount = t.Amount * 1.1 }); // Apply 10% tax normalization

        // 3. Query Syntax (SQL-like declarative style)
        // Equivalent logic written using 'from', 'where', and 'select' keywords.
        var querySyntax = from t in transactions
                          where t.IsValid && t.Amount > 0
                          select new { OriginalId = t.Id, NormalizedAmount = t.Amount * 1.1 };

        // 4. Immediate Execution
        // Calling .ToList() forces the query to execute and materialize the results into memory.
        // Without this, the query is just a definition.
        var results = methodQuery.ToList();

        // 5. Outputting results
        Console.WriteLine("--- Processed Transactions (Method Syntax) ---");
        foreach (var item in results)
        {
            Console.WriteLine($"ID: {item.OriginalId}, Normalized Amount: {item.NormalizedAmount}");
        }
    }
}
