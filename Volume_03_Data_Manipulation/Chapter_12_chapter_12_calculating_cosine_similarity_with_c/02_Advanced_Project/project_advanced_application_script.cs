
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

# Source File: project_advanced_application_script.cs
# Description: Advanced Application Script
# ==========================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Book3_Chapter12_Advanced
{
    class Program
    {
        static void Main(string[] args)
        {
            // ---------------------------------------------------------
            // 1. REAL-WORLD CONTEXT
            // ---------------------------------------------------------
            // Imagine a news aggregation platform that ingests thousands of articles daily.
            // To prevent showing duplicate or highly similar content to users, we need a pipeline
            // that cleans the raw text, converts it into numerical vectors (embeddings), and 
            // calculates similarity scores. We will simulate this using functional LINQ pipelines.

            // ---------------------------------------------------------
            // 2. RAW DATA SOURCE (SIMULATED)
            // ---------------------------------------------------------
            // A collection of raw text snippets representing articles.
            // In a real scenario, this might come from an API or database.
            List<string> rawArticles = new List<string>
            {
                "The quick brown fox jumps over the lazy dog.",
                "A quick brown dog leaps over the sleepy fox.",
                "The weather is sunny and warm today.",
                "It is a sunny and warm day with clear skies.",
                "C# and LINQ provide powerful data manipulation tools.",
                "LINQ in C# allows for declarative data querying."
            };

            // ---------------------------------------------------------
            // 3. DATA PREPROCESSING PIPELINE (FUNCTIONAL)
            // ---------------------------------------------------------
            // We define a pipeline that cleans and normalizes text.
            // CRITICAL: This uses Deferred Execution. The query is not executed 
            // until .ToList() is called. This allows us to compose logic without 
            // iterating the data multiple times yet.
            var processedTexts = rawArticles
                .Select(text => text.ToLowerInvariant()) // Normalize case
                .Select(text => new string(text.Where(c => char.IsLetterOrDigit(c) || char.IsWhiteSpace(c)).ToArray())) // Remove punctuation
                .Select(text => string.Join(" ", text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))) // Normalize whitespace
                .ToList(); // IMMEDIATE EXECUTION: Forces evaluation and creates a concrete list.

            // ---------------------------------------------------------
            // 4. VOCABULARY CONSTRUCTION
            // ---------------------------------------------------------
            // To create vectors, we need a unique set of all words (tokens) across all documents.
            // We use functional composition to flatten the split words and distinct them.
            var vocabulary = processedTexts
                .SelectMany(text => text.Split(' ')) // Flatten all words from all sentences into one stream
                .Distinct() // Get unique words
                .OrderBy(word => word) // Ensure deterministic order
                .ToList();

            // ---------------------------------------------------------
            // 5. VECTORIZATION (BAG OF WORDS)
            // ---------------------------------------------------------
            // Convert each text into a dense vector based on the vocabulary.
            // We use a functional approach to generate a vector for each text.
            // Note: For high-performance scenarios with massive vocabularies, 
            // System.Numerics.Vector<T> is used for SIMD operations. 
            // Here, we use standard arrays to represent the concept clearly.
            var documentVectors = processedTexts
                .Select(text => CreateTermFrequencyVector(text, vocabulary))
                .ToList();

            // ---------------------------------------------------------
            // 6. PARALLEL SIMILARITY CALCULATION
            // ---------------------------------------------------------
            // We calculate the cosine similarity between every pair of documents.
            // We use PLINQ (.AsParallel()) to utilize multiple CPU cores.
            // We project the data into an anonymous type containing metadata and the score.
            var similarityResults = documentVectors
                .SelectMany((vecA, indexA) => documentVectors
                    .Select((vecB, indexB) => new { IndexA = indexA, IndexB = indexB, VectorA = vecA, VectorB = vecB })
                    .Where(pair => pair.IndexA < pair.IndexB) // Avoid duplicate pairs (A,B) and (A,A)
                )
                .AsParallel() // Enable parallel processing
                .Select(pair => new
                {
                    Doc1 = rawArticles[pair.IndexA],
                    Doc2 = rawArticles[pair.IndexB],
                    Score = CalculateCosineSimilarity(pair.VectorA, pair.VectorB)
                })
                .ToList(); // Execute the heavy computation

            // ---------------------------------------------------------
            // 7. RESULT ANALYSIS & FILTERING
            // ---------------------------------------------------------
            // Filter results to find highly similar documents (e.g., score > 0.8).
            // This is another functional pipeline acting on the previous results.
            var duplicates = similarityResults
                .Where(r => r.Score > 0.80)
                .OrderByDescending(r => r.Score)
                .ToList();

            // ---------------------------------------------------------
            // 8. OUTPUT
            // ---------------------------------------------------------
            Console.WriteLine($"Processed {rawArticles.Count} articles.");
            Console.WriteLine($"Vocabulary Size: {vocabulary.Count} unique words.\n");

            Console.WriteLine("--- Potential Duplicates (Similarity > 0.8) ---");
            foreach (var dup in duplicates)
            {
                Console.WriteLine($"Score: {dup.Score:F4}");
                Console.WriteLine($"  > {dup.Doc1}");
                Console.WriteLine($"  > {dup.Doc2}");
                Console.WriteLine();
            }

            // ---------------------------------------------------------
            // 9. DEMONSTRATING DEFERRED EXECUTION
            // ---------------------------------------------------------
            // Let's prove that the 'processedTexts' query was truly deferred.
            // We modify the source 'rawArticles' AFTER defining the query.
            rawArticles.Add("The fox is quick and the dog is lazy."); // New article

            // Now we execute the pipeline again.
            // The 'processedTexts' variable was a List created earlier, so it won't change.
            // However, if we re-define the query without .ToList(), it would include the new data.
            var dynamicQuery = rawArticles
                .Select(text => text.ToLowerInvariant())
                .Where(text => text.Contains("fox")); // Filter for foxes

            Console.WriteLine("--- Deferred Execution Demo ---");
            Console.WriteLine($"Query defined. Articles count in source: {rawArticles.Count}");
            
            // Execution happens here:
            var foxArticles = dynamicQuery.ToList(); 
            Console.WriteLine($"Fox articles found: {foxArticles.Count}");
            // Note: The new article "The fox is quick..." was included because we didn't 
            // force execution (ToList) until after modifying the source.
        }

        // ---------------------------------------------------------
        // HELPER: Create Term Frequency Vector
        // ---------------------------------------------------------
        // Pure function: No side effects. Takes input, returns output.
        // Maps the text to a numerical vector based on the global vocabulary.
        static int[] CreateTermFrequencyVector(string text, List<string> vocab)
        {
            var words = text.Split(' ');
            
            // Initialize vector of zeros
            int[] vector = new int[vocab.Count];

            // Map words to indices in the vocabulary
            // We use a simple loop here because LINQ cannot easily modify an external array index
            // (and modifying external variables inside Select is forbidden).
            // However, we can use a functional approach with Aggregate or just a simple foreach 
            // since we are modifying the array directly (this is a boundary where imperative code is acceptable 
            // for performance, though pure LINQ could map indices first).
            foreach (var word in words)
            {
                int index = vocab.IndexOf(word);
                if (index >= 0)
                {
                    vector[index]++;
                }
            }
            return vector;
        }

        // ---------------------------------------------------------
        // HELPER: Calculate Cosine Similarity
        // ---------------------------------------------------------
        // Pure function. Calculates the cosine of the angle between two vectors.
        // Formula: dotProduct(A, B) / (||A|| * ||B||)
        static double CalculateCosineSimilarity(int[] vecA, int[] vecB)
        {
            // 1. Calculate Dot Product
            // We use LINQ's Zip to process both arrays in parallel functional style.
            double dotProduct = vecA.Zip(vecB, (a, b) => a * b).Sum();

            // 2. Calculate Magnitudes (Euclidean Norm)
            // We use LINQ's Aggregate for the sum of squares.
            double magnitudeA = Math.Sqrt(vecA.Sum(x => (double)x * x));
            double magnitudeB = Math.Sqrt(vecB.Sum(x => (double)x * x));

            // 3. Handle division by zero
            if (magnitudeA == 0 || magnitudeB == 0)
                return 0;

            return dotProduct / (magnitudeA * magnitudeB);
        }
    }
}
