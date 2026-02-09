
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

# Source File: solution_exercise_2.cs
# Description: Solution for Exercise 2
# ==========================================

using System;
using System.Threading;
using System.Threading.Tasks;

// Reusing definitions from Exercise 1 for brevity
public class TensorProcessedEventArgs : EventArgs 
{ 
    public double[] ResultTensor { get; set; }
    public long ProcessingTimeMs { get; set; }
    public string ModelName { get; set; }
}

public class AIEngine 
{
    public event EventHandler<TensorProcessedEventArgs> ModelFinishedThinking;

    public void RunInferenceAsync(string modelName)
    {
        Task.Run(() =>
        {
            Thread.Sleep(2000);
            var args = new TensorProcessedEventArgs(new double[] { 1.0, 2.0, 3.0 }, 2000, modelName);
            ModelFinishedThinking?.Invoke(this, args);
        });
    }
}

public class Program
{
    public static void Main(string[] args)
    {
        var engine = new AIEngine();
        int handlerId = 1; // Variable to be captured by closure

        // 1. Subscription using Lambda Expression
        // We use the specific delegate type EventHandler<TensorProcessedEventArgs>
        engine.ModelFinishedThinking += (sender, e) => 
        {
            Console.WriteLine($"\n[Handler {handlerId}] Lambda received data.");
            Console.WriteLine($"   Model: {e.ModelName}");
            handlerId++; // Modifying the captured variable
        };

        // 2. Second Subscription with a different Lambda
        engine.ModelFinishedThinking += (sender, e) =>
        {
            Console.WriteLine($"[Handler 2] Processing Time: {e.ProcessingTimeMs}ms");
        };

        // Run inference
        engine.RunInferenceAsync("Transformer-XL");
        
        Console.WriteLine("Main thread waiting for async callbacks...");
        Console.ReadLine();
    }
}
