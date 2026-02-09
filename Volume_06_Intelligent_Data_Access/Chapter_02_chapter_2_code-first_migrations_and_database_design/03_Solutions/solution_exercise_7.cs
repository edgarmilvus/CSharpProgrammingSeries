
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

# Source File: solution_exercise_7.cs
# Description: Solution for Exercise 7
# ==========================================

using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace KnowledgeRepository.Domain
{
    public class Document
    {
        // ... existing properties ...

        // 1. Optimistic Concurrency Property
        [Timestamp]
        public byte[]? RowVersion { get; set; }
    }

    public class ConcurrencyService
    {
        private readonly KnowledgeRepositoryContext _context;

        public ConcurrencyService(KnowledgeRepositoryContext context)
        {
            _context = context;
        }

        // 2. Transaction Scope for Atomicity
        public async Task AddDocumentWithChunksTransactionAsync(Document document, List<DocumentChunk> chunks)
        {
            // Begin transaction
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                await _context.Documents.AddAsync(document);
                await _context.SaveChangesAsync(); // Save document to get ID

                // Assign FKs
                foreach (var chunk in chunks)
                {
                    chunk.DocumentId = document.Id;
                }
                
                await _context.DocumentChunks.AddRangeAsync(chunks);
                await _context.SaveChangesAsync();

                // Commit if successful
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                // Rollback on any error
                await transaction.RollbackAsync();
                throw; // Re-throw to notify caller
            }
        }

        // 3. Handling Concurrency Conflicts
        public async Task UpdateDocumentTitleAsync(Guid documentId, string newTitle)
        {
            bool saveFailed;
            do
            {
                saveFailed = false;
                var document = await _context.Documents.FindAsync(documentId);

                if (document != null)
                {
                    document.Title = newTitle;
                    document.LastModifiedDate = DateTime.UtcNow;

                    try
                    {
                        await _context.SaveChangesAsync();
                    }
                    catch (DbUpdateConcurrencyException ex)
                    {
                        saveFailed = true;

                        // 4. Conflict Resolution Strategy
                        // a. Reload the entity from the database
                        await ex.Entries.Single().ReloadAsync();

                        // b. Log the conflict (Simulated)
                        Console.WriteLine($"Concurrency conflict detected for Document {documentId}. The data was modified by another user.");

                        // c. Decision: Overwrite user changes with DB values or notify user.
                        // Here we simply retry the loop, which will apply the new title 
                        // to the freshly reloaded entity (potentially overwriting other changes).
                    }
                }
            } while (saveFailed);
        }
    }
}

// Migration for Concurrency:
/*
   protected override void Up(MigrationBuilder migrationBuilder)
   {
       migrationBuilder.AddColumn<byte[]>(
           name: "RowVersion",
           table: "Documents",
           type: "rowversion",
           rowVersion: true,
           nullable: true);
   }
*/
