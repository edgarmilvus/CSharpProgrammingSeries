
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

# Source File: solution_exercise_2.cs
# Description: Solution for Exercise 2
# ==========================================

using System;
using System.Collections.Generic;
using System.Linq;

public class Exercise2
{
    public static void Run()
    {
        var transactions = new List<int> { 50, 120, 80, 200 };
        Console.WriteLine($"Initial Transactions: [{string.Join(", ", transactions)}]");

        // 1. Deferred Query Definition
        // No execution occurs here. 'highValueQuery' is just a blueprint.
        var highValueQuery = transactions.Where(amt => amt > 100);

        // 2. Modify Source Data
        transactions.Add(150);
        Console.WriteLine($"Updated Transactions: [{string.Join(", ", transactions)}]");

        // 3. First Execution
        // The query runs now, scanning the *current* list (including 150).
        Console.WriteLine($"Count (Deferred): {highValueQuery.Count()}"); 

        // 4. Materialization (Snapshot)
        // Creates a concrete list in memory containing the results at this specific moment.
        var snapshot = highValueQuery.ToList();
        Console.WriteLine($"Snapshot Count: {snapshot.Count}");

        // 5. Modify Source Data Again
        transactions.Add(300);
        Console.WriteLine($"Final Transactions: [{string.Join(", ", transactions)}]");

        // 6. Second Execution (Deferred)
        // The deferred query re-evaluates against the updated source.
        Console.WriteLine($"Count (Deferred - Post Addition): {highValueQuery.Count()}"); 

        // 7. Snapshot Integrity
        // The snapshot remains unchanged because it is disconnected from the source.
        Console.WriteLine($"Snapshot Count (Post Addition): {snapshot.Count}");
    }
}
