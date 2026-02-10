
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
using System.Linq;

namespace AdvancedAppScript
{
    // ---------------------------------------------------------
    // 1. Domain Entities (Basic Classes)
    // ---------------------------------------------------------
    // Real-world context: A Library Management System.
    // We need to track Books and their Authors.
    public class Author
    {
        public int AuthorId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
    }

    public class Book
    {
        public int BookId { get; set; }
        public string Title { get; set; }
        public int YearPublished { get; set; }
        public int AuthorId { get; set; } // Foreign Key

        // Navigation Property (EF Core Concept)
        public Author Author { get; set; }
    }

    // ---------------------------------------------------------
    // 2. Infrastructure: DbContext and Database Setup
    // ---------------------------------------------------------
    // We simulate a database using an InMemory provider for this console app.
    // In a real scenario, this would connect to SQL Server or PostgreSQL.
    public class LibraryContext : DbContext
    {
        public DbSet<Author> Authors { get; set; }
        public DbSet<Book> Books { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Using Microsoft.EntityFrameworkCore.InMemory for demonstration
            optionsBuilder.UseInMemoryDatabase("LibraryDb");
        }
    }

    // ---------------------------------------------------------
    // 3. The "Direct Context" Approach (Subsection Focus)
    // ---------------------------------------------------------
    // Instead of wrapping every operation in a Repository class,
    // we expose the DbContext directly to the Application Service.
    // This allows us to leverage IQueryable<T> for complex filtering
    // without abstraction overhead.
    public class LibraryService
    {
        private readonly LibraryContext _context;

        public LibraryService(LibraryContext context)
        {
            _context = context;
        }

        // Method: Add Data
        // We perform basic validation and direct insertion.
        public void SeedData()
        {
            if (_context.Authors.Any()) return; // Check if already seeded

            var author1 = new Author { Name = "Jane Doe", Email = "jane@example.com" };
            var author2 = new Author { Name = "John Smith", Email = "john@example.com" };

            _context.Authors.AddRange(author1, author2);
            _context.SaveChanges();

            var books = new[]
            {
                new Book { Title = "C# Basics", YearPublished = 2020, AuthorId = author1.AuthorId },
                new Book { Title = "Advanced EF Core", YearPublished = 2023, AuthorId = author1.AuthorId },
                new Book { Title = "Database Design", YearPublished = 2022, AuthorId = author2.AuthorId }
            };

            _context.Books.AddRange(books);
            _context.SaveChanges();
        }

        // Method: Complex Query using Direct Context
        // This demonstrates the power of IQueryable.
        // We filter books published after 2021 AND join with Author data.
        // Notice: We are NOT using a generic repository. We use _context directly.
        public void GetRecentBooksByAuthor(string authorName)
        {
            Console.WriteLine($"\n--- Searching for books by '{authorName}' published after 2021 ---");

            // 1. Build the query using IQueryable.
            // The query is not executed yet. It is an expression tree.
            var query = from b in _context.Books
                        join a in _context.Authors on b.AuthorId equals a.AuthorId
                        where a.Name == authorName && b.YearPublished > 2021
                        select new { b.Title, b.YearPublished, Author = a.Name };

            // 2. Execution happens here (Deferred Execution).
            // We iterate over the results.
            var results = query.ToList();

            if (results.Count == 0)
            {
                Console.WriteLine("No books found matching criteria.");
            }
            else
            {
                foreach (var item in results)
                {
                    Console.WriteLine($"Title: {item.Title} | Year: {item.YearPublished} | Author: {item.Author}");
                }
            }
        }

        // Method: Direct Update
        // Updating an entity without a repository layer.
        // We fetch the entity, modify it, and save.
        public void UpdateBookTitle(int bookId, string newTitle)
        {
            var book = _context.Books.Find(bookId); // Find uses the primary key
            if (book != null)
            {
                book.Title = newTitle;
                int changes = _context.SaveChanges();
                Console.WriteLine($"\n--- Update Result: {changes} record(s) updated. ---");
            }
        }

        // Method: Complex Aggregation
        // Counting books per author using raw SQL or LINQ.
        // Direct Context allows mixing raw SQL with LINQ easily.
        public void GetAuthorStats()
        {
            Console.WriteLine("\n--- Author Statistics (Direct Context Aggregation) ---");

            // Using LINQ to Objects (client-side) vs Database-side (server-side).
            // Since we are using InMemory, it's similar, but in SQL, this executes on DB.
            var stats = from b in _context.Books
                        group b by b.AuthorId into g
                        select new { AuthorId = g.Key, Count = g.Count() };

            foreach (var stat in stats)
            {
                // We need to fetch the author name separately or join.
                var authorName = _context.Authors.Find(stat.AuthorId)?.Name ?? "Unknown";
                Console.WriteLine($"Author: {authorName} | Total Books: {stat.Count}");
            }
        }
    }

    // ---------------------------------------------------------
    // 4. Main Execution
    // ---------------------------------------------------------
    class Program
    {
        static void Main(string[] args)
        {
            // Setup Dependency Injection manually (simplified for console app)
            using (var context = new LibraryContext())
            {
                var service = new LibraryService(context);

                // 1. Seed the database
                service.SeedData();

                // 2. Perform Complex Query
                service.GetRecentBooksByAuthor("Jane Doe");

                // 3. Perform Update
                // Let's find an ID (usually known from previous query)
                // In this seeded data, "C# Basics" is ID 1.
                service.UpdateBookTitle(1, "C# Fundamentals");

                // 4. Verify Update via Aggregation
                service.GetAuthorStats();
            }

            Console.WriteLine("\nApplication finished. Press any key to exit.");
            Console.ReadKey();
        }
    }
}
