
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

# Source File: solution_exercise_4.cs
# Description: Solution for Exercise 4
# ==========================================

using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KnowledgeRepository.Services
{
    // DTO for projection
    public record DocumentChunkDto(Guid Id, string Content, int Sequence, string DocumentTitle);

    public static class DataSeeder
    {
        public static async Task SeedAsync(KnowledgeRepositoryContext context)
        {
            // Ensure database is created
            await context.Database.EnsureCreatedAsync();

            if (await context.Documents.AnyAsync()) return; // Already seeded

            var doc1 = new Document { Title = "AI Overview", FileName = "ai.pdf", ContentType = "application/pdf" };
            var doc2 = new Document { Title = "Quantum Computing", FileName = "quantum.pdf", ContentType = "application/pdf" };

            // Add Documents first
            context.Documents.AddRange(doc1, doc2);
            await context.SaveChangesAsync(); // Save to generate IDs

            // Create chunks with explicit foreign key assignment
            var chunks = new List<DocumentChunk>
            {
                new DocumentChunk { DocumentId = doc1.Id, Sequence = 1, Content = "AI is intelligence demonstrated by machines.", EmbeddingDimension = 1536 },
                new DocumentChunk { DocumentId = doc1.Id, Sequence = 2, Content = "Unlike natural intelligence, it is a simulation.", EmbeddingDimension = 1536 },
                new DocumentChunk { DocumentId = doc1.Id, Sequence = 3, Content = "Machine learning is a subset of AI.", EmbeddingDimension = 1536 },
                new DocumentChunk { DocumentId = doc2.Id, Sequence = 1, Content = "Quantum computing uses qubits.", EmbeddingDimension = 1536 },
                new DocumentChunk { DocumentId = doc2.Id, Sequence = 2, Content = "Superposition allows multiple states.", EmbeddingDimension = 1536 },
                new DocumentChunk { DocumentId = doc2.Id, Sequence = 3, Content = "Entanglement is a key phenomenon.", EmbeddingDimension = 1536 }
            };

            context.DocumentChunks.AddRange(chunks);
            await context.SaveChangesAsync();
        }
    }

    public class Repository
    {
        private readonly KnowledgeRepositoryContext _context;

        public Repository(KnowledgeRepositoryContext context)
        {
            _context = context;
        }

        // 2. Complex Querying with Projection
        public async Task<List<DocumentChunkDto>> GetDocumentChunksOrderedAsync(Guid documentId)
        {
            return await _context.DocumentChunks
                .AsNoTracking() // Optimization for read-only
                .Where(c => c.DocumentId == documentId)
                .OrderBy(c => c.Sequence)
                .Select(c => new DocumentChunkDto(
                    c.Id,
                    c.Content,
                    c.Sequence,
                    c.Document.Title // Navigation property access in projection
                ))
                .ToListAsync();
        }

        // 3. Soft Delete Handling
        public async Task SoftDeleteDocumentAsync(Guid documentId)
        {
            var doc = await _context.Documents.FindAsync(documentId);
            if (doc != null)
            {
                doc.IsDeleted = true;
                await _context.SaveChangesAsync();
            }
        }
    }
}
