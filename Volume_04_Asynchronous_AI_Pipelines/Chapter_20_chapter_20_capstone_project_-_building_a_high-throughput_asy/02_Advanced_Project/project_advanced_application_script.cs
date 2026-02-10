
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

// Source File: project_advanced_application_script.cs
// Description: Advanced Application Script
// ==========================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncDocumentIngestionEngine
{
    // Core Data Model: Represents a document in our pipeline.
    // We use a simple class to hold metadata and content.
    public class Document
    {
        public string Id { get; set; }
        public string SourceUrl { get; set; }
        public string Content { get; set; }
        public DateTime IngestedAt { get; set; }
    }

    // Simulates a Vector Database (e.g., Pinecone, Milvus, or Qdrant) for RAG.
    // In a real scenario, this would involve network calls to an external service.
    public class VectorStore
    {
        // Thread-safe counter to simulate successful writes.
        private int _storedCount = 0;

        public async Task StoreAsync(Document doc)
        {
            // Simulate network latency and non-blocking I/O.
            // In a real app, this would be an HTTP POST or gRPC call.
            await Task.Delay(new Random().Next(50, 150)); 
            
            // Simulate "Vectorization" (embedding generation).
            // This is CPU-intensive but often offloaded to specialized services.
            // Here we just ensure the content exists.
            if (string.IsNullOrEmpty(doc.Content))
            {
                throw new InvalidOperationException($"Document {doc.Id} has no content.");
            }

            Interlocked.Increment(ref _storedCount);
            Console.WriteLine($"[VectorStore] Stored document {doc.Id} (Total: {_storedCount})");
        }
    }

    // Handles fetching raw data from a source (simulating HTTP requests).
    public class DocumentFetcher
    {
        private readonly HttpClient _httpClient;

        public DocumentFetcher()
        {
            // Initialize HttpClient (ideally singleton in real apps).
            _httpClient = new HttpClient();
            // Set a reasonable timeout.
            _httpClient.Timeout = TimeSpan.FromSeconds(10);
        }

        public async Task<string> FetchContentAsync(string url)
        {
            try
            {
                // Simulate network delay for fetching.
                await Task.Delay(new Random().Next(20, 100));
                
                // In a real scenario: return await _httpClient.GetStringAsync(url);
                // Here we simulate content retrieval based on URL length.
                return $"Content fetched from {url} with length {url.Length}.";
            }
            catch (TaskCanceledException)
            {
                Console.WriteLine($"[Fetcher] Timeout fetching {url}");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Fetcher] Error fetching {url}: {ex.Message}");
                return null;
            }
        }
    }

    // Handles parsing and cleaning of raw text.
    public class DocumentParser
    {
        public Document Parse(string rawContent, string sourceUrl, string id)
        {
            if (string.IsNullOrEmpty(rawContent)) return null;

            // Simulate parsing logic (e.g., removing HTML tags, extracting text).
            // We perform synchronous string manipulation here.
            string cleanedContent = rawContent.Replace("\n", " ").Trim();
            
            return new Document
            {
                Id = id,
                SourceUrl = sourceUrl,
                Content = cleanedContent,
                IngestedAt = DateTime.UtcNow
            };
        }
    }

    // The Orchestrator: Manages the Producer-Consumer pattern using Channels.
    public class IngestionPipeline
    {
        private readonly DocumentFetcher _fetcher;
        private readonly DocumentParser _parser;
        private readonly VectorStore _store;
        
        // We use a bounded channel to limit memory usage (Backpressure).
        // This acts as a buffer between fetching and processing.
        private readonly System.Threading.Channels.Channel<string> _urlChannel;
        private readonly int _maxConcurrency;

        public IngestionPipeline(int maxConcurrency = 5)
        {
            _fetcher = new DocumentFetcher();
            _parser = new DocumentParser();
            _store = new VectorStore();
            _maxConcurrency = maxConcurrency;

            // Create a bounded channel. If full, the producer (enqueuing) will wait.
            _urlChannel = System.Threading.Channels.Channel.CreateBounded<string>(
                new BoundedChannelOptions(100) 
                {
                    FullMode = BoundedChannelFullMode.Wait
                });
        }

        // PRODUCER: Adds URLs to the channel.
        public async Task EnqueueUrlsAsync(List<string> urls)
        {
            foreach (var url in urls)
            {
                // WriteAsync waits if the channel is full (Backpressure).
                await _urlChannel.Writer.WriteAsync(url);
                Console.WriteLine($"[Producer] Enqueued: {url}");
            }

            // Signal that no more items will be written.
            _urlChannel.Writer.Complete();
        }

        // CONSUMER: Processes items from the channel concurrently.
        public async Task ProcessIngestionAsync()
        {
            var tasks = new List<Task>();

            // Spin up multiple worker tasks to simulate high-throughput processing.
            for (int i = 0; i < _maxConcurrency; i++)
            {
                tasks.Add(Task.Run(async () =>
                {
                    // Iterate over the channel reader asynchronously.
                    // This loop exits when the channel is empty and complete.
                    await foreach (var url in _urlChannel.Reader.ReadAllAsync())
                    {
                        // 1. Fetch (Async I/O)
                        string rawContent = await _fetcher.FetchContentAsync(url);
                        if (rawContent == null) continue;

                        // 2. Parse (Sync CPU work)
                        // Note: In a pure async pipeline, heavy CPU work should be 
                        // offloaded to ThreadPool to avoid blocking the async context.
                        Document doc = null;
                        await Task.Run(() => 
                        {
                            doc = _parser.Parse(rawContent, url, Guid.NewGuid().ToString());
                        });

                        if (doc == null) continue;

                        // 3. Store (Async I/O)
                        try
                        {
                            await _store.StoreAsync(doc);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[Worker] Failed to store {doc.Id}: {ex.Message}");
                        }
                    }
                }));
            }

            // Wait for all consumer tasks to finish processing the channel.
            await Task.WhenAll(tasks);
        }
    }

    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Starting High-Throughput Async Document Ingestion Engine...");
            
            // 1. Define Input Data (Simulating a list of URLs to ingest).
            var urls = new List<string>();
            for (int i = 1; i <= 20; i++)
            {
                urls.Add($"https://example.com/doc/{i}");
            }

            // 2. Initialize Pipeline with concurrency limit.
            var pipeline = new IngestionPipeline(maxConcurrency: 5);

            // 3. Start the Producer and Consumers concurrently.
            // We use Task.WhenAll to run the Enqueue and Processing in parallel.
            // The Enqueue task finishes quickly (writing to channel), 
            // while Processing tasks run until the channel is drained.
            var producerTask = pipeline.EnqueueUrlsAsync(urls);
            var consumerTask = pipeline.ProcessIngestionAsync();

            await Task.WhenAll(producerTask, consumerTask);

            Console.WriteLine("Ingestion Complete. All documents processed.");
        }
    }
}
