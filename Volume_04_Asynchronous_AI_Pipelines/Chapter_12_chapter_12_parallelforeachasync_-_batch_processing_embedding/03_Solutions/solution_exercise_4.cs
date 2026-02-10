
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

using System.Collections.Concurrent;
using System.Net.Http;
using System.Threading.Tasks;
using System.Random;

public class FaultTolerantProcessor
{
    public record ProcessingResult(
        ConcurrentBag<TextEmbedding> Successes, 
        ConcurrentBag<(string Text, Exception Error)> Failures
    );

    public async Task<ProcessingResult> ProcessWithFaultToleranceAsync(
        List<string> documents, 
        IEmbeddingService service)
    {
        var successes = new ConcurrentBag<TextEmbedding>();
        var failures = new ConcurrentBag<(string, Exception)>();
        
        // Random instance is not thread-safe, so we use a thread-local instance
        var random = new ThreadLocal<Random>(() => new Random(Guid.NewGuid().GetHashCode()));

        await Parallel.ForEachAsync(documents, async (document, cancellationToken) =>
        {
            try
            {
                // Simulate transient failure (e.g., 10% chance of failure)
                if (random.Value.Next(0, 10) == 0)
                {
                    throw new HttpRequestException("Simulated network timeout", null, System.Net.HttpStatusCode.RequestTimeout);
                }

                var vector = await service.GetEmbeddingAsync(document);
                successes.Add(new TextEmbedding(document, vector));
            }
            catch (Exception ex)
            {
                // Capture the exception and the associated document
                // Do not rethrow if we want the loop to continue
                failures.Add((document, ex));
            }
        });

        return new ProcessingResult(successes, failures);
    }
}
