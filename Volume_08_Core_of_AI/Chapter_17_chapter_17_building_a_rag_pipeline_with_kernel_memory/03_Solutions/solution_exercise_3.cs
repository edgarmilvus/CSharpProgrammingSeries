
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

// Source File: solution_exercise_3.cs
// Description: Solution for Exercise 3
// ==========================================

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.AI;
using Microsoft.KernelMemory.MemoryStorage;
using System.ComponentModel;

namespace KernelMemory.Exercises;

public class RerankingPlugin
{
    private readonly IKernelMemory _memory;
    
    public RerankingPlugin(IKernelMemory memory)
    {
        _memory = memory;
    }

    [KernelFunction, Description("Expands the query, retrieves documents, and re-ranks them based on relevance.")]
    public async Task<string> RerankAndFilterAsync(
        Kernel kernel,
        [Description("The user query")] string query,
        [Description("The collection to search in")] string collection = "default",
        [Description("Threshold for relevance (0.0 to 1.0)")] double threshold = 0.75
    )
    {
        // 1. Query Expansion (Simulated LLM call)
        // In a real scenario, you'd invoke an LLM function to generate variations.
        // Here we simulate it for the exercise.
        var expandedQueries = await ExpandQueryAsync(kernel, query);
        
        var allResults = new List<MemoryRecord>();

        // 2. Hybrid Retrieval (Parallel Search)
        var searchTasks = expandedQueries.Select(q => _memory.SearchAsync(q, collection, limit: 5));
        var resultsSets = await Task.WhenAll(searchTasks);

        foreach (var set in resultsSets)
        {
            foreach (var item in set.Results)
            {
                // Deduplicate based on ID
                if (allResults.All(x => x.Id != item.Memory.Id))
                {
                    // Attach the specific query that found this result for context
                    item.Memory.Metadata["MatchedQuery"] = item.Query; 
                    allResults.Add(item.Memory);
                }
            }
        }

        // 3. Re-Ranking (Cross-Encoder Simulation)
        // We simulate a cross-encoder score. In production, use a specialized model.
        var rankedResults = new List<(MemoryRecord Record, double Score)>();
        
        foreach (var record in allResults)
        {
            // Simulate relevance score based on text overlap or semantic similarity
            // In real code: await _crossEncoder.ScoreAsync(query, record.Text);
            double score = await SimulateRelevanceScoreAsync(kernel, query, record.Text);
            
            if (score >= threshold)
            {
                rankedResults.Add((record, score));
            }
        }

        // Sort by score descending
        var topResults = rankedResults.OrderByDescending(r => r.Score).Take(3).ToList();

        // 4. Format context for the LLM
        if (!topResults.Any()) return "No relevant documents found.";

        var contextBuilder = new System.Text.StringBuilder();
        contextBuilder.AppendLine("Context based on retrieved documents:");
        
        foreach (var (record, score) in topResults)
        {
            contextBuilder.AppendLine($"[Relevance: {score:F2}] {record.Text}");
        }

        return contextBuilder.ToString();
    }

    private async Task<List<string>> ExpandQueryAsync(Kernel kernel, string query)
    {
        // Simulated LLM call for query expansion
        // Real implementation would use: kernel.CreateFunctionFromPrompt(...)
        return new List<string> 
        { 
            query, 
            $"{query} synonyms", 
            $"{query} explanation" 
        };
    }

    private async Task<double> SimulateRelevanceScoreAsync(Kernel kernel, string query, string text)
    {
        // Placeholder for Cross-Encoder logic
        // Returns a random score for demonstration, but implies async network call
        await Task.Delay(10); 
        return new Random().NextDouble(); 
    }
}

// Integration into a Kernel Function (Streaming)
public class OrchestratorPlugin
{
    [KernelFunction, Description("Orchestrates RAG with Reranking and Summarization")]
    public async IAsyncEnumerable<StreamingKernelContent> GenerateResponseAsync(
        Kernel kernel,
        [Description("User question")] string question
    )
    {
        // 1. Get Context via RerankingPlugin
        var context = await kernel.InvokeAsync<string>(typeof(RerankingPlugin), "RerankAndFilterAsync", 
            new KernelArguments { ["query"] = question });

        // 2. Check if context needs summarization (Stub)
        if (context.Length > 2000) // Arbitrary limit
        {
            context = await SummarizeContextAsync(kernel, context);
        }

        // 3. Stream LLM Response
        var prompt = $"Answer the question based on the context:\n{context}\n\nQuestion: {question}";
        var function = kernel.CreateFunctionFromPrompt(prompt);
        
        await foreach (var chunk in kernel.InvokeStreamingAsync(function))
        {
            yield return chunk;
        }
    }

    private async Task<string> SummarizeContextAsync(Kernel kernel, string context)
    {
        // Stub implementation
        return await Task.FromResult("Summarized: " + context.Substring(0, 100) + "...");
    }
}
