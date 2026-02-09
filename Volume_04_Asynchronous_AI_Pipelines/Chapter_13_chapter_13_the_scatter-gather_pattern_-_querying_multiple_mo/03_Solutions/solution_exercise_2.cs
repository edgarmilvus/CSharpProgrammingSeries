
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
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class SentimentAnalyzer
{
    // 2. Simulate the analysis method
    public async Task<double> AnalyzeSentiment(string comment, string modelName, CancellationToken ct)
    {
        // Random delay to simulate variable processing times (50ms to 300ms)
        var delay = new Random().Next(50, 300);
        
        // Register the delay with the cancellation token
        try
        {
            await Task.Delay(delay, ct);
        }
        catch (TaskCanceledException)
        {
            // Log or handle specific cancellation logic if needed
            throw; // Re-throw to let the caller know it was cancelled
        }

        // Simulate calculation (pseudo-random score based on string length)
        double score = (comment.Length % 10 - 5) / 5.0; 
        return Math.Clamp(score, -1.0, 1.0);
    }

    // 3. & 4. Generate tasks dynamically and run
    public async Task<double> CalculateAverageSentiment(List<string> comments)
    {
        // 6. Interactive Challenge: Timeout Mechanism
        using var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromSeconds(2)); // 2-second timeout

        var models = new List<string> { "Model A", "Model B", "Model C" };

        // 3. Generate IEnumerable<Task<double>> dynamically
        // We flatten the logic: For every comment, run it against every model
        var tasks = comments
            .SelectMany(comment => models
                .Select(model => AnalyzeSentiment(comment, model, cts.Token)))
            .ToList();

        Console.WriteLine($"Starting {tasks.Count} analysis tasks...");

        try
        {
            // 4. Await all tasks
            await Task.WhenAll(tasks);
        }
        catch (TaskCanceledException)
        {
            // Handle the timeout scenario
            Console.WriteLine("Processing timed out. Calculating average from completed tasks only.");
            // Note: WhenAll throws immediately on cancellation. 
            // We need to filter completed tasks manually below.
        }

        // 5. Calculate average safely
        // We filter for tasks that are RanToCompletion. 
        // Faulted or Canceled tasks are ignored for the average.
        var completedResults = tasks
            .Where(t => t.IsCompletedSuccessfully)
            .Select(t => t.Result)
            .ToList();

        if (completedResults.Count == 0) return 0.0;

        return completedResults.Average();
    }
}

// Example Usage
public class Program
{
    public static async Task Main()
    {
        var analyzer = new SentimentAnalyzer();
        var comments = new List<string> 
        { 
            "I love this product!", 
            "Terrible service.", 
            "It was okay, nothing special.", 
            "Absolutely fantastic experience.",
            "Worst day ever." 
        };

        double averageScore = await analyzer.CalculateAverageSentiment(comments);
        Console.WriteLine($"Average Sentiment Score: {averageScore:F2}");
    }
}
