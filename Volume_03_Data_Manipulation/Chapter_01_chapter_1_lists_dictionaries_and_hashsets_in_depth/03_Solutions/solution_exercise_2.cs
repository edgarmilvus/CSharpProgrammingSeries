
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

public class CollisionItem
{
    public int Id { get; set; }

    public CollisionItem(int id) { Id = id; }

    // Force collisions: Only 5 buckets exist (0, 1, 2, 3, 4)
    public override int GetHashCode()
    {
        return Id % 5;
    }

    public override bool Equals(object obj)
    {
        return obj is CollisionItem other && this.Id == other.Id;
    }
}

public class CollisionSimulator
{
    public static void Run()
    {
        var set = new HashSet<CollisionItem>();
        
        // Add items 1 through 20
        for (int i = 1; i <= 20; i++)
        {
            set.Add(new CollisionItem(i));
        }

        Console.WriteLine("Visualizing HashSet Internal Buckets (Simulated via Hash Code):");
        
        // We cannot access internal buckets directly, so we iterate and group manually
        // to show where items landed.
        var buckets = new Dictionary<int, List<int>>();
        
        foreach (var item in set)
        {
            int bucketIndex = item.GetHashCode();
            if (!buckets.ContainsKey(bucketIndex))
            {
                buckets[bucketIndex] = new List<int>();
            }
            buckets[bucketIndex].Add(item.Id);
        }

        // Manual iteration (No LINQ)
        foreach (var kvp in buckets)
        {
            Console.Write($"Bucket {kvp.Key}: ");
            foreach (var id in kvp.Value)
            {
                Console.Write($"[{id}] ");
            }
            Console.WriteLine();
        }

        Console.WriteLine("\nAnalysis:");
        Console.WriteLine("Even though we added 20 items, they are distributed into only 5 buckets.");
        Console.WriteLine("Bucket 0 contains items: 5, 10, 15, 20.");
        Console.WriteLine("If we search for Item 20, the HashSet finds Bucket 0 via hash.");
        Console.WriteLine("Then it must check: 5? No. 10? No. 15? No. 20? Yes.");
        Console.WriteLine("This is effectively a linear scan inside the bucket. O(N) inside the bucket.");
        Console.WriteLine("To fix this in production, we would need a better hash function or a larger initial bucket count.");
    }
}
