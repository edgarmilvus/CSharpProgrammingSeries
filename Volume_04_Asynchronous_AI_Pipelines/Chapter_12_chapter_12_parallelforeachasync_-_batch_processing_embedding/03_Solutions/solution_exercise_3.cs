
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

using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

public class StreamingEmbeddingProcessor
{
    public async Task<ConcurrentBag<TextEmbedding>> ProcessWithProgressAsync(
        List<string> texts, 
        IEmbeddingService service, 
        IProgress<int> progress, 
        CancellationToken ct)
    {
        var results = new ConcurrentBag<TextEmbedding>();
        int processedCount = 0; // Local counter for the loop
        int totalItems = texts.Count;

        // Use ParallelOptions to pass the CancellationToken
        var parallelOptions = new ParallelOptions
        {
            MaxDegreeOfParallelism = 8,
            CancellationToken = ct
        };

        try
        {
            await Parallel.ForEachAsync(texts, parallelOptions, async (text, token) =>
            {
                // 1. Check for cancellation (handled automatically by ParallelOptions, 
                // but good practice to check inside long operations)
                token.ThrowIfCancellationRequested();

                // 2. Process the embedding
                var vector = await service.GetEmbeddingAsync(text);
                results.Add(new TextEmbedding(text, vector));

                // 3. Thread-safe progress reporting
                // Interlocked.Increment ensures atomic read-modify-write
                int currentCount = Interlocked.Increment(ref processedCount);
                
                // Report progress (usually decoupled from the loop to avoid UI blocking)
                progress.Report(currentCount);
            });
        }
        catch (OperationCanceledException)
        {
            // Handle cancellation gracefully (optional logging here)
            // The loop stops immediately when the token is cancelled
        }

        return results;
    }
}

// Example usage context
public class ProgressReporter : IProgress<int>
{
    public void Report(int value)
    {
        // In a real UI app, this would marshal back to the UI thread
        Console.WriteLine($"Processed: {value}");
    }
}
