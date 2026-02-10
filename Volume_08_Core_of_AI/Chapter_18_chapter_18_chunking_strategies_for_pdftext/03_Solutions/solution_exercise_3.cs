
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
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Services;

namespace ChunkingExercises
{
    public class SemanticChunker
    {
        private readonly ITextEmbeddingGenerationService _embeddingService;
        private readonly double _similarityThreshold;

        public SemanticChunker(ITextEmbeddingGenerationService embeddingService, double similarityThreshold = 0.5)
        {
            _embeddingService = embeddingService;
            _similarityThreshold = similarityThreshold;
        }

        public async Task<List<string>> ChunkTextAsync(string text)
        {
            // 1. Split text into sentences
            // Simple regex to split on periods followed by space or newline
            var sentences = Regex.Split(text, @"(?<=[\.!\?])\s+")
                                 .Where(s => !string.IsNullOrWhiteSpace(s))
                                 .ToList();

            if (sentences.Count == 0) return new List<string>();

            // 2. Generate embeddings for each sentence
            var embeddings = new List<float[]>();
            foreach (var sentence in sentences)
            {
                // Note: In a real scenario, batching these calls is more efficient
                var embedding = await _embeddingService.GenerateEmbeddingAsync(sentence);
                embeddings.Add(embedding.ToArray());
            }

            // 3. Calculate Cosine Similarity and Group
            var chunks = new List<string>();
            var currentChunkSentences = new List<string> { sentences[0] };

            for (int i = 1; i < sentences.Count; i++)
            {
                // Calculate similarity between previous and current sentence
                double similarity = CosineSimilarity(embeddings[i - 1], embeddings[i]);

                if (similarity < _similarityThreshold)
                {
                    // Semantic break detected: finalize current chunk
                    chunks.Add(string.Join(" ", currentChunkSentences));
                    currentChunkSentences.Clear();
                }

                // Add current sentence to the (potentially new) chunk
                currentChunkSentences.Add(sentences[i]);
            }

            // Add the final chunk
            if (currentChunkSentences.Count > 0)
            {
                chunks.Add(string.Join(" ", currentChunkSentences));
            }

            return chunks;
        }

        private double CosineSimilarity(float[] vectorA, float[] vectorB)
        {
            double dotProduct = 0.0;
            double magnitudeA = 0.0;
            double magnitudeB = 0.0;

            for (int i = 0; i < vectorA.Length; i++)
            {
                dotProduct += vectorA[i] * vectorB[i];
                magnitudeA += vectorA[i] * vectorA[i];
                magnitudeB += vectorB[i] * vectorB[i];
            }

            magnitudeA = Math.Sqrt(magnitudeA);
            magnitudeB = Math.Sqrt(magnitudeB);

            if (magnitudeA == 0 || magnitudeB == 0) return 0;
            return dotProduct / (magnitudeA * magnitudeB);
        }
    }

    // Note: To run this, you would need a valid Kernel configuration.
    // Example usage:
    /*
    public class Program 
    {
        public static async Task Main(string[] args)
        {
            var kernel = Kernel.CreateBuilder()
                .AddOpenAITextEmbedding("text-embedding-ada-002", "API_KEY")
                .Build();

            var embeddingService = kernel.GetRequiredService<ITextEmbeddingGenerationService>();
            var chunker = new SemanticChunker(embeddingService, 0.5);

            string text = "Quantum entanglement is a physical phenomenon..."; // Sample text
            var chunks = await chunker.ChunkTextAsync(text);
            
            foreach(var chunk in chunks) Console.WriteLine(chunk);
        }
    }
    */
}
