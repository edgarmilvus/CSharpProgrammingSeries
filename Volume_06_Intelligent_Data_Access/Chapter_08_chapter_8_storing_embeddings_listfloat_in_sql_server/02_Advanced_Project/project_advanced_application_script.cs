
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

using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SqlServerVectorRag
{
    class Program
    {
        // Database connection string (Update with your server details)
        private const string ConnectionString = "Server=your_server.database.windows.net;Database=VectorDbDemo;User Id=your_user;Password=your_password;TrustServerCertificate=True;";

        static async Task Main(string[] args)
        {
            Console.WriteLine("=== SQL Server 2022 Vector RAG Demo ===");

            try
            {
                // Step 1: Ensure the database and table exist
                await InitializeDatabaseSchemaAsync();

                // Step 2: Generate and Store Embeddings (Simulating a Document Ingestion Pipeline)
                Console.WriteLine("\n[1] Ingesting Documents...");
                var documents = new List<Document>
                {
                    new Document { Id = 1, Content = "SQL Server 2022 introduced the vector data type.", Embedding = GenerateMockVector(1.0f, 2.0f, 3.0f) },
                    new Document { Id = 2, Content = "Vector search enables efficient similarity matching.", Embedding = GenerateMockVector(1.1f, 1.9f, 2.8f) }, // Similar to doc 1
                    new Document { Id = 3, Content = "C# and EF Core can interact with these vectors.", Embedding = GenerateMockVector(10.0f, 11.0f, 12.0f) } // Different
                };

                foreach (var doc in documents)
                {
                    await InsertDocumentAsync(doc);
                    Console.WriteLine($"   Stored Document ID: {doc.Id}");
                }

                // Step 3: Perform a RAG Query (Simulating User Search)
                Console.WriteLine("\n[2] Performing Vector Search (RAG Query)...");
                
                // User query vector (simulating "SQL Server features")
                var queryVector = GenerateMockVector(1.0f, 2.1f, 3.0f); 
                
                Console.WriteLine($"   Query Vector: [{string.Join(", ", queryVector)}]");

                // Execute Cosine Similarity Search
                var results = await SearchSimilarDocumentsAsync(queryVector, topK: 2);

                // Step 4: Display Results (Context Retrieval)
                Console.WriteLine("\n[3] Search Results (Ranked by Relevance):");
                foreach (var result in results)
                {
                    Console.WriteLine($"   - ID: {result.Id}, Score: {result.SimilarityScore:F4}");
                    Console.WriteLine($"     Content: {result.Content}");
                }

                // Step 5: Simulate LLM Context Injection
                Console.WriteLine("\n[4] Generating Response (Simulated LLM):");
                if (results.Count > 0)
                {
                    string context = string.Join("\n", results.Select(r => r.Content));
                    Console.WriteLine($"   System Prompt: Use the following context to answer the user.\n   Context:\n{context}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        // --- Database Operations ---

        static async Task InitializeDatabaseSchemaAsync()
        {
            using var connection = new SqlConnection(ConnectionString);
            await connection.OpenAsync();

            // Note: VECTOR data type requires SQL Server 2022 or Azure SQL
            string createTableSql = @"
                IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Documents')
                BEGIN
                    CREATE TABLE Documents (
                        Id INT PRIMARY KEY,
                        Content NVARCHAR(500),
                        Embedding VECTOR(3) -- Dimension 3 for this demo
                    );
                    CREATE INDEX idx_documents_embedding ON Documents(Embedding) 
                    WITH (DISTANCE_FUNCTION = COSINE); -- Optimized for Cosine Similarity
                END";
            
            using var command = new SqlCommand(createTableSql, connection);
            await command.ExecuteNonQueryAsync();
        }

        static async Task InsertDocumentAsync(Document doc)
        {
            using var connection = new SqlConnection(ConnectionString);
            await connection.OpenAsync();

            // Converting List<float> to SQL Vector format: '[1.0, 2.0, 3.0]'
            string vectorString = "[" + string.Join(", ", doc.Embedding) + "]";

            string insertSql = "INSERT OR UPDATE INTO Documents (Id, Content, Embedding) VALUES (@Id, @Content, CAST(@VectorStr AS VECTOR));";
            
            using var command = new SqlCommand(insertSql, connection);
            command.Parameters.AddWithValue("@Id", doc.Id);
            command.Parameters.AddWithValue("@Content", doc.Content);
            command.Parameters.AddWithValue("@VectorStr", vectorString);

            await command.ExecuteNonQueryAsync();
        }

        static async Task<List<SearchResult>> SearchSimilarDocumentsAsync(List<float> queryVector, int topK)
        {
            var results = new List<SearchResult>();
            using var connection = new SqlConnection(ConnectionString);
            await connection.OpenAsync();

            // SQL Server 2022 Vector Syntax: vector_distance_function(vector1, vector2)
            // We use COSINE_DISTANCE (0 = identical, 2 = opposite). 
            // Note: SQL Server stores COSINE_SIMILARITY (1 = identical). 
            // For this demo, we calculate distance explicitly.
            
            string querySql = @"
                SELECT TOP(@TopK) 
                    Id, 
                    Content, 
                    vector_distance('cosine', CAST(@QueryVec AS VECTOR), Embedding) AS Distance
                FROM Documents
                ORDER BY Distance ASC;";

            using var command = new SqlCommand(querySql, connection);
            
            // Prepare query vector string
            string queryVecStr = "[" + string.Join(", ", queryVector) + "]";
            command.Parameters.AddWithValue("@QueryVec", queryVecStr);
            command.Parameters.AddWithValue("@TopK", topK);

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                results.Add(new SearchResult
                {
                    Id = reader.GetInt32(0),
                    Content = reader.GetString(1),
                    // In Cosine Distance: 0.0 is perfect match. 
                    // SQL Server returns 0 to 2. We invert for a "Relevance Score" (1.0 - Distance/2) if needed, 
                    // but here we keep raw distance for transparency.
                    SimilarityScore = 1.0f - (float)reader.GetDouble(2) 
                });
            }

            return results;
        }

        // --- Helpers ---

        static List<float> GenerateMockVector(float x, float y, float z)
        {
            // In a real app, this comes from an AI model (e.g., OpenAI, Azure AI)
            return new List<float> { x, y, z };
        }
    }

    // --- Models ---

    public class Document
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public List<float> Embedding { get; set; }
    }

    public class SearchResult
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public float SimilarityScore { get; set; } // 0.0 to 1.0 (1.0 = Perfect Match)
    }
}
