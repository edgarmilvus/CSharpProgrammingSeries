
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

// Source File: solution_exercise_1.cs
// Description: Solution for Exercise 1
// ==========================================

using System;
using System.Threading;
using System.Threading.Tasks;

public class TokenStreamProcessor
{
    // Simulates reading tokens from a stream into the provided Memory buffer.
    // Returns the number of tokens written in the final chunk.
    public async Task<int> ProduceTokensAsync(Memory<int> buffer, int chunkSize, int totalTokens, CancellationToken ct)
    {
        int tokensWritten = 0;
        int offset = 0;

        while (tokensWritten < totalTokens && !ct.IsCancellationRequested)
        {
            // Calculate how many tokens to write in this chunk
            int tokensToWrite = Math.Min(chunkSize, totalTokens - tokensWritten);
            
            // Simulate I/O latency
            await Task.Delay(10, ct);

            // Fill the buffer segment with sequential integers
            // We use the Span property for efficient access
            var currentChunk = buffer.Slice(offset, tokensToWrite);
            for (int i = 0; i < tokensToWrite; i++)
            {
                currentChunk.Span[i] = tokensWritten + i;
            }

            tokensWritten += tokensToWrite;
            offset += tokensToWrite;

            // If we've filled the buffer, stop (for this specific fixed-size requirement)
            if (offset >= buffer.Length) break;
        }

        return tokensWritten;
    }

    // Consumes tokens from the buffer, calculating averages per chunk.
    // Awaits the producer to process data as it becomes available.
    public async Task ConsumeAndAverageAsync(Memory<int> buffer, int chunkSize, int totalTokens)
    {
        int processedTokens = 0;
        int offset = 0;

        while (processedTokens < totalTokens)
        {
            // In a real scenario, we might wait on a signal that data is ready.
            // Here we simulate processing by awaiting the producer delay logic roughly.
            // For this exercise, we assume the producer and consumer run in lock-step or 
            // the consumer waits for the producer to fill specific segments.
            
            // To strictly follow the requirement of "awaiting the producer method", 
            // we structure this as a coordinated task, though typically 
            // producer and consumer run concurrently.
            
            int tokensToProcess = Math.Min(chunkSize, totalTokens - processedTokens);
            
            // Simulate waiting for the chunk to be filled (mirroring the delay in producer)
            await Task.Delay(10); 

            var chunk = buffer.Slice(offset, tokensToProcess);
            double sum = 0;
            
            // Requirement: Use .Span, do not convert to array
            foreach (int token in chunk.Span)
            {
                sum += token;
            }

            double average = sum / tokensToProcess;
            Console.WriteLine($"Processed Chunk {offset / chunkSize}: Average Token ID = {average:F2}");

            processedTokens += tokensToProcess;
            offset += tokensToProcess;
        }
    }
}

// Demonstration
public class Program
{
    public static async Task Main()
    {
        var processor = new TokenStreamProcessor();
        int bufferSize = 1024;
        int chunkSize = 128;
        int totalTokens = 1024; // Exactly filling the buffer

        Memory<int> buffer = new int[bufferSize];
        CancellationTokenSource cts = new CancellationTokenSource();

        // Run producer and consumer concurrently
        // Note: In a strict producer-consumer, we'd use Channels or BlockingCollection.
        // Here, we simulate the pattern by running tasks that coordinate via shared memory and delays.
        var producerTask = processor.ProduceTokensAsync(buffer, chunkSize, totalTokens, cts.Token);
        var consumerTask = processor.ConsumeAndAverageAsync(buffer, chunkSize, totalTokens);

        await Task.WhenAll(producerTask, consumerTask);
    }
}
