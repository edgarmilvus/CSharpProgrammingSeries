
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
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace EfCoreCodeFirstDemo
{
    // 1. The Domain Model
    // This class represents the data we want to store.
    // It is a plain C# object (POCO).
    public class Book
    {
        // 'Id' is the Primary Key by convention
        public int Id { get; set; }

        [Required] // Data annotation for validation and database constraints
        public string Title { get; set; }

        public string? Author { get; set; } // Nullable reference type
    }

    // 2. The DbContext
    // This class acts as the bridge between your domain models and the database.
    public class LibraryContext : DbContext
    {
        public DbSet<Book> Books { get; set; } // Represents the 'Books' table

        // Constructor accepting DbContextOptions allows for dependency injection
        public LibraryContext(DbContextOptions<LibraryContext> options) 
            : base(options)
        {
        }

        // 3. Model Configuration (Optional but good practice)
        // This method allows us to configure the schema using Fluent API.
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Example: Setting a maximum length for the Title column
            modelBuilder.Entity<Book>()
                .Property(b => b.Title)
                .HasMaxLength(200);
        }
    }

    // 4. The Application Entry Point
    class Program
    {
        static async Task Main(string[] args)
        {
            // Setup Dependency Injection (Standard .NET pattern)
            var services = new ServiceCollection();
            
            // Configure the DbContext to use SQLite (In-memory for this demo)
            // In a real app, this connection string points to a physical file.
            services.AddDbContext<LibraryContext>(options =>
                options.UseSqlite("Data Source=library.db"));

            var serviceProvider = services.BuildServiceProvider();

            // Ensure the database is created
            using (var scope = serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<LibraryContext>();
                
                // This applies any pending migrations. If no migrations exist, 
                // it creates the database based on the current model.
                // For this simple demo, we use EnsureCreated() instead of Migrations 
                // to keep it "Hello World" simple.
                await context.Database.EnsureCreatedAsync();
                
                Console.WriteLine("Database created successfully.");
            }

            // Perform CRUD Operations
            using (var scope = serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<LibraryContext>();

                // C: Create
                var newBook = new Book { Title = "The Fellowship of the Ring", Author = "J.R.R. Tolkien" };
                context.Books.Add(newBook);
                await context.SaveChangesAsync(); // Commit to DB
                Console.WriteLine($"Added: {newBook.Title}");

                // R: Read
                var book = await context.Books
                    .FirstOrDefaultAsync(b => b.Title.Contains("Fellowship"));
                
                if (book != null)
                {
                    Console.WriteLine($"Found: {book.Title} by {book.Author}");
                }

                // U: Update
                if (book != null)
                {
                    book.Author = "J.R.R. Tolkien (Updated)";
                    await context.SaveChangesAsync();
                    Console.WriteLine("Updated author name.");
                }

                // D: Delete
                context.Books.Remove(book);
                await context.SaveChangesAsync();
                Console.WriteLine("Book deleted.");
            }

            Console.WriteLine("Demo completed.");
        }
    }
}
