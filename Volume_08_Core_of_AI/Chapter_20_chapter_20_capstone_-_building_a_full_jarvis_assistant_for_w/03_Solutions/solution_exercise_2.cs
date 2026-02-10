
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

// Source File: solution_exercise_2.cs
// Description: Solution for Exercise 2
// ==========================================

// Project: Jarvis.MemoryTest.csproj
// Dependencies: Microsoft.SemanticKernel, Microsoft.SemanticKernel.Memory

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Memory;
using System;
using System.Threading.Tasks;

namespace Jarvis.MemoryTest
{
    public class UserMemoryManager
    {
        private readonly ISemanticTextMemory _memory;

        public UserMemoryManager(Kernel kernel)
        {
            // In a real scenario, we would inject a persistent store like QdrantMemoryStore.
            // For this exercise, we use VolatileMemoryStore but treat it as if it were persistent 
            // to demonstrate the API pattern.
            _memory = kernel.Memory;
        }

        public async Task SavePreferenceAsync(string userId, string key, string value)
        {
            string collection = $"user_preferences_{userId}";
            
            // We save the value as the text, and the key is part of the metadata or the text itself.
            // For semantic search to work best, we embed the full context.
            string memoryText = $"{key}: {value}";

            await _memory.SaveInformationAsync(
                collection: collection,
                text: memoryText,
                id: Guid.NewGuid().ToString(), // Unique ID for this memory entry
                description: $"User preference for {key}");
        }

        public async Task<string> RecallPreferenceAsync(string userId, string query)
        {
            string collection = $"user_preferences_{userId}";

            // Search for the most relevant memory based on the semantic meaning of the query
            var searchResults = _memory.SearchAsync(
                collection: collection,
                query: query,
                limit: 1,
                minRelevanceScore: 0.5); // Threshold for confidence

            await foreach (var result in searchResults)
            {
                return result.Metadata.Text; // Return the stored text (e.g., "working_hours: 9am-5pm")
            }

            return "I don't recall that preference.";
        }
    }

    class Program
    {
        static async Task Main(string[] args)
        {
            // 1. Configure Semantic Kernel
            // Note: For memory embedding, we technically need an embedding generator.
            // In a real app, we'd configure Azure OpenAI embeddings. 
            // For this demo, VolatileMemoryStore can work without embeddings for exact matches,
            // but Semantic Search requires them. We will simulate the flow.
            
            var builder = Kernel.CreateBuilder();
            // builder.AddAzureOpenAIChatCompletion(...) // Required for real embeddings
            var kernel = builder.Build();

            // 2. Initialize Manager
            var manager = new UserMemoryManager(kernel);

            // 3. Save a preference
            Console.WriteLine("Saving preference...");
            await manager.SavePreferenceAsync("user1", "working_hours", "9am-5pm");

            // 4. Recall with a different phrasing (Semantic Search)
            Console.WriteLine("Recalling with semantic query...");
            string query = "When do I typically work?";
            string result = await manager.RecallPreferenceAsync("user1", query);

            Console.WriteLine($"Result: {result}");
            
            // Explanation of nuance (printed to console for clarity):
            Console.WriteLine("\n--- Instructor's Note on Nuance ---");
            Console.WriteLine("Why Vector Memory? A simple dictionary (Key-Value) requires exact matching.");
            Console.WriteLine("If user asks 'What are my hours?' (Key: 'working_hours'), a dictionary fails.");
            Console.WriteLine("Vector memory embeds the meaning. 'When do I work?' maps to 'working_hours' semantically.");
        }
    }
}
