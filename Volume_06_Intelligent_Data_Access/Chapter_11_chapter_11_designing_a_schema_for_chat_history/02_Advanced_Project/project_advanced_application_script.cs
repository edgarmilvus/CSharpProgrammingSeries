
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

namespace ChatHistoryMemorySystem
{
    // ==========================================
    // PART 1: Domain Models & Schema Design
    // ==========================================

    /// <summary>
    /// Represents a single message in a conversation.
    /// This is the core entity in our relational schema.
    /// </summary>
    public class ChatMessage
    {
        public Guid Id { get; set; }
        public Guid ConversationId { get; set; }
        public string Role { get; set; } // "User" or "Assistant"
        public string Content { get; set; }
        public DateTime Timestamp { get; set; }
        
        // Vector Embedding Representation (Simplified for Console App)
        // In a real scenario, this might be a float[] stored in a specialized column or a linked vector DB.
        public string EmbeddingVector { get; set; } 
        
        // Metadata for filtering
        public string Tags { get; set; } // Comma separated: "support,urgent,billing"
    }

    /// <summary>
    /// Represents a conversation thread.
    /// </summary>
    public class Conversation
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
    }

    // ==========================================
    // PART 2: Memory Storage (In-Memory Database)
    // ==========================================

    /// <summary>
    /// Simulates an EF Core DbContext for storing chat history.
    /// In a real application, this would inherit from DbContext and use DbSet<T>.
    /// </summary>
    public class ChatMemoryContext
    {
        public List<Conversation> Conversations { get; set; } = new List<Conversation>();
        
        // Simulating a Vector Index for semantic search
        // Key: Message ID, Value: Vector Embedding (simplified as string for demo)
        public Dictionary<Guid, string> VectorIndex { get; set; } = new Dictionary<Guid, string>();

        public void AddConversation(Conversation conv)
        {
            Conversations.Add(conv);
        }

        public void AddMessage(ChatMessage message)
        {
            // Find the conversation
            var conv = Conversations.Find(c => c.Id == message.ConversationId);
            if (conv != null)
            {
                conv.Messages.Add(message);
                
                // Index the vector for RAG retrieval
                if (!string.IsNullOrEmpty(message.EmbeddingVector))
                {
                    VectorIndex[message.Id] = message.EmbeddingVector;
                }
            }
        }
    }

    // ==========================================
    // PART 3: Hybrid Memory Provider (The Logic)
    // ==========================================

    /// <summary>
    /// Unifies relational storage and vector retrieval strategies.
    /// </summary>
    public class HybridMemoryProvider
    {
        private readonly ChatMemoryContext _context;

        public HybridMemoryProvider(ChatMemoryContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retrieves context using a hybrid approach:
        /// 1. Exact match (Relational: Conversation ID)
        /// 2. Semantic match (Vector: Similarity Search)
        /// 3. Metadata filtering (Relational: Tags)
        /// </summary>
        public List<ChatMessage> RetrieveContext(Guid conversationId, string queryEmbedding, string requiredTag = null)
        {
            var results = new List<ChatMessage>();

            // 1. Relational Retrieval: Get immediate history of the current conversation
            var conversation = _context.Conversations.Find(c => c.Id == conversationId);
            if (conversation != null)
            {
                // Take the last 3 messages for immediate context
                int count = conversation.Messages.Count;
                int start = Math.Max(0, count - 3);
                for (int i = start; i < count; i++)
                {
                    results.Add(conversation.Messages[i]);
                }
            }

            // 2. Vector Retrieval: Semantic Search across ALL conversations (RAG)
            // We simulate vector similarity calculation (Cosine Similarity)
            if (!string.IsNullOrEmpty(queryEmbedding))
            {
                foreach (var kvp in _context.VectorIndex)
                {
                    // Skip if it's already in results (avoid duplicates)
                    if (results.Exists(m => m.Id == kvp.Key)) continue;

                    double similarity = CalculateCosineSimilarity(queryEmbedding, kvp.Value);
                    
                    // Threshold: 0.8 (Arbitrary for demo)
                    if (similarity > 0.8)
                    {
                        // Find the full message entity
                        var message = FindMessageById(kvp.Key);
                        if (message != null)
                        {
                            // 3. Metadata Filtering
                            if (!string.IsNullOrEmpty(requiredTag))
                            {
                                if (message.Tags != null && message.Tags.Contains(requiredTag))
                                {
                                    results.Add(message);
                                }
                            }
                            else
                            {
                                results.Add(message);
                            }
                        }
                    }
                }
            }

            return results;
        }

        // Helper to simulate finding a message across conversations
        private ChatMessage FindMessageById(Guid id)
        {
            foreach (var conv in _context.Conversations)
            {
                foreach (var msg in conv.Messages)
                {
                    if (msg.Id == id) return msg;
                }
            }
            return null;
        }

        // Simulates Vector Math (In reality, use a library like ML.NET or specialized Vector DB client)
        private double CalculateCosineSimilarity(string vecA, string vecB)
        {
            // In a real app, these strings would be parsed into float arrays.
            // Here, we simulate similarity based on string length and character matching 
            // to demonstrate the logic flow without heavy math dependencies.
            if (string.IsNullOrEmpty(vecA) || string.IsNullOrEmpty(vecB)) return 0.0;

            // Simulation Logic:
            // 1. Length difference penalty
            int lenDiff = Math.Abs(vecA.Length - vecB.Length);
            double lenScore = 1.0 - (lenDiff * 0.1);
            if (lenScore < 0) lenScore = 0;

            // 2. Character overlap bonus
            int matches = 0;
            int checkLen = Math.Min(vecA.Length, vecB.Length);
            for (int i = 0; i < checkLen; i++)
            {
                if (vecA[i] == vecB[i]) matches++;
            }
            double charScore = (double)matches / checkLen;

            return (lenScore + charScore) / 2.0;
        }
    }

    // ==========================================
    // PART 4: Main Application (Console Simulation)
    // ==========================================

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("--- Intelligent Data Access: Chat History & RAG System ---\n");

            // 1. Initialize the In-Memory Database
            var dbContext = new ChatMemoryContext();
            var memoryProvider = new HybridMemoryProvider(dbContext);

            // 2. Seed Data (Simulating existing history in the database)
            Guid convId1 = Guid.NewGuid();
            Guid convId2 = Guid.NewGuid();

            // Conversation 1: Technical Support
            dbContext.AddConversation(new Conversation { Id = convId1, Title = "Server Error", CreatedAt = DateTime.Now.AddDays(-1) });
            
            // Conversation 2: Billing Inquiry
            dbContext.AddConversation(new Conversation { Id = convId2, Title = "Invoice Issue", CreatedAt = DateTime.Now.AddDays(-2) });

            // Add Messages with "Embeddings" (Simplified strings representing vectors)
            // Pattern: "Role:Topic:DetailLevel"
            
            // Message 1 (Tech Support)
            dbContext.AddMessage(new ChatMessage 
            { 
                Id = Guid.NewGuid(), 
                ConversationId = convId1, 
                Role = "User", 
                Content = "My server is throwing 500 errors.",
                EmbeddingVector = "user:server:error:500:critical",
                Tags = "tech,support,urgent",
                Timestamp = DateTime.Now.AddHours(-2)
            });

            // Message 2 (Tech Support)
            dbContext.AddMessage(new ChatMessage 
            { 
                Id = Guid.NewGuid(), 
                ConversationId = convId1, 
                Role = "Assistant", 
                Content = "Please check the logs in /var/log/nginx.",
                EmbeddingVector = "assistant:server:logs:nginx",
                Tags = "tech,support",
                Timestamp = DateTime.Now.AddHours(-1.9)
            });

            // Message 3 (Billing - Semantic match to "Payment")
            dbContext.AddMessage(new ChatMessage 
            { 
                Id = Guid.NewGuid(), 
                ConversationId = convId2, 
                Role = "User", 
                Content = "I haven't received my invoice for March.",
                EmbeddingVector = "user:billing:invoice:march:payment",
                Tags = "billing,finance",
                Timestamp = DateTime.Now.AddDays(-1.5)
            });

            // 3. Simulate a New User Interaction (RAG Scenario)
            Console.WriteLine(">>> New User Query: 'How do I check server logs for payment errors?'\n");
            
            // Generate a mock embedding for the new query
            string queryEmbedding = "user:server:logs:payment:error"; 
            
            // 4. Retrieve Context using Hybrid Strategy
            // We pass the current conversation ID (simulating we are in convId1 context)
            // We also pass a tag filter "tech" to prioritize technical solutions
            var contextResults = memoryProvider.RetrieveContext(convId1, queryEmbedding, "tech");

            // 5. Display Results
            Console.WriteLine("--- Retrieved Context for LLM (RAG Input) ---");
            if (contextResults.Count == 0)
            {
                Console.WriteLine("No relevant context found.");
            }
            else
            {
                foreach (var msg in contextResults)
                {
                    Console.WriteLine($"[{msg.Role.ToUpper()}] (Sim. Score: {memoryProvider.CalculateCosineSimilarity(queryEmbedding, msg.EmbeddingVector):P2})");
                    Console.WriteLine($"  Content: {msg.Content}");
                    Console.WriteLine($"  Tags: {msg.Tags}");
                    Console.WriteLine();
                }
            }

            // 6. Explanation of Logic
            Console.WriteLine("--- Architecture Breakdown ---");
            ExplainArchitecture();
        }

        static void ExplainArchitecture()
        {
            Console.WriteLine("1. SCHEMA DESIGN:");
            Console.WriteLine("   - ChatMessage: Relational entity storing text, roles, and vector pointers.");
            Console.WriteLine("   - Conversation: Aggregation root for threading.");
            Console.WriteLine("   - Trade-off: Relational DB ensures ACID compliance for chat logs, while vector index enables fuzzy semantic search.");
            
            Console.WriteLine("\n2. HYBRID RETRIEVAL LOGIC:");
            Console.WriteLine("   - Step A: Fetch immediate history (Relational Query - Conversation ID).");
            Console.WriteLine("   - Step B: Semantic Search (Vector Query - Cosine Similarity).");
            Console.WriteLine("   - Step C: Filter by Metadata (Relational Query - Tags).");
            
            Console.WriteLine("\n3. RAG INTEGRATION:");
            Console.WriteLine("   - The retrieved context is injected into the LLM's system prompt.");
            Console.WriteLine("   - This prevents hallucinations by grounding the model in specific chat history.");
        }
    }
}
