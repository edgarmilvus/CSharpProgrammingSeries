
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
using System.Text;

namespace HybridSearchApp
{
    // Real-world context: A digital library system allowing users to search for books.
    // Users can search by keywords (e.g., "science fiction") or by conceptual meaning (e.g., "space travel").
    // This application demonstrates a hybrid search engine that combines both methods for better results.

    class Program
    {
        static void Main(string[] args)
        {
            // 1. Initialize the data store (simulating a database).
            BookRepository repository = new BookRepository();
            List<Book> allBooks = repository.GetAllBooks();

            // 2. Define the user's search query.
            // In a real app, this comes from UI input.
            string userQuery = "history of ancient civilizations";

            Console.WriteLine($"Searching for: \"{userQuery}\"\n");

            // 3. Perform Hybrid Search.
            List<SearchResult> finalResults = PerformHybridSearch(allBooks, userQuery);

            // 4. Display results.
            Console.WriteLine("Top 5 Relevant Results:");
            Console.WriteLine("-----------------------");
            foreach (var result in finalResults)
            {
                Console.WriteLine($"[Score: {result.RelevanceScore:F2}] {result.Book.Title} by {result.Book.Author}");
                Console.WriteLine($"   Keywords Match: {result.KeywordScore:F2} | Vector Match: {result.VectorScore:F2}");
                Console.WriteLine($"   Description: {result.Book.Description}\n");
            }
        }

        // The Core Hybrid Search Logic
        static List<SearchResult> PerformHybridSearch(List<Book> books, string query)
        {
            // Step A: Preprocess Query (Tokenization and Normalization)
            string[] queryKeywords = PreprocessText(query);
            float[] queryVector = EmbedText(query); // Simulate generating an embedding vector

            // Step B: Calculate Scores for each book
            List<SearchResult> scoredResults = new List<SearchResult>();

            foreach (Book book in books)
            {
                // 1. Keyword Search Score (BM25 Simulation)
                float keywordScore = CalculateKeywordScore(book, queryKeywords);

                // 2. Vector Search Score (Cosine Similarity Simulation)
                float vectorScore = CalculateVectorScore(book.Embedding, queryVector);

                // 3. Combine Scores (Weighted Hybrid Fusion)
                // We weight keyword search slightly higher for precise term matching, 
                // but vector search helps with semantic understanding.
                float finalScore = (0.6f * keywordScore) + (0.4f * vectorScore);

                scoredResults.Add(new SearchResult
                {
                    Book = book,
                    KeywordScore = keywordScore,
                    VectorScore = vectorScore,
                    RelevanceScore = finalScore
                });
            }

            // Step C: Sort by Relevance (Descending)
            // Using a simple Bubble Sort for demonstration (avoiding LINQ per constraints).
            for (int i = 0; i < scoredResults.Count - 1; i++)
            {
                for (int j = 0; j < scoredResults.Count - i - 1; j++)
                {
                    if (scoredResults[j].RelevanceScore < scoredResults[j + 1].RelevanceScore)
                    {
                        // Swap
                        SearchResult temp = scoredResults[j];
                        scoredResults[j] = scoredResults[j + 1];
                        scoredResults[j + 1] = temp;
                    }
                }
            }

            // Return top 5 results
            List<SearchResult> topResults = new List<SearchResult>();
            for (int i = 0; i < Math.Min(5, scoredResults.Count); i++)
            {
                topResults.Add(scoredResults[i]);
            }

            return topResults;
        }

        // --- Helper Methods ---

        // Simulates text normalization and tokenization (Basic NLP)
        static string[] PreprocessText(string text)
        {
            // Convert to lowercase
            text = text.ToLower();
            // Remove punctuation (simplified)
            text = text.Replace(".", "").Replace(",", "").Replace(":", "");
            // Split into words
            return text.Split(' ');
        }

        // Simulates converting text to a dense vector (Embedding)
        // In reality, this uses a model like BERT or OpenAI Ada-002.
        // Here, we generate a deterministic vector based on character sums for simulation.
        static float[] EmbedText(string text)
        {
            float[] vector = new float[5]; // 5-dimensional vector for simplicity
            for (int i = 0; i < text.Length; i++)
            {
                // Distribute character code sum across dimensions
                vector[i % 5] += (float)text[i];
            }
            // Normalize (L2 Norm)
            float magnitude = 0;
            foreach (var val in vector) magnitude += val * val;
            magnitude = (float)Math.Sqrt(magnitude);
            for (int i = 0; i < vector.Length; i++) vector[i] /= magnitude;
            
            return vector;
        }

        // Simulates BM25 / TF-IDF scoring
        static float CalculateKeywordScore(Book book, string[] queryKeywords)
        {
            float score = 0;
            string bookContent = (book.Title + " " + book.Description).ToLower();

            foreach (string keyword in queryKeywords)
            {
                if (bookContent.Contains(keyword))
                {
                    // Simple frequency count simulation
                    score += 1.0f; 
                }
            }
            return score;
        }

        // Calculates Cosine Similarity between two vectors
        static float CalculateVectorScore(float[] vecA, float[] vecB)
        {
            if (vecA.Length != vecB.Length) return 0;

            float dotProduct = 0;
            for (int i = 0; i < vecA.Length; i++)
            {
                dotProduct += vecA[i] * vecB[i];
            }

            // Cosine Similarity = Dot Product (since vectors are normalized)
            // Returns value between -1 and 1. We normalize to 0-1 range for scoring.
            return Math.Max(0, dotProduct); 
        }
    }

    // --- Data Models ---

    class Book
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string Description { get; set; }
        public float[] Embedding { get; set; } // The vector representation
    }

    class SearchResult
    {
        public Book Book { get; set; }
        public float KeywordScore { get; set; }
        public float VectorScore { get; set; }
        public float RelevanceScore { get; set; }
    }

    // --- Mock Data Repository ---

    class BookRepository
    {
        public List<Book> GetAllBooks()
        {
            return new List<Book>
            {
                new Book 
                { 
                    Id = 1, 
                    Title = "The Roman Empire", 
                    Author = "J. Smith", 
                    Description = "A comprehensive history of the rise and fall of Rome.", 
                    Embedding = Program.EmbedText("history roman empire rise fall")
                },
                new Book 
                { 
                    Id = 2, 
                    Title = "Space Odyssey", 
                    Author = "A. Clarke", 
                    Description = "A fictional journey through the stars and beyond.", 
                    Embedding = Program.EmbedText("fiction space stars travel")
                },
                new Book 
                { 
                    Id = 3, 
                    Title = "Ancient Civilizations of Egypt", 
                    Author = "D. Brown", 
                    Description = "Exploring the pyramids and the history of Egypt.", 
                    Embedding = Program.EmbedText("ancient egypt history pyramids")
                },
                new Book 
                { 
                    Id = 4, 
                    Title = "Modern Cooking Techniques", 
                    Author = "G. Ramsay", 
                    Description = "A guide to molecular gastronomy and cooking.", 
                    Embedding = Program.EmbedText("cooking food science techniques")
                },
                new Book 
                { 
                    Id = 5, 
                    Title = "History of Mathematics", 
                    Author = "E. Newton", 
                    Description = "From ancient counting to modern calculus.", 
                    Embedding = Program.EmbedText("math history numbers calculus")
                }
            };
        }
    }
}
