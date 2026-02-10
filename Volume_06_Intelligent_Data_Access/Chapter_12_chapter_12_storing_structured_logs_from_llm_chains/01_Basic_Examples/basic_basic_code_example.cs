
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
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

// 1. Define the Domain Models
// These represent the structured data we want to extract from the unstructured LLM output.
public class LlmExecutionTrace
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    public string ChainName { get; set; } = string.Empty;
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? EndedAt { get; set; }
    
    // Navigation property to related steps
    public List<TraceStep> Steps { get; set; } = new();

    // Calculated property (not mapped to DB)
    [NotMapped]
    public TimeSpan? Duration => EndedAt.HasValue ? EndedAt.Value - StartedAt : null;
}

public class TraceStep
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    public Guid LlmExecutionTraceId { get; set; } // Foreign Key
    public LlmExecutionTrace? Trace { get; set; } // Navigation property
    
    public int Order { get; set; }
    public string StepType { get; set; } = string.Empty; // e.g., "Retrieval", "Generation", "Decision"
    public string Prompt { get; set; } = string.Empty;
    public string Response { get; set; } = string.Empty;
    
    // Using JSON to store complex metadata (e.g., token counts, model name)
    // This allows flexibility without schema changes for every new metric.
    public string MetadataJson { get; set; } = string.Empty;

    [NotMapped]
    public Dictionary<string, object> Metadata
    {
        get => string.IsNullOrEmpty(MetadataJson) 
            ? new Dictionary<string, object>() 
            : JsonSerializer.Deserialize<Dictionary<string, object>>(MetadataJson) ?? new();
        set => MetadataJson = JsonSerializer.Serialize(value);
    }
}

// 2. Define the DbContext
// Handles high-throughput writes and semantic querying.
public class LlmLogContext : DbContext
{
    public DbSet<LlmExecutionTrace> ExecutionTraces { get; set; }
    public DbSet<TraceStep> TraceSteps { get; set; }

    public LlmLogContext(DbContextOptions<LlmLogContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure relationships
        modelBuilder.Entity<LlmExecutionTrace>()
            .HasMany(t => t.Steps)
            .WithOne(s => s.Trace)
            .HasForeignKey(s => s.LlmExecutionTraceId)
            .OnDelete(DeleteBehavior.Cascade); // Deleting a trace deletes its steps

        // Optimize for write-heavy logging scenarios
        // Use InMemory provider for this example; in production, use SQL Server/Postgres
        // and consider Indexes on ChainName and StartedAt for querying.
        modelBuilder.Entity<LlmExecutionTrace>()
            .HasIndex(t => t.ChainName);
        
        modelBuilder.Entity<LlmExecutionTrace>()
            .HasIndex(t => t.StartedAt);
    }
}

// 3. The "Hello World" Application Logic
class Program
{
    static async Task Main(string[] args)
    {
        // Setup Dependency Injection (Standard .NET 6+ pattern)
        var services = new ServiceCollection();
        
        // NOTE: In a real app, use AddDbContext with a SQL provider.
        // We use InMemory for a self-contained, runnable example.
        services.AddDbContext<LlmLogContext>(options => 
            options.UseInMemoryDatabase(databaseName: "LlmLogsDb"));

        var serviceProvider = services.BuildServiceProvider();

        // Scenario: We are building a RAG (Retrieval-Augmented Generation) chatbot.
        // We need to log the execution trace to debug why a specific answer was generated.
        
        await using (var scope = serviceProvider.CreateAsyncScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<LlmLogContext>();
            
            // Ensure DB is created
            await context.Database.EnsureCreatedAsync();

            // --- CAPTURE LOG DATA ---
            // Simulating an LLM Chain execution
            var trace = new LlmExecutionTrace
            {
                ChainName = "RAG-QA-Chain-v1",
                StartedAt = DateTime.UtcNow.AddSeconds(-5), // Simulating start time
                Steps = new List<TraceStep>
                {
                    new TraceStep
                    {
                        Order = 1,
                        StepType = "Retrieval",
                        Prompt = "Query: 'What is EF Core?'",
                        Response = "Retrieved 3 documents from Vector DB.",
                        Metadata = new Dictionary<string, object>
                        {
                            { "VectorDistance", 0.15 },
                            { "DocumentsCount", 3 }
                        }
                    },
                    new TraceStep
                    {
                        Order = 2,
                        StepType = "Generation",
                        Prompt = "Context: [Docs...] Question: What is EF Core?",
                        Response = "EF Core is a modern ORM for .NET...",
                        Metadata = new Dictionary<string, object>
                        {
                            { "Model", "gpt-4-turbo" },
                            { "TokensUsed", 150 }
                        }
                    }
                }
            };

            // Add the trace to the context
            context.ExecutionTraces.Add(trace);

            // Save changes (High-throughput write)
            await context.SaveChangesAsync();
            
            Console.WriteLine($"Trace {trace.Id} saved successfully.");
        }

        // --- QUERY THE LOG DATA ---
        // Scenario: We want to find all traces where the 'Generation' step took longer than expected
        // or contained specific keywords.
        using (var scope = serviceProvider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<LlmLogContext>();

            // Semantic-like query: Find traces containing "EF Core" in the response
            var searchResults = await context.ExecutionTraces
                .Where(t => t.Steps.Any(s => s.Response.Contains("EF Core")))
                .OrderByDescending(t => t.StartedAt)
                .Select(t => new 
                {
                    t.Id,
                    t.ChainName,
                    t.StartedAt,
                    // Project specific fields from the JSON metadata
                    TokensUsed = t.Steps.FirstOrDefault(s => s.StepType == "Generation") != null
                        ? t.Steps.First(s => s.StepType == "Generation").Metadata["TokensUsed"]
                        : null
                })
                .ToListAsync();

            Console.WriteLine("\n--- Query Results ---");
            foreach (var result in searchResults)
            {
                Console.WriteLine($"Chain: {result.ChainName}, ID: {result.Id}, Tokens: {result.TokensUsed}");
            }
        }
    }
}
