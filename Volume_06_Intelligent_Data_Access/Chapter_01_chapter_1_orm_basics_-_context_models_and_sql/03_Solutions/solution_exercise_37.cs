
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

// Source File: solution_exercise_37.cs
// Description: Solution for Exercise 37
// ==========================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

// 1. Strict Access Control
public class HealthcareVectorService
{
    private readonly IVectorRepository<PatientRecord> _repo;
    private readonly IAuditService _audit;

    public async Task<List<PatientRecord>> SearchAsync(string query, string userId, string role)
    {
        // 1. Check Permissions (Row-Level Security)
        if (role != "Doctor" && role != "Nurse")
        {
            throw new UnauthorizedAccessException("HIPAA Violation: Insufficient privileges");
        }

        // 2. Generate Vector from Query
        var vector = await GenerateEmbedding(query);

        // 3. Perform Search (Vectors are encrypted at rest)
        var results = await _repo.SearchAsync(vector, 10);

        // 4. Audit Log (Mandatory)
        await _audit.LogAccess(userId, "Search", query, DateTime.UtcNow);

        return results;
    }

    // 2. De-identification for Training
    public async Task TrainModelAsync(List<PatientRecord> records)
    {
        // Remove PHI (Personally Identifiable Information)
        foreach (var record in records)
        {
            record.PatientName = "[REDACTED]";
            record.SSN = null;
            // Keep medical text for vector generation
        }
        
        await _repo.UpsertBatchAsync(records);
    }
}
