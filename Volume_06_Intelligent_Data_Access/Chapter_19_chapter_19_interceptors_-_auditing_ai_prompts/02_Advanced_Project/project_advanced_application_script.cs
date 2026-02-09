
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

# Source File: project_advanced_application_script.cs
# Description: Advanced Application Script
# ==========================================

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Chapter19_AuditingAI
{
    // =================================================================================================
    // 1. DOMAIN MODELS: Audit Log & AI Context
    // =================================================================================================
    // We define a dedicated entity to store the audit trails. This separates concerns from
    // the operational data (e.g., User or Product tables) and allows for specific retention policies.
    public class AiPromptAuditLog
    {
        [Key]
        public int Id { get; set; }

        // Timestamp of when the prompt was intercepted.
        public DateTime Timestamp { get; set; }

        // The actual text sent to the AI model (or the user's query before vectorization).
        [MaxLength(4000)]
        public string PromptText { get; set; } = string.Empty;

        // Metadata: Which AI model or endpoint was targeted?
        [MaxLength(100)]
        public string TargetModel { get; set; } = string.Empty;

        // Metadata: Who triggered this request? (User ID, Service Account, etc.)
        [MaxLength(100)]
        public string RequestorId { get; set; } = string.Empty;

        // Performance metric: How long did the operation take?
        public long ExecutionTimeMs { get; set; }

        // Status: Success, Failure, or Timeout.
        [MaxLength(20)]
        public string Status { get; set; } = string.Empty;
    }

    // A simple context object representing the data being prepared for RAG (Retrieval-Augmented Generation).
    public class RAGContext
    {
        public string UserQuery { get; set; } = string.Empty;
        public string DocumentId { get; set; } = string.Empty;
        public string ModelName { get; set; } = "gpt-4-turbo";
    }

    // =================================================================================================
    // 2. INTERCEPTOR IMPLEMENTATION
    // =================================================================================================
    // This is the core of Chapter 19. We implement IDbCommandInterceptor to hook into the 
    // database command lifecycle. We use this to intercept custom SQL commands that simulate 
    // sending prompts to an AI vector store.
    public class AuditInterceptor : IDbCommandInterceptor
    {
        // A shared logger for console output (in a real app, this would be ILogger<T>)
        private readonly ILogger<AuditInterceptor> _logger;

        public AuditInterceptor(ILogger<AuditInterceptor> logger)
        {
            _logger = logger;
        }

        // ---------------------------------------------------------------------------------------------
        // INTERCEPTION POINT: ReaderExecuting (Async)
        // ---------------------------------------------------------------------------------------------
        // This method is called just before a DbDataReader is executed.
        // We will inspect the command text. If it matches our "AI Prompt" signature, we audit it.
        public InterceptionResult<DbDataReader> ReaderExecuting(
            DbCommand command, 
            ReaderEventData eventData, 
            InterceptionResult<DbDataReader> result)
        {
            // 1. Identify the operation
            if (IsAiPromptCommand(command))
            {
                var stopwatch = Stopwatch.StartNew();
                
                // 2. Parse the context from the command parameters (simulating extraction)
                var context = ExtractContext(command);

                // 3. Perform the actual database operation (the base behavior)
                // Note: In a real scenario, we might not modify the command, but here we ensure 
                // the command executes normally.
                var baseResult = base.ReaderExecuting(command, eventData, result);

                stopwatch.Stop();

                // 4. Log the audit entry asynchronously to a dedicated table
                // We use a fire-and-forget pattern or a separate context to avoid recursion.
                LogAuditEntry(context, stopwatch.ElapsedMilliseconds, "Success");
            }

            return result;
        }

        // Async version is required for modern EF Core usage
        public async ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
            DbCommand command, 
            ReaderEventData eventData, 
            InterceptionResult<DbDataReader> result,
            CancellationToken cancellationToken = default)
        {
            if (IsAiPromptCommand(command))
            {
                var stopwatch = Stopwatch.StartNew();
                var context = ExtractContext(command);

                // Execute the base command
                var baseResult = await base.ReaderExecutingAsync(command, eventData, result, cancellationToken);

                stopwatch.Stop();
                await LogAuditEntryAsync(context, stopwatch.ElapsedMilliseconds, "Success", cancellationToken);
            }

            return result;
        }

        // ---------------------------------------------------------------------------------------------
        // HELPER: Command Identification
        // ---------------------------------------------------------------------------------------------
        // In a real system, we might look for a specific SQL comment or a stored procedure name.
        // Here, we look for a custom marker in the CommandText.
        private bool IsAiPromptCommand(DbCommand command)
        {
            return command.CommandText.Contains("EXECUTE_AI_PROMPT", StringComparison.OrdinalIgnoreCase);
        }

        // ---------------------------------------------------------------------------------------------
        // HELPER: Data Extraction
        // ---------------------------------------------------------------------------------------------
        // Extracts parameters from the SQL command to build our audit context.
        // This mimics capturing data before it is serialized into vector embeddings.
        private RAGContext ExtractContext(DbCommand command)
        {
            var context = new RAGContext();
            
            // Iterate through parameters safely
            foreach (DbParameter param in command.Parameters)
            {
                if (param.ParameterName.Equals("@Prompt", StringComparison.OrdinalIgnoreCase))
                    context.UserQuery = param.Value?.ToString() ?? string.Empty;
                
                if (param.ParameterName.Equals("@DocId", StringComparison.OrdinalIgnoreCase))
                    context.DocumentId = param.Value?.ToString() ?? string.Empty;

                if (param.ParameterName.Equals("@Model", StringComparison.OrdinalIgnoreCase))
                    context.ModelName = param.Value?.ToString() ?? "Unknown";
            }

            return context;
        }

        // ---------------------------------------------------------------------------------------------
        // HELPER: Audit Logging (Sync)
        // ---------------------------------------------------------------------------------------------
        // Writes the audit log. Note: We must be careful not to trigger the interceptor again 
        // recursively. We use a separate DbContext instance for auditing.
        private void LogAuditEntry(RAGContext context, long ms, string status)
        {
            try
            {
                // In a real app, use Dependency Injection to get a fresh context
                using var auditContext = new AppDbContext(); 
                
                var log = new AiPromptAuditLog
                {
                    Timestamp = DateTime.UtcNow,
                    PromptText = context.UserQuery,
                    TargetModel = context.ModelName,
                    RequestorId = "System_User", // In real app, extract from Thread.CurrentPrincipal
                    ExecutionTimeMs = ms,
                    Status = status
                };

                auditContext.AuditLogs.Add(log);
                auditContext.SaveChanges(); // Synchronous save for simplicity in this example
            }
            catch (Exception ex)
            {
                // Log to a file or external sink if DB logging fails
                Console.WriteLine($"CRITICAL AUDIT FAILURE: {ex.Message}");
            }
        }

        // ---------------------------------------------------------------------------------------------
        // HELPER: Audit Logging (Async)
        // ---------------------------------------------------------------------------------------------
        private async Task LogAuditEntryAsync(RAGContext context, long ms, string status, CancellationToken ct)
        {
            try
            {
                using var auditContext = new AppDbContext();
                
                var log = new AiPromptAuditLog
                {
                    Timestamp = DateTime.UtcNow,
                    PromptText = context.UserQuery,
                    TargetModel = context.ModelName,
                    RequestorId = "System_User",
                    ExecutionTimeMs = ms,
                    Status = status
                };

                auditContext.AuditLogs.Add(log);
                await auditContext.SaveChangesAsync(ct);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"CRITICAL ASYNC AUDIT FAILURE: {ex.Message}");
            }
        }
    }

    // =================================================================================================
    // 3. DB CONTEXT & CONFIGURATION
    // =================================================================================================
    public class AppDbContext : DbContext
    {
        public DbSet<AiPromptAuditLog> AuditLogs { get; set; }

        // Configuration flag to toggle the interceptor
        private readonly bool _useInterceptor;

        public AppDbContext(bool useInterceptor = true)
        {
            _useInterceptor = useInterceptor;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Using SQLite for portability of the example
            optionsBuilder.UseSqlite("Data Source=ai_audit.db");

            // Enable sensitive data logging for debugging (disable in production)
            optionsBuilder.LogTo(Console.WriteLine, LogLevel.Information);

            if (_useInterceptor)
            {
                // Register the interceptor.
                // NOTE: In a real app, this would be registered via DI (services.AddDbContext with AddInterceptors)
                optionsBuilder.AddInterceptors(new AuditInterceptor(LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<AuditInterceptor>()));
            }
        }
    }

    // =================================================================================================
    // 4. MAIN APPLICATION LOGIC
    // =================================================================================================
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== Chapter 19: AI Prompt Auditing Demo ===\n");

            // 1. Initialize Database
            // We ensure the database exists and the audit table is created.
            using (var context = new AppDbContext(useInterceptor: false)) 
            {
                context.Database.EnsureDeleted(); // Clean slate for demo
                context.Database.EnsureCreated();
                Console.WriteLine("Database initialized.");
            }

            // 2. Simulate RAG Workflow
            // We will execute a "SQL Command" that represents sending a prompt to an AI service.
            // The Interceptor will catch this, audit it, and allow the "query" to proceed.
            SimulateRagQuery(
                prompt: "What is the capital of France?",
                docId: "DOC-1024",
                model: "gpt-4-turbo"
            );

            SimulateRagQuery(
                prompt: "Explain quantum entanglement in simple terms.",
                docId: "DOC-5500",
                model: "gpt-3.5-turbo"
            );

            // 3. Verify Audit Logs
            // We query the AuditLogs table to prove the interception worked.
            Console.WriteLine("\n--- Verifying Audit Logs ---");
            VerifyAuditLogs();

            Console.WriteLine("\nDemo Complete.");
        }

        // ---------------------------------------------------------------------------------------------
        // SIMULATION: The RAG Query
        // ---------------------------------------------------------------------------------------------
        static void SimulateRagQuery(string prompt, string docId, string model)
        {
            Console.WriteLine($"\nProcessing Request: '{prompt}'");

            // We use a fresh context with the interceptor ENABLED.
            using (var context = new AppDbContext(useInterceptor: true))
            {
                // We construct a raw SQL command that mimics an AI service call.
                // In a real application, this might be an HTTP call via HttpClient, 
                // but we are simulating it as a DB command to utilize the IDbCommandInterceptor.
                var connection = context.Database.GetDbConnection();
                
                if (connection.State != ConnectionState.Open)
                    connection.Open();

                using (var cmd = connection.CreateCommand())
                {
                    // The command text is a marker for our interceptor.
                    cmd.CommandText = "EXECUTE_AI_PROMPT @Prompt, @DocId, @Model";
                    
                    // Add parameters representing the data being sent to the AI
                    var pPrompt = cmd.CreateParameter();
                    pPrompt.ParameterName = "@Prompt";
                    pPrompt.Value = prompt;
                    cmd.Parameters.Add(pPrompt);

                    var pDoc = cmd.CreateParameter();
                    pDoc.ParameterName = "@DocId";
                    pDoc.Value = docId;
                    cmd.Parameters.Add(pDoc);

                    var pModel = cmd.CreateParameter();
                    pModel.ParameterName = "@Model";
                    pModel.Value = model;
                    cmd.Parameters.Add(pModel);

                    // EXECUTE
                    // The 'ReaderExecuting' interceptor in 'AuditInterceptor' class triggers here.
                    // It logs the data BEFORE the command is actually sent (conceptually).
                    using (var reader = cmd.ExecuteReader())
                    {
                        // In this simulation, we don't actually have results, 
                        // but the interceptor has already done its work.
                    }
                }
            }
        }

        // ---------------------------------------------------------------------------------------------
        // VERIFICATION: Reading the Audit Table
        // ---------------------------------------------------------------------------------------------
        static void VerifyAuditLogs()
        {
            // Use a context without the interceptor to avoid recursion when reading the audit table
            using (var context = new AppDbContext(useInterceptor: false))
            {
                var logs = context.AuditLogs.ToList();
                
                Console.WriteLine($"Found {logs.Count} audit entries:");
                Console.WriteLine("-------------------------------------------------------------");
                Console.WriteLine("| ID | Timestamp           | Model           | Status | Time |");
                Console.WriteLine("-------------------------------------------------------------");

                foreach (var log in logs)
                {
                    Console.WriteLine($"| {log.Id,-2} | {log.Timestamp:HH:mm:ss}        | {log.TargetModel,-15} | {log.Status,-6} | {log.ExecutionTimeMs}ms |");
                    Console.WriteLine($"|    | Prompt: {log.PromptText.Substring(0, Math.Min(30, log.PromptText.Length))}... |");
                    Console.WriteLine("-------------------------------------------------------------");
                }
            }
        }
    }
}
