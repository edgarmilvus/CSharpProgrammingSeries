
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

// Source File: solution_exercise_1.cs
// Description: Solution for Exercise 1
// ==========================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

// 1. Define the Plugin Interface
public interface IPlugin
{
    string Name { get; }
    Task<string> ExecuteAsync(string input);
}

// 2. Concrete Plugin Implementations
public class GreeterPlugin : IPlugin
{
    public string Name => "Greeter";
    public async Task<string> ExecuteAsync(string input)
    {
        // Simulate async work
        await Task.Delay(50); 
        return $"Hello, {input}!";
    }
}

public class CalculatorPlugin : IPlugin
{
    public string Name => "Calculator";
    public async Task<string> ExecuteAsync(string input)
    {
        await Task.Delay(50);
        // Simple parsing logic for demonstration
        if (double.TryParse(input, out double result))
        {
            return $"Result: {result * 2} (Doubled)"; 
        }
        return "Error: Invalid number format.";
    }
}

// 3. The Plugin Registry
public class PluginRegistry
{
    // Dictionary mapping keys to factory delegates
    private readonly Dictionary<string, Func<IPlugin>> _factories = new();

    public void RegisterPlugin(string key, Func<IPlugin> factory)
    {
        if (_factories.ContainsKey(key))
            throw new InvalidOperationException($"Plugin key '{key}' already registered.");
        
        _factories[key] = factory;
    }

    public IPlugin? GetPlugin(string key)
    {
        if (_factories.TryGetValue(key, out var factory))
        {
            // Invoke the delegate to create a new instance
            return factory();
        }
        return null;
    }
}

// 4. Main Program
public class Program
{
    public static async Task Main()
    {
        var registry = new PluginRegistry();

        // Registering using Lambda Expressions
        // These act as anonymous factory methods
        registry.RegisterPlugin("greet", () => new GreeterPlugin());
        registry.RegisterPlugin("calc", () => new CalculatorPlugin());

        Console.WriteLine("Chatbot Ready. Commands: 'greet:Name' or 'calc:Number'");
        
        // Simulating an input loop
        string[] inputs = { "greet:Alice", "calc:10", "unknown:test" };
        
        foreach (var input in inputs)
        {
            Console.WriteLine($"\nUser Input: {input}");
            
            // Parse command (key:payload)
            var parts = input.Split(':', 2);
            if (parts.Length != 2) 
            {
                Console.WriteLine("Invalid format.");
                continue;
            }

            var key = parts[0];
            var payload = parts[1];

            // Retrieve and execute
            var plugin = registry.GetPlugin(key);
            if (plugin != null)
            {
                var result = await plugin.ExecuteAsync(payload);
                Console.WriteLine($"Bot [{plugin.Name}]: {result}");
            }
            else
            {
                Console.WriteLine($"Bot: No plugin found for '{key}'.");
            }
        }
    }
}
