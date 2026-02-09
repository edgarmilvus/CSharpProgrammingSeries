
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

# Source File: solution_exercise_1.cs
# Description: Solution for Exercise 1
# ==========================================

using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

// 1. Entity Definitions

public enum ExecutionStatus { Running, Completed, Failed }

public class ChainExecution
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTimeOffset StartTime { get; set; }
    public DateTimeOffset EndTime { get; set; }
    public ExecutionStatus Status { get; set; }
    
    // Navigation property
    public ICollection<Step> Steps { get; set; } = new List<Step>();
}

public class Step
{
    [Key]
    public Guid StepId { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string StepType { get; set; } = string.Empty; // "Tool", "Chain", "LLM"
    
    // JSON payloads stored as strings
    public string InputPayload { get; set; } = string.Empty;
    public string OutputPayload { get; set; } = string.Empty;
    public long DurationMs { get; set; }

    // Self-referencing hierarchy
    public Guid? ParentStepId { get; set; }
    public Step? ParentStep { get; set; }
    public ICollection<Step> Children { get; set; } = new List<Step>();

    // Foreign Key for ChainExecution
    public Guid ChainExecutionId { get; set; }
    public ChainExecution ChainExecution { get; set; } = null!;

    // Tags (Many-to-Many)
    public ICollection<StepTag> Tags { get; set; } = new List<StepTag>();
}

// Normalized Tag Entity
public class StepTag
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Value { get; set; } = string.Empty;

    // Many-to-Many linking table
    public ICollection<Step> Steps { get; set; } = new List<Step>();
}

// 2. DbContext and Configuration

public class LlmLogContext : DbContext
{
    public DbSet<ChainExecution> ChainExecutions { get; set; }
    public DbSet<Step> Steps { get; set; }
    public DbSet<StepTag> StepTags { get; set; }

    public LlmLogContext(DbContextOptions<LlmLogContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ChainExecution Configuration
        modelBuilder.Entity<ChainExecution>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Status).HasConversion<string>(); // Store Enum as string
        });

        // Step Configuration
        modelBuilder.Entity<Step>(entity =>
        {
            entity.HasKey(e => e.StepId);

            // Relationship: ChainExecution -> Steps (1 to Many)
            entity.HasOne(s => s.ChainExecution)
                  .WithMany(c => c.Steps)
                  .HasForeignKey(s => s.ChainExecutionId)
                  .OnDelete(DeleteBehavior.Cascade); // Cascade delete steps when execution is deleted

            // Self-Referencing Relationship: Parent -> Children
            entity.HasOne(s => s.ParentStep)
                  .WithMany(p => p.Children)
                  .HasForeignKey(s => s.ParentStepId)
                  .OnDelete(DeleteBehavior.ClientCascade); 
                  // NOTE: ClientCascade prevents SQL Server from creating a 
                  // circular delete trigger. It handles deletes in the app layer.
                  // If you strictly require DB-side cascade, use DeleteBehavior.Cascade 
                  // but ensure the DB provider supports it on self-referencing keys.
        });

        // StepTag Configuration (Many-to-Many)
        modelBuilder.Entity<Step>()
            .HasMany(s => s.Tags)
            .WithMany(t => t.Steps)
            .UsingEntity(j => j.ToTable("StepTags")); // Maps to a separate table
    }

    // 3. Data Seeding
    public void SeedComplexTrace()
    {
        var execution = new ChainExecution
        {
            StartTime = DateTimeOffset.UtcNow.AddMinutes(-10),
            EndTime = DateTimeOffset.UtcNow,
            Status = ExecutionStatus.Completed
        };

        var stepA = new Step
        {
            Name = "Generate SQL Query",
            StepType = "LLM",
            InputPayload = "{\"prompt\": \"Select * from users\"}",
            OutputPayload = "{\"sql\": \"SELECT * FROM users\"}",
            DurationMs = 120
        };

        var stepB = new Step
        {
            Name = "Execute Query",
            StepType = "Tool",
            InputPayload = "{\"sql\": \"SELECT * FROM users\"}",
            OutputPayload = "{\"rows\": 100}",
            DurationMs = 45,
            ParentStep = stepA // Nested under A
        };

        var stepC = new Step
        {
            Name = "Format Result",
            StepType = "Tool",
            InputPayload = "{\"data\": \"...\"}",
            OutputPayload = "{\"text\": \"Found 100 users\"}",
            DurationMs = 10
        };

        // Add Tags
        var expensiveTag = new StepTag { Value = "expensive" };
        var piiTag = new StepTag { Value = "pii" };

        stepA.Tags.Add(expensiveTag);
        stepB.Tags.Add(piiTag);

        execution.Steps.Add(stepA);
        execution.Steps.Add(stepC);

        ChainExecutions.Add(execution);
        SaveChanges();
    }
}
