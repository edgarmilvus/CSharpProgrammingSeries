
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
using System.IO;
using System.Text;

namespace IntelligentDataAccessORMBasics
{
    // ---------------------------------------------------------
    // 1. DATA MODELS
    // ---------------------------------------------------------
    // Represents a single document or text snippet in our system.
    // In a vector database context, this would eventually hold
    // an embedding vector (array of floats). For this chapter,
    // we focus on the relational structure: ID and Content.
    public class Document
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }

        // Constructor for easy instantiation
        public Document(string title, string content)
        {
            Title = title;
            Content = content;
            CreatedAt = DateTime.Now;
        }

        // Default constructor for ORM materialization
        public Document() { }
    }

    // Represents a "Memory" or Context window in a RAG pipeline.
    // This links a user query to a specific document retrieved.
    public class InteractionMemory
    {
        public int Id { get; set; }
        public string UserQuery { get; set; }
        public int RetrievedDocumentId { get; set; }
        public DateTime InteractionTime { get; set; }

        public InteractionMemory(string query, int docId)
        {
            UserQuery = query;
            RetrievedDocumentId = docId;
            InteractionTime = DateTime.Now;
        }

        public InteractionMemory() { }
    }

    // ---------------------------------------------------------
    // 2. DATABASE CONTEXT (SIMULATED)
    // ---------------------------------------------------------
    // Simulates EF Core's DbContext.
    // Real EF Core manages connections, change tracking, and SQL generation.
    // Here, we simulate a persistent store using in-memory lists and file I/O
    // to demonstrate the lifecycle of data access without external dependencies.
    public class AppDbContext : IDisposable
    {
        // Simulating database tables
        public List<Document> Documents { get; private set; }
        public List<InteractionMemory> Memories { get; private set; }

        // File paths for persistent storage simulation
        private const string DbFile = "intelligent_data_store.db";

        public AppDbContext()
        {
            Documents = new List<Document>();
            Memories = new List<InteractionMemory>();
            LoadData();
        }

        // Simulates loading existing data (DbContext initialization)
        private void LoadData()
        {
            if (!File.Exists(DbFile)) return;

            try
            {
                var lines = File.ReadAllLines(DbFile);
                // Simple parsing logic (not robust, purely for demo)
                bool inDocuments = false;
                bool inMemories = false;

                foreach (var line in lines)
                {
                    if (line.StartsWith("[DOCUMENTS]")) { inDocuments = true; inMemories = false; continue; }
                    if (line.StartsWith("[MEMORIES]")) { inMemories = true; inDocuments = false; continue; }

                    if (inDocuments)
                    {
                        var parts = line.Split('|');
                        if (parts.Length == 4)
                        {
                            Documents.Add(new Document
                            {
                                Id = int.Parse(parts[0]),
                                Title = parts[1],
                                Content = parts[2],
                                CreatedAt = DateTime.Parse(parts[3])
                            });
                        }
                    }
                    else if (inMemories)
                    {
                        var parts = line.Split('|');
                        if (parts.Length == 4)
                        {
                            Memories.Add(new InteractionMemory
                            {
                                Id = int.Parse(parts[0]),
                                UserQuery = parts[1],
                                RetrievedDocumentId = int.Parse(parts[2]),
                                InteractionTime = DateTime.Parse(parts[3])
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading context: {ex.Message}");
            }
        }

        // Simulates SaveChangesAsync in EF Core.
        // Commits all tracked changes to the persistent store.
        public void SaveChanges()
        {
            var sb = new StringBuilder();
            sb.AppendLine("[DOCUMENTS]");
            foreach (var doc in Documents)
            {
                sb.AppendLine($"{doc.Id}|{doc.Title}|{doc.Content}|{doc.CreatedAt}");
            }

            sb.AppendLine("[MEMORIES]");
            foreach (var mem in Memories)
            {
                sb.AppendLine($"{mem.Id}|{mem.UserQuery}|{mem.RetrievedDocumentId}|{mem.InteractionTime}");
            }

            File.WriteAllText(DbFile, sb.ToString());
            Console.WriteLine(">> Context Saved: Changes committed to persistent store.");
        }

        // Simulates disposing the context (connection cleanup)
        public void Dispose()
        {
            Documents.Clear();
            Memories.Clear();
        }
    }

    // ---------------------------------------------------------
    // 3. REPOSITORY & LOGIC LAYER
    // ---------------------------------------------------------
    // Handles data access logic. In advanced scenarios, this abstracts
    // the difference between a relational database and a vector store.
    public class DataRepository
    {
        private readonly AppDbContext _context;

        public DataRepository(AppDbContext context)
        {
            _context = context;
        }

        // Simulates a SQL INSERT command
        public void AddDocument(Document doc)
        {
            // Simulate ID generation (Auto-increment)
            int newId = _context.Documents.Count + 1;
            if (newId == 0) newId = 1; 
            
            // Check for duplicates (basic logic)
            foreach(var existing in _context.Documents)
            {
                if (existing.Title == doc.Title && existing.Content == doc.Content) return;
            }

            doc.Id = newId;
            _context.Documents.Add(doc);
            Console.WriteLine($"[SQL INSERT] Document added: '{doc.Title}' (ID: {doc.Id})");
        }

        // Simulates a SQL SELECT with a WHERE clause (Full Text Search simulation)
        // This bridges the gap to RAG: Retrieving relevant context.
        public List<Document> SearchDocuments(string keyword)
        {
            var results = new List<Document>();
            
            // Simulating LINQ: _context.Documents.Where(d => d.Content.Contains(keyword))
            // Using basic loops as requested by constraints.
            foreach (var doc in _context.Documents)
            {
                // Basic string matching (simulating tokenization/vector search)
                if (doc.Content.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    results.Add(doc);
                }
            }

            return results;
        }

        // Logs an interaction for Memory Storage
        public void LogInteraction(string query, int docId)
        {
            int newId = _context.Memories.Count + 1;
            if (newId == 0) newId = 1;

            var memory = new InteractionMemory(query, docId) { Id = newId };
            _context.Memories.Add(memory);
            Console.WriteLine($"[MEMORY STORE] Interaction logged for query: '{query}'");
        }

        // Simulates a complex SQL JOIN to retrieve conversation history
        public void PrintInteractionHistory()
        {
            Console.WriteLine("\n--- Interaction Memory History ---");
            foreach (var mem in _context.Memories)
            {
                // Find the associated document (Simulating a Foreign Key Join)
                Document? associatedDoc = null;
                foreach(var doc in _context.Documents)
                {
                    if (doc.Id == mem.RetrievedDocumentId)
                    {
                        associatedDoc = doc;
                        break;
                    }
                }

                if (associatedDoc != null)
                {
                    Console.WriteLine($"[{mem.InteractionTime}] Q: {mem.UserQuery}");
                    Console.WriteLine($"    Retrieved Doc: {associatedDoc.Title}");
                }
            }
            Console.WriteLine("----------------------------------");
        }
    }

    // ---------------------------------------------------------
    // 4. MAIN APPLICATION (PIPELINE ORCHESTRATOR)
    // ---------------------------------------------------------
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Initializing Intelligent Data Access System...\n");

            // 1. Initialize Context (Lifetime Management)
            // In a web app, this would be scoped to the HTTP request.
            using (var context = new AppDbContext())
            {
                var repository = new DataRepository(context);

                // 2. Seed Data (Simulating Migration/Initial Data)
                // We check if data exists; if not, we populate it.
                if (context.Documents.Count == 0)
                {
                    Console.WriteLine("Seeding initial dataset...");
                    repository.AddDocument(new Document("AI Ethics Guidelines", 
                        "Artificial Intelligence must be developed responsibly. Safety is paramount."));
                    repository.AddDocument(new Document("Vector Database Intro", 
                        "Vector databases store embeddings for semantic search. Useful for RAG."));
                    repository.AddDocument(new Document("ORM Best Practices", 
                        "Use EF Core for object relational mapping. Optimize SQL queries."));
                    
                    context.SaveChanges(); // Commit seed data
                }
                else
                {
                    Console.WriteLine($"Database loaded with {context.Documents.Count} documents.");
                }

                // 3. Simulate User Interaction (RAG Pipeline Step 1: Retrieval)
                string userQuery = "search for: vector";
                Console.WriteLine($"\nUser Input: '{userQuery}'");
                
                // Extract keyword (simplified NLP)
                string keyword = "vector"; 

                // Perform SQL-like retrieval
                var retrievedDocs = repository.SearchDocuments(keyword);

                // 4. Process Results (RAG Pipeline Step 2: Augmentation/Generation)
                if (retrievedDocs.Count > 0)
                {
                    Console.WriteLine($"Retrieved {retrievedDocs.Count} relevant documents:");
                    foreach (var doc in retrievedDocs)
                    {
                        Console.WriteLine($" - {doc.Title}: \"{doc.Content}\"");
                        
                        // Log this retrieval into memory
                        repository.LogInteraction(userQuery, doc.Id);
                    }
                }
                else
                {
                    Console.WriteLine("No relevant context found.");
                }

                // 5. Demonstrate Memory Persistence (RAG Pipeline Step 3: Memory)
                // Save the new interactions
                context.SaveChanges();

                // 6. Query the Memory Store (Hybrid Architecture)
                repository.PrintInteractionHistory();

                // 7. Demonstrate Context Lifecycle
                // 'using' block calls Dispose() here, simulating connection closure.
            }

            Console.WriteLine("\nSession ended. Context disposed.");
        }
    }
}
