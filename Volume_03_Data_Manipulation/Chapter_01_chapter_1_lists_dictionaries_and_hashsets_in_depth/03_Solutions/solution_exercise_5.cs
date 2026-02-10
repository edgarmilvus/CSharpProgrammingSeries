
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

public class TaskItem
{
    public string Name { get; set; }
    public int Priority { get; set; } // Lower number = Higher priority

    public TaskItem(string name, int priority)
    {
        Name = name;
        Priority = priority;
    }

    public override string ToString() => $"({Name}, P: {Priority})";
}

public class PriorityQueueSimulator
{
    // Key: Priority, Value: Queue of tasks with that priority
    private SortedDictionary<int, Queue<TaskItem>> priorityMap;

    public PriorityQueueSimulator()
    {
        // By default, SortedDictionary sorts keys in ascending order.
        // So the smallest key (highest priority) will be at the start.
        priorityMap = new SortedDictionary<int, Queue<TaskItem>>();
    }

    public void Enqueue(TaskItem item)
    {
        if (!priorityMap.ContainsKey(item.Priority))
        {
            priorityMap[item.Priority] = new Queue<TaskItem>();
        }
        
        priorityMap[item.Priority].Enqueue(item);
    }

    public TaskItem Dequeue()
    {
        if (priorityMap.Count == 0)
            throw new InvalidOperationException("Queue is empty");

        // Get the highest priority group (smallest key)
        // SortedDictionary's enumerator returns items in sorted order.
        // We just need the first one.
        var firstPriorityGroup = priorityMap.GetEnumerator();
        firstPriorityGroup.MoveNext();
        int highestPriority = firstPriorityGroup.Current.Key;
        Queue<TaskItem> queue = firstPriorityGroup.Current.Value;

        TaskItem item = queue.Dequeue();

        // If the queue for this priority is now empty, remove the key from the dictionary
        if (queue.Count == 0)
        {
            priorityMap.Remove(highestPriority);
        }

        return item;
    }

    public bool HasTasks() => priorityMap.Count > 0;
}

public class PrioritySimulatorDemo
{
    public static void Run()
    {
        var pq = new PriorityQueueSimulator();

        pq.Enqueue(new TaskItem("Low Priority A", 5));
        pq.Enqueue(new TaskItem("High Priority A", 1));
        pq.Enqueue(new TaskItem("Medium Priority A", 3));
        pq.Enqueue(new TaskItem("High Priority B", 1));
        pq.Enqueue(new TaskItem("Critical Priority", 0));

        Console.WriteLine("Processing Tasks by Priority:");
        while (pq.HasTasks())
        {
            Console.WriteLine($"Processing: {pq.Dequeue()}");
        }
    }
}
