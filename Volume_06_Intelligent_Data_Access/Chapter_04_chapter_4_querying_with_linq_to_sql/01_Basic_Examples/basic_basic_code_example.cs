
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

// 1. Define the Domain Model (The "Book" entity)
public class Book
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Author { get; set; }
    public int PublicationYear { get; set; }
    public decimal Price { get; set; }
}

// 2. Define the Data Context (The bridge between C# and SQL)
public class LibraryContext : DbContext
{
    public DbSet<Book> Books { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        // We use a local database file for this self-contained example.
        // In a real app, this connection string would come from configuration.
        options.UseSqlite("Data Source=library.db");
    }
}

// 3. The Main Program Execution
public class Program
{
    public static async Task Main(string[] args)
    {
        Console.WriteLine("--- Library Query Demo ---");

        // A. Setup: Ensure the database exists and has data
        await InitializeDatabaseAsync();

        // B. Execution: Run various LINQ queries
        await RunQueriesAsync();
    }

    // --- Setup Logic ---
    private static async Task InitializeDatabaseAsync()
    {
        using var context = new LibraryContext();

        // Create the database if it doesn't exist
        await context.Database.EnsureCreatedAsync();

        // Only seed data if the table is empty
        if (!await context.Books.AnyAsync())
        {
            var books = new List<Book>
            {
                new Book { Title = "The Great Gatsby", Author = "F. Scott Fitzgerald", PublicationYear = 1925, Price = 10.99m },
                new Book { Title = "1984", Author = "George Orwell", PublicationYear = 1949, Price = 8.50m },
                new Book { Title = "C# in Depth", Author = "Jon Skeet", PublicationYear = 2018, Price = 45.00m },
                new Book { Title = "Clean Code", Author = "Robert C. Martin", PublicationYear = 2008, Price = 35.50m }
            };

            context.Books.AddRange(books);
            await context.SaveChangesAsync();
            Console.WriteLine("Database seeded with initial data.");
        }
    }

    // --- Query Logic ---
    private static async Task RunQueriesAsync()
    {
        using var context = new LibraryContext();

        // QUERY 1: Basic Filtering (Where)
        // Context: Find all books written by "George Orwell".
        Console.WriteLine("\n1. Books by George Orwell:");
        var orwellBooks = await context.Books
            .Where(b => b.Author == "George Orwell")
            .ToListAsync();

        foreach (var book in orwellBooks)
        {
            Console.WriteLine($"   - {book.Title} (${book.Price})");
        }

        // QUERY 2: Sorting (OrderBy)
        // Context: List all books sorted by Publication Year (Oldest to Newest).
        Console.WriteLine("\n2. Books by Publication Year:");
        var chronologicalBooks = await context.Books
            .OrderBy(b => b.PublicationYear)
            .ToListAsync();

        foreach (var book in chronologicalBooks)
        {
            Console.WriteLine($"   - {book.Title} ({book.PublicationYear})");
        }

        // QUERY 3: Projection (Select)
        // Context: Get just the titles of books costing more than $20.
        // We project the result into an anonymous type to save bandwidth.
        Console.WriteLine("\n3. Expensive Book Titles (> $20):");
        var expensiveTitles = await context.Books
            .Where(b => b.Price > 20.00m)
            .Select(b => new { b.Title, b.Price }) // Projection
            .ToListAsync();

        foreach (var item in expensiveTitles)
        {
            Console.WriteLine($"   - {item.Title}: ${item.Price}");
        }
        
        // QUERY 4: Aggregation (Count, Max, Average)
        // Context: Get statistics about the book collection.
        Console.WriteLine("\n4. Library Statistics:");
        int count = await context.Books.CountAsync();
        decimal maxPrice = await context.Books.MaxAsync(b => b.Price);
        decimal avgPrice = await context.Books.AverageAsync(b => b.Price);

        Console.WriteLine($"   - Total Books: {count}");
        Console.WriteLine($"   - Most Expensive: ${maxPrice}");
        Console.WriteLine($"   - Average Price: ${avgPrice:C}");
    }
}
