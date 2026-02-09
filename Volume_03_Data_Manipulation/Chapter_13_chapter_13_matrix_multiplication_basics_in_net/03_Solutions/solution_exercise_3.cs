
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

# Source File: solution_exercise_3.cs
# Description: Solution for Exercise 3
# ==========================================

using System;
using System.Collections.Generic;
using System.Linq;

public class Transaction
{
    public string Region { get; set; }
    public string Category { get; set; }
    public double Amount { get; set; }
}

public class GroupingExercise
{
    public static void Run()
    {
        var transactions = new List<Transaction>
        {
            new Transaction { Region = "North", Category = "Electronics", Amount = 100 },
            new Transaction { Region = "North", Category = "Books", Amount = 50 },
            new Transaction { Region = "South", Category = "Electronics", Amount = 200 },
            new Transaction { Region = "North", Category = "Electronics", Amount = 150 },
            new Transaction { Region = "South", Category = "Books", Amount = 30 }
        };

        // Grouping and Aggregation
        var salesSummary = transactions
            // Group by a composite key using an anonymous type
            .GroupBy(t => new { t.Region, t.Category }) 
            .Select(g => new
            {
                // Access the key properties
                Region = g.Key.Region,
                Category = g.Key.Category,
                // Aggregate the group members
                TotalAmount = g.Sum(t => t.Amount),
                TransactionCount = g.Count()
            })
            // Order for readable output
            .OrderBy(x => x.Region)
            .ThenBy(x => x.Category);

        foreach (var summary in salesSummary)
        {
            Console.WriteLine($"{summary.Region} - {summary.Category}: " +
                              $"Total={summary.TotalAmount}, Count={summary.TransactionCount}");
        }
    }
}
