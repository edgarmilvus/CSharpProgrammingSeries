
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

namespace MultiTenantSaaSApp
{
    // =================================================================================================
    // REAL-WORLD PROBLEM CONTEXT
    // =================================================================================================
    // Scenario: A SaaS platform called "LegalDoc AI" provides AI-powered semantic search and memory 
    // storage for law firms. Each law firm is a "Tenant". 
    // 
    // Challenge: We cannot mix Tenant A's confidential case files with Tenant B's files in the same 
    // search results or memory storage. We need strict logical isolation.
    //
    // Solution: We will implement a "Shared Table with Tenant ID" strategy (Row-Level Security pattern)
    // using basic C# constructs. We will simulate a Vector Database and RAG Memory storage that 
    // dynamically filters data based on the currently active Tenant Context.
    // =================================================================================================

    public class Program
    {
        public static void Main(string[] args)
        {
            // 1. Initialize the Data Store (Simulating a database)
            // In a real app, this would be SQL Server with Row-Level Security or CosmosDB.
            SharedDatabaseContext database = new SharedDatabaseContext();

            // 2. Seed Data for Multiple Tenants
            // Tenant A: "Baker & Partners" (Corporate Law)
            SeedDatabase(database, "TENANT_A", new string[] { "merger agreement", "stock options" });
            
            // Tenant B: "Doe & Associates" (Family Law)
            SeedDatabase(database, "TENANT_B", new string[] { "divorce settlement", "custody battle" });

            // 3. Simulate User Sessions
            Console.WriteLine("--- Starting SaaS AI Session: Tenant A ---");
            RunTenantSession("TENANT_A", database);

            Console.WriteLine("\n--- Starting SaaS AI Session: Tenant B ---");
            RunTenantSession("TENANT_B", database);
        }

        // Simulates the application logic for a specific tenant session
        static void RunTenantSession(string tenantId, SharedDatabaseContext db)
        {
            // A. Establish Tenant Context
            // This mimics how ASP.NET Core Middleware would set TenantId based on HTTP headers.
            var tenantContext = new TenantContext(tenantId);

            // B. Initialize AI Services with the Context
            // The VectorStore and MemoryBank are injected with the TenantContext to ensure isolation.
            var vectorStore = new VectorStore(tenantContext);
            var memoryBank = new RagMemoryBank(tenantContext);

            // C. Perform Vector Search (RAG Retrieval)
            string query = "Find documents regarding legal agreements";
            Console.WriteLine($"[AI Query]: '{query}'");

            // The VectorStore internally filters by TenantId
            List<Document> results = vectorStore.SemanticSearch(query);

            // D. Display Results (Isolation Verification)
            Console.WriteLine($"[Retrieved {results.Count} Documents]:");
            foreach (var doc in results)
            {
                // In a real app, this would feed into an LLM Context Window
                Console.WriteLine($" - ID: {doc.Id}, Content: '{doc.Content}' (Relevance: {doc.Score}%)");
            }

            // E. Store AI Interaction Memory
            // We store the query and results in the memory bank.
            string memoryEntry = $"User searched for '{query}' and found {results.Count} docs.";
            memoryBank.StoreMemory(memoryEntry);
            
            Console.WriteLine($"[Memory Stored]: '{memoryEntry}'");
            
            // F. Verify Memory Isolation
            // Attempting to retrieve memory should only show items for this tenant.
            Console.WriteLine($"[Memory Retrieval Check]:");
            memoryBank.RetrieveMemories();
        }

        static void SeedDatabase(SharedDatabaseContext db, string tenantId, string[] contents)
        {
            int idCounter = 1;
            foreach (string content in contents)
            {
                // Generate a fake vector (array of doubles) for the content
                double[] vector = GenerateFakeVector(content);
                db.Documents.Add(new Document 
                { 
                    Id = idCounter++, 
                    TenantId = tenantId, 
                    Content = content, 
                    Vector = vector 
                });
            }
        }

        // Helper to generate a deterministic "vector" based on string length (simulating embeddings)
        static double[] GenerateFakeVector(string text)
        {
            double[] vector = new double[3];
            for (int i = 0; i < 3; i++)
            {
                vector[i] = text.Length + (i * 0.5);
            }
            return vector;
        }
    }

    // =================================================================================================
    // CORE ARCHITECTURAL COMPONENTS
    // =================================================================================================

    /// <summary>
    /// Represents the Tenant Context for the current execution scope.
    /// This is the "Key" that unlocks data isolation.
    /// </summary>
    public class TenantContext
    {
        public string TenantId { get; private set; }

        public TenantContext(string tenantId)
        {
            if (string.IsNullOrEmpty(tenantId)) 
                throw new ArgumentException("Tenant ID is required.");
            
            this.TenantId = tenantId;
        }
    }

    /// <summary>
    /// Simulates the Shared Database (e.g., SQL Server with Row-Level Security).
    /// In a real EF Core implementation, this would be a DbSet<T> where T has a TenantId property.
    /// </summary>
    public class SharedDatabaseContext
    {
        public List<Document> Documents { get; set; } = new List<Document>();
        public List<MemoryEntry> MemoryStore { get; set; } = new List<MemoryEntry>();
    }

    /// <summary>
    /// Domain Entity: Document.
    /// Contains the TenantId property to enforce multi-tenancy at the data level.
    /// </summary>
    public class Document
    {
        public int Id { get; set; }
        public string TenantId { get; set; } // The isolation key
        public string Content { get; set; }
        public double[] Vector { get; set; } // Simulated Vector Embedding
        public double Score { get; set; }    // Search Relevance Score
    }

    /// <summary>
    /// Domain Entity: Memory Entry for RAG.
    /// </summary>
    public class MemoryEntry
    {
        public int Id { get; set; }
        public string TenantId { get; set; } // The isolation key
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // =================================================================================================
    // INTELLIGENT DATA ACCESS LAYER (Vector & RAG)
    // =================================================================================================

    /// <summary>
    /// Simulates a Vector Database (e.g., Pinecone, Qdrant, or Azure Vector Search).
    /// It enforces Tenant Scoping during the retrieval phase.
    /// </summary>
    public class VectorStore
    {
        private readonly TenantContext _context;
        private readonly SharedDatabaseContext _db;

        public VectorStore(TenantContext context)
        {
            _context = context;
            // In a real app, we would inject the DbContext here via Dependency Injection.
            // For this console app, we access the global static instance (simplified).
            _db = new SharedDatabaseContext(); // Note: In real code, this would be injected, not new'd.
        }

        /// <summary>
        /// Performs a Semantic Search using Vector Similarity.
        /// CRITICAL: Applies a hard filter on TenantId to prevent data leakage.
        /// </summary>
        public List<Document> SemanticSearch(string query)
        {
            // 1. Convert Query to Vector (Simulated)
            double[] queryVector = GenerateVectorFromQuery(query);
            
            List<Document> results = new List<Document>();

            // 2. Iterate through the Shared Table
            foreach (var doc in _db.Documents)
            {
                // 3. SECURITY CHECK: Is this document owned by the current tenant?
                if (doc.TenantId != _context.TenantId)
                {
                    continue; // Strict Isolation: Skip records from other tenants
                }

                // 4. Calculate Cosine Similarity (Simulated Logic)
                // In production, use a library like ML.NET or native DB functions.
                double similarity = CalculateSimilarity(doc.Vector, queryVector);

                // 5. Apply Threshold (e.g., > 50% match)
                if (similarity > 0.5)
                {
                    doc.Score = similarity;
                    results.Add(doc);
                }
            }

            return results;
        }

        // Helper: Simulates vector generation
        private double[] GenerateVectorFromQuery(string query)
        {
            return GenerateFakeVector(query); // Reusing the helper from Program
        }

        // Helper: Simulates Cosine Similarity calculation
        private double CalculateSimilarity(double[] vecA, double[] vecB)
        {
            if (vecA.Length != vecB.Length) return 0;
            double dotProduct = 0;
            double magnitudeA = 0;
            double magnitudeB = 0;

            for (int i = 0; i < vecA.Length; i++)
            {
                dotProduct += vecA[i] * vecB[i];
                magnitudeA += vecA[i] * vecA[i];
                magnitudeB += vecB[i] * vecB[i];
            }

            magnitudeA = Math.Sqrt(magnitudeA);
            magnitudeB = Math.Sqrt(magnitudeB);

            if (magnitudeA == 0 || magnitudeB == 0) return 0;
            
            // Return percentage
            return (dotProduct / (magnitudeA * magnitudeB)) * 100;
        }
    }

    /// <summary>
    /// Simulates a Long-Term Memory Store (e.g., Redis or CosmosDB).
    /// Used for RAG (Retrieval-Augmented Generation) to give the AI "memory".
    /// </summary>
    public class RagMemoryBank
    {
        private readonly TenantContext _context;
        private readonly SharedDatabaseContext _db;

        public RagMemoryBank(TenantContext context)
        {
            _context = context;
            _db = new SharedDatabaseContext();
        }

        /// <summary>
        /// Stores an AI interaction into the memory bank.
        /// </summary>
        public void StoreMemory(string content)
        {
            var entry = new MemoryEntry
            {
                Id = _db.MemoryStore.Count + 1,
                TenantId = _context.TenantId, // Enforce ownership
                Content = content,
                CreatedAt = DateTime.Now
            };

            _db.MemoryStore.Add(entry);
        }

        /// <summary>
        /// Retrieves all memories for the current tenant.
        /// </summary>
        public void RetrieveMemories()
        {
            bool found = false;
            for (int i = 0; i < _db.MemoryStore.Count; i++)
            {
                if (_db.MemoryStore[i].TenantId == _context.TenantId)
                {
                    Console.WriteLine($" - [Memory]: {_db.MemoryStore[i].Content}");
                    found = true;
                }
            }

            if (!found)
            {
                Console.WriteLine(" - No memories found for this tenant.");
            }
        }
    }

    // Helper class for vector generation (duplicated for scope isolation in this file)
    public static class VectorHelper
    {
        public static double[] Generate(string text)
        {
            double[] vector = new double[3];
            for (int i = 0; i < 3; i++)
            {
                vector[i] = text.Length + (i * 0.5);
            }
            return vector;
        }
    }
}
