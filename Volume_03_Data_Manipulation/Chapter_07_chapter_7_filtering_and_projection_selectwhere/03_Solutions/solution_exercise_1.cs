
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

using System;
using System.Collections.Generic;
using System.Linq;

public class User
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public int Age { get; set; }
    public bool IsSubscribed { get; set; }
}

public class Exercise1
{
    public static void Run()
    {
        // 1. Data Setup
        var users = new List<User>
        {
            new User { Id = 1, Name = "Alice", Email = "alice@example.com", Age = 25, IsSubscribed = true },
            new User { Id = 2, Name = "Bob", Email = "  ", Age = 17, IsSubscribed = true }, // Invalid: Age, Email
            new User { Id = 3, Name = "Charlie", Email = null, Age = 30, IsSubscribed = false }, // Invalid: Subscribed
            new User { Id = 4, Name = "Diana", Email = "diana@example.com", Age = 35, IsSubscribed = true },
            new User { Id = 5, Name = "Eve", Email = "eve@example.com", Age = -5, IsSubscribed = true } // Invalid: Age
        };

        // 2. LINQ Query (Declarative Pipeline)
        // We chain Where clauses for readability. 
        // Execution is deferred until ToList() is called.
        var emailQuery = users
            .Where(u => u.IsSubscribed)           // Filter 1: Must be subscribed
            .Where(u => u.Age >= 18)              // Filter 2: Must be adult
            .Where(u => !string.IsNullOrWhiteSpace(u.Email)) // Filter 3: Valid email string
            .Select(u => new                      // Projection: Transform to new shape
            {
                UserId = u.Id,
                EmailAddress = u.Email.ToUpper()  // Transformation: Uppercase email
            });

        // 3. Materialization
        // .ToList() triggers the execution of the pipeline.
        var results = emailQuery.ToList();

        // 4. Output
        Console.WriteLine("Valid Subscribed Users:");
        foreach (var user in results)
        {
            Console.WriteLine($"ID: {user.UserId}, Email: {user.EmailAddress}");
        }
    }
}
