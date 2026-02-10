
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

// Source File: solution_exercise_5.cs
// Description: Solution for Exercise 5
// ==========================================

using Bogus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

public record KnowledgeBaseArticle(Guid Id, string Title, string Body, string EmbeddedQuestionHash);

public class RagDataSeeder
{
    public List<KnowledgeBaseArticle> GenerateTestDataset()
    {
        // 1. Seed Questions
        var seedQuestions = new List<string>
        {
            "How do I reset my password?",
            "What is the refund policy?",
            "How to upgrade my subscription?",
            "Where can I find my invoices?",
            "How to contact support?"
        };

        // 2. Setup Faker for Titles
        var faker = new Faker();
        
        var articles = new List<KnowledgeBaseArticle>();

        // 3. Generate 50 Articles (10 per question)
        foreach (var question in seedQuestions)
        {
            // Calculate the SHA256 Hash of the seed question for the logical key
            string hash = ComputeSha256Hash(question);

            // Generate 10 articles for this specific question
            for (int i = 0; i < 10; i++)
            {
                var title = faker.Commerce.ProductName(); // Random title
                var body = GenerateBodyFromQuestion(question, faker); // Synthesized body

                articles.Add(new KnowledgeBaseArticle(
                    Id: Guid.NewGuid(),
                    Title: title,
                    Body: body,
                    EmbeddedQuestionHash: hash
                ));
            }
        }

        return articles;
    }

    // Helper to synthesize body text based on the question (Simulating LLM/Rules)
    private string GenerateBodyFromQuestion(string question, Faker faker)
    {
        // In a real scenario, this would be an LLM call.
        // Here, we use simple concatenation to simulate variation.
        var synonyms = new[] { "To accomplish", "In order to", "Simply" };
        var action = faker.PickRandom(synonyms);
        
        return $"{action} {question.ToLower()} you should follow these steps: " +
               $"{faker.Lorem.Sentence()} {faker.Lorem.Sentence()} " +
               $"Ensure you have the necessary permissions before proceeding.";
    }

    // Helper for SHA256 Hashing
    private static string ComputeSha256Hash(string rawData)
    {
        using (SHA256 sha256Hash = SHA256.Create())
        {
            byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
                builder.Append(bytes[i].ToString("x2"));
            }
            return builder.ToString();
        }
    }

    public void ValidateDistribution(List<KnowledgeBaseArticle> articles)
    {
        // 4. Validation using LINQ
        var distribution = articles
            .GroupBy(a => a.EmbeddedQuestionHash)
            .Select(g => new { Hash = g.Key, Count = g.Count() })
            .OrderBy(x => x.Hash)
            .ToList();

        Console.WriteLine("Validation Results:");
        foreach (var item in distribution)
        {
            Console.WriteLine($"Hash: {item.Hash.Substring(0, 8)}... | Count: {item.Count}");
        }

        // Assert even distribution
        bool isEven = distribution.All(d => d.Count == 10);
        Console.WriteLine($"Distribution is even: {isEven}");
    }
}
