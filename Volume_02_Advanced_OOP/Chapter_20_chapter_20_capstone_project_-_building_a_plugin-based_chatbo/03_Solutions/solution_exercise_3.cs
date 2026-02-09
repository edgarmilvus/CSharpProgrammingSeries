
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

// 1. Strategy Interface
public interface IContextStrategy
{
    string Interpret(string input);
}

// 2. Concrete Strategies
public class CasualStrategy : IContextStrategy
{
    public string Interpret(string input) => $"{input} lol";
}

public class TechnicalStrategy : IContextStrategy
{
    public string Interpret(string input) => input.ToUpper() + " [TECHNICAL]";
}

public class StrictStrategy : IContextStrategy
{
    public string Interpret(string input)
    {
        if (input.Length < 5) return "Input too short.";
        return input;
    }
}

// 3. Context Manager
public class ContextManager
{
    private IContextStrategy _strategy;

    public ContextManager(IContextStrategy initialStrategy)
    {
        _strategy = initialStrategy;
    }

    public void SetStrategy(IContextStrategy strategy)
    {
        _strategy = strategy;
        Console.WriteLine($"[System] Strategy switched to: {_strategy.GetType().Name}");
    }

    public string ProcessInput(string input)
    {
        return _strategy.Interpret(input);
    }
}

// 4. Usage with Dynamic Switching
public class StrategyDemo
{
    public static void Main()
    {
        // Initialize with Casual
        var manager = new ContextManager(new CasualStrategy());

        // Simulate user input modes
        string[] inputs = { "hello", "system update", "a" };

        foreach (var input in inputs)
        {
            // Dynamic switching logic using Lambda Expressions to select strategy
            if (input.Contains("update"))
            {
                manager.SetStrategy(new TechnicalStrategy());
            }
            else if (input.Length < 3)
            {
                // Using a lambda to create the strategy instance on the fly
                // This demonstrates how delegates can be used for strategy selection
                Func<IContextStrategy> strictFactory = () => new StrictStrategy();
                manager.SetStrategy(strictFactory());
            }
            else
            {
                manager.SetStrategy(new CasualStrategy());
            }

            string result = manager.ProcessInput(input);
            Console.WriteLine($"Input: '{input}' -> Output: '{result}'");
        }
    }
}
