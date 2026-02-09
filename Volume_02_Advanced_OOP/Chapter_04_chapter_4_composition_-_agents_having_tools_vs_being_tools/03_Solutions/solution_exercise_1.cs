
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

// The existing interface the Agent expects
public interface ITool
{
    string Name { get; }
    string Execute(string input);
}

// The legacy class (provided, unchangeable)
public class LegacyWeatherReader
{
    public string GetTemperatureInCelsius()
    {
        return "25°C";
    }

    public string GetWeatherCondition()
    {
        return "Sunny";
    }
}

// The Adapter Class
public class WeatherAdapter : ITool
{
    private readonly LegacyWeatherReader _reader;

    public string Name => "Weather Adapter";

    public WeatherAdapter(LegacyWeatherReader reader)
    {
        _reader = reader;
    }

    public string Execute(string input)
    {
        // Translate the generic string request to the specific legacy method calls
        if (input == "temperature")
        {
            return _reader.GetTemperatureInCelsius();
        }
        else if (input == "condition")
        {
            return _reader.GetWeatherCondition();
        }
        else
        {
            return $"Error: Unknown weather query '{input}'";
        }
    }
}

// Usage Example
public class Program
{
    public static void Main()
    {
        // 1. Instantiate the legacy component
        LegacyWeatherReader legacyReader = new LegacyWeatherReader();

        // 2. Wrap it in the adapter
        ITool adaptedTool = new WeatherAdapter(legacyReader);

        // 3. The ToolAgent can now use this tool seamlessly
        Console.WriteLine($"Tool: {adaptedTool.Name}");
        Console.WriteLine($"Query: 'temperature' -> Result: {adaptedTool.Execute("temperature")}");
        Console.WriteLine($"Query: 'condition' -> Result: {adaptedTool.Execute("condition")}");
        Console.WriteLine($"Query: 'humidity' -> Result: {adaptedTool.Execute("humidity")}");
    }
}
