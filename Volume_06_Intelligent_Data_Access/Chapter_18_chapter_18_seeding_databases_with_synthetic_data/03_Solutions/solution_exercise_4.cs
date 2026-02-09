
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

# Source File: solution_exercise_4.cs
# Description: Solution for Exercise 4
# ==========================================

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

// Mock Entity and Vector types
public record Article(Guid Id, string Content);
public record VectorEmbedding(Guid ArticleId, float[] Vector);

public class VectorPipeline
{
    // Mock Embedding Generator
    private async Task<float[]> GenerateEmbeddingAsync(string content)
    {
        // Simulate expensive CPU/GPU operation
        await Task.Delay(50); 
        return new float[128]; // Simplified vector
    }

    // Mock Vector DB
    private async Task BatchInsertVectorsAsync(List<VectorEmbedding> batch)
    {
        // Simulate DB IO
        await Task.Delay(100);
        Console.WriteLine($"Inserted batch of {batch.Count} vectors.");
    }

    public async Task MigrateAsync(List<Article> articles)
    {
        // 1. Create a Bounded Channel (Backpressure support)
        // Capacity set to 100. Producers will await if full.
        var channel = Channel.CreateBounded<VectorEmbedding>(new BoundedChannelOptions(100)
        {
            FullMode = BoundedChannelFullMode.Wait
        });

        // 2. Producer Tasks (Parallel)
        // We create multiple producers to read from articles and generate embeddings
        var producerTasks = new List<Task>();
        for (int i = 0; i < 3; i++) // 3 Producers
        {
            var producerId = i;
            producerTasks.Add(Task.Run(async () =>
            {
                foreach (var article in articles)
                {
                    // Simulate distributing work
                    if (article.Id.GetHashCode() % 3 != producerId) continue;

                    var vector = await GenerateEmbeddingAsync(article.Content);
                    var embedding = new VectorEmbedding(article.Id, vector);
                    
                    // Write to channel. If full, this awaits until space is available.
                    await channel.Writer.WriteAsync(embedding);
                }
            }));
        }

        // 3. Consumer Tasks (Parallel Batch Insert)
        var consumerTasks = new List<Task>();
        for (int i = 0; i < 2; i++) // 2 Consumers
        {
            consumerTasks.Add(Task.Run(async () =>
            {
                var batch = new List<VectorEmbedding>();
                
                // await foreach reads from the channel as data becomes available
                await foreach (var embedding in channel.Reader.ReadAllAsync())
                {
                    batch.Add(embedding);

                    // Batch size check (e.g., 50 items)
                    if (batch.Count >= 50)
                    {
                        await BatchInsertVectorsAsync(batch);
                        batch.Clear();
                    }
                }

                // Insert remaining items after channel closes
                if (batch.Count > 0)
                {
                    await BatchInsertVectorsAsync(batch);
                }
            }));
        }

        // 4. Signal completion to consumers
        // Wait for all producers to finish writing
        await Task.WhenAll(producerTasks);
        
        // Signal that no more data will be written
        channel.Writer.Complete();

        // Wait for all consumers to finish processing
        await Task.WhenAll(consumerTasks);
    }
}
