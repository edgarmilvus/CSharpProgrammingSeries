
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

// Source File: solution_exercise_2.cs
// Description: Solution for Exercise 2
// ==========================================

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Text.Json;

public class MultiTenantDbContext : DbContext
{
    // Current Tenant ID (injected via constructor or service accessor)
    private readonly Guid _currentTenantId;

    public MultiTenantDbContext(DbContextOptions<MultiTenantDbContext> options, Guid currentTenantId)
        : base(options)
    {
        _currentTenantId = currentTenantId;
    }

    public DbSet<Document> Documents { get; set; }
    public DbSet<Chunk> Chunks { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // 1. Shadow Property: TenantId
        // We add this to all entities that require isolation
        modelBuilder.Entity<Document>().Property<Guid>("TenantId");
        modelBuilder.Entity<Chunk>().Property<Guid>("TenantId");

        // 2. Global Query Filter
        // Automatically filters all queries to the current tenant
        modelBuilder.Entity<Document>().HasQueryFilter(d => EF.Property<Guid>(d, "TenantId") == _currentTenantId);
        modelBuilder.Entity<Chunk>().HasQueryFilter(c => EF.Property<Guid>(c, "TenantId") == _currentTenantId);

        // 3. Composite Index for Multi-tenant Vector Search
        // Optimizes queries filtering by TenantId AND performing vector ops
        modelBuilder.Entity<Document>()
            .HasIndex(d => new { 
                TenantId = EF.Property<Guid>(d, "TenantId"),
                // Assuming Vector property exists on Document
                d.Id 
            }); 
            // Note: In real scenarios, you'd index the vector column itself if supported by provider

        // 4. Value Converter for Vector Embeddings (float[] to JSON or binary)
        var vectorConverter = new ValueConverter<float[], byte[]>(
            v => JsonSerializer.SerializeToUtf8Bytes(v, (JsonSerializerOptions)null),
            v => JsonSerializer.Deserialize<float[]>(v, (JsonSerializerOptions)null)
        );

        modelBuilder.Entity<Document>()
            .Property(d => d.Vector)
            .HasConversion(vectorConverter)
            .Metadata.SetValueConverter(vectorConverter);

        // 5. Seeding Data with Tenant Specificity
        modelBuilder.Entity<Document>().HasData(
            new Document 
            { 
                Id = 1, 
                Title = "Tenant A Doc", 
                Vector = new float[] { 0.1f, 0.2f } 
            }
            // Note: Shadow properties cannot be set in HasData directly in older EF versions, 
            // usually requires raw SQL or post-migration scripts, or custom logic in SaveChanges.
        );
    }

    public override int SaveChanges()
    {
        // 6. Automatic Shadow Property Injection on Save
        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.State == EntityState.Added && 
                (entry.Entity is Document || entry.Entity is Chunk))
            {
                entry.Property("TenantId").CurrentValue = _currentTenantId;
            }
        }
        return base.SaveChanges();
    }

    // 7. Method to Disable Filter for Admin
    public IQueryable<Document> GetAllTenantsDocuments()
    {
        // Ignores the global query filter defined in OnModelCreating
        return Documents.IgnoreQueryFilters();
    }
}

// Entity definitions (updated with Vector property)
public class Document
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Content { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<Chunk> Chunks { get; set; } = new();
    
    // Vector property for the exercise
    public float[] Vector { get; set; }
}

public class Chunk
{
    public int Id { get; set; }
    public int DocumentId { get; set; }
    public string Text { get; set; }
    public int StartPosition { get; set; }
    public Document Document { get; set; }
}
