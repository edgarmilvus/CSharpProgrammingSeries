
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
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class ParallelProcessor
{
    // Simulates an expensive operation (e.g., Sentiment Analysis)
    private async Task<string> ProcessTokenAsync(string token, int delayMs)
    {
        // Randomize delay to simulate variable processing times
        var actualDelay = new Random().Next(delayMs / 2, delayMs * 2);
        await Task.Delay(actualDelay);
        return token.ToUpper(); // Simple transformation for demo
    }

    public async IAsyncEnumerable<string> ProcessStreamParallelAsync(
        IAsyncEnumerable<string> tokens, 
        int degreeOfParallelism)
    {
        // We use a SemaphoreSlim to throttle concurrency manually, 
        // or we can rely on Parallel.ForEachAsync options in .NET 6+.
        
        // To preserve order, we cannot simply yield results as they complete.
        // We must buffer them or process them in a way that awaits completion in order.
        
        // Strategy: Buffer the tokens into a list first (if memory allows), 
        // or use a sliding window approach. 
        // For this exercise, we will buffer to a list to simplify ordering logic,
        // but acknowledge that for infinite streams, a sliding window is required.
        
        var inputList = await tokens.ToListAsync(); // Materialize to index items
        
        // Create an array to hold tasks
        var tasks = new Task<string>[inputList.Count];

        var options = new ParallelOptions { MaxDegreeOfParallelism = degreeOfParallelism };

        // Parallel.ForEachAsync does not guarantee order, so we assign tasks to specific indices
        await Parallel.ForEachAsync(Enumerable.Range(0, inputList.Count), options, async (i, ct) =>
        {
            // We assign the result to the specific index to preserve order later
            tasks[i] = ProcessTokenAsync(inputList[i], 200);
        });

        // Await tasks in the original order
        for (int i = 0; i < tasks.Length; i++)
        {
            yield return await tasks[i];
        }
    }
}

public class Program
{
    public static async Task Main(string[] args)
    {
        var tokens = new List<string> { "token1", "token2", "token3", "token4", "token5" }.ToAsyncEnumerable();
        var processor = new ParallelProcessor();

        Console.WriteLine("Processing in parallel (Order preserved):");
        
        await foreach (var result in processor.ProcessStreamParallelAsync(tokens, 2))
        {
            Console.WriteLine($"Received: {result}");
        }
    }
}
