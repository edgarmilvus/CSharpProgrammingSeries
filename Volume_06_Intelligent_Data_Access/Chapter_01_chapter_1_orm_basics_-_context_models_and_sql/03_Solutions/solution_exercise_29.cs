
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

# Source File: solution_exercise_29.cs
# Description: Solution for Exercise 29
# ==========================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// 1. Multilingual Model Strategy
public class MultilingualEmbeddingService
{
    // Use a single model that supports multiple languages (e.g., LaBSE, Sentence-BERT multilingual)
    // This maps "Hello" and "Hola" to the same vector space.

    public async Task<float[]> GenerateAsync(string text, string languageHint)
    {
        // The model doesn't strictly need the language hint, 
        // but it helps with tokenization (e.g., splitting words correctly).
        
        // 1. Tokenize based on language (if using a non-universal tokenizer)
        var tokens = Tokenize(text, languageHint);

        // 2. Generate vector
        return await CallModelAsync(tokens);
    }

    private string[] Tokenize(string text, string lang)
    {
        // Different languages have different whitespace rules
        return lang switch
        {
            "zh" => text.Split(' ', StringSplitOptions.RemoveEmptyEntries), // Chinese usually no spaces
            _ => text.Split(' ', StringSplitOptions.RemoveEmptyEntries)
        };
    }

    private async Task<float[]> CallModelAsync(string[] tokens) => new float[768]; // Mock
}

// 2. Language Detection & Routing
public class CrossLingualSearch
{
    private readonly MultilingualEmbeddingService _embeddingService;
    private readonly IVectorRepository<Document> _vectorRepo;

    public async Task<List<Document>> SearchAnyLanguage(string query)
    {
        // 1. Detect Language (Optional, but helpful for tokenization)
        var lang = DetectLanguage(query);

        // 2. Generate Embedding
        // "Hello" (en) and "Bonjour" (fr) will have similar vectors if using a multilingual model
        var vector = await _embeddingService.GenerateAsync(query, lang);

        // 3. Search Vector DB (Language Agnostic)
        return await _vectorRepo.SearchAsync(vector, 10);
    }

    private string DetectLanguage(string text)
    {
        // Mock detection logic
        return text.Any(c => c > 128) ? "non-ascii" : "en";
    }
}
