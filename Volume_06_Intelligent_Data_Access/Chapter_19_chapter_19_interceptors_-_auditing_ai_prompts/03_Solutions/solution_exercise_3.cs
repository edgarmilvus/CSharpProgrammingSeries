
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

# Source File: solution_exercise_3.cs
# Description: Solution for Exercise 3
# ==========================================

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Data.Common;
using System.Text.Json;
using System;

// 1. Static Context for Correlation
public static class RagContext
{
    private static readonly AsyncLocal<string> _correlationId = new AsyncLocal<string>();
    private static readonly AsyncLocal<string> _userQuery = new AsyncLocal<string>();

    public static string CorrelationId 
    { 
        get => _correlationId.Value; 
        set => _correlationId.Value = value; 
    }

    public static string UserQuery 
    { 
        get => _userQuery.Value; 
        set => _userQuery.Value = value; 
    }
}

// 2. Structured Log Entry
public class StructuredAuditEntry
{
    public string CorrelationId { get; set; }
    public DateTime Timestamp { get; set; }
    public string OperationType { get; set; }
    public object Payload { get; set; }
}

// 3. The Interceptor
public class RagAuditInterceptor : DbCommandInterceptor
{
    public override InterceptionResult<DbDataReader> ReaderExecuting(
        DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result)
    {
        AuditCommand(command, "VectorSearch");
        return base.ReaderExecuting(command, eventData, result);
    }

    public override InterceptionResult<int> NonQueryExecuting(
        DbCommand command, CommandEventData eventData, InterceptionResult<int> result)
    {
        // Check for Vector Embeddings update/insert
        if (command.CommandText.Contains("VectorEmbeddings"))
        {
            AuditCommand(command, "VectorUpdate");
        }
        return base.NonQueryExecuting(command, eventData, result);
    }

    private void AuditCommand(DbCommand command, string operationType)
    {
        // 4. Read from RagContext
        var correlationId = RagContext.CorrelationId ?? "Unknown";
        
        // 5. Construct Structured JSON Log
        var auditEntry = new StructuredAuditEntry
        {
            CorrelationId = correlationId,
            Timestamp = DateTime.UtcNow,
            OperationType = operationType,
            Payload = new 
            {
                Command = command.CommandText,
                // In a real app, inspect parameters to find the specific Vector Metadata
                Parameters = command.Parameters.Count > 0 ? command.Parameters[0].Value : null,
                ContextQuery = RagContext.UserQuery
            }
        };

        var jsonLog = JsonSerializer.Serialize(auditEntry, new JsonSerializerOptions { WriteIndented = true });
        
        // Output to simulate storage
        Console.WriteLine($"[AUDIT LOG]: {jsonLog}");
    }
}

// --- Usage Example (Simulated Controller) ---
public class RagController
{
    public void ProcessUserRequest(string query)
    {
        // 1. Generate Correlation ID at start of request
        RagContext.CorrelationId = Guid.NewGuid().ToString();
        RagContext.UserQuery = query;

        // ... Logic to convert query to vector ...

        // Simulate DB Call (Interceptor triggers here)
        // _dbContext.VectorEmbeddings.Add(...);
        // _dbContext.SaveChanges();
    }
}
