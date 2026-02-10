
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
using System.Threading.Tasks;

public class LLMEndpointUnavailableException : Exception
{
    public LLMEndpointUnavailableException(string message) : base(message) { }
}

public class LLMCaller
{
    // 1. Refactor CallLLMAsync to randomly throw
    public async Task<string> CallLLMAsync(string endpoint, string prompt)
    {
        await Task.Delay(50); // Simulate network
        var rand = new Random(Guid.NewGuid().GetHashCode());
        
        // 30% chance of failure
        if (rand.Next(0, 10) < 3) 
        {
            throw new LLMEndpointUnavailableException($"Endpoint {endpoint} is unavailable.");
        }
        
        return $"Response from {endpoint}: Processed '{prompt}'";
    }
}

public class Program
{
    public static async Task Main()
    {
        var caller = new LLMCaller();
        var prompt = "Generate code";
        
        var primaryEndpoints = new List<string> { "EndpointA", "EndpointB", "EndpointC" };
        var backupEndpoints = new List<string> { "BackupA", "BackupB", "BackupC" };

        // 2. Primary Batch Execution
        var primaryTasks = primaryEndpoints
            .Select(ep => caller.CallLLMAsync(ep, prompt))
            .ToList();

        var primaryResults = new List<string>();
        var failedIndices = new List<int>(); // Track which endpoints failed

        try
        {
            await Task.WhenAll(primaryTasks);
            primaryResults.AddRange(primaryTasks.Select(t => t.Result));
        }
        catch (AggregateException ae)
        {
            // 2. Catch AggregateException and inspect
            Console.WriteLine("Primary batch had failures. Analyzing...");
            
            // Map tasks to their original index to identify which endpoints failed
            for (int i = 0; i < primaryTasks.Count; i++)
            {
                if (primaryTasks[i].IsFaulted)
                {
                    failedIndices.Add(i);
                    Console.WriteLine($" - Failure on {primaryEndpoints[i]}: {primaryTasks[i].Exception?.InnerException?.Message}");
                }
                else
                {
                    primaryResults.Add(primaryTasks[i].Result);
                }
            }
        }

        // 3. Retry failed requests using Backup Endpoints
        if (failedIndices.Count > 0)
        {
            Console.WriteLine("\nInitiating Backup Batch...");
            var backupTasks = new List<Task<string>>();
            
            // Map failed indices to backup endpoints (assuming 1:1 mapping for simplicity)
            foreach (var idx in failedIndices)
            {
                if (idx < backupEndpoints.Count)
                {
                    backupTasks.Add(caller.CallLLMAsync(backupEndpoints[idx], prompt));
                }
            }

            try
            {
                await Task.WhenAll(backupTasks);
                primaryResults.AddRange(backupTasks.Select(t => t.Result));
            }
            catch (AggregateException ae)
            {
                // 6. Log but do not halt merging
                Console.WriteLine("Backup batch also had failures. Logging and continuing.");
                foreach (var ex in ae.Flatten().InnerExceptions)
                {
                    Console.WriteLine($" - Backup Failure: {ex.Message}");
                }
                
                // Add successful backup results
                foreach (var t in backupTasks.Where(t => t.IsCompletedSuccessfully))
                {
                    primaryResults.Add(t.Result);
                }
            }
        }

        // 5. Merge results
        Console.WriteLine("\n--- Final Aggregated Results ---");
        foreach (var res in primaryResults)
        {
            Console.WriteLine(res);
        }
    }
}
