
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

# Source File: solution_exercise_14.cs
# Description: Solution for Exercise 14
# ==========================================

using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

// 1. Polymorphic Base Entity
public abstract class MediaAsset
{
    public int Id { get; set; }
    public string Title { get; set; }
    
    // Common properties
    public DateTime CreatedAt { get; set; }

    // 2. Different Vector Dimensions
    // We store these as byte[] in DB but handle via Value Converters
    public byte[] VectorData { get; set; }

    // Navigation property for inheritance
    public List<Tag> Tags { get; set; } = new();
}

public class TextAsset : MediaAsset
{
    // Text specific: e.g., 1536 dims (OpenAI Ada-002)
    public string Content { get; set; }
}

public class ImageAsset : MediaAsset
{
    // Image specific: e.g., 512 dims (CLIP)
    public string ImageUrl { get; set; }
}

public class AudioAsset : MediaAsset
{
    // Audio specific: e.g., 1024 dims
    public string AudioUrl { get; set; }
}

public class Tag
{
    public int Id { get; set; }
    public string Name { get; set; }
}

// 3. Unified Context
public class MultiModalContext : DbContext
{
    public DbSet<MediaAsset> MediaAssets { get; set; }
    public DbSet<TextAsset> TextAssets { get; set; }
    public DbSet<ImageAsset> ImageAssets { get; set; }
    public DbSet<AudioAsset> AudioAssets { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // TPH (Table Per Hierarchy) Strategy
        // All assets in one table with a Discriminator column
        modelBuilder.Entity<MediaAsset>()
            .HasDiscriminator<string>("AssetType")
            .HasValue<TextAsset>("Text")
            .HasValue<ImageAsset>("Image")
            .HasValue<AudioAsset>("Audio");

        // 4. Value Converters per Type (Handling different dimensions)
        modelBuilder.Entity<TextAsset>()
            .Property(a => a.VectorData)
            .HasConversion(new VectorConverter(1536));

        modelBuilder.Entity<ImageAsset>()
            .Property(a => a.VectorData)
            .HasConversion(new VectorConverter(512));
    }

    // 5. Unified Search Method
    public async Task<List<MediaAsset>> SearchAllModalities(float[] queryVector, string modality = null)
    {
        var query = MediaAssets.AsNoTracking();

        if (!string.IsNullOrEmpty(modality))
        {
            query = query.Where(a => EF.Property<string>(a, "AssetType") == modality);
        }

        // Note: We cannot do a generic vector similarity here easily because dimensions differ.
        // We must handle this in application logic or have separate indexes.
        
        // For this exercise, we assume we have a helper to normalize or handle dimensions.
        return await query.ToListAsync();
    }
}

// Mock Vector Converter
public class VectorConverter : ValueConverter<byte[], byte[]>
{
    public VectorConverter(int dimensions) : base(v => v, v => v) { }
}
