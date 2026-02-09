
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
using System.Collections.Generic;
using System.Linq;

public interface IWorkerTool
{
    string Name { get; }
    string PerformAction(string data);
}

// Concrete Tool 1: Validator
public class DataValidator : IWorkerTool
{
    public string Name => "DataValidator";
    public string PerformAction(string data)
    {
        bool isNumeric = double.TryParse(data, out _);
        // Returning "true" or "false" as string for pipeline compatibility
        return isNumeric ? "true" : "false"; 
    }
}

// Concrete Tool 2: Parser
public class DataParser : IWorkerTool
{
    public string Name => "DataParser";
    public string PerformAction(string data)
    {
        // Extracts digits from a string
        return new string(data.Where(char.IsDigit).ToArray());
    }
}

// The Supervisor (The Agent)
public class SupervisorAgent
{
    private List<IWorkerTool> _tools = new List<IWorkerTool>();

    public void RegisterTool(IWorkerTool tool)
    {
        _tools.Add(tool);
    }

    public string CoordinateWork(string rawData)
    {
        // 1. Get Validator
        IWorkerTool validator = _tools.Find(t => t.Name == "DataValidator");
        if (validator == null) return "Error: Validator missing";

        // 2. Validate
        string isValid = validator.PerformAction(rawData);
        if (isValid != "true")
        {
            return $"Validation Failed for input: {rawData}";
        }

        // 3. Get Parser
        IWorkerTool parser = _tools.Find(t => t.Name == "DataParser");
        if (parser == null) return "Error: Parser missing";

        // 4. Parse
        string parsedData = parser.PerformAction(rawData);
        
        return $"Success: Parsed Data -> {parsedData}";
    }
}

// Usage Example
public class Program
{
    public static void Main()
    {
        SupervisorAgent supervisor = new SupervisorAgent();
        
        // Registering tools (Composition)
        supervisor.RegisterTool(new DataValidator());
        supervisor.RegisterTool(new DataParser());

        // Execution
        Console.WriteLine(supervisor.CoordinateWork("123"));   // Valid numeric
        Console.WriteLine(supervisor.CoordinateWork("ID: 456")); // Valid string containing number
        Console.WriteLine(supervisor.CoordinateWork("ABC"));    // Invalid
    }
}
