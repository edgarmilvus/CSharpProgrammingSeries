
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
using System.Threading;
using System.Threading.Tasks;

// 1. Define Custom Arguments
public class TensorProcessedEventArgs : EventArgs
{
    public double[] ResultTensor { get; set; }
    public long ProcessingTimeMs { get; set; }
    public string ModelName { get; set; }

    public TensorProcessedEventArgs(double[] tensor, long time, string name)
    {
        ResultTensor = tensor;
        ProcessingTimeMs = time;
        ModelName = name;
    }
}

// 2. The Publisher Class
public class AIEngine
{
    // Event declaration using the generic EventHandler<T> delegate
    public event EventHandler<TensorProcessedEventArgs> ModelFinishedThinking;

    public void RunInferenceAsync(string modelName)
    {
        Console.WriteLine($"[AI Engine] Starting inference for {modelName} on background thread...");
        
        // Simulate asynchronous work
        Task.Run(() =>
        {
            // Simulate heavy computation
            var sw = System.Diagnostics.Stopwatch.StartNew();
            Thread.Sleep(2000); 
            sw.Stop();

            // Prepare data
            double[] outputTensor = { 1.0, 2.0, 3.0 };
            
            // Create event args
            var args = new TensorProcessedEventArgs(outputTensor, sw.ElapsedMilliseconds, modelName);

            // 3. Raise the event safely
            OnModelFinishedThinking(args);
        });
    }

    // Protected virtual method to raise the event (Standard .NET Pattern)
    protected virtual void OnModelFinishedThinking(TensorProcessedEventArgs e)
    {
        // Thread-safe invocation using the null-conditional operator and delegate invocation
        // This checks if there are subscribers before invoking.
        ModelFinishedThinking?.Invoke(this, e);
    }
}

// 3. The Consumer / Subscriber
public class Program
{
    public static void Main(string[] args)
    {
        var engine = new AIEngine();

        // Subscribe to the event
        engine.ModelFinishedThinking += HandleTensorResult;

        // Trigger the async process
        engine.RunInferenceAsync("ResNet-50");

        // Keep console open to observe the async callback
        Console.WriteLine("Main thread is doing other work...");
        Console.ReadLine(); 
    }

    // Event Handler Method
    private static void HandleTensorResult(object? sender, TensorProcessedEventArgs e)
    {
        Console.WriteLine($"\n[UI Thread] Event Received!");
        Console.WriteLine($"   Model: {e.ModelName}");
        Console.WriteLine($"   Time: {e.ProcessingTimeMs}ms");
        Console.WriteLine($"   First Value: {e.ResultTensor[0]}");
    }
}
