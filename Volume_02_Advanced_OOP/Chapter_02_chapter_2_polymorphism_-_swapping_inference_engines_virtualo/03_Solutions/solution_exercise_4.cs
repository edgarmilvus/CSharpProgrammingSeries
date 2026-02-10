
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

// 1. Extended Interface
public interface IInferenceEngine
{
    string Execute(float[] input);
    bool SupportsBatchProcessing { get; }
}

// 2. Updated NeuralNetworkEngine
public class NeuralNetworkEngine : IInferenceEngine
{
    public bool SupportsBatchProcessing => true; // Property implementation

    public string Execute(float[] input)
    {
        return $"NN Prediction: {input[0] * 2}";
    }
}

// 2. Updated ExpertSystemEngine
public class ExpertSystemEngine : IInferenceEngine
{
    public bool SupportsBatchProcessing => true;

    public string Execute(float[] input)
    {
        string result = input[0] > 0.5 ? "High" : "Low";
        return $"Rule-Based Result: {result}";
    }
}

// 3. Legacy Engine
public class LegacyRuleEngine : IInferenceEngine
{
    public bool SupportsBatchProcessing => false;

    public string Execute(float[] input)
    {
        if (input.Length > 1)
        {
            throw new InvalidOperationException("Legacy engine does not support batch processing.");
        }
        return $"Legacy Result: {input[0]}";
    }
}

// 4. Updated SystemMonitor
public class SystemMonitor
{
    public void ProcessBatch(List<IInferenceEngine> engines, float[] input)
    {
        Console.WriteLine("--- Starting Extended Batch Process ---");

        foreach (IInferenceEngine engine in engines)
        {
            try
            {
                if (engine.SupportsBatchProcessing)
                {
                    // Standard path
                    string result = engine.Execute(input);
                    Console.WriteLine($"[Monitor]: {result}");
                }
                else
                {
                    // Legacy path: Manual loop
                    Console.WriteLine($"[Monitor]: Engine {engine.GetType().Name} does not support batch. Processing individually...");
                    foreach (float item in input)
                    {
                        string result = engine.Execute(new float[] { item });
                        Console.WriteLine($"  -> {result}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Monitor Error]: {ex.Message}");
            }
        }
    }
}

// Helper method for 'as' operator demonstration
public class EngineConfigurator
{
    public void ConfigureEngine(IInferenceEngine engine)
    {
        // 5. Using 'as' operator
        NeuralNetworkEngine nn = engine as NeuralNetworkEngine;
        
        if (nn != null)
        {
            Console.WriteLine("Configuring Neural Network specific parameters...");
            // nn.LearningRate = 0.01f; // (Hypothetical property)
        }
        else
        {
            Console.WriteLine("Engine is not a NeuralNetworkEngine. Skipping configuration.");
        }
    }
}

public class Program
{
    public static void Main()
    {
        var engines = new List<IInferenceEngine>
        {
            new NeuralNetworkEngine(),
            new LegacyRuleEngine()
        };

        float[] data = { 0.5f, 0.8f };

        SystemMonitor monitor = new SystemMonitor();
        monitor.ProcessBatch(engines, data);
        
        // Demonstrate 'as' operator
        EngineConfigurator config = new EngineConfigurator();
        config.ConfigureEngine(new NeuralNetworkEngine());
        config.ConfigureEngine(new ExpertSystemEngine());
    }
}
