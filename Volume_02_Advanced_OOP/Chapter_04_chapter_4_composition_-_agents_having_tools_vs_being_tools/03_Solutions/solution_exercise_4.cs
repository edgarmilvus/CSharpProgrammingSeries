
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

// 1. Define the Tool Interface
public interface ICommandTool
{
    string Name { get; }
    string Execute(int a, int b); // Specific signature for calculation
}

// 2. Create Specialized Tools
public class AddCommand : ICommandTool
{
    public string Name => "add";
    public string Execute(int a, int b) => (a + b).ToString();
}

public class SubtractCommand : ICommandTool
{
    public string Name => "subtract";
    public string Execute(int a, int b) => (a - b).ToString();
}

// 3. Create a Logger Tool (Separation of Concerns)
public class LoggerTool
{
    public void Log(string message)
    {
        Console.WriteLine($"[LOG]: {message}");
    }
}

// 4. The Refactored Agent
public class CalculatorAgent
{
    private List<ICommandTool> _commands = new List<ICommandTool>();
    private LoggerTool _logger;

    public CalculatorAgent(LoggerTool logger)
    {
        _logger = logger;
    }

    public void RegisterCommand(ICommandTool command)
    {
        _commands.Add(command);
    }

    public string Process(string operation, int x, int y)
    {
        // Find the appropriate tool
        ICommandTool tool = _commands.Find(t => t.Name == operation);
        
        if (tool == null)
        {
            _logger.Log($"Operation '{operation}' not found.");
            return "Error";
        }

        // Delegate calculation
        string result = tool.Execute(x, y);

        // Delegate logging (Composition in action)
        _logger.Log($"Executed {operation} on {x}, {y}. Result: {result}");

        return result;
    }
}

// Usage Example
public class Program
{
    public static void Main()
    {
        LoggerTool logger = new LoggerTool();
        CalculatorAgent agent = new CalculatorAgent(logger);

        agent.RegisterCommand(new AddCommand());
        agent.RegisterCommand(new SubtractCommand());

        Console.WriteLine(agent.Process("add", 10, 5));
        Console.WriteLine(agent.Process("subtract", 10, 5));
    }
}
