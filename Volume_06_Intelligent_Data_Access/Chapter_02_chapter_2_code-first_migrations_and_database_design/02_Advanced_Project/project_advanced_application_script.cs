
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

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace IntelligentDataAccess.Chapter2
{
    // ---------------------------------------------------------
    // REAL-WORLD CONTEXT: Smart Inventory Management System
    // ---------------------------------------------------------
    // Problem: A warehouse needs to track inventory items not just by SKU,
    // but by visual similarity (e.g., "find items that look like this damaged product")
    // and semantic descriptions (e.g., "small red electronic components").
    // We use Code-First Migrations to define a schema that supports standard relational
    // data (quantities, locations) alongside unstructured data (JSON descriptions) and
    // vector embeddings (for semantic search).
    // ---------------------------------------------------------

    // ---------------------------------------------------------
    // 1. DOMAIN MODELS (The "Code" in Code-First)
    // ---------------------------------------------------------
    // We define our entities as standard C# classes.
    // Attributes from System.ComponentModel.DataAnnotations are used to
    // configure the database schema without writing fluent API code.

    /// <summary>
    /// Represents a physical location within the warehouse.
    /// </summary>
    public class WarehouseLocation
    {
        [Key] // Primary Key
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Zone { get; set; } // e.g., "Aisle 1", "Cold Storage"

        [MaxLength(100)]
        public string Description { get; set; }
    }

    /// <summary>
    /// Represents an inventory item.
    /// </summary>
    public class InventoryItem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Sku { get; set; } // Stock Keeping Unit

        [Required]
        public string Name { get; set; }

        // Storing unstructured data (JSON) in a relational database.
        // This is crucial for Intelligent Data Access where metadata varies.
        // We use 'string' to store JSON, which EF Core will map to NVARCHAR(MAX) or JSONB.
        public string MetadataJson { get; set; }

        // Storing Vector Embeddings.
        // In a real scenario, this might be a vector type (e.g., pgvector).
        // For this console app (using SQL Server/SQLite), we simulate this as a comma-separated
        // string of floats to demonstrate the schema design.
        // EF Core maps this to NVARCHAR(MAX).
        public string EmbeddingVector { get; set; }

        // Foreign Key relationship
        public int LocationId { get; set; }
        public WarehouseLocation Location { get; set; }

        public int Quantity { get; set; }

        public DateTime LastStocked { get; set; }
    }

    // ---------------------------------------------------------
    // 2. DB CONTEXT (The "First" in Code-First)
    // ---------------------------------------------------------
    // The context acts as the bridge between domain models and the database.
    // It defines which tables exist and how they relate.

    public class WarehouseContext : DbContext
    {
        public DbSet<InventoryItem> InventoryItems { get; set; }
        public DbSet<WarehouseLocation> Locations { get; set; }

        // In a real app, connection string comes from configuration.
        // For this demo, we override OnConfiguring.
        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            // Using SQLite for portability in this console app example.
            // In production, this would likely be SQL Server or PostgreSQL.
            options.UseSqlite("Data Source=warehouse_intelligent.db");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Explicit configuration for complex types if needed, 
            // though Attributes handle most basics.
            base.OnModelCreating(modelBuilder);
        }
    }

    // ---------------------------------------------------------
    // 3. MIGRATION FACTORY
    // ---------------------------------------------------------
    // Required for the 'dotnet ef migrations add' command to work
    // without a dependency injection container in the startup project.

    public class WarehouseContextFactory : IDesignTimeDbContextFactory<WarehouseContext>
    {
        public WarehouseContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<WarehouseContext>();
            optionsBuilder.UseSqlite("Data Source=warehouse_intelligent.db");
            return new WarehouseContext();
        }
    }

    // ---------------------------------------------------------
    // 4. APPLICATION LOGIC (Console App)
    // ---------------------------------------------------------
    // Simulates the intelligent data access workflow:
    // 1. Database Initialization (Migrations)
    // 2. Vector Embedding Simulation (Basic Math)
    // 3. CRUD Operations
    // 4. Semantic Search (Cosine Similarity)

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("--- Intelligent Inventory System: Code-First Migrations Demo ---");

            // Step 1: Ensure Database is created and migrated
            // In a real app, this runs automatically or via CLI.
            using (var context = new WarehouseContext())
            {
                Console.WriteLine("Applying Migrations...");
                context.Database.EnsureCreated(); // Creates DB if not exists
                Console.WriteLine("Database Ready.");
            }

            // Step 2: Seed Data (Simulating initial inventory)
            SeedDatabase();

            // Step 3: Perform Intelligent Search
            // Scenario: A customer describes a product: "Small red electronic component".
            // We convert this text to a vector (simulated) and find similar items in DB.
            Console.WriteLine("\n--- Performing Semantic Search ---");
            Console.WriteLine("Query: 'Small red electronic component'");

            // Simulate the query vector (e.g., from an AI model like OpenAI Embeddings)
            float[] queryVector = new float[] { 0.9f, 0.1f, 0.5f }; // Represents "Red", "Small", "Electronic"

            PerformSemanticSearch(queryVector);

            Console.WriteLine("\n--- End of Demo ---");
        }

        // ---------------------------------------------------------
        // LOGIC BLOCK: Data Seeding
        // ---------------------------------------------------------
        static void SeedDatabase()
        {
            using (var context = new WarehouseContext())
            {
                // Check if data already exists to avoid duplicates
                if (context.InventoryItems.Any()) return;

                // Create Locations
                var locations = new List<WarehouseLocation>
                {
                    new WarehouseLocation { Zone = "Zone A", Description = "Electronics Shelf" },
                    new WarehouseLocation { Zone = "Zone B", Description = "General Storage" }
                };
                context.Locations.AddRange(locations);
                context.SaveChanges();

                // Create Items with Metadata and Vectors
                // Note: In a real app, vectors are generated by an ML model.
                // Here, we manually assign vectors based on attributes:
                // Index 0: Redness, Index 1: Size, Index 2: Electronic nature
                
                var item1 = new InventoryItem
                {
                    Sku = "EL-001",
                    Name = "Red Resistor",
                    Quantity = 500,
                    LocationId = 1,
                    LastStocked = DateTime.Now,
                    MetadataJson = "{ \"color\": \"red\", \"type\": \"resistor\", \"voltage\": \"5V\" }",
                    EmbeddingVector = "0.8,0.2,0.6" // High red, small, electronic
                };

                var item2 = new InventoryItem
                {
                    Sku = "EL-002",
                    Name = "Blue Capacitor",
                    Quantity = 200,
                    LocationId = 1,
                    LastStocked = DateTime.Now,
                    MetadataJson = "{ \"color\": \"blue\", \"type\": \"capacitor\", \"voltage\": \"10V\" }",
                    EmbeddingVector = "0.1,0.3,0.7" // Low red, small, electronic
                };

                var item3 = new InventoryItem
                {
                    Sku = "HD-001",
                    Name = "Red Plastic Bucket",
                    Quantity = 50,
                    LocationId = 2,
                    LastStocked = DateTime.Now,
                    MetadataJson = "{ \"color\": \"red\", \"material\": \"plastic\" }",
                    EmbeddingVector = "0.9,0.8,0.1" // High red, large, non-electronic
                };

                context.InventoryItems.AddRange(item1, item2, item3);
                context.SaveChanges();
                Console.WriteLine("Database Seeded with 3 items.");
            }
        }

        // ---------------------------------------------------------
        // LOGIC BLOCK: Semantic Search Engine
        // ---------------------------------------------------------
        static void PerformSemanticSearch(float[] queryVector)
        {
            using (var context = new WarehouseContext())
            {
                var allItems = context.InventoryItems.Include(i => i.Location).ToList();

                Console.WriteLine("Calculating Similarity Scores...");
                Console.WriteLine("{0,-15} {1,-25} {2,-10} {3,-10}", "SKU", "Name", "Score", "Location");
                Console.WriteLine(new string('-', 60));

                // Iterate through items to calculate Cosine Similarity
                // Since we stored vectors as strings, we must parse them back to float arrays.
                foreach (var item in allItems)
                {
                    float[] itemVector = ParseVector(item.EmbeddingVector);
                    float similarity = CalculateCosineSimilarity(queryVector, itemVector);

                    // Threshold: Only show items with > 50% similarity
                    if (similarity > 0.5f)
                    {
                        Console.WriteLine("{0,-15} {1,-25} {2,-10:F4} {3,-10}", 
                            item.Sku, item.Name, similarity, item.Location.Zone);
                    }
                }
            }
        }

        // ---------------------------------------------------------
        // HELPER: Vector Parser
        // ---------------------------------------------------------
        // Converts the string "0.1,0.2,0.3" back to float[] for calculation.
        static float[] ParseVector(string vectorString)
        {
            var parts = vectorString.Split(',');
            var vector = new float[parts.Length];
            for (int i = 0; i < parts.Length; i++)
            {
                float.TryParse(parts[i], out vector[i]);
            }
            return vector;
        }

        // ---------------------------------------------------------
        // HELPER: Cosine Similarity Calculation
        // ---------------------------------------------------------
        // Formula: A · B / (||A|| * ||B||)
        // Measures the cosine of the angle between two vectors.
        // Returns a value between -1 and 1 (1 = identical direction).
        static float CalculateCosineSimilarity(float[] vecA, float[] vecB)
        {
            if (vecA.Length != vecB.Length) return 0;

            float dotProduct = 0;
            float magnitudeA = 0;
            float magnitudeB = 0;

            for (int i = 0; i < vecA.Length; i++)
            {
                dotProduct += vecA[i] * vecB[i];
                magnitudeA += vecA[i] * vecA[i];
                magnitudeB += vecB[i] * vecB[i];
            }

            magnitudeA = (float)Math.Sqrt(magnitudeA);
            magnitudeB = (float)Math.Sqrt(magnitudeB);

            if (magnitudeA == 0 || magnitudeB == 0) return 0;

            return dotProduct / (magnitudeA * magnitudeB);
        }
    }
}
