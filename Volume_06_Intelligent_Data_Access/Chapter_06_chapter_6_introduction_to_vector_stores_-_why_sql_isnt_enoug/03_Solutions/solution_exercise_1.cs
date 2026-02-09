
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

# Source File: solution_exercise_1.cs
# Description: Solution for Exercise 1
# ==========================================

// Project: VectorEmbedding.Core
// File: IEmbeddingService.cs

using System.Threading;
using System.Threading.Tasks;

namespace VectorEmbedding.Core
{
    /// <summary>
    /// Defines a contract for generating vector embeddings from text.
    /// </summary>
    public interface IEmbeddingService
    {
        /// <summary>
        /// Generates a high-dimensional vector representation of the input text asynchronously.
        /// </summary>
        /// <param name="text">The raw text to embed.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>A task representing the async operation, containing a float array (vector).</returns>
        Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken = default);
    }
}

// Project: VectorEmbedding.Core
// File: MockEmbeddingService.cs

using System;
using System.Threading;
using System.Threading.Tasks;

namespace VectorEmbedding.Core
{
    /// <summary>
    /// A mock implementation of IEmbeddingService simulating an external AI provider.
    /// Supports deterministic generation via a constructor seed.
    /// </summary>
    public class MockEmbeddingService : IEmbeddingService
    {
        private const int VectorDimension = 1536;
        private readonly int? _seed;

        /// <summary>
        /// Initializes a new instance of the MockEmbeddingService.
        /// </summary>
        /// <param name="seed">Optional seed for deterministic vector generation.</param>
        public MockEmbeddingService(int? seed = null)
        {
            _seed = seed;
        }

        public async Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken = default)
        {
            // Simulate network latency (e.g., HTTP request to OpenAI)
            await Task.Delay(50, cancellationToken);

            // Initialize Random with a seed if provided, ensuring determinism.
            // We combine the text's hash code with the seed to ensure different texts
            // still produce different vectors even with a fixed seed.
            var random = _seed.HasValue 
                ? new Random(_seed.Value ^ text.GetHashCode()) 
                : Random.Shared;

            var vector = new float[VectorDimension];

            // Populate vector with random values between -1.0 and 1.0
            for (int i = 0; i < VectorDimension; i++)
            {
                // NextDouble() returns [0.0, 1.0). We map to [-1.0, 1.0).
                vector[i] = (float)(random.NextDouble() * 2.0 - 1.0);
            }

            return vector;
        }
    }
}

// Project: VectorEmbedding.ServiceRegistration (Console App or separate library for demo)
// File: Program.cs

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using VectorEmbedding.Core;

var builder = Host.CreateDefaultBuilder(args);

builder.ConfigureServices(services =>
{
    // Register the service with a singleton lifetime (appropriate for stateless or seeded services)
    // In a real scenario, if the service manages heavy resources, Transient or Scoped might be better.
    services.AddSingleton<IEmbeddingService>(provider => new MockEmbeddingService(seed: 42));
});

var host = builder.Build();

// Demonstration of usage
Console.WriteLine("Initializing Embedding Service...");
var embeddingService = host.Services.GetRequiredService<IEmbeddingService>();

var textToEmbed = "Sample product description: High-performance running shoes.";
Console.WriteLine($"Generating embedding for: '{textToEmbed}'");

var vector = await embeddingService.GenerateEmbeddingAsync(textToEmbed);

Console.WriteLine($"Generated vector of dimension: {vector.Length}");
Console.WriteLine($"First 5 values: {string.Join(", ", vector.Take(5))}");

// Interactive Challenge: Prove Determinism
Console.WriteLine("\n--- Proving Determinism ---");
var testVector1 = await embeddingService.GenerateEmbeddingAsync("test");
var testVector2 = await embeddingService.GenerateEmbeddingAsync("test");

bool areIdentical = true;
if (testVector1.Length != testVector2.Length)
{
    areIdentical = false;
}
else
{
    for (int i = 0; i < testVector1.Length; i++)
    {
        if (Math.Abs(testVector1[i] - testVector2[i]) > 0.0001f)
        {
            areIdentical = false;
            break;
        }
    }
}

Console.WriteLine($"Vectors for 'test' are identical: {areIdentical}");
