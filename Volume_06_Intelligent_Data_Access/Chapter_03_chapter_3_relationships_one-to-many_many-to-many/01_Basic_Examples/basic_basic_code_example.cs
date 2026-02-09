
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

# Source File: basic_basic_code_example.cs
# Description: Basic Code Example
# ==========================================

using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EfCoreRelationshipsDemo
{
    // 1. Domain Entities
    // Represents a blog post. The 'Author' navigation property points to the Author.
    public class BlogPost
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;

        // Foreign Key property (optional but recommended for explicit control)
        public int AuthorId { get; set; }

        // Navigation Property (One side of the relationship)
        public Author Author { get; set; } = null!;

        // Navigation Property (Reverse side of a potential One-to-Many)
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    }

    // Represents the author of posts.
    public class Author
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        // Navigation Property (The 'Many' side of the relationship)
        // Using a concrete List<T> ensures we can Add() items immediately without null checks.
        public ICollection<BlogPost> BlogPosts { get; set; } = new List<BlogPost>();
    }

    // Represents a comment on a post (demonstrating a secondary relationship).
    public class Comment
    {
        public int Id { get; set; }
        public string Text { get; set; } = string.Empty;

        // Foreign Key
        public int BlogPostId { get; set; }

        // Navigation Property
        public BlogPost BlogPost { get; set; } = null!;
    }

    // 2. DbContext Configuration
    public class BloggingContext : DbContext
    {
        public DbSet<Author> Authors { get; set; }
        public DbSet<BlogPost> BlogPosts { get; set; }
        public DbSet<Comment> Comments { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Using SQLite for a lightweight, file-based database that requires no server setup.
            optionsBuilder.UseSqlite("Data Source=blogging.db");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // CONFIGURATION: One-to-Many Relationship
            // We configure the relationship between Author and BlogPost.
            // HasOne<BlogPost>: Indicates the Author has one BlogPost (conceptually).
            // WithMany(a => a.Author): Indicates a BlogPost has many Authors (wait, no, let's correct this logic).
            // Actually: An Author has Many BlogPosts. A BlogPost has One Author.
            
            // Correct Configuration:
            modelBuilder.Entity<Author>()
                .HasMany(a => a.BlogPosts)   // Author has many BlogPosts
                .WithOne(b => b.Author)      // BlogPost has one Author
                .HasForeignKey(b => b.AuthorId) // The foreign key in BlogPost
                .OnDelete(DeleteBehavior.Cascade); // If Author is deleted, delete their posts.

            // Configuration for Comment -> BlogPost (One BlogPost has Many Comments)
            modelBuilder.Entity<BlogPost>()
                .HasMany(bp => bp.Comments)
                .WithOne(c => c.BlogPost)
                .HasForeignKey(c => c.BlogPostId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }

    // 3. Usage Example
    class Program
    {
        static async Task Main(string[] args)
        {
            // Clean up previous database file for a fresh run
            if (System.IO.File.Exists("blogging.db")) System.IO.File.Delete("blogging.db");

            // A. Setup and Seeding Data
            using (var context = new BloggingContext())
            {
                await context.Database.EnsureCreatedAsync();

                // Create an Author
                var author = new Author { Name = "Jane Doe" };

                // Create BlogPosts and associate them with the Author
                // Note: We add to the Author's collection, EF Core tracks the relationship.
                var post1 = new BlogPost { Title = "Intro to EF Core", Content = "..." };
                var post2 = new BlogPost { Title = "Advanced Relationships", Content = "..." };

                author.BlogPosts.Add(post1);
                author.BlogPosts.Add(post2);

                // Add comments to post1
                post1.Comments.Add(new Comment { Text = "Great article!" });
                post1.Comments.Add(new Comment { Text = "Very helpful." });

                // Add to the context and save
                context.Authors.Add(author);
                await context.SaveChangesAsync();

                Console.WriteLine($"Seeded Author '{author.Name}' with {author.BlogPosts.Count} posts.");
            }

            // B. Reading Data (Eager Loading)
            using (var context = new BloggingContext())
            {
                // We use .Include() to load the related data in a single query (Eager Loading).
                // Without .Include(), accessing 'Author' or 'Comments' would trigger separate queries (Lazy Loading)
                // or return null (if Lazy Loading is disabled).
                var authorWithPosts = await context.Authors
                    .Include(a => a.BlogPosts)       // Load the One-to-Many collection
                        .ThenInclude(bp => bp.Comments) // Load the nested One-to-Many collection
                    .FirstOrDefaultAsync(a => a.Name == "Jane Doe");

                if (authorWithPosts != null)
                {
                    Console.WriteLine($"\nRetrieved Author: {authorWithPosts.Name}");
                    foreach (var post in authorWithPosts.BlogPosts)
                    {
                        Console.WriteLine($" - Post: {post.Title}");
                        foreach (var comment in post.Comments)
                        {
                            Console.WriteLine($"    Comment: {comment.Text}");
                        }
                    }
                }
            }

            // C. Modifying Relationships
            using (var context = new BloggingContext())
            {
                // Fetch a specific post and a new author
                var postToMove = await context.BlogPosts.FirstAsync();
                var newAuthor = new Author { Name = "John Smith" };
                context.Authors.Add(newAuthor);

                // Change the relationship by updating the Foreign Key property directly
                postToMove.AuthorId = newAuthor.Id;
                
                // Alternatively, we could update the Navigation Property:
                // postToMove.Author = newAuthor;
                // However, updating the FK is often more performant as it avoids loading the old Author entity.

                await context.SaveChangesAsync();
                Console.WriteLine($"\nMoved post '{postToMove.Title}' to new author: {newAuthor.Name}");
            }
        }
    }
}
