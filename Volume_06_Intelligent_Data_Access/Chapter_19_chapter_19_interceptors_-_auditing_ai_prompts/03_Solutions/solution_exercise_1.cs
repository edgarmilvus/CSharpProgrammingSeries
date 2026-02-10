
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

// Source File: solution_exercise_1.cs
// Description: Solution for Exercise 1
// ==========================================

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Data.Common;
using System.Collections.Generic;
using System.Text.RegularExpressions;

// 1. The static audit trail to store captured logs.
public static class AuditStore
{
    public static List<string> AuditTrail { get; } = new List<string>();
}

// 2. The Interceptor implementation.
public class PromptAuditInterceptor : DbCommandInterceptor
{
    // We override the non-query execution to intercept INSERT statements.
    public override InterceptionResult<DbDataReader> ReaderExecuting(
        DbCommand command, 
        CommandEventData eventData, 
        InterceptionResult<DbDataReader> result)
    {
        // 3. Target specific operations: Check for the PromptLogs insert.
        // Note: EF Core typically uses "INSERT INTO [TableName]" syntax.
        if (command.CommandText.Contains("INSERT INTO \"PromptLogs\"") || 
            command.CommandText.Contains("INSERT INTO [PromptLogs]"))
        {
            AuditPrompt(command);
        }

        return base.ReaderExecuting(command, eventData, result);
    }

    public override InterceptionResult<int> NonQueryExecuting(
        DbCommand command, 
        CommandEventData eventData, 
        InterceptionResult<int> result)
    {
        if (command.CommandText.Contains("INSERT INTO \"PromptLogs\"") || 
            command.CommandText.Contains("INSERT INTO [PromptLogs]"))
        {
            AuditPrompt(command);
        }

        return base.NonQueryExecuting(command, eventData, result);
    }

    // 4. Extract Data and Audit Logic.
    private void AuditPrompt(DbCommand command)
    {
        // EF Core parameters are named @p0, @p1, etc. 
        // Assuming the 'Content' property is the first parameter in the INSERT.
        // In a real scenario, we might inspect the parameter names, but for standard EF mappings,
        // the order is usually consistent with the entity property order.
        
        if (command.Parameters.Count > 0)
        {
            // Extracting the first parameter value as the prompt content.
            var promptContent = command.Parameters[0].Value?.ToString();
            
            if (!string.IsNullOrEmpty(promptContent))
            {
                var logEntry = $"[{DateTime.UtcNow:O}] CAPTURED PROMPT: {promptContent}";
                AuditStore.AuditTrail.Add(logEntry);
                
                // Simulating console output for verification
                Console.WriteLine(logEntry);
            }
        }
    }
}

// --- Integration Setup (Conceptual) ---

public class AppDbContext : DbContext
{
    public DbSet<PromptLog> PromptLogs { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // 5. Register the interceptor.
        optionsBuilder.AddInterceptors(new PromptAuditInterceptor());
        // optionsBuilder.UseSqlServer("..."); 
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Ensure the table name matches the check in the interceptor
        modelBuilder.Entity<PromptLog>().ToTable("PromptLogs");
    }
}

public class PromptLog
{
    public int Id { get; set; }
    public string Content { get; set; }
    public DateTime CreatedAt { get; set; }
}
