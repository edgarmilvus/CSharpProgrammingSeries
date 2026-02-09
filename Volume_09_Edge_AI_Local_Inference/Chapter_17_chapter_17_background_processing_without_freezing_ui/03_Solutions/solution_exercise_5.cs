
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

# Source File: solution_exercise_5.cs
# Description: Solution for Exercise 5
# ==========================================

using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

public class InferenceController : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    private string _status;
    public string Status
    {
        get => _status;
        set { _status = value; OnPropertyChanged(nameof(Status)); }
    }

    private int _progress;
    public int Progress
    {
        get => _progress;
        set { _progress = value; OnPropertyChanged(nameof(Progress)); }
    }

    private string _generatedText;
    public string GeneratedText
    {
        get => _generatedText;
        set { _generatedText = value; OnPropertyChanged(nameof(GeneratedText)); }
    }

    private bool _isBusy;
    public bool IsBusy
    {
        get => _isBusy;
        set { _isBusy = value; OnPropertyChanged(nameof(IsBusy)); }
    }

    private CancellationTokenSource _cts;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public async Task StartInferenceAsync(string prompt)
    {
        if (IsBusy) return;

        _cts = new CancellationTokenSource();
        IsBusy = true;
        Status = "Processing";
        Progress = 0;
        GeneratedText = "";

        try
        {
            // Simulate background work
            for (int i = 1; i <= 10; i++)
            {
                // Check if cancellation was requested
                if (_cts.Token.IsCancellationRequested)
                {
                    Status = "Cancelling...";
                    // Simulate cleanup time
                    await Task.Delay(500, _cts.Token); 
                    break;
                }

                await Task.Delay(200, _cts.Token);
                
                Progress = i * 10;
                GeneratedText += $"Token_{i} ";
            }

            if (!_cts.Token.IsCancellationRequested)
            {
                Status = "Completed";
            }
        }
        catch (OperationCanceledException)
        {
            Status = "Cancelled";
        }
        catch (Exception ex)
        {
            Status = $"Error: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
            _cts.Dispose();
            _cts = null;
        }
    }

    public void CancelInference()
    {
        if (IsBusy && _cts != null)
        {
            // Trigger cancellation
            _cts.Cancel();
            Status = "Cancelling...";
        }
    }
}

public class Program
{
    public static async Task Main(string[] args)
    {
        var controller = new InferenceController();
        
        // Simulate UI binding by subscribing to events
        controller.PropertyChanged += (s, e) =>
        {
            // Simulate UI Marshaling (Console app runs on single thread, but this mimics UI logic)
            Console.WriteLine($"[UI Update] {e.PropertyName}: " + 
                e.PropertyName == "Status" ? controller.Status :
                e.PropertyName == "Progress" ? controller.Progress.ToString() : "");
        };

        Console.WriteLine($"Start: {DateTime.Now:HH:mm:ss}");
        
        // Start inference
        var task = controller.StartInferenceAsync("Test Prompt");

        // Simulate user cancelling after 600ms
        await Task.Delay(600);
        controller.CancelInference();

        await task;
        Console.WriteLine($"End: {DateTime.Now:HH:mm:ss} - Final Status: {controller.Status}");
    }
}
