
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

# Source File: solution_exercise_18.cs
# Description: Solution for Exercise 18
# ==========================================

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

// 1. Immutable Audit Entity
public class VectorAuditLog
{
    public Guid Id { get; private set; }
    public string UserId { get; private set; }
    public string Operation { get; private set; } // Search, Update, Delete
    public string VectorHash { get; private set; } // Hash of vector to prove what was searched
    public string QueryMetadata { get; private set; } // JSON payload
    public DateTime Timestamp { get; private set; }
    public string Signature { get; private set; } // Cryptographic signature

    // Private constructor to enforce immutability
    private VectorAuditLog() { }

    public static VectorAuditLog Create(string userId, string operation, float[] vector, object metadata)
    {
        // Calculate Hash (SHA256 of the vector bytes)
        var vectorBytes = vector.SelectMany(BitConverter.GetBytes).ToArray();
        using var sha = System.Security.Cryptography.SHA256.Create();
        var hash = Convert.ToBase64String(sha.ComputeHash(vectorBytes));

        var log = new VectorAuditLog
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Operation = operation,
            VectorHash = hash,
            QueryMetadata = JsonSerializer.Serialize(metadata),
            Timestamp = DateTime.UtcNow
        };

        // Sign the log entry (Private Key simulation)
        log.Signature = SignEntry(log);

        return log;
    }

    private static string SignEntry(VectorAuditLog log)
    {
        // In reality, use RSA/ECDSA with a private key
        return $"Signature_{log.Id}";
    }
}

// 2. Audit Interceptor
public class VectorAuditInterceptor : SaveChangesInterceptor
{
    private readonly string _currentUserId;

    public VectorAuditInterceptor(string currentUserId) => _currentUserId = currentUserId;

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData, InterceptionResult<int> result)
    {
        var context = eventData.Context;
        var auditLogs = new List<VectorAuditLog>();

        foreach (var entry in context.ChangeTracker.Entries())
        {
            if (entry.Entity is Document doc && entry.State == EntityState.Modified)
            {
                // Log the update
                auditLogs.Add(VectorAuditLog.Create(
                    _currentUserId, 
                    "Update", 
                    doc.Vector, 
                    new { doc.Id, doc.Title }
                ));
            }
        }

        if (auditLogs.Any())
        {
            // Add to context to be saved in the same transaction
            context.AddRange(auditLogs);
        }

        return result;
    }
}

// 3. GDPR Right to be Forgotten
public class ComplianceService
{
    private readonly AppDbContext _context;

    public async Task DeleteUserData(string userId)
    {
        // 1. Find all documents owned by user
        var docs = await _context.Documents.Where(d => d.OwnerId == userId).ToListAsync();

        // 2. Delete from Vector DB (Crypto-shredding is better, but deletion is standard)
        // Delete from Vector DB index...

        // 3. Delete from Relational DB
        _context.Documents.RemoveRange(docs);
        
        // 4. Delete Audit Logs (or anonymize them depending on law)
        var logs = await _context.VectorAuditLogs.Where(l => l.UserId == userId).ToListAsync();
        _context.VectorAuditLogs.RemoveRange(logs);

        await _context.SaveChangesAsync();
    }
}
