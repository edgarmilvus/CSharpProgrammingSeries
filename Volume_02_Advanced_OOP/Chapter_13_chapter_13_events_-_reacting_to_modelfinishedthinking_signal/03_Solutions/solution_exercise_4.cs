
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
using System.Threading;
using System.Threading.Tasks;

// Reusing AIEngine and TensorProcessedEventArgs
public class TensorProcessedEventArgs : EventArgs 
{
    public double[] ResultTensor { get; set; }
    public long ProcessingTimeMs { get; set; }
    public string ModelName { get; set; }
    // Added a flag to prevent infinite loops in chaining
    public bool IsFinalStage { get; set; } 
}

public class AIEngine
{
    public event EventHandler<TensorProcessedEventArgs> ModelFinishedThinking;

    public void RunInferenceAsync(string modelName, bool isFinalStage = false)
    {
        Task.Run(() =>
        {
            Thread.Sleep(1000); // Simulate inference
            
            double[] outputTensor = { new Random().NextDouble() };
            var args = new TensorProcessedEventArgs(outputTensor, 1000, modelName) { IsFinalStage = isFinalStage };
            
            // Thread-safe invocation
            ModelFinishedThinking?.Invoke(this, args);
        });
    }
}

public class Program
{
    public static void Main(string[] args)
    {
        var engine = new AIEngine();

        // The Challenge: Implementing the Chain via Lambda
        engine.ModelFinishedThinking += (sender, e) =>
        {
            Console.WriteLine($"[Orchestrator] {e.ModelName} finished. Output: {e.ResultTensor[0]}");

            if (!e.IsFinalStage)
            {
                if (e.ModelName == "FeatureExtractor")
                {
                    Console.WriteLine("[Orchestrator] Triggering downstream model: Classifier...");
                    // We cast sender back to AIEngine to call RunInferenceAsync
                    var sourceEngine = sender as AIEngine;
                    sourceEngine?.RunInferenceAsync("Classifier", isFinalStage: true);
                }
            }
            else
            {
                Console.WriteLine("[Orchestrator] Pipeline Complete.");
            }
        };

        // Start the pipeline
        Console.WriteLine("Starting Pipeline...");
        engine.RunInferenceAsync("FeatureExtractor");

        Console.ReadLine();
    }
}
