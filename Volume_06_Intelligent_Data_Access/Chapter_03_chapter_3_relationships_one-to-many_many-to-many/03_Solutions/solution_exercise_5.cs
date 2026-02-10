
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

// Source File: solution_exercise_5.cs
// Description: Solution for Exercise 5
// ==========================================

using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace VersioningSystem.Data
{
    // 1. Refactored Entities

    public class Document
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        
        // A Document has multiple versions
        public ICollection<DocumentVersion> Versions { get; set; } = new List<DocumentVersion>();
    }

    public class DocumentVersion
    {
        public Guid Id { get; set; }
        public string VersionNumber { get; set; } = string.Empty; // e.g., "1.0", "1.1"
        public Guid DocumentId { get; set; }
        
        public Document Document { get; set; } = null!;

        // Version owns the Sections
        public ICollection<Section> Sections { get; set; } = new List<Section>();
        
        // Version owns the Tags
        public ICollection<DocumentVersionTag> DocumentVersionTags { get; set; } = new List<DocumentVersionTag>();
    }

    public class Section
    {
        public Guid Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public int Level { get; set; } // 1 = Header, 2 = Subheader, etc.

        // FK to Version, not Document
        public Guid DocumentVersionId { get; set; }
        public DocumentVersion DocumentVersion { get; set; } = null!;

        // Self-referencing for hierarchy within a version
        public Guid? ParentSectionId { get; set; }
        public Section? ParentSection { get; set; }
        public ICollection<Section> ChildSections { get; set; } = new List<Section>();
    }

    public class Tag
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public ICollection<DocumentVersionTag> DocumentVersionTags { get; set; } = new List<DocumentVersionTag>();
    }

    public class DocumentVersionTag
    {
        public Guid DocumentVersionId { get; set; }
        public DocumentVersion DocumentVersion { get; set; } = null!;

        public Guid TagId { get; set; }
        public Tag Tag { get; set; } = null!;

        public DateTime AssignedDate { get; set; }
    }

    public class VersioningContext : DbContext
    {
        public DbSet<Document> Documents { get; set; }
        public DbSet<DocumentVersion> DocumentVersions { get; set; }
        public DbSet<Section> Sections { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<DocumentVersionTag> DocumentVersionTags { get; set; }

        public VersioningContext(DbContextOptions<VersioningContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Document 1-to-Many DocumentVersion
            builder.Entity<Document>()
                .HasMany(d => d.Versions)
                .WithOne(v => v.Document)
                .HasForeignKey(v => v.DocumentId)
                .OnDelete(DeleteBehavior.Cascade);

            // DocumentVersion 1-to-Many Section
            builder.Entity<DocumentVersion>()
                .HasMany(v => v.Sections)
                .WithOne(s => s.DocumentVersion)
                .HasForeignKey(s => s.DocumentVersionId)
                .OnDelete(DeleteBehavior.Cascade);

            // Section Self-Referencing
            builder.Entity<Section>()
                .HasOne(s => s.ParentSection)
                .WithMany(p => p.ChildSections)
                .HasForeignKey(s => s.ParentSectionId)
                .OnDelete(DeleteBehavior.ClientCascade);

            // DocumentVersion Many-to-Many Tag (Explicit)
            builder.Entity<DocumentVersionTag>(entity =>
            {
                entity.HasKey(dvt => new { dvt.DocumentVersionId, dvt.TagId });

                entity.HasOne(dvt => dvt.DocumentVersion)
                    .WithMany(v => v.DocumentVersionTags)
                    .HasForeignKey(dvt => dvt.DocumentVersionId);

                entity.HasOne(dvt => dvt.Tag)
                    .WithMany(t => t.DocumentVersionTags)
                    .HasForeignKey(dvt => dvt.TagId);
            });
        }
    }

    public class DocumentService
    {
        private readonly VersioningContext _context;

        public DocumentService(VersioningContext context)
        {
            _context = context;
        }

        // 4. Query Construction
        public async Task<DocumentVersion?> GetVersionWithSectionsAndTagsAsync(Guid versionId)
        {
            return await _context.DocumentVersions
                .Include(v => v.Sections)
                    .OrderBy(s => s.Level) // Ordering by hierarchy level
                .Include(v => v.DocumentVersionTags)
                    .ThenInclude(dvt => dvt.Tag)
                .FirstOrDefaultAsync(v => v.Id == versionId);
        }
    }
}
