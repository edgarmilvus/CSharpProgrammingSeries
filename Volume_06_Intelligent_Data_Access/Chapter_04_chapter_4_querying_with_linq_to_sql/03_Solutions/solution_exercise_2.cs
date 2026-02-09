
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

# Source File: solution_exercise_2.cs
# Description: Solution for Exercise 2
# ==========================================

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

// Entities
public class Author
{
    public int Id { get; set; }
    public string Name { get; set; }
    public List<Book> Books { get; set; } = new List<Book>();
}

public class Book
{
    public int Id { get; set; }
    public string Title { get; set; }
    public int AuthorId { get; set; }
    public Author Author { get; set; }
}

public class LibraryContext : DbContext
{
    public DbSet<Author> Authors { get; set; }
    public DbSet<Book> Books { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Enable logging to see SQL
        optionsBuilder
            .UseSqlServer("YourConnectionString")
            .UseLoggerFactory(LoggerFactory.Create(builder => builder.AddConsole()));
    }
}

public class AuthorRepository
{
    private readonly LibraryContext _context;

    public AuthorRepository(LibraryContext context)
    {
        _context = context;
    }

    // 1. The Problematic Code (Anti-Pattern)
    public void GetAuthorsNPlus1()
    {
        // This loads all authors. No books loaded yet.
        var authors = _context.Authors.ToList(); 
        
        foreach (var author in authors)
        {
            // PROBLEM: For every author, a new SQL query is fired to count books.
            // 100 authors = 101 database round trips (1 for authors, 100 for books).
            var bookCount = author.Books.Count(); 
            Console.WriteLine($"{author.Name} has {bookCount} books.");
        }
    }

    // 2. Solution 1: Eager Loading
    public List<AuthorDto> GetAuthorsEager()
    {
        Stopwatch sw = Stopwatch.StartNew();
        
        // .Include() fetches Authors and related Books in a SINGLE query (via JOIN).
        // Projecting to DTO immediately detaches data from the Change Tracker, saving memory.
        var result = _context.Authors
            .Include(a => a.Books) // Eager load the collection
            .Select(a => new AuthorDto
            {
                Name = a.Name,
                BookCount = a.Books.Count(),
                BookTitles = a.Books.Select(b => b.Title).ToList()
            })
            .ToList();

        sw.Stop();
        Console.WriteLine($"Eager Loading took: {sw.ElapsedMilliseconds}ms");
        return result;
    }

    // 3. Solution 2: Explicit Loading (Advanced)
    public void GetAuthorsExplicit()
    {
        // Fetch only specific authors first (e.g., those with > 5 books requires checking DB)
        // However, to check count, we usually need to load data first or use a subquery.
        // Here we demonstrate loading books ONLY for authors that meet a condition fetched in a prior step.
        
        var authors = _context.Authors
            .Where(a => a.Name.StartsWith("A")) // Initial filter
            .ToList();

        foreach (var author in authors)
        {
            // Explicitly load books only for this author if needed
            _context.Entry(author)
                .Collection(a => a.Books)
                .Load(); 
            
            // Now we can access author.Books without a lazy-load penalty (it's already loaded)
            // But this still results in N queries if done in a loop for many authors.
            // Explicit loading is best used when you need to conditionally load data for a SINGLE entity or small subset.
        }
    }

    // 4. Performance Metric Simulation
    public void RunPerformanceTest()
    {
        // Setup Data (Simulated)
        if (!_context.Authors.Any())
        {
            for (int i = 0; i < 10; i++) // Reduced to 10 for demo speed
            {
                var auth = new Author { Name = $"Author {i}" };
                for (int j = 0; j < 5; j++)
                {
                    auth.Books.Add(new Book { Title = $"Book {i}-{j}" });
                }
                _context.Authors.Add(auth);
            }
            _context.SaveChanges();
        }

        Console.WriteLine("--- Testing Eager Loading ---");
        GetAuthorsEager();

        Console.WriteLine("\n--- Testing N+1 (Warning: Slow) ---");
        // GetAuthorsNPlus1(); // Commented out to prevent slow execution in demo
    }
}

public class AuthorDto
{
    public string Name { get; set; }
    public int BookCount { get; set; }
    public List<string> BookTitles { get; set; }
}
