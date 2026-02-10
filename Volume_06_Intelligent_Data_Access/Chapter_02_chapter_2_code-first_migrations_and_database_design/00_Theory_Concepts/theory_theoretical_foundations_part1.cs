
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

// Source File: theory_theoretical_foundations_part1.cs
// Description: Theoretical Foundations
// ==========================================

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace IntelligentDataAccess.Domain
{
    // Using a 'record' for immutable data structures is crucial in AI systems.
    // Once an embedding is generated, it should not change. If the model changes,
    // we generate a new embedding, we do not mutate the old one.
    public record MemoryEmbedding
    {
        // Storing the vector as a string or byte array depends on the database provider.
        // PostgreSQL with pgvector often uses a float array, but for generic EF Core
        // migrations, we often treat it as a specialized type.
        public required string VectorData { get; init; }
        
        // The dimensionality of the vector (e.g., 1536 for OpenAI ada-002).
        public int Dimensions { get; init; }
    }

    // The core entity representing a piece of knowledge.
    public class MemoryChunk
    {
        public Guid Id { get; set; }
        
        // The raw text content. This is the "Source of Truth" for regeneration.
        public string Content { get; set; } = string.Empty;
        
        // The semantic representation of the content.
        public MemoryEmbedding Embedding { get; set; } = null!;
        
        // Metadata is critical for RAG filtering.
        // Example: "User:123", "Topic:Finance", "Confidence:0.95"
        public Dictionary<string, string> Metadata { get; set; } = new();
        
        // Temporal context is vital for memory decay and recency weighting.
        public DateTimeOffset CreatedAt { get; set; }
        
        // Navigation property for graph-like traversal.
        public ICollection<MemoryAssociation> AssociatedMemories { get; set; } = new List<MemoryAssociation>();
    }

    // Explicit join entity to model weighted relationships between memories.
    // This allows the AI to traverse a graph of concepts.
    public class MemoryAssociation
    {
        public Guid SourceMemoryId { get; set; }
        public MemoryChunk SourceMemory { get; set; } = null!;
        
        public Guid TargetMemoryId { get; set; }
        public MemoryChunk TargetMemory { get; set; } = null!;
        
        // A score representing the strength of the association (0.0 to 1.0).
        public float AssociationStrength { get; set; }
    }
}
