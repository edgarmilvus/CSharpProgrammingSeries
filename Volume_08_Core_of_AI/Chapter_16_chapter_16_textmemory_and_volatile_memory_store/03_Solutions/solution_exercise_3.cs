
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

# Source File: solution_exercise_3.cs
# Description: Solution for Exercise 3
# ==========================================

using System;
using System.Collections.Generic;
using System.Linq;

namespace VectorSearchMechanics
{
    class Program
    {
        static void Main()
        {
            // 1. Setup Data
            var records = new List<MemoryRecord>
            {
                new MemoryRecord { Key = "1", Text = "Internet dropping", Embedding = new float[] { 0.9f, 0.2f } },
                new MemoryRecord { Key = "2", Text = "Subscription upgrade", Embedding = new float[] { 0.1f, 0.9f } },
                new MemoryRecord { Key = "3", Text = "Router lights red", Embedding = new float[] { 0.8f, 0.3f } }
            };

            // Query Vector (Simulating "Connection issues")
            // In a real scenario, this would come from an embedding model.
            var queryVector = new float[] { 0.85f, 0.25f };

            // 2. Perform Manual Search
            var manualResult = SearchManual(queryVector, records);
            Console.WriteLine($"Manual Search Winner: Key {manualResult.Key} - '{manualResult.Text}' (Score: {manualResult.Score})");

            // 3. Edge Case: Zero Vector
            var zeroVector = new float[] { 0f, 0f };
            var edgeResult = SearchManual(zeroVector, records);
            Console.WriteLine($"Edge Case Result: {edgeResult?.Text ?? "Handled Gracefully (Null)"}");
        }

        public static MemoryRecord? SearchManual(float[] queryVector, List<MemoryRecord> records)
        {
            MemoryRecord? bestMatch = null;
            double highestSimilarity = -1.0;

            foreach (var record in records)
            {
                double similarity = CalculateCosineSimilarity(queryVector, record.Embedding);

                // Handle NaN (division by zero)
                if (double.IsNaN(similarity)) continue;

                if (similarity > highestSimilarity)
                {
                    highestSimilarity = similarity;
                    bestMatch = record;
                    bestMatch.Score = similarity; // Attach score for display
                }
            }

            return bestMatch;
        }

        public static double CalculateCosineSimilarity(float[] vectorA, float[] vectorB)
        {
            if (vectorA.Length != vectorB.Length)
                throw new ArgumentException("Vectors must be of the same dimension.");

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

            // Edge Case Handling: Division by Zero
            if (magnitudeA == 0 || magnitudeB == 0)
            {
                return double.NaN;
            }

            return dotProduct / (magnitudeA * magnitudeB);
        }
    }

    public class MemoryRecord
    {
        public string Key { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public float[] Embedding { get; set; } = Array.Empty<float>();
        public double Score { get; set; } // Helper property for display
    }
}
