
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
using System.Threading.Tasks;

// --- 1. Identified Risks in Original Code ---
/*
 * Risk 1: Partial Failure (Data Inconsistency)
 *   - If `await _context.SaveChangesAsync()` succeeds but `await _vectorStore.UpsertAsync(...)` fails, 
 *     the document exists in SQL but is missing from the vector store. 
 *     The RAG system will fail to retrieve this document during search.
 * 
 * Risk 2: Lack of Atomicity
 *   - There is no transaction scope. The system enters an invalid state where 
 *     one data store is updated and the other is not.
 * 
 * Risk 3: Resource Leaks & Error Handling
 *   - If an exception occurs, the DbContext might be left in an inconsistent state 
 *     for subsequent operations in the same scope, or connections might not be closed properly.
 */

// --- 2. Refactored Code ---

public class RagIngestionService
{
    private readonly IUnitOfWork _unitOfWork;

    public RagIngestionService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task IngestAsync(string documentId, string content)
    {
        // We assume the IUnitOfWork wraps the DbContext and VectorStoreClient
        // and handles the transaction logic internally (as implemented in Exercise 1).
        
        var doc = new Document 
        { 
            Id = documentId, 
            Content = content,
            CreatedAt = DateTime.UtcNow
        };

        // Add to the context tracked by the UoW
        _unitOfWork.Context.Documents.Add(doc);

        // The CommitAsync method handles the transaction coordination:
        // 1. Saves SQL changes
        // 2. Performs Vector Store Upsert
        // 3. Commits SQL transaction if Vector Store succeeds
        // 4. Rolls back SQL transaction if Vector Store fails
        try 
        {
            await _unitOfWork.CommitAsync();
            Console.WriteLine("Ingestion successful.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ingestion failed: {ex.Message}");
            // The UoW has already rolled back the SQL transaction.
            // The caller does not need to take further action for consistency.
            throw;
        }
    }
}

// --- 3. Visualization (Graphviz DOT Code) ---

/*
digraph RAGPipeline {
    rankdir=TB;
    node [shape=rectangle];

    Start [label="Start IngestAsync"];
    AddToContext [label="Add Document to EF Context"];
    BeginTx [label="Begin SQL Transaction"];
    SaveSQL [label="SaveChangesAsync (Stage)"];
    GenEmbed [label="Generate Embedding"];
    UpsertVector [label="Upsert Vector Store"];
    
    Success [label="Commit SQL Transaction"];
    Rollback [label="Rollback SQL Transaction"];
    Error [label="Throw Exception"];

    // Success Path
    Start -> AddToContext;
    AddToContext -> BeginTx;
    BeginTx -> SaveSQL;
    SaveSQL -> GenEmbed;
    GenEmbed -> UpsertVector;
    UpsertVector -> Success;

    // Failure Path
    UpsertVector -> Rollback [label="Vector Store Failure"];
    SaveSQL -> Error [label="SQL Save Failure"];
    Rollback -> Error;
}
*/
