
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

// Source File: solution_exercise_4.cs
// Description: Solution for Exercise 4
// ==========================================

using System;
using System.Collections.Generic;

public class AITask
{
    public string Name { get; set; }
    // Changed from custom delegate to Action
    public Action Execute { get; set; }

    public AITask(string name, Action execute)
    {
        Name = name;
        Execute = execute;
    }
}

public class Scheduler
{
    private List<AITask> _tasks = new List<AITask>();

    public void AddTask(string name, Action logic)
    {
        _tasks.Add(new AITask(name, logic));
    }

    // Higher-Order Function: Takes a factory function to generate the logic
    public void GenerateTask(string taskType, Func<string, Action> factory)
    {
        // Ask the factory to produce the logic based on the string
        Action logic = factory(taskType);

        // Safety check: Only add the task if the factory returned valid logic
        if (logic != null)
        {
            AddTask(taskType, logic);
        }
        else
        {
            Console.WriteLine($"Warning: Unknown task type '{taskType}' ignored.");
        }
    }

    public void RunAll()
    {
        Console.WriteLine("--- Executing Scheduler Queue ---");
        foreach (var task in _tasks)
        {
            Console.Write($"[{task.Name}]: ");
            // Invoke the action
            task.Execute();
        }
    }
}

public class Program
{
    public static void Main()
    {
        Scheduler scheduler = new Scheduler();

        // Define the Factory Lambda
        // This lambda takes a string (taskType) and returns an Action (the logic to run)
        Func<string, Action> taskFactory = (type) =>
        {
            // Switch expression returning Lambdas
            return type switch
            {
                "Scan" => () => Console.WriteLine("Scanning environment..."),
                "Attack" => () => Console.WriteLine("Engaging target!"),
                "Defend" => () => Console.WriteLine("Shields up."),
                _ => null // Default case returns null
            };
        };

        // Generate tasks using the factory
        scheduler.GenerateTask("Scan", taskFactory);
        scheduler.GenerateTask("Attack", taskFactory);
        scheduler.GenerateTask("Defend", taskFactory);
        
        // Test the safety check with an invalid type
        scheduler.GenerateTask("Fly", taskFactory);

        // Execute the queue
        scheduler.RunAll();
    }
}
