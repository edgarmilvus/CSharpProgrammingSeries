
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
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Diagnostics;

namespace BatchProcessing
{
    // 1. Metadata Handling: Data class for document structure
    public record Document(string Content, Dictionary<string, object> Metadata);

    // 2. Vector Store Simulation
    public class VectorStore
    {
        public async Task UpsertBatchAsync(List<Document> documents)
        {
            Console.WriteLine($"Writing batch of {documents.Count} documents to DB...");
            // Simulate DB latency
            await Task.Delay(500);
            Console.WriteLine("Batch write complete.");
        }
    }

    public static class BatchProcessor
    {
        public static async Task ProcessAndBatchAsync(ChannelReader<Document> reader, VectorStore vectorStore, int batchSize = 10)
        {
            var batch = new List<Document>();
            var timeout = Task.Delay(2000); // 2 second timeout for partial batches

            try
            {
                while (await reader.WaitToReadAsync())
                {
                    while (reader.TryRead(out var doc))
                    {
                        batch.Add(doc);

                        // Check if batch is full
                        if (batch.Count >= batchSize)
                        {
                            await vectorStore.UpsertBatchAsync(batch);
                            batch.Clear();
                            // Reset timeout after successful batch
                            timeout = Task.Delay(2000);
                        }
                    }

                    // Check if timeout occurred while waiting for more items
                    if (timeout.IsCompleted)
                    {
                        if (batch.Count > 0)
                        {
                            Console.WriteLine("Timeout reached, flushing partial batch...");
                            await vectorStore.UpsertBatchAsync(batch);
                            batch.Clear();
                        }
                        timeout = Task.Delay(2000);
                    }
                }
            }
            finally
            {
                // 3. Flush Mechanism: Ensure remaining items are saved
                if (batch.Count > 0)
                {
                    Console.WriteLine($"Flushing final {batch.Count} items...");
                    await vectorStore.UpsertBatchAsync(batch);
                }
            }
        }
    }

    public class Program
    {
        public static async Task Main()
        {
            var channel = Channel.CreateUnbounded<Document>();
            var vectorStore = new VectorStore();

            // Simulate incoming queue of raw data
            var producer = Task.Run(async () =>
            {
                for (int i = 0; i < 25; i++)
                {
                    await channel.Writer.WriteAsync(new Document(
                        $"Document content {i}",
                        new Dictionary<string, object> { { "source", "url_" + i } }
                    ));
                    await Task.Delay(100); // Simulate slow production
                }
                channel.Writer.Complete();
            });

            await BatchProcessor.ProcessAndBatchAsync(channel.Reader, vectorStore);
            await producer;
        }
    }
}
