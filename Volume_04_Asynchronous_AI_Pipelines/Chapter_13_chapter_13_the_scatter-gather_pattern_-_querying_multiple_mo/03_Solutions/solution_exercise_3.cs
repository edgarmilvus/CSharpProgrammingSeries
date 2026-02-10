
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
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class ImageRecognitionResult
{
    public string ObjectLabel { get; set; }
    public double Confidence { get; set; }
    public string ProviderName { get; set; }
}

public class ImageRecognizer
{
    // Simulates a provider with specific latency and random confidence
    private async Task<ImageRecognitionResult> SimulateProvider(string name, int delayMs, CancellationToken ct)
    {
        try
        {
            await Task.Delay(delayMs, ct);
            
            // Random confidence between 0.5 and 1.0
            var rng = new Random();
            var confidence = 0.5 + (rng.NextDouble() * 0.5);
            
            return new ImageRecognitionResult
            {
                ObjectLabel = $"{name}_DetectedObject",
                Confidence = confidence,
                ProviderName = name
            };
        }
        catch (TaskCanceledException)
        {
            // Return null if cancelled to indicate no result
            return null;
        }
    }

    public async Task<ImageRecognitionResult> GetBestRecognitionAsync(string imagePath)
    {
        using var cts = new CancellationTokenSource();

        // 2. Simulate varying latencies: X(500ms), Y(200ms), Z(1000ms)
        var tasks = new List<Task<ImageRecognitionResult>>
        {
            SimulateProvider("Provider X", 500, cts.Token),
            SimulateProvider("Provider Y", 200, cts.Token),
            SimulateProvider("Provider Z", 1000, cts.Token)
        };

        // Minimum confidence threshold
        const double minConfidence = 0.7;

        // 4. Loop to handle the "wait for next" logic
        while (tasks.Count > 0)
        {
            // 3. Await the first completed task
            var completedTask = await Task.WhenAny(tasks);
            
            // Remove the completed task from the list so we don't await it again
            tasks.Remove(completedTask);

            // Safely get result (handle potential exceptions)
            ImageRecognitionResult result = null;
            try 
            {
                result = await completedTask;
            }
            catch 
            {
                // If the task faulted, just continue to the next one
                continue;
            }

            // Check if result is null (cancelled)
            if (result == null) continue;

            // Interactive Challenge: Check Confidence
            if (result.Confidence >= minConfidence)
            {
                // Found a satisfactory result! Cancel others.
                cts.Cancel();
                return result;
            }
            
            // If confidence is too low, we discard this result and 
            // continue the loop to wait for the next fastest task.
            Console.WriteLine($"Discarding result from {result.ProviderName} (Confidence: {result.Confidence:F2})");
        }

        // If we exit the loop, all tasks failed or had low confidence
        return null;
    }
}

// Example Usage
public class Program
{
    public static async Task Main()
    {
        var recognizer = new ImageRecognizer();
        var result = await recognizer.GetBestRecognitionAsync("image.jpg");

        if (result != null)
        {
            Console.WriteLine($"Best Result: {result.ObjectLabel} from {result.ProviderName} (Conf: {result.Confidence:F2})");
        }
        else
        {
            Console.WriteLine("No satisfactory recognition found.");
        }
    }
}
