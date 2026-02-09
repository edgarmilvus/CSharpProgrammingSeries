
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
using System.Threading;
using System.Threading.Tasks;

public class Resource
{
    public string Name { get; init; }
    private readonly object _lock = new object();
    
    public void Lock() => Monitor.Enter(_lock);
    public void Unlock() => Monitor.Exit(_lock);
}

public class DeadlockSimulator
{
    private static readonly Resource _r1 = new Resource { Name = "Resource1" };
    private static readonly Resource _r2 = new Resource { Name = "Resource2" };

    // Scenario: Tool A locks R1 then R2; Tool B locks R2 then R1
    public static void ToolA()
    {
        lock (_r1)
        {
            Console.WriteLine("ToolA: Locked Resource 1");
            Thread.Sleep(50); // Simulate work
            Console.WriteLine("ToolA: Waiting for Resource 2...");
            
            lock (_r2) // DEADLOCK RISK: ToolB might hold this
            {
                Console.WriteLine("ToolA: Locked Resource 2");
            }
        }
    }

    public static void ToolB()
    {
        lock (_r2)
        {
            Console.WriteLine("ToolB: Locked Resource 2");
            Thread.Sleep(50); // Simulate work
            Console.WriteLine("ToolB: Waiting for Resource 1...");
            
            lock (_r1) // DEADLOCK RISK: ToolA might hold this
            {
                Console.WriteLine("ToolB: Locked Resource 1");
            }
        }
    }
}

// Refactored Solution: Lock Ordering
public class SafeToolProcessor
{
    // Define a global order (e.g., by Name)
    private static int CompareResources(Resource a, Resource b) 
        => string.Compare(a.Name, b.Name, StringComparison.Ordinal);

    public static void SafeToolA(Resource r1, Resource r2)
    {
        // Ensure we always lock the "smaller" resource first
        var first = CompareResources(r1, r2) < 0 ? r1 : r2;
        var second = first == r1 ? r2 : r1;

        lock (first)
        {
            Console.WriteLine($"SafeToolA: Locked {first.Name}");
            Thread.Sleep(50);
            lock (second)
            {
                Console.WriteLine($"SafeToolA: Locked {second.Name}");
            }
        }
    }

    public static void SafeToolB(Resource r1, Resource r2)
    {
        // SAME LOGIC: Always lock the "smaller" resource first
        var first = CompareResources(r1, r2) < 0 ? r1 : r2;
        var second = first == r1 ? r2 : r1;

        lock (first)
        {
            Console.WriteLine($"SafeToolB: Locked {first.Name}");
            Thread.Sleep(50);
            lock (second)
            {
                Console.WriteLine($"SafeToolB: Locked {second.Name}");
            }
        }
    }
}

public class DeadlockTestHarness
{
    public static async Task RunDeadlockTest()
    {
        Console.WriteLine("--- Testing Deadlock Scenario ---");
        var t1 = new Thread(DeadlockSimulator.ToolA);
        var t2 = new Thread(DeadlockSimulator.ToolB);
        
        t1.Start();
        t2.Start();

        // Wait for completion with a timeout
        bool completed = await Task.Run(() => 
        {
            t1.Join();
            t2.Join();
            return true;
        }).WaitAsync(TimeSpan.FromSeconds(2)); // Short timeout to detect deadlock

        if (completed)
            Console.WriteLine("Deadlock scenario finished (rarely happens without perfect timing).");
        else
            Console.WriteLine("Deadlock detected! Program hung.");

        Console.WriteLine("\n--- Testing Safe Lock Ordering ---");
        
        // Run Safe Tools concurrently
        var tasks = new List<Task>();
        for(int i = 0; i < 10; i++)
        {
            tasks.Add(Task.Run(() => SafeToolProcessor.SafeToolA(_r1, _r2)));
            tasks.Add(Task.Run(() => SafeToolProcessor.SafeToolB(_r1, _r2)));
        }
        await Task.WhenAll(tasks);
        Console.WriteLine("Safe lock ordering completed successfully.");
    }
}
