
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

# Source File: solution_exercise_16.cs
# Description: Solution for Exercise 16
# ==========================================

using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// 1. Dual-Write Service
public class MigrationService
{
    private readonly IPineconeClient _oldDb;
    private readonly IPgVectorClient _newDb;
    private readonly VerificationService _verifier;

    public async Task MigrateBatchAsync(List<Document> documents)
    {
        var tasks = documents.Select(async doc =>
        {
            // 2. Dual Write Strategy
            // We write to both. If Old fails, we stop. If New fails, we retry/log.
            try
            {
                // Write to Old (Source of Truth initially)
                await _oldDb.UpsertAsync(doc.Id.ToString(), doc.Vector);

                // Write to New (Target)
                await _newDb.UpsertAsync(doc.Id.ToString(), doc.Vector);

                // 3. Verification
                // Check consistency immediately
                var oldVec = await _oldDb.FetchAsync(doc.Id.ToString());
                var newVec = await _newDb.FetchAsync(doc.Id.ToString());

                if (!_verifier.IsSimilar(oldVec, newVec))
                {
                    throw new Exception("Data inconsistency detected!");
                }
            }
            catch (Exception ex)
            {
                // 4. Rollback Plan
                // If New fails, we don't touch Old.
                // If Old fails, we abort.
                // A background worker will retry failed batches.
                Console.WriteLine($"Migration failed for {doc.Id}: {ex.Message}");
            }
        });

        await Task.WhenAll(tasks);
    }

    // 5. Schema Differences Handler
    public async Task HandleSchemaDifferences()
    {
        // Pinecone might have metadata as JSON, pgvector might have structured columns.
        // We need a transformation layer.
        // Example: Extract metadata from Pinecone JSON, insert into pgvector relational columns.
    }
}

// 6. Verification System
public class VerificationService
{
    public bool IsSimilar(float[] a, float[] b)
    {
        // Compare vectors. Allow small floating point differences.
        if (a.Length != b.Length) return false;
        for (int i = 0; i < a.Length; i++)
        {
            if (Math.Abs(a[i] - b[i]) > 0.0001) return false;
        }
        return true;
    }
}
