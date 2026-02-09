
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

# Source File: solution_exercise_3.cs
# Description: Solution for Exercise 3
# ==========================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public record UserProfile(int UserId, string Bio);

public class HybridPipeline
{
    // Simulates fetching data from an API (I/O-bound)
    public async Task<List<UserProfile>> FetchProfilesAsync(List<int> userIds)
    {
        var fetchTasks = userIds.Select(async id =>
        {
            // Simulate network latency
            await Task.Delay(200); 
            return new UserProfile(id, $"Bio for user {id}");
        });

        // Concurrently wait for all fetches
        var profiles = await Task.WhenAll(fetchTasks);
        return profiles.ToList();
    }

    // Simulates CPU-intensive summary generation
    public Task<List<string>> GenerateSummariesAsync(List<UserProfile> profiles)
    {
        // We use Parallel.ForEach for CPU-bound work here, but wrapped in a Task
        // to match the async signature.
        var summaries = new List<string>(profiles.Count);
        
        // Parallel.ForEach is synchronous, so we run it on a background thread
        // to avoid blocking the caller if this method was called from a UI thread.
        return Task.Run(() =>
        {
            var options = new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount };
            Parallel.ForEach(profiles, options, profile =>
            {
                // Simulate CPU work (e.g., analyzing text)
                string summary = $"Summary: {profile.Bio.ToUpper()} (ID: {profile.UserId})";
                summaries.Add(summary);
            });
            return summaries;
        });
    }
}

class Program
{
    static async Task Main(string[] args)
    {
        var pipeline = new HybridPipeline();
        var userIds = Enumerable.Range(1, 10).ToList();

        Console.WriteLine("Starting Hybrid Pipeline...");

        // Stage 1: Concurrent I/O
        Console.WriteLine("Fetching profiles concurrently...");
        var profiles = await pipeline.FetchProfilesAsync(userIds);
        Console.WriteLine($"Fetched {profiles.Count} profiles.");

        // Stage 2: Parallel CPU Processing
        Console.WriteLine("Generating summaries in parallel...");
        var summaries = await pipeline.GenerateSummariesAsync(profiles);

        Console.WriteLine("\nFinal Summaries:");
        summaries.ForEach(Console.WriteLine);
    }
}
