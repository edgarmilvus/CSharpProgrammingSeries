
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

// Source File: solution_exercise_4.cs
// Description: Solution for Exercise 4
// ==========================================

using System;
using System.Collections.Generic;

namespace ChatSystem.Core
{
    // 1. Vectorizer Interface
    public interface IVectorizer<T>
    {
        float[] Embed(T input);
    }

    // Concrete implementation for strings (dummy logic)
    public class SimpleStringVectorizer : IVectorizer<string>
    {
        public float[] Embed(string input)
        {
            // Simple hashing logic for demonstration
            float sum = 0;
            foreach (char c in input) sum += (int)c;
            return new float[] { sum };
        }
    }

    // 2. Generic Vector Record
    public record VectorRecord<T>
    {
        public T Data { get; init; }
        public float[] Vector { get; init; }

        // Private constructor to enforce creation via factory
        private VectorRecord(T data, float[] vector)
        {
            Data = data;
            Vector = vector;
        }

        // Factory method to compute vector immediately
        public static VectorRecord<T> Create(T data, IVectorizer<T> vectorizer)
        {
            var vec = vectorizer.Embed(data);
            return new VectorRecord<T>(data, vec);
        }
    }

    public static class SemanticSearchEngine
    {
        // 3. Challenge: Euclidean Distance Search without LINQ
        public static T FindClosest<T>(List<VectorRecord<T>> records, float[] queryVector)
        {
            if (records == null || records.Count == 0) throw new ArgumentException("No records provided.");

            double minDistance = double.MaxValue;
            VectorRecord<T> bestMatch = null;

            foreach (var record in records)
            {
                double distance = CalculateDistance(record.Vector, queryVector);
                
                if (distance < minDistance)
                {
                    minDistance = distance;
                    bestMatch = record;
                }
            }

            return bestMatch.Data;
        }

        private static double CalculateDistance(float[] v1, float[] v2)
        {
            double sum = 0;
            // Assuming vectors are same length for simplicity
            for (int i = 0; i < v1.Length; i++)
            {
                double diff = v1[i] - v2[i];
                sum += diff * diff;
            }
            return Math.Sqrt(sum);
        }
    }

    public class Exercise4Runner
    {
        public static void Run()
        {
            var vectorizer = new SimpleStringVectorizer();
            var db = new List<VectorRecord<string>>();

            db.Add(VectorRecord<string>.Create("The cat sat on the mat", vectorizer));
            db.Add(VectorRecord<string>.Create("The dog ran in the park", vectorizer));
            db.Add(VectorRecord<string>.Create("Feline behavior analysis", vectorizer));

            // Query vector
            float[] queryVec = vectorizer.Embed("cat");

            string result = SemanticSearchEngine.FindClosest(db, queryVec);

            Console.WriteLine($"Best Match: {result}");
        }
    }
}
