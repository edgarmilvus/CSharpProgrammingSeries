
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

# Source File: solution_exercise_48.cs
# Description: Solution for Exercise 48
# ==========================================

using System;
using System.Collections.Generic;
using System.Data;

// 1. Legacy Adapter Pattern
public class LegacyDbAdapter
{
    private readonly IDbConnection _legacyConnection; // e.g., Oracle, DB2

    public List<Document> FetchDocuments()
    {
        // 1. Extract data from Legacy DB
        var dataTable = new DataTable();
        // _legacyConnection.Execute("SELECT * FROM LEGACY_TABLE");

        var docs = new List<Document>();
        foreach (DataRow row in dataTable.Rows)
        {
            docs.Add(new Document 
            { 
                Id = (int)row["ID"], 
                Content = row["CONTENT"].ToString() 
            });
        }
        return docs;
    }
}

// 2. Sync Service
public class EnterpriseSyncService
{
    private readonly LegacyDbAdapter _legacyAdapter;
    private readonly IVectorRepository<Document> _modernRepo;

    public async Task SyncAsync()
    {
        // 1. Extract from Legacy
        var legacyDocs = _legacyAdapter.FetchDocuments();

        // 2. Transform & Enrich
        foreach (var doc in legacyDocs)
        {
            doc.Vector = await GenerateEmbedding(doc.Content);
        }

        // 3. Load to Modern Vector DB
        await _modernRepo.UpsertBatchAsync(legacyDocs);
    }
}
