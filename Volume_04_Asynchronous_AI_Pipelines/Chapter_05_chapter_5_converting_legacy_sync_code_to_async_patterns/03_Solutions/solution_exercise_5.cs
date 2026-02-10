
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

// Source File: solution_exercise_5.cs
// Description: Solution for Exercise 5
// ==========================================

using System;
using System.Threading.Tasks;

// Legacy class using Event-based Asynchronous Pattern (EAP)
public class LegacyDataProcessor
{
    public event Action<string> DataProcessed;
    public event Action<string> ErrorOccurred;

    public void StartProcessing()
    {
        // Simulate async work in background
        Task.Run(async () =>
        {
            await Task.Delay(500);
            // Randomly decide success or failure
            if (DateTime.Now.Ticks % 2 == 0)
            {
                DataProcessed?.Invoke("Legacy Result Data");
            }
            else
            {
                ErrorOccurred?.Invoke("Legacy Processing Failed");
            }
        });
    }
}

public static class LegacyExtensions
{
    public static Task<string> ProcessDataAsync(this LegacyDataProcessor processor)
    {
        var tcs = new TaskCompletionSource<string>();

        // Define local handlers
        void OnDataProcessed(string result)
        {
            // Unsubscribe to prevent memory leaks
            processor.DataProcessed -= OnDataProcessed;
            processor.ErrorOccurred -= OnErrorOccurred;
            
            // Set the result on the TaskCompletionSource
            tcs.TrySetResult(result);
        }

        void OnErrorOccurred(string error)
        {
            // Unsubscribe
            processor.DataProcessed -= OnDataProcessed;
            processor.ErrorOccurred -= OnErrorOccurred;
            
            // Set the exception on the TaskCompletionSource
            tcs.TrySetException(new InvalidOperationException(error));
        }

        // Subscribe to events
        processor.DataProcessed += OnDataProcessed;
        processor.ErrorOccurred += OnErrorOccurred;

        // Start the legacy processing
        processor.StartProcessing();

        return tcs.Task;
    }
}

public class Program
{
    public static async Task Main(string[] args)
    {
        var processor = new LegacyDataProcessor();
        try
        {
            string result = await processor.ProcessDataAsync();
            Console.WriteLine($"Result: {result}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}
