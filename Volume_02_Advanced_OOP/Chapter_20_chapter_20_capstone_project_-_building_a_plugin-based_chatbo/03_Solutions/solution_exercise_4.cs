
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
using System.Threading.Tasks;

// --- Reusing Definitions from Previous Exercises ---
// (Mocking dependencies for a standalone solution)

public interface IPlugin { string Name { get; } Task<string> ExecuteAsync(string input); }
public class InfoPlugin : IPlugin 
{
    public string Name => "Info";
    public async Task<string> ExecuteAsync(string input) { await Task.Delay(10); return "Version 2.0"; }
}
public class MathPlugin : IPlugin 
{
    public string Name => "Math";
    public async Task<string> ExecuteAsync(string input) { await Task.Delay(10); return $"Calculated: {double.Parse(input) * 2}"; }
}

public class Tensor 
{ 
    public double[] Values { get; }
    public Tensor(double[] values) { Values = values; }
    public override string ToString() => $"[{string.Join(", ", Values)}]";
}

public class TensorProcessor 
{
    public Tensor Process(string text) => new Tensor(new double[] { text.Length, text.Length / 2.0 });
}

public class PluginRegistry
{
    private readonly Dictionary<string, Func<IPlugin>> _factories = new();
    public void RegisterPlugin(string key, Func<IPlugin> factory) => _factories[key] = factory;
    public IPlugin? GetPlugin(string key) => _factories.TryGetValue(key, out var f) ? f() : null;
    
    // Challenge Requirement 5: Generic Registration
    public void RegisterPluginFromType<T>() where T : IPlugin, new()
    {
        string key = typeof(T).Name.Replace("Plugin", "").ToLower();
        // Lambda expression resolving the generic type
        _factories[key] = () => new T();
    }
}

// --- The Challenge Implementation ---

public class ChatBot
{
    private readonly PluginRegistry _registry;
    private readonly TensorProcessor _processor;

    public ChatBot(PluginRegistry registry, TensorProcessor processor)
    {
        _registry = registry;
        _processor = processor;
    }

    public async Task<string> RespondAsync(string userInput)
    {
        // Step A: Pass userInput to TensorProcessor to generate a context vector
        Tensor context = _processor.Process(userInput);
        
        // Step B: Analyze the vector (Mock logic: check length)
        // In a real scenario, this would be dot-product similarity
        double magnitude = Math.Sqrt(context.Values.Sum(v => v * v));
        
        // Step C: Check for Command Prefix
        string[] parts = userInput.Split(':', 2);
        string commandKey = parts[0];
        IPlugin? plugin = _registry.GetPlugin(commandKey);

        if (plugin != null)
        {
            string payload = parts.Length > 1 ? parts[1] : "";
            string result = await plugin.ExecuteAsync(payload);
            return $"[Plugin Result]: {result} (Context Vector: {context})";
        }

        // Step D: Fallback Logic using Context Vector
        if (magnitude > 50)
        {
            return "That seems like a complex query (High Tensor Magnitude). I'll need a plugin to handle that.";
        }

        return "I didn't understand the command. Context vector generated.";
    }
}

// --- Integration ---

public class CapstoneDemo
{
    public static async Task Main()
    {
        // 1. Setup Subsystems
        var registry = new PluginRegistry();
        var processor = new TensorProcessor();

        // 2. Register Plugins using Lambdas
        registry.RegisterPlugin("info", () => new InfoPlugin());
        registry.RegisterPlugin("math", () => new MathPlugin());
        
        // 3. Instantiate ChatBot
        var bot = new ChatBot(registry, processor);

        // 4. Simulate Conversation
        Console.WriteLine("--- Chatbot Session Start ---");
        
        // Case A: No plugin match, simple input (Low magnitude)
        Console.WriteLine(await bot.RespondAsync("hello world"));

        // Case B: Plugin match
        Console.WriteLine(await bot.RespondAsync("info:version"));

        // Case C: Complex input (no plugin match, triggers fallback based on vector)
        Console.WriteLine(await bot.RespondAsync("math:100")); 
    }
}
