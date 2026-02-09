
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

# Source File: theory_theoretical_foundations_part5.cs
# Description: Theoretical Foundations
# ==========================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

public class EmbeddingPipeline
{
    // Represents a raw document
    public record Document(string Id, string Content);

    // Represents a tokenized, normalized entry ready for vectorization
    public record ProcessedToken(string DocId, string Token, int Position);

    public static IEnumerable<ProcessedToken> PrepareForEmbedding(IEnumerable<Document> documents)
    {
        // Define the pipeline (Deferred Execution)
        var pipeline = documents
            // 1. Cleaning: Filter out documents with insufficient content
            .Where(d => !string.IsNullOrEmpty(d.Content) && d.Content.Length > 10)
            
            // 2. Normalization: Lowercase and remove special characters
            .Select(d => new Document(
                d.Id, 
                Regex.Replace(d.Content.ToLower(), @"[^\w\s]", "")
            ))
            
            // 3. Tokenization: Flatten documents into individual words using SelectMany
            .SelectMany(d => 
                d.Content.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                    .Select((word, index) => new { Word = word, Index = index })
            , (doc, tokenInfo) => new ProcessedToken(
                doc.Id, 
                tokenInfo.Word, 
                tokenInfo.Index
            ))
            
            // 4. Filtering: Remove stop words (conceptually)
            .Where(t => t.Token.Length > 2); // Simple filter for demo

        // The pipeline is defined but not executed yet.
        // We can now iterate or convert to list.
        return pipeline;
    }

    public static void ExecutePipeline()
    {
        var rawData = new List<Document>
        {
            new("Doc1", "The quick brown fox!"),
            new("Doc2", "Jumps over the lazy dog."),
            new("Doc3", "") // Empty, will be filtered
        };

        // Execution happens here (Deferred)
        foreach (var token in PrepareForEmbedding(rawData))
        {
            Console.WriteLine($"Token: {token.Token} (Doc: {token.DocId})");
        }
    }
}
