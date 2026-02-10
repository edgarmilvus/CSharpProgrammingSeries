
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

// 1. Define the interface
public interface IInferenceEngine
{
    string Execute(float[] input);
}

// 2. Create Concrete Class: NeuralNetworkEngine
public class NeuralNetworkEngine : IInferenceEngine
{
    public string Execute(float[] input)
    {
        // Requirement: "NN Prediction: " + (input[0] * 2)
        // We assume input is not empty for this specific logic.
        return $"NN Prediction: {input[0] * 2}";
    }
}

// 2. Create Concrete Class: ExpertSystemEngine
public class ExpertSystemEngine : IInferenceEngine
{
    public string Execute(float[] input)
    {
        // Requirement: "Rule-Based Result: " + (input[0] > 0.5 ? "High" : "Low")
        string result = input[0] > 0.5 ? "High" : "Low";
        return $"Rule-Based Result: {result}";
    }
}

// 3. Create SystemMonitor Class
public class SystemMonitor
{
    public void ProcessBatch(List<IInferenceEngine> engines, float[] input)
    {
        Console.WriteLine("--- Starting Batch Process ---");
        
        // Iterate through the list. The type is IInferenceEngine, 
        // but the objects are concrete implementations.
        foreach (IInferenceEngine engine in engines)
        {
            // Polymorphic call: the specific Execute method is resolved at runtime
            string result = engine.Execute(input);
            Console.WriteLine($"[Monitor Log]: {result}");
        }
        
        Console.WriteLine("--- Batch Process Complete ---\n");
    }
}

// Main Program to run the exercise
public class Program
{
    public static void Main()
    {
        // Instantiate the engines
        IInferenceEngine nn = new NeuralNetworkEngine();
        IInferenceEngine es = new ExpertSystemEngine();

        // Create the heterogeneous list
        List<IInferenceEngine> engineBatch = new List<IInferenceEngine> { nn, es };

        // Input data
        float[] data = { 0.6f };

        // Run the monitor
        SystemMonitor monitor = new SystemMonitor();
        monitor.ProcessBatch(engineBatch, data);
        
        // Demonstration of compile-time safety:
        // engineBatch.Add(new DataPreprocessor()); // This would cause a compile error
    }
}

// Dummy class to demonstrate compile error mentioned in discussion
public class DataPreprocessor 
{
    public void CleanData() { }
}
