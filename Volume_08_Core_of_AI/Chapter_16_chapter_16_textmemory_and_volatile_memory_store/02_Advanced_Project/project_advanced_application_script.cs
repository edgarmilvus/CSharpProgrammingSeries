
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
using System.Text;

/*
 * REAL-WORLD PROBLEM CONTEXT:
 * ---------------------------
 * Imagine you are building a "Smart Local Guide" for a mobile app that helps tourists
 * navigate a small town's hidden gems (cafes, viewpoints, historical spots).
 *
 * The challenge: The app needs to answer natural language queries like:
 * "Where can I find a quiet place to read?" or "Show me a spot with a great sunset view."
 *
 * The data is unstructured text descriptions. We need a way to store these descriptions
 * as "memories" and retrieve them based on semantic meaning, not just keyword matching.
 * This code demonstrates how to use Semantic Kernel's VolatileMemoryStore to act
 * as the brain for this local guide, enabling semantic search over local knowledge.
 */

public class LocalGuideMemory
{
    // The core interface for interacting with memory.
    // We will simulate the creation of this through our manual implementation below.
    private readonly ITextMemory _memory;

    public LocalGuideMemory(ITextMemory memory)
    {
        _memory = memory;
    }

    public async Task LoadKnowledgeBaseAsync()
    {
        Console.WriteLine("Loading local knowledge base into volatile memory...");

        // Data: Real-world spots described by locals.
        var spots = new[]
        {
            ("The Rusty Anchor", "A dimly lit, quiet pub in the basement of an old building. Smells of old wood and whiskey. Perfect for reading alone."),
            ("Skyline Peak", "A high mountain peak accessible by a steep trail. Offers a panoramic view of the valley, especially at sunset. Windy and cold."),
            ("Grandma's Bakery", "A warm, bustling bakery on Main Street. The smell of fresh cinnamon rolls fills the air. Loud chatter and laughter."),
            ("Silent Creek", "A hidden trail behind the old mill. The sound of flowing water is soothing. Lots of shade and mossy rocks. Very quiet."),
            ("The Neon Arcade", "A retro arcade filled with the sounds of 8-bit music and button mashing. Bright lights and crowds.")
        };

        // Storing memories using unique keys.
        // In a real app, these keys would be database IDs.
        foreach (var (key, description) in spots)
        {
            // We use the 'collection' parameter to group these memories (like a table name).
            await _memory.SaveInformationAsync("LocalSpots", description, key);
        }
    }

    public async Task FindBestSpotAsync(string userQuery)
    {
        Console.WriteLine($"\nUser Query: \"{userQuery}\"");
        Console.WriteLine("Searching memory for relevant context...");

        // This performs the vector search (Cosine Similarity) internally.
        // It returns the key of the most relevant memory.
        var result = await _memory.RecallAsync("LocalSpots", userQuery);

        if (result != null)
        {
            Console.WriteLine($"Recommendation: Check out '{result.Key}'");
            Console.WriteLine($"Description: {result.Value}");
        }
        else
        {
            Console.WriteLine("I don't have a recommendation for that specific vibe.");
        }
    }
}

/*
 * IMPLEMENTATION DETAILS:
 * Below is the custom implementation of the VolatileMemoryStore and TextMemory.
 * This mimics how Semantic Kernel works under the hood for this specific chapter's scope.
 */

public interface IMemoryStore
{
    Task<string> SaveAsync(string collection, string key, string embedding, string text);
    Task<(string Key, string Text)?> GetNearestAsync(string collection, string embedding);
}

public interface ITextMemory
{
    Task SaveInformationAsync(string collection, string text, string key);
    Task<(string Key, string Text)?> RecallAsync(string collection, string query);
}

public class VolatileMemoryStore : IMemoryStore
{
    // The "Volatile" storage: A Dictionary held in RAM.
    // Structure: Collection -> Key -> (EmbeddingString, Text)
    private readonly Dictionary<string, Dictionary<string, (string Embedding, string Text)>> _storage;

    public VolatileMemoryStore()
    {
        _storage = new Dictionary<string, Dictionary<string, (string, string)>>();
    }

    public Task<string> SaveAsync(string collection, string key, string embedding, string text)
    {
        if (!_storage.ContainsKey(collection))
        {
            _storage[collection] = new Dictionary<string, (string, string)>();
        }

        _storage[collection][key] = (embedding, text);
        return Task.FromResult(key);
    }

    public Task<(string Key, string Text)?> GetNearestAsync(string collection, string embedding)
    {
        if (!_storage.ContainsKey(collection))
        {
            return Task.FromResult<(string Key, string Text)?>(null);
        }

        (string Key, string Text)? bestMatch = null;
        double highestSimilarity = -1.0;

        // Iterate through all items in the collection to find the best match.
        foreach (var item in _storage[collection])
        {
            // Calculate similarity between the query embedding and stored embedding.
            double similarity = CalculateCosineSimilarity(embedding, item.Value.Embedding);

            if (similarity > highestSimilarity)
            {
                highestSimilarity = similarity;
                bestMatch = (item.Key, item.Value.Text);
            }
        }

        // Only return if similarity is above a threshold (0.5 is arbitrary for this demo).
        if (highestSimilarity > 0.5)
        {
            return Task.FromResult(bestMatch);
        }

        return Task.FromResult<(string Key, string Text)?>(null);
    }

    // Simulated Vector Math:
    // In a real system, this uses high-dimensional vectors (e.g., 1536 dimensions).
    // Here, we simulate it by treating the text as a "bag of words" and counting overlaps.
    // This is a "toy" implementation to demonstrate the *logic* without heavy libraries.
    private double CalculateCosineSimilarity(string textA, string textB)
    {
        // Simple normalization: convert to lowercase, remove punctuation.
        var wordsA = textA.ToLower().Split(new[] { ' ', ',', '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);
        var wordsB = textB.ToLower().Split(new[] { ' ', ',', '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);

        // Create frequency maps (Vector representation)
        var vecA = new Dictionary<string, int>();
        var vecB = new Dictionary<string, int>();

        foreach (var w in wordsA) vecA[w] = vecA.ContainsKey(w) ? vecA[w] + 1 : 1;
        foreach (var w in wordsB) vecB[w] = vecB.ContainsKey(w) ? vecB[w] + 1 : 1;

        // Dot Product
        double dotProduct = 0;
        foreach (var w in vecA.Keys)
        {
            if (vecB.ContainsKey(w))
            {
                dotProduct += vecA[w] * vecB[w];
            }
        }

        // Magnitudes
        double magA = 0;
        foreach (var v in vecA.Values) magA += v * v;
        magA = Math.Sqrt(magA);

        double magB = 0;
        foreach (var v in vecB.Values) magB += v * v;
        magB = Math.Sqrt(magB);

        if (magA == 0 || magB == 0) return 0;

        return dotProduct / (magA * magB);
    }
}

public class TextMemory : ITextMemory
{
    private readonly IMemoryStore _store;
    private readonly ITextEmbeddingGeneration _embeddingGenerator;

    public TextMemory(IMemoryStore store, ITextEmbeddingGeneration embeddingGenerator)
    {
        _store = store;
        _embeddingGenerator = embeddingGenerator;
    }

    public async Task SaveInformationAsync(string collection, string text, string key)
    {
        // Step 1: Convert text to a vector (embedding).
        string embedding = await _embeddingGenerator.GenerateEmbeddingAsync(text);

        // Step 2: Store the vector and text in the volatile store.
        await _store.SaveAsync(collection, key, embedding, text);
    }

    public async Task<(string Key, string Text)?> RecallAsync(string collection, string query)
    {
        // Step 1: Convert the user's query into a vector.
        string queryEmbedding = await _embeddingGenerator.GenerateEmbeddingAsync(query);

        // Step 2: Ask the store to find the nearest neighbor.
        return await _store.GetNearestAsync(collection, queryEmbedding);
    }
}

public interface ITextEmbeddingGeneration
{
    Task<string> GenerateEmbeddingAsync(string text);
}

/*
 * FAKE EMBEDDING GENERATOR:
 * This simulates an AI model (like Azure OpenAI) that turns text into numbers.
 * It ensures the code runs standalone without API keys.
 */
public class FakeEmbeddingGenerator : ITextEmbeddingGeneration
{
    // A simple hash-based string to "vector" logic for demonstration.
    // In reality, this would be a call to an external API.
    public Task<string> GenerateEmbeddingAsync(string text)
    {
        // We normalize the text to ensure similar sentences get similar "embeddings"
        // (In this fake version, we just return the normalized text string itself,
        // which our VolatileStore's 'CalculateCosineSimilarity' knows how to interpret).
        return Task.FromResult(text.ToLower());
    }
}

public class Program
{
    public static async Task Main(string[] args)
    {
        // 1. Initialize the components (Dependency Injection style)
        var embeddingGenerator = new FakeEmbeddingGenerator();
        var memoryStore = new VolatileMemoryStore();
        var textMemory = new TextMemory(memoryStore, embeddingGenerator);

        // 2. Create our application logic
        var localGuide = new LocalGuideMemory(textMemory);

        // 3. Load data
        await localGuide.LoadKnowledgeBaseAsync();

        // 4. Perform Semantic Search
        // Notice: The queries don't match keywords exactly, but the meaning is preserved.
        await localGuide.FindBestSpotAsync("I want a place to read a book in peace.");
        
        await localGuide.FindBestSpotAsync("Where is the best view for photography?");
        
        await localGuide.FindBestSpotAsync("Somewhere loud with friends.");
    }
}
