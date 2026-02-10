
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

// Source File: solution_exercise_3.cs
// Description: Solution for Exercise 3
// ==========================================

using System;
using System.Threading;
using System.Threading.Tasks;

// Reusing TensorProcessedEventArgs and AIEngine from previous exercises
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

// 1. The Fake UI Component
public class FakeUI
{
    public string StatusText { get; set; } = "Idle";
    public int UIThreadId { get; private set; }

    public FakeUI()
    {
        // Assume the UI is created on the main thread
        UIThreadId = Thread.CurrentThread.ManagedThreadId;
    }

    public void Render()
    {
        // Simulates a UI render loop
        Console.WriteLine($"[UI Render] Current Status: {StatusText} (Thread: {Thread.CurrentThread.ManagedThreadId})");
    }
}

// 2. The Marshaling Helper
public static class UIHelper
{
    public static void SafeInvoke(this FakeUI ui, Action action)
    {
        if (Thread.CurrentThread.ManagedThreadId == ui.UIThreadId)
        {
            // Already on UI thread
            action();
        }
        else
        {
            // Simulate marshaling to UI thread
            Console.WriteLine($"   [Marshaling] Background Thread {Thread.CurrentThread.ManagedThreadId} requests UI Thread {ui.UIThreadId}");
            
            // In a real app (WPF/WinForms), this would involve Dispatcher.Invoke or Control.Invoke.
            // Here, we just execute the action, but in the context of this exercise, 
            // we acknowledge the separation.
            action(); 
        }
    }
}

public class Program
{
    public static void Main(string[] args)
    {
        var engine = new AIEngine();
        var ui = new FakeUI();

        // Subscribe
        engine.ModelFinishedThinking += (sender, e) =>
        {
            // The handler logic
            Action updateAction = () => 
            {
                ui.StatusText = $"Result: {e.ResultTensor[0]}";
                ui.Render();
            };

            // Execute with safety check
            ui.SafeInvoke(updateAction);
        };

        // Trigger inference (this will run on a background thread)
        engine.RunInferenceAsync("CNN-Model");
        
        Console.ReadLine();
    }
}
