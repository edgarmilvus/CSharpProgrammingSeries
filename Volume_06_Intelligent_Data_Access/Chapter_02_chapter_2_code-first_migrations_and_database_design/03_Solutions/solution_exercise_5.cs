
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

# Source File: solution_exercise_5.cs
# Description: Solution for Exercise 5
# ==========================================

using System;
using System.Linq;
using System.Text.Json;

namespace KnowledgeRepository.Domain
{
    // 1. Refactored Entity (Option B: JSON)
    public class DocumentChunkRefactored
    {
        public Guid Id { get; set; }
        // ... other properties ...
        
        // Changed from byte[] to string for JSON storage
        public string EmbeddingJson { get; set; } = "[]";

        // Helper to get float array
        public float[] GetEmbeddingArray()
        {
            return JsonSerializer.Deserialize<float[]>(EmbeddingJson) ?? Array.Empty<float>();
        }
    }
}

// Migration Code Snippet for 'RefactorEmbeddingStorage'
/*
   protected override void Up(MigrationBuilder migrationBuilder)
   {
       // 1. Add the new column
       migrationBuilder.AddColumn<string>(
           name: "EmbeddingJson",
           table: "DocumentChunks",
           type: "nvarchar(max)",
           nullable: true);

       // 2. Raw SQL to transform binary data to JSON string
       // Note: This is a conceptual example. Actual binary parsing depends on the original byte[] structure.
       // Assuming byte[] was a serialized float array (4 bytes per float).
       migrationBuilder.Sql(@"
           UPDATE dc
           SET dc.EmbeddingJson = (
               SELECT CAST([Value] AS FLOAT) AS [value]
               FROM OPENROWSET(BULK N'...', SINGLE_BLOB) AS f 
               -- In reality, you would parse the binary column in SQL or use a CLR function.
               -- For this exercise, we simulate the intent of data migration.
               -- A simpler approach if data isn't critical: Set default '[]' or handle in app code.
               -- Here we assume a complex SQL conversion or we just set it to empty to proceed.
               '[]'
           )
           FROM DocumentChunks dc;
       ");

       // 3. Drop old column
       migrationBuilder.DropColumn(
           name: "Embedding",
           table: "DocumentChunks");
   }
*/
