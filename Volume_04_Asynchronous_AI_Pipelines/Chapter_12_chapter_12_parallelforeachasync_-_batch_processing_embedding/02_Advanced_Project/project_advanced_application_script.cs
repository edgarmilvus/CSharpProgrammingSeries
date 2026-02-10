
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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BatchEmbeddingGenerator
{
    // 1. Real-World Context:
    // Imagine you are building a semantic search engine for a large legal document repository.
    // You have thousands of documents (text chunks) that need to be converted into vector embeddings
    // to enable similarity search. Calling an embedding API sequentially (one by one) would be too slow,
    // taking hours for large datasets. This application simulates generating embeddings for 100 text chunks
    // using Parallel.ForEachAsync to maximize throughput while respecting API rate limits.

    class Program
    {
        // Entry point of the application
        static async Task Main(string[] args)
        {
            Console.WriteLine("Starting Batch Embedding Generation...");
            Console.WriteLine("=====================================");

            // 2. Data Preparation:
            // We simulate a list of text chunks that need processing.
            // In a real scenario, this data would come from a database or file system.
            List<string> textChunks = GenerateMockTextData(100);

            // 3. Configuration:
            // We define the maximum number of concurrent operations (Degree of Parallelism).
            // This is crucial for managing resource contention (e.g., API rate limits, memory).
            // We simulate an API that allows 5 concurrent requests.
            int maxConcurrency = 5;

            // 4. Thread-Safe Collection:
            // Since we are processing in parallel, we cannot use a standard List<T>.
            // We use ConcurrentBag<T> to safely store results from multiple threads without manual locking.
            var embeddingsStore = new ConcurrentBag<EmbeddingResult>();

            // 5. Parallel Processing Logic:
            // We use Parallel.ForEachAsync to process the text chunks asynchronously.
            // It handles the distribution of work across the available threads.
            await Parallel.ForEachAsync(textChunks, new ParallelOptions { MaxDegreeOfParallelism = maxConcurrency }, async (textChunk, cancellationToken) =>
            {
                // Simulate generating an embedding for a specific text chunk
                var embeddingResult = await GenerateEmbeddingAsync(textChunk);

                // Add the result to our thread-safe collection
                embeddingsStore.Add(embeddingResult);

                // Optional: Log progress (Console.WriteLine is thread-safe but output order is non-deterministic)
                Console.WriteLine($"Processed chunk ID: {embeddingResult.ChunkId}");
            });

            // 6. Result Aggregation:
            // After parallel processing completes, we aggregate the results.
            // We sort them by ID to ensure deterministic output for verification.
            var sortedResults = embeddingsStore.OrderBy(r => r.ChunkId).ToList();

            Console.WriteLine("\n=====================================");
            Console.WriteLine($"Processing Complete. Total Embeddings: {sortedResults.Count}");
            Console.WriteLine("Sample Output (First 5):");
            
            foreach (var result in sortedResults.Take(5))
            {
                Console.WriteLine($"ID: {result.ChunkId} | Vector: [{string.Join(", ", result.Vector)}]");
            }
        }

        // Helper method to generate mock text data
        static List<string> GenerateMockTextData(int count)
        {
            var data = new List<string>();
            for (int i = 0; i < count; i++)
            {
                data.Add($"Legal clause {i}: The defendant shall be liable for damages incurred.");
            }
            return data;
        }

        // 7. Simulation of Async API Call:
        // This method simulates a network call to an AI Embedding Service (e.g., OpenAI, Azure AI).
        // It includes a delay to mimic network latency and processing time.
        static async Task<EmbeddingResult> GenerateEmbeddingAsync(string text)
        {
            // Simulate network latency (between 100ms and 300ms)
            Random rnd = new Random();
            int delay = rnd.Next(100, 300);
            await Task.Delay(delay);

            // Simulate extracting an ID from the text (for tracking purposes)
            // In reality, this might be a database primary key.
            int id = int.Parse(text.Split(' ')[1].TrimEnd(':'));

            // Simulate generating a vector (e.g., 1536 dimensions for text-embedding-ada-002)
            // We generate random floats to represent the embedding vector.
            float[] vector = new float[10]; // Using 10 for brevity in console output
            for (int i = 0; i < vector.Length; i++)
            {
                vector[i] = (float)rnd.NextDouble();
            }

            return new EmbeddingResult { ChunkId = id, Vector = vector, SourceText = text };
        }
    }

    // 8. Data Structure:
    // A simple class to hold the result of the embedding generation.
    // Note: We use a standard class (not a Record) to align with basic C# concepts
    // typically introduced before advanced functional features.
    public class EmbeddingResult
    {
        public int ChunkId { get; set; }
        public float[] Vector { get; set; }
        public string SourceText { get; set; }
    }
}
