
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

// Source File: solution_exercise_2.cs
// Description: Solution for Exercise 2
// ==========================================

using System;
using System.Collections.Generic;

public interface ITool
{
    string Name { get; }
    string Execute(string input);
}

public class AdderTool : ITool
{
    public string Name => "Adder";
    public string Execute(string input)
    {
        // Input format: "5,3"
        var parts = input.Split(',');
        if (parts.Length == 2 && int.TryParse(parts[0], out int a) && int.TryParse(parts[1], out int b))
        {
            return (a + b).ToString();
        }
        return "Error: Invalid input for Adder";
    }
}

public class ReverserTool : ITool
{
    public string Name => "Reverser";
    public string Execute(string input)
    {
        char[] charArray = input.ToCharArray();
        Array.Reverse(charArray);
        return new string(charArray);
    }
}

public class ToolAgent
{
    // Simulating a tensor state using a dictionary
    public Dictionary<string, string> InternalState { get; private set; }
    private List<ITool> _tools;

    public ToolAgent()
    {
        InternalState = new Dictionary<string, string>();
        _tools = new List<ITool>();
    }

    public void LearnTool(ITool tool)
    {
        if (!_tools.Contains(tool))
        {
            _tools.Add(tool);
            Console.WriteLine($"Agent learned new tool: {tool.Name}");
        }
    }

    public void ExecuteTask(string taskDescription)
    {
        string result = "No action taken.";
        string inputParam = "";

        // Simple parsing logic for demonstration
        if (taskDescription.Contains("add"))
        {
            // Extract numbers (simplified parsing)
            int firstNum = 10; // Hardcoded for simplicity in exercise
            int secondNum = 20; 
            inputParam = $"{firstNum},{secondNum}";
            
            ITool tool = _tools.Find(t => t.Name == "Adder");
            if (tool != null)
            {
                result = tool.Execute(inputParam);
                InternalState["last_result"] = result;
            }
        }
        else if (taskDescription.Contains("reverse"))
        {
            // Check if we need to reverse the previous result
            if (taskDescription.Contains("previous") && InternalState.ContainsKey("last_result"))
            {
                inputParam = InternalState["last_result"];
            }
            else
            {
                inputParam = "Hello World";
            }

            ITool tool = _tools.Find(t => t.Name == "Reverser");
            if (tool != null)
            {
                result = tool.Execute(inputParam);
                InternalState["last_result"] = result;
            }
        }

        Console.WriteLine($"Task: '{taskDescription}' | Input: '{inputParam}' | Result: {result}");
    }
}

// Usage Example
public class Program
{
    public static void Main()
    {
        ToolAgent agent = new ToolAgent();

        // Dynamic composition
        agent.LearnTool(new AdderTool());
        agent.LearnTool(new ReverserTool());

        // Execute tasks
        agent.ExecuteTask("add two numbers");
        agent.ExecuteTask("reverse text");
        agent.ExecuteTask("reverse previous"); // Uses state from the add operation
    }
}
