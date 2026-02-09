
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

# Source File: basic_basic_code_example.cs
# Description: Basic Code Example
# ==========================================

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Memory;
using Microsoft.SemanticKernel.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TextMemoryBasics
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // 1. Initialize the Semantic Kernel
            // The kernel is the orchestrator of AI plugins and memory.
            var kernel = Kernel.CreateBuilder()
                .Build();

            // 2. Initialize the VolatileMemoryStore
            // This is an in-memory vector store. It is ephemeral (lost on app restart)
            // and optimized for speed. It implements ISemanticTextMemory.
            var memoryStore = new VolatileMemoryStore();

            // 3. Create a SemanticTextMemory instance
            // This wrapper handles the logic of generating embeddings and storing them.
            var memory = new SemanticTextMemory(memoryStore, kernel.Services);

            // Define a collection name (like a table in a relational DB)
            const string collectionName = "UserPreferences";

            Console.WriteLine("--- Storing Memories ---");

            // 4. Store specific text memories with embeddings
            // We are creating a semantic relationship between a unique ID and the text.
            // The kernel automatically generates the vector embedding behind the scenes.
            await memory.SaveInformationAsync(
                collection: collectionName,
                id: "pref1",
                text: "User prefers dark mode UI and compact layouts."
            );

            await memory.SaveInformationAsync(
                collection: collectionName,
                id: "pref2",
                text: "User likes spicy food and Italian cuisine."
            );

            await memory.SaveInformationAsync(
                collection: collectionName,
                id: "pref3",
                text: "User enjoys reading sci-fi novels on weekends."
            );

            Console.WriteLine("Memories saved successfully.\n");

            // 5. Perform a Semantic Search
            // We want to find relevant memories based on a query, not just keywords.
            // "What food does the user like?" is semantically close to "spicy Italian".
            Console.WriteLine("--- Searching for 'Favorite cuisine' ---");
            var searchResults = memory.SearchAsync(
                collection: collectionName,
                query: "What food does the user like?",
                limit: 2, // Top 2 results
                minRelevanceScore: 0.0 // Filter out irrelevant results if needed
            );

            await foreach (var result in searchResults)
            {
                Console.WriteLine($"ID: {result.Metadata.Id}");
                Console.WriteLine($"Text: {result.Metadata.Text}");
                Console.WriteLine($"Relevance Score: {result.Relevance:F4}");
                Console.WriteLine("-----------------------------");
            }

            // 6. Retrieve a specific memory by ID
            // Useful when you know the exact ID but need the full text/context.
            Console.WriteLine("\n--- Retrieving specific memory by ID 'pref3' ---");
            var specificMemory = await memory.GetAsync(collectionName, "pref3");

            if (specificMemory != null)
            {
                Console.WriteLine($"Retrieved: {specificMemory.Text}");
            }

            // 7. List all memories in a collection
            // Useful for debugging or dumping state.
            Console.WriteLine("\n--- Listing all memories in collection ---");
            await foreach (var item in memory.SearchAsync(collectionName, "", limit: 10))
            {
                Console.WriteLine($"- {item.Metadata.Id}: {item.Metadata.Text}");
            }
        }
    }
}
