
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

// Source File: basic_basic_code_example.cs
// Description: Basic Code Example
// ==========================================

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Storage;
using System.Data.Common;
using System.Text.Json;

// ==========================================
// 1. Domain Models
// ==========================================

/// <summary>
/// Represents a user's interaction with an AI model.
/// This is the entity that will be stored in the database for auditing purposes.
/// </summary>
public class AuditLog
{
    public int Id { get; set; }
    public DateTime Timestamp { get; set; }
    public string? UserId { get; set; }
    public string? Prompt { get; set; }
    public string? ContextData { get; set; } // JSON serialized context (e.g., RAG documents)
    public string? ModelName { get; set; }
}

// ==========================================
// 2. The Interceptor
// ==========================================

/// <summary>
/// Intercepts database commands to extract AI-related context and audit them.
/// This implements the <see cref="IDbCommandInterceptor"/> interface.
/// </summary>
public class AiPromptInterceptor : IDbCommandInterceptor
{
    // We use a ThreadLocal to ensure data integrity in async/parallel scenarios.
    // In a real-world web app, you might use IHttpContextAccessor or AsyncLocal.
    private static readonly ThreadLocal<string?> _currentPrompt = new();
    private static readonly ThreadLocal<string?> _currentContext = new();
    private static readonly ThreadLocal<string?> _currentUserId = new();

    /// <summary>
    /// Static method to set the context before executing an AI query.
    /// This is called by your service layer before invoking the AI model.
    /// </summary>
    public static void SetContext(string prompt, string contextData, string userId)
    {
        _currentPrompt.Value = prompt;
        _currentContext.Value = contextData;
        _currentUserId.Value = userId;
    }

    /// <summary>
    /// Static method to clear the context after execution.
    /// </summary>
    public static void ClearContext()
    {
        _currentPrompt.Value = null;
        _currentContext.Value = null;
        _currentUserId.Value = null;
    }

    // We need access to the DbContext to insert the audit log.
    // Since interceptors are registered at the DbContext level, we can inject services here.
    private readonly AuditingDbContext _dbContext;

    public AiPromptInterceptor(AuditingDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Intercepts the execution of a command. This is the "After" event.
    /// We use this to capture the audit data and save it.
    /// </summary>
    public InterceptionResult<DbDataReader> ReaderExecuting(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<DbDataReader> result)
    {
        // 1. Check if we have context data to audit.
        // In a real scenario, we might check a specific tag or command text pattern.
        if (!string.IsNullOrEmpty(_currentPrompt.Value))
        {
            // 2. Create the audit log entry.
            var auditLog = new AuditLog
            {
                Timestamp = DateTime.UtcNow,
                UserId = _currentUserId.Value,
                Prompt = _currentPrompt.Value,
                ContextData = _currentContext.Value, // This contains the RAG context
                ModelName = "GPT-4" // Hardcoded for this example, or parsed from command
            };

            // 3. Insert into the database.
            // IMPORTANT: We must be careful not to create an infinite loop.
            // If we use _dbContext.AuditLogs.Add(auditLog) and SaveChanges(),
            // the interceptor will trigger again for the INSERT command.
            // We use a raw SQL command to bypass the interceptor for the audit write.
            
            var connection = _dbContext.Database.GetDbConnection();
            using var transaction = connection.BeginTransaction();
            try
            {
                using var cmd = connection.CreateCommand();
                cmd.Transaction = transaction;
                cmd.CommandText = "INSERT INTO AuditLogs (Timestamp, UserId, Prompt, ContextData, ModelName) VALUES (@p0, @p1, @p2, @p3, @p4)";
                
                AddParameter(cmd, "@p0", auditLog.Timestamp);
                AddParameter(cmd, "@p1", auditLog.UserId);
                AddParameter(cmd, "@p2", auditLog.Prompt);
                AddParameter(cmd, "@p3", auditLog.ContextData);
                AddParameter(cmd, "@p4", auditLog.ModelName);
                
                cmd.ExecuteNonQuery();
                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                // Log error externally, don't swallow it silently in production
                throw;
            }
            finally
            {
                ClearContext();
            }
        }

        return result; // Pass through the original result
    }

    private void AddParameter(DbCommand cmd, string name, object? value)
    {
        var param = cmd.CreateParameter();
        param.ParameterName = name;
        param.Value = value ?? DBNull.Value;
        cmd.Parameters.Add(param);
    }

    // We must implement other members of the interface, but they simply pass through.
    public void NonQueryExecuted(DbCommand command, CommandExecutedEventData eventData, int result) { }
    public void NonQueryExecuting(DbCommand command, CommandEventData eventData, InterceptionResult<int> result) { }
    public void ReaderExecuted(DbCommand command, CommandExecutedEventData eventData, DbDataReader result) { }
    public void ScalarExecuted(DbCommand command, CommandExecutedEventData eventData, object? result) { }
    public void ScalarExecuting(DbCommand command, CommandEventData eventData, InterceptionResult<object?> result) { }
    public Task<DbDataReader> ReaderExecutingAsync(DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result, CancellationToken cancellationToken = default) => Task.FromResult(result);
    public Task<int> NonQueryExecutingAsync(DbCommand command, CommandEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default) => Task.FromResult(result);
    public Task<object?> ScalarExecutingAsync(DbCommand command, CommandEventData eventData, InterceptionResult<object?> result, CancellationToken cancellationToken = default) => Task.FromResult(result);
    public void CommandFailed(DbCommand command, CommandErrorEventData eventData) { }
    public Task CommandFailedAsync(DbCommand command, CommandErrorEventData eventData, CancellationToken cancellationToken = default) => Task.CompletedTask;
    public void CommandSucceeded(DbCommand command, CommandExecutedEventData eventData) { }
    public Task CommandSucceededAsync(DbCommand command, CommandExecutedEventData eventData, CancellationToken cancellationToken = default) => Task.CompletedTask;
}

// ==========================================
// 3. DbContext and Configuration
// ==========================================

public class AuditingDbContext : DbContext
{
    public DbSet<AuditLog> AuditLogs { get; set; }

    public AuditingDbContext(DbContextOptions<AuditingDbContext> options) : base(options) { }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Registering the interceptor
        optionsBuilder.AddInterceptors(new AiPromptInterceptor(this));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Timestamp).IsRequired();
            entity.Property(e => e.Prompt).HasMaxLength(2000);
            entity.Property(e => e.ContextData).HasColumnType("nvarchar(max)"); // For JSON
        });
    }
}

// ==========================================
// 4. Usage Example (Simulated)
// ==========================================

public class AiService
{
    private readonly AuditingDbContext _context;

    public AiService(AuditingDbContext context)
    {
        _context = context;
    }

    public async Task<string> GetRagResponseAsync(string userQuestion, string retrievedDocumentsJson)
    {
        // 1. Set the context BEFORE executing the database query that triggers the AI call
        // (In this simulation, the "AI Call" is just a dummy SQL query, but the interceptor captures the context)
        AiPromptInterceptor.SetContext(
            prompt: userQuestion,
            contextData: retrievedDocumentsJson,
            userId: "user_123"
        );

        // 2. Execute a database command. 
        // The interceptor will catch this, audit the context, and then execute the command.
        // In a real app, this might be a call to an AI vector search or an external API.
        // Here, we simulate it by querying a dummy table.
        var result = await _context.Database.ExecuteSqlRawAsync("SELECT 1 AS FakeAiResponse");

        return "AI Response processed and logged.";
    }
}

// ==========================================
// 5. Main Execution
// ==========================================

class Program
{
    static async Task Main(string[] args)
    {
        // Setup In-Memory Database for the example
        var options = new DbContextOptionsBuilder<AuditingDbContext>()
            .UseInMemoryDatabase(databaseName: "AiAuditDb")
            .Options;

        using var context = new AuditingDbContext(options);
        
        // Ensure database is created
        await context.Database.EnsureCreatedAsync();

        var aiService = new AiService(context);

        // Simulate a RAG workflow
        string ragContext = JsonSerializer.Serialize(new 
        { 
            Source = "Financial Report 2023", 
            Snippet = "Revenue increased by 15%..." 
        });

        await aiService.GetRagResponseAsync("What was the revenue growth?", ragContext);

        // Verify the audit log was created
        var logs = await context.AuditLogs.ToListAsync();
        Console.WriteLine($"Audit Logs Count: {logs.Count}");
        if (logs.Count > 0)
        {
            var log = logs.First();
            Console.WriteLine($"User: {log.UserId}");
            Console.WriteLine($"Prompt: {log.Prompt}");
            Console.WriteLine($"Context: {log.ContextData}");
        }
    }
}
