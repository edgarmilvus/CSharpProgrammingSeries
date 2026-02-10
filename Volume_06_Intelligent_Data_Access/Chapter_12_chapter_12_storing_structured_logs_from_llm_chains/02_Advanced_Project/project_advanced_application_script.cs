
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

// Source File: project_advanced_application_script.cs
// Description: Advanced Application Script
// ==========================================

using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace LLMChainStructuredLogging
{
    // ------------------------------------------------------------
    // 1. Domain Models: Structured representation of LLM Chain execution
    // ------------------------------------------------------------
    
    /// <summary>
    /// Represents a single execution trace of an LLM Chain.
    /// This captures the "who, when, and why" of a specific run.
    /// </summary>
    public class ExecutionTrace
    {
        [Key]
        public Guid Id { get; set; }
        
        public DateTime Timestamp { get; set; }
        
        [MaxLength(200)]
        public string ChainName { get; set; }
        
        [MaxLength(50)]
        public string Status { get; set; } // e.g., "Success", "Failed", "Timeout"
        
        // Navigation property: One trace has many steps
        public virtual ICollection<ChainStep> Steps { get; set; } = new List<ChainStep>();
    }

    /// <summary>
    /// Represents a granular step within a chain execution.
    /// This captures the hierarchical nature of the logs (Trace -> Steps).
    /// </summary>
    public class ChainStep
    {
        [Key]
        public Guid Id { get; set; }
        
        public Guid ExecutionTraceId { get; set; } // Foreign Key
        
        [MaxLength(100)]
        public string StepName { get; set; }
        
        public int StepOrder { get; set; }
        
        // Storing complex unstructured data as JSON strings (Provider-agnostic approach)
        public string InputData { get; set; }
        public string OutputData { get; set; }
        
        public long DurationMs { get; set; }
        
        // Navigation property
        public virtual ExecutionTrace ExecutionTrace { get; set; }
    }

    // ------------------------------------------------------------
    // 2. Data Access Layer: Custom DbContext for High-Throughput Writes
    // ------------------------------------------------------------
    
    public class LlmLogContext : DbContext
    {
        public DbSet<ExecutionTrace> ExecutionTraces { get; set; }
        public DbSet<ChainStep> ChainSteps { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Using SQLite for portability in this demo. 
            // In production, this would be SQL Server, PostgreSQL, etc.
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlite("Data Source=llm_logs.db");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure relationships explicitly
            modelBuilder.Entity<ExecutionTrace>()
                .HasMany(t => t.Steps)
                .WithOne(s => s.ExecutionTrace)
                .HasForeignKey(s => s.ExecutionTraceId)
                .OnDelete(DeleteBehavior.Cascade);

            // Optimize indexes for high-throughput querying
            modelBuilder.Entity<ExecutionTrace>()
                .HasIndex(t => new { t.ChainName, t.Timestamp });
                
            modelBuilder.Entity<ChainStep>()
                .HasIndex(s => s.ExecutionTraceId);
        }
    }

    // ------------------------------------------------------------
    // 3. Application Logic: Simulating LLM Chain Execution & Persistence
    // ------------------------------------------------------------
    
    public class ChainExecutor
    {
        private readonly LlmLogContext _context;

        public ChainExecutor(LlmLogContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Simulates a complex LLM Chain execution and persists the structured logs.
        /// </summary>
        public async Task<Guid> ExecuteAndLogChainAsync(string chainName)
        {
            var traceId = Guid.NewGuid();
            var startTime = DateTime.UtcNow;
            var trace = new ExecutionTrace
            {
                Id = traceId,
                ChainName = chainName,
                Timestamp = startTime,
                Status = "Processing"
            };

            // Initial write to reserve the trace ID
            _context.ExecutionTraces.Add(trace);
            await _context.SaveChangesAsync();

            // Simulate Chain Steps
            var steps = new List<ChainStep>();
            try
            {
                // Step 1: Input Validation
                var step1Start = DateTime.UtcNow;
                await Task.Delay(50); // Simulate processing
                var step1 = CreateStep(traceId, "Input Validation", 1, "{ \"query\": \"user input\" }", "{ \"valid\": true }", step1Start);
                steps.Add(step1);

                // Step 2: Context Retrieval (RAG)
                var step2Start = DateTime.UtcNow;
                await Task.Delay(120); // Simulate vector DB search
                var step2 = CreateStep(traceId, "Context Retrieval", 2, "{ \"embedding\": [0.1, ...] }", "{ \"docs\": [\"doc1\", \"doc2\"] }", step2Start);
                steps.Add(step2);

                // Step 3: LLM Generation
                var step3Start = DateTime.UtcNow;
                await Task.Delay(300); // Simulate LLM inference
                var step3 = CreateStep(traceId, "LLM Generation", 3, "{ \"context\": \"...\", \"query\": \"...\" }", "{ \"response\": \"Final Answer\" }", step3Start);
                steps.Add(step3);

                trace.Status = "Success";
            }
            catch (Exception ex)
            {
                trace.Status = "Failed";
                // Log error step
                var errorStep = CreateStep(traceId, "Error Handler", 99, "{}", $"{{ \"error\": \"{ex.Message}\" }}", DateTime.UtcNow);
                steps.Add(errorStep);
            }

            // Persist all steps in a single transaction for atomicity
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // Update Trace Status
                    var existingTrace = await _context.ExecutionTraces.FindAsync(traceId);
                    existingTrace.Status = trace.Status;
                    
                    // Add Steps
                    await _context.ChainSteps.AddRangeAsync(steps);
                    
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }

            return traceId;
        }

        private ChainStep CreateStep(Guid traceId, string name, int order, string input, string output, DateTime start)
        {
            var end = DateTime.UtcNow;
            return new ChainStep
            {
                Id = Guid.NewGuid(),
                ExecutionTraceId = traceId,
                StepName = name,
                StepOrder = order,
                InputData = input,
                OutputData = output,
                DurationMs = (long)(end - start).TotalMilliseconds
            };
        }
    }

    // ------------------------------------------------------------
    // 4. Querying: Semantic Analysis of Execution Traces
    // ------------------------------------------------------------
    
    public class LogAnalyzer
    {
        private readonly LlmLogContext _context;

        public LogAnalyzer(LlmLogContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Analyzes performance bottlenecks in the LLM chain.
        /// </summary>
        public void ReportSlowSteps(int thresholdMs)
        {
            Console.WriteLine($"--- Analysis Report: Steps exceeding {thresholdMs}ms ---");
            
            // Fetch data from the database
            var traces = _context.ExecutionTraces
                .Where(t => t.Status == "Success")
                .ToList();

            foreach (var trace in traces)
            {
                // Explicit loading of steps (simulating complex query logic without heavy LINQ)
                _context.Entry(trace).Collection(t => t.Steps).Load();
                
                foreach (var step in trace.Steps)
                {
                    if (step.DurationMs > thresholdMs)
                    {
                        Console.WriteLine($"[Trace: {trace.Id.ToString().Substring(0, 8)}] " +
                                          $"Step '{step.StepName}' took {step.DurationMs}ms. " +
                                          $"Input Length: {step.InputData.Length} chars.");
                    }
                }
            }
        }
    }

    // ------------------------------------------------------------
    // 5. Main Execution Entry Point
    // ------------------------------------------------------------
    
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Initializing LLM Structured Logging System...");
            
            // Ensure database is created
            using (var context = new LlmLogContext())
            {
                await context.Database.EnsureCreatedAsync();
            }

            // Execute Chain 1: Complex Query
            using (var context = new LlmLogContext())
            {
                var executor = new ChainExecutor(context);
                await executor.ExecuteAndLogChainAsync("RAG-Query-Chain");
            }

            // Execute Chain 2: Simple Summarization
            using (var context = new LlmLogContext())
            {
                var executor = new ChainExecutor(context);
                await executor.ExecuteAndLogChainAsync("Summarization-Chain");
            }

            // Analyze Logs
            using (var context = new LlmLogContext())
            {
                var analyzer = new LogAnalyzer(context);
                analyzer.ReportSlowSteps(thresholdMs: 100);
            }

            Console.WriteLine("Processing complete. Data persisted to SQLite.");
        }
    }
}
