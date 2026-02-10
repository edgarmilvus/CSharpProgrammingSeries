
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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public record UserProfile(int UserId, string Bio);

public class ResilientPipeline
{
    public async Task<List<UserProfile>> FetchProfilesAsync(List<int> userIds, CancellationToken ct)
    {
        var fetchTasks = userIds.Select(async id =>
        {
            // 1. Check for cancellation before starting work
            ct.ThrowIfCancellationRequested();

            // Simulate network latency
            await Task.Delay(200, ct); // Pass token to Delay

            // 2. Error Simulation: Randomly throw exception
            if (id == 3) 
            {
                throw new InvalidOperationException($"API Error fetching user {id}");
            }

            return new UserProfile(id, $"Bio for user {id}");
        });

        // 3. Wait for all tasks (if one fails, Task.WhenAll throws immediately)
        try 
        {
            var profiles = await Task.WhenAll(fetchTasks);
            return profiles.ToList();
        }
        catch (Exception)
        {
            // Cancel remaining tasks if one fails (optional, but good for "fail fast")
            // Note: Tasks already in flight will complete or throw, but we stop waiting.
            throw;
        }
    }

    public async Task<List<string>> GenerateSummariesAsync(List<UserProfile> profiles, CancellationToken ct)
    {
        var summaries = new List<string>();
        
        // Parallel.ForEachAsync handles cancellation tokens natively
        await Parallel.ForEachAsync(profiles, new ParallelOptions { CancellationToken = ct }, async (profile, token) =>
        {
            // Simulate CPU work
            await Task.Delay(50, token); 
            string summary = $"Summary: {profile.Bio.ToUpper()}";
            summaries.Add(summary);
        });

        return summaries;
    }
}

class Program
{
    static async Task Main(string[] args)
    {
        var pipeline = new ResilientPipeline();
        var userIds = Enumerable.Range(1, 5).ToList();

        // Setup Cancellation
        using var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromSeconds(0.8)); // Cancel after 800ms

        Console.WriteLine("Starting Resilient Pipeline (will cancel after 800ms or fail on user 3)...");

        try
        {
            // Stage 1
            var profiles = await pipeline.FetchProfilesAsync(userIds, cts.Token);
            
            // Stage 2
            var summaries = await pipeline.GenerateSummariesAsync(profiles, cts.Token);
            
            Console.WriteLine("Pipeline Completed Successfully.");
            summaries.ForEach(Console.WriteLine);
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Pipeline was cancelled by the user or timeout.");
        }
        catch (Exception ex)
        {
            // Handles the simulated API error
            Console.WriteLine($"Pipeline failed with error: {ex.Message}");
            Console.WriteLine("Note: In-flight tasks may still be running or completing.");
        }
    }
}
