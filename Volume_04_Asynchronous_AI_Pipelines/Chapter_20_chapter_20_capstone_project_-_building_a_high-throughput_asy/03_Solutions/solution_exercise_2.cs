
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
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Threading;

namespace ProducerConsumerPattern
{
    public static class DocumentFetcher
    {
        // Simulates fetching a document from a URL
        public static async Task<string> FetchDocumentAsync(string url, SemaphoreSlim semaphore)
        {
            await semaphore.WaitAsync();
            try
            {
                Console.WriteLine($"Fetching {url}...");
                // Simulate network latency
                await Task.Delay(Random.Shared.Next(100, 500));
                return $"Content of {url}";
            }
            finally
            {
                semaphore.Release();
            }
        }

        // Producer: Puts URLs into the channel
        public static async Task ProduceUrlsAsync(ChannelWriter<string> writer, List<string> urls)
        {
            foreach (var url in urls)
            {
                await writer.WriteAsync(url);
                Console.WriteLine($"Produced: {url}");
            }
            // Signal completion (equivalent to putting None/sentinel)
            writer.Complete();
        }

        // Consumer: Reads from channel and processes
        public static async Task ConsumeUrlsAsync(ChannelReader<string> reader, SemaphoreSlim semaphore, int consumerId)
        {
            // Iterate until the channel is completed and empty
            await foreach (var url in reader.ReadAllAsync())
            {
                try
                {
                    var content = await FetchDocumentAsync(url, semaphore);
                    Console.WriteLine($"Consumer {consumerId} processed: {content[..Math.Min(20, content.Length)]}...");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Consumer {consumerId} error: {ex.Message}");
                }
            }
            Console.WriteLine($"Consumer {consumerId} shutting down.");
        }
    }

    public class Program
    {
        public static async Task Main()
        {
            var urls = new List<string>();
            for (int i = 0; i < 20; i++) urls.Add($"https://example.com/doc/{i}");

            // Create a bounded channel to act as the Queue
            var channel = Channel.CreateBounded<string>(new BoundedChannelOptions(10) {
                FullMode = BoundedChannelFullMode.Wait
            });
            
            var semaphore = new SemaphoreSlim(5); // Limit to 5 concurrent requests

            // 1. Create the producer task
            var producerTask = DocumentFetcher.ProduceUrlsAsync(channel.Writer, urls);

            // 2. Create multiple consumer tasks (e.g., 3 consumers)
            var consumerTasks = new List<Task>();
            for (int i = 0; i < 3; i++)
            {
                int id = i;
                consumerTasks.Add(Task.Run(() => DocumentFetcher.ConsumeUrlsAsync(channel.Reader, semaphore, id)));
            }

            // 3. Wait for producer to finish writing
            await producerTask;

            // Wait for consumers to finish processing all items
            // Note: In C#, we don't strictly need queue.join() because 
            // ChannelReader.WaitToReadAsync or ReadAllAsync handles flow.
            // We simply await the consumer tasks which will exit when channel completes.
            await Task.WhenAll(consumerTasks);
            
            Console.WriteLine("All tasks completed.");
        }
    }
}
