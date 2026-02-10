
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

// Source File: solution_exercise_2.cs
// Description: Solution for Exercise 2
// ==========================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// 1. Define custom exceptions
public class ChunkTooLargeException : Exception
{
    public ChunkTooLargeException(string message) : base(message) { }
}

public class ProcessingTimeoutException : Exception
{
    public ProcessingTimeoutException(string message) : base(message) { }
}

public class Summarizer
{
    // 1. Method that randomly throws exceptions
    public async Task<string> SummarizeChunkAsync(string chunk)
    {
        await Task.Delay(50); // Simulate processing
        var random = new Random(Guid.NewGuid().GetHashCode()); // Seed based on chunk to vary results
        
        switch (random.Next(0, 3))
        {
            case 0:
                throw new ChunkTooLargeException($"Chunk '{chunk}' exceeds size limit.");
            case 1:
                throw new ProcessingTimeoutException($"Timeout processing '{chunk}'.");
            default:
                return $"Summary of {chunk}";
        }
    }
}

public class Program
{
    public static async Task Main()
    {
        var summarizer = new Summarizer();
        
        // 2. Create list of 10 chunks
        var chunks = Enumerable.Range(1, 10).Select(i => $"Chunk{i}").ToList();
        
        // 3. Launch tasks
        var tasks = chunks.Select(c => summarizer.SummarizeChunkAsync(c)).ToList();

        Console.WriteLine("Processing chunks in parallel...");
        
        try
        {
            // 3. & 4. Await Task.WhenAll wrapped in try-catch
            await Task.WhenAll(tasks);
        }
        catch (AggregateException ae)
        {
            // 5. Use Flatten() and iterate InnerExceptions
            // Flatten is useful if exceptions are nested, though Task.WhenAll usually flattens them.
            var flattenedExceptions = ae.Flatten().InnerExceptions;
            
            Console.WriteLine($"\nCaught {flattenedExceptions.Count} failures:");
            
            foreach (var ex in flattenedExceptions)
            {
                // 6. Log specific type and message
                Console.WriteLine($"- Type: {ex.GetType().Name}, Message: {ex.Message}");
            }
        }

        // 7. Ensure program continues
        Console.WriteLine("\nPipeline cleanup complete. Execution continued despite failures.");
    }
}
