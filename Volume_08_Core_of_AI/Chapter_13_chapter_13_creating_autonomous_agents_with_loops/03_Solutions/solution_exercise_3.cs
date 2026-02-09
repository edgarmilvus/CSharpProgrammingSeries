
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

using Microsoft.SemanticKernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class ExpenseReconciler
{
    public class Expense
    {
        public string Name { get; set; }
        public decimal Amount { get; set; }
    }

    public static async Task RunAsync()
    {
        // 1. Setup Kernel
        var builder = Kernel.CreateBuilder();
        builder.AddOpenAIChatCompletion("gpt-4o-mini", Environment.GetEnvironmentVariable("OPENAI_API_KEY"));
        
        // Register Native Functions
        builder.Plugins.AddFromType<ExpenseTools>();
        
        var kernel = builder.Build();

        // 2. State Management
        var expenses = new List<Expense>
        {
            new Expense { Name = "Laptop", Amount = 1500m },
            new Expense { Name = "Software License", Amount = 300m },
            new Expense { Name = "Office Chair", Amount = 400m },
            new Expense { Name = "Coffee Machine", Amount = 100m }
        };
        
        decimal budget = 1000m;
        Console.WriteLine($"Initial Budget: ${budget}");
        Console.WriteLine("Initial Expenses: " + string.Join(", ", expenses.Select(e => $"{e.Name}(${e.Amount})")));
        Console.WriteLine();

        // 3. The Reconciliation Loop
        bool keepReconciling = true;
        
        while (keepReconciling)
        {
            // A. Calculate Total using Kernel Function
            // We pass the list to the kernel function
            var totalResult = await kernel.InvokeAsync<decimal>("ExpenseTools", "CalculateTotal", 
                new KernelArguments { ["expenses"] = expenses });
            
            Console.WriteLine($"Current Total: ${totalResult}");

            // B. Check Condition
            if (totalResult <= budget || expenses.Count == 0)
            {
                Console.WriteLine("Budget condition met or no expenses left.");
                keepReconciling = false;
                continue;
            }

            // C. Find Highest Expense using Kernel Function
            var highestResult = await kernel.InvokeAsync<Expense>("ExpenseTools", "FindHighestExpense", 
                new KernelArguments { ["expenses"] = expenses });
            
            // D. Rejection Logic
            Console.WriteLine($"  -> Rejected: {highestResult.Name} - ${highestResult.Amount}");
            expenses.Remove(highestResult);
        }

        // 4. Final Output
        Console.WriteLine("\n--- Final Approved Expenses ---");
        if (expenses.Count == 0)
        {
            Console.WriteLine("All expenses were rejected.");
        }
        else
        {
            foreach (var exp in expenses)
            {
                Console.WriteLine($"- {exp.Name}: ${exp.Amount}");
            }
            Console.WriteLine($"Remaining Budget: ${budget - expenses.Sum(e => e.Amount)}");
        }
    }
}

// Native Functions (Tools)
public class ExpenseTools
{
    [KernelFunction]
    public decimal CalculateTotal(List<Expense> expenses)
    {
        // Using standard LINQ here for the calculation logic within the function
        return expenses.Sum(e => e.Amount);
    }

    [KernelFunction]
    public Expense FindHighestExpense(List<Expense> expenses)
    {
        return expenses.OrderByDescending(e => e.Amount).First();
    }
}
