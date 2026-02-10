
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

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;

// 1. Configuration Class
public record ChunkingOptions
{
    public int TargetSize { get; init; } = 512;
    public int OverlapSize { get; init; } = 64;
    public Dictionary<string, string> Metadata { get; init; } = new();
}

// 2. EF Core Entity Configuration
public class DocumentChunk
{
    [Key]
    public Guid Id { get; set; }
    public Guid DocumentId { get; set; }
    public string Content { get; set; } = string.Empty;
    public byte[] Embedding { get; set; } = Array.Empty<byte>();
    public string MetadataJson { get; set; } = string.Empty;
}

public class AppDbContext : DbContext
{
    public DbSet<DocumentChunk> DocumentChunks { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DocumentChunk>(entity =>
        {
            entity.Property(e => e.Content)
                  .HasColumnType("nvarchar(max)"); // Suitable for full-text search in SQL Server

            entity.Property(e => e.Embedding)
                  .HasColumnType("varbinary(max)"); // Generic binary storage

            entity.Property(e => e.MetadataJson)
                  .HasColumnType("nvarchar(max)");
        });
    }
}

// 3. Smart Chunking Logic
public static class DocumentProcessor
{
    public static IEnumerable<string> ChunkDocument(string rawText, ChunkingOptions options)
    {
        if (string.IsNullOrWhiteSpace(rawText)) yield break;

        // Sanitize overlap to prevent infinite loops or excessive duplication
        int overlap = Math.Min(options.OverlapSize, options.TargetSize);
        
        // Pre-process: Extract Code Blocks to treat as atomic units
        var segments = SegmentByCodeBlocks(rawText);
        
        StringBuilder currentChunk = new StringBuilder();
        
        foreach (var segment in segments)
        {
            if (segment.IsCode)
            {
                // Handle Code Segment
                if (segment.Text.Length >= options.TargetSize)
                {
                    // Split hard at target size, respecting line breaks if possible
                    var splitCode = SplitCodeBlock(segment.Text, options.TargetSize);
                    foreach (var part in splitCode)
                    {
                        yield return CreateFinalChunk(part, options.Metadata);
                    }
                }
                else
                {
                    // Check if adding this code exceeds target
                    if (currentChunk.Length + segment.Text.Length > options.TargetSize)
                    {
                        yield return CreateFinalChunk(currentChunk.ToString(), options.Metadata);
                        currentChunk.Clear();
                    }
                    currentChunk.Append(segment.Text);
                }
            }
            else
            {
                // Handle Text Segment (Split by sentences)
                var sentences = Regex.Split(segment.Text, @"(?<=[.!?])\s+");
                foreach (var sentence in sentences)
                {
                    if (string.IsNullOrWhiteSpace(sentence)) continue;

                    if (currentChunk.Length + sentence.Length > options.TargetSize)
                    {
                        // If current chunk is not empty, flush it
                        if (currentChunk.Length > 0)
                        {
                            yield return CreateFinalChunk(currentChunk.ToString(), options.Metadata);
                            
                            // Handle Overlap
                            string lastChunk = currentChunk.ToString();
                            currentChunk.Clear();
                            
                            // Extract overlap text (last 'overlap' chars)
                            if (overlap > 0)
                            {
                                int start = Math.Max(0, lastChunk.Length - overlap);
                                int length = Math.Min(overlap, lastChunk.Length);
                                string overlapText = lastChunk.Substring(start, length);
                                currentChunk.Append(overlapText);
                            }
                        }
                    }
                    
                    // If the single sentence is larger than TargetSize, split it hard
                    if (sentence.Length > options.TargetSize)
                    {
                        var hardSplit = SplitString(sentence, options.TargetSize);
                        foreach (var part in hardSplit)
                        {
                            // Flush current buffer before hard split parts
                            if (currentChunk.Length > 0)
                            {
                                yield return CreateFinalChunk(currentChunk.ToString(), options.Metadata);
                                currentChunk.Clear();
                            }
                            yield return CreateFinalChunk(part, options.Metadata);
                        }
                    }
                    else
                    {
                        currentChunk.Append(sentence);
                    }
                }
            }
        }

        // Flush remaining
        if (currentChunk.Length > 0)
        {
            yield return CreateFinalChunk(currentChunk.ToString(), options.Metadata);
        }
    }

    // Helper: Segment text into Code vs Text blocks
    private static List<TextSegment> SegmentByCodeBlocks(string text)
    {
        var segments = new List<TextSegment>();
        var codeBlockPattern = @"