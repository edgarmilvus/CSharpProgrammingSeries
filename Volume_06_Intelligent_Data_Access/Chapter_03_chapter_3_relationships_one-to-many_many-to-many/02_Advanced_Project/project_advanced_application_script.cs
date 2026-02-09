
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

namespace RAG_Data_Access_Simulation
{
    // ==========================================
    // 1. DATA MODELS (Simulating EF Core Entities)
    // ==========================================

    // Represents a Knowledge Source (e.g., a PDF or Documentation Page)
    public class KnowledgeSource
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }

        // One-to-Many: One Source has many Chunks
        public List<KnowledgeChunk> Chunks { get; set; } = new List<KnowledgeChunk>();

        // Many-to-Many: One Source can be referenced by many Queries
        public List<UserQuery> RelatedQueries { get; set; } = new List<UserQuery>();
    }

    // Represents a specific segment of text extracted for Vectorization
    public class KnowledgeChunk
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public int KnowledgeSourceId { get; set; } // Foreign Key simulation
    }

    // Represents a User's Question
    public class UserQuery
    {
        public int Id { get; set; }
        public string Question { get; set; }
        
        // Many-to-Many: One Query can reference many Sources
        public List<KnowledgeSource> RelevantSources { get; set; } = new List<KnowledgeSource>();
    }

    // ==========================================
    // 2. RELATIONAL LOGIC SIMULATOR (The "Context")
    // ==========================================
    
    // This class simulates the behavior of a DbContext and the Change Tracker
    public class RAGContext
    {
        // Simulating database tables using Lists (Arrays/Collections)
        public List<KnowledgeSource> Sources { get; set; } = new List<KnowledgeSource>();
        public List<KnowledgeChunk> Chunks { get; set; } = new List<KnowledgeChunk>();
        public List<UserQuery> Queries { get; set; } = new List<UserQuery>();

        // ---------------------------------------------------------
        // ONE-TO-MANY: Configuration & Data Entry
        // ---------------------------------------------------------
        public void AddDocumentWithChunks(string title, string[] chunkTexts)
        {
            // 1. Create Parent
            var source = new KnowledgeSource
            {
                Id = Sources.Count + 1,
                Title = title,
                Content = string.Join(" ", chunkTexts)
            };

            // 2. Create Children
            foreach (var text in chunkTexts)
            {
                var chunk = new KnowledgeChunk
                {
                    Id = Chunks.Count + 1,
                    Text = text,
                    KnowledgeSourceId = source.Id // Setting Foreign Key
                };
                
                Chunks.Add(chunk);
                source.Chunks.Add(chunk); // Adding to Navigation Property
            }

            Sources.Add(source);
            Console.WriteLine($"[DB] Saved Document '{source.Title}' with {source.Chunks.Count} chunks.");
        }

        // ---------------------------------------------------------
        // MANY-TO-MANY: Configuration & Data Entry
        // ---------------------------------------------------------
        public void LinkQueryToSources(int queryId, int[] sourceIds)
        {
            // Find the Query
            UserQuery query = Queries.Find(q => q.Id == queryId);
            if (query == null) return;

            // Simulating the implicit Join Table logic
            foreach (var sourceId in sourceIds)
            {
                var source = Sources.Find(s => s.Id == sourceId);
                if (source != null)
                {
                    // Check for duplicates (Edge Case Handling)
                    bool alreadyLinked = false;
                    foreach (var existing in query.RelevantSources)
                    {
                        if (existing.Id == source.Id)
                        {
                            alreadyLinked = true;
                            break;
                        }
                    }

                    if (!alreadyLinked)
                    {
                        query.RelevantSources.Add(source);
                        // In a real Many-to-Many, we would also add to source.RelatedQueries
                        // to maintain bidirectional navigation if needed.
                    }
                }
            }
            Console.WriteLine($"[DB] Linked Query '{query.Question}' to {sourceIds.Length} sources.");
        }

        // ---------------------------------------------------------
        // DATA RETRIEVAL (Eager Loading Simulation)
        // ---------------------------------------------------------
        
        // Simulates: context.Queries.Include(q => q.RelevantSources).ThenInclude(s => s.Chunks)
        public UserQuery GetQueryWithContext(int queryId)
        {
            UserQuery query = null;

            // Manual Loop to find Query (Simulating LINQ .Find())
            foreach (var q in Queries)
            {
                if (q.Id == queryId)
                {
                    query = q;
                    break;
                }
            }

            if (query == null) return null;

            // Eager Loading Simulation: Manually populating the navigation properties
            // since we are not using a real ORM to track relationships automatically.
            
            // 1. Load Related Sources
            foreach (var source in Sources)
            {
                // Check if this source is linked to the query (Simulating Join Table)
                foreach (var linkedSource in query.RelevantSources)
                {
                    if (source.Id == linkedSource.Id)
                    {
                        // 2. Load Children of Source (Chunks) - Eager Loading Depth 2
                        source.Chunks = new List<KnowledgeChunk>();
                        foreach (var chunk in Chunks)
                        {
                            if (chunk.KnowledgeSourceId == source.Id)
                            {
                                source.Chunks.Add(chunk);
                            }
                        }
                    }
                }
            }

            return query;
        }
    }

    // ==========================================
    // 3. CONSOLE APPLICATION (The RAG Pipeline)
    // ==========================================

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== RAG Data Access Simulation (EF Core Relationships) ===\n");

            // Initialize Context
            var context = new RAGContext();

            // ---------------------------------------------------------
            // SCENARIO 1: ONE-TO-MANY (Document Ingestion)
            // ---------------------------------------------------------
            Console.WriteLine("--- Ingesting Knowledge Base (One-to-Many) ---");
            
            // Simulating a PDF split into chunks
            context.AddDocumentWithChunks(
                "EF Core Documentation", 
                new string[] 
                { 
                    "Relationships define how entities connect.", 
                    "One-to-Many uses Foreign Keys.", 
                    "Many-to-Many uses implicit join tables." 
                }
            );

            context.AddDocumentWithChunks(
                "RAG Pipeline Guide", 
                new string[] 
                { 
                    "Retrieval Augmented Generation requires vector storage.", 
                    "Context windows limit token input." 
                }
            );

            // ---------------------------------------------------------
            // SCENARIO 2: MANY-TO-MANY (Query Association)
            // ---------------------------------------------------------
            Console.WriteLine("\n--- Associating Queries (Many-to-Many) ---");

            // Create a User Query
            var query = new UserQuery 
            { 
                Id = 1, 
                Question = "How do relationships work in EF Core?" 
            };
            context.Queries.Add(query);

            // Link Query to multiple Sources (Simulating the Join Table)
            // Source 1 (EF Core Doc) and Source 2 (RAG Guide) are both relevant
            context.LinkQueryToSources(1, new int[] { 1, 2 });

            // ---------------------------------------------------------
            // SCENARIO 3: ADVANCED RETRIEVAL (Eager Loading)
            // ---------------------------------------------------------
            Console.WriteLine("\n--- Retrieving Context for LLM (Eager Loading) ---");

            // We need the full graph: Query -> Sources -> Chunks
            var queryContext = context.GetQueryWithContext(1);

            if (queryContext != null)
            {
                Console.WriteLine($"Query: \"{queryContext.Question}\"\n");
                Console.WriteLine("Retrieved Context:");

                // Iterating through the Many-to-Many relationship
                foreach (var source in queryContext.RelevantSources)
                {
                    Console.WriteLine($"  Source: {source.Title}");
                    
                    // Iterating through the One-to-Many relationship
                    foreach (var chunk in source.Chunks)
                    {
                        Console.WriteLine($"    - Chunk: \"{chunk.Text}\"");
                    }
                    Console.WriteLine();
                }
            }

            // ---------------------------------------------------------
            // SCENARIO 4: PERFORMANCE CONSIDERATION (Lazy Loading Simulation)
            // ---------------------------------------------------------
            Console.WriteLine("--- Performance Note: N+1 Problem Simulation ---");
            
            // If we didn't use Eager Loading (GetQueryWithContext), we would loop:
            // 1. Loop through Sources
            // 2. For each Source, query Chunks (Database Roundtrip)
            
            int roundTrips = 0;
            foreach (var source in queryContext.RelevantSources)
            {
                roundTrips++; // One trip per source to get chunks
            }
            Console.WriteLine($"Lazy Loading Approach: {roundTrips} database roundtrips for chunks.");
            Console.WriteLine("Eager Loading Approach: 1 database roundtrip (combined graph).");

            Console.WriteLine("\n=== Simulation Complete ===");
        }
    }
}
