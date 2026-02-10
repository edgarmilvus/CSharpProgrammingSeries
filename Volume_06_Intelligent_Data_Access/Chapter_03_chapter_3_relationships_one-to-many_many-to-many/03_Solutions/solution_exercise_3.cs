
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

// Source File: solution_exercise_3.cs
// Description: Solution for Exercise 3
// ==========================================

using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LibrarySystem.Data
{
    public class Book
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public ICollection<Page> Pages { get; set; } = new List<Page>();
    }

    public class Page
    {
        public Guid Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public Guid BookId { get; set; }
        public Book Book { get; set; } = null!;
    }

    public class LibraryContext : DbContext
    {
        public DbSet<Book> Books { get; set; }
        public DbSet<Page> Pages { get; set; }

        public LibraryContext(DbContextOptions<LibraryContext> options) : base(options) 
        {
            // NOTE: Lazy Loading requires proxies (e.g., Microsoft.EntityFrameworkCore.Proxies)
            // ChangeTracker.LazyLoadingEnabled = true; 
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Book>()
                .HasMany(b => b.Pages)
                .WithOne(p => p.Book)
                .HasForeignKey(p => p.BookId);
        }
    }

    public class LibraryService
    {
        private readonly LibraryContext _context;

        public LibraryService(LibraryContext context)
        {
            _context = context;
        }

        // 1. N+1 Problem (Lazy Loading Simulation)
        public void DemonstrateNPlusOne()
        {
            // Assume Lazy Loading is enabled.
            // This query fetches all Books (1 query).
            var books = _context.Books.ToList();

            // The loop below triggers a separate query for EACH book to fetch its pages.
            // If there are 100 books, this results in 101 queries (1 for books + 100 for pages).
            foreach (var book in books)
            {
                // Accessing book.Pages triggers the proxy to load data.
                Console.WriteLine($"Book: {book.Title}, Pages: {book.Pages.Count}");
            }
        }

        // 2. Eager Loading Solution
        public async Task<List<Book>> GetBooksWithPagesEagerlyAsync()
        {
            // Single query with JOIN to fetch Books and Pages.
            // Reduces database roundtrips from N+1 to 1.
            return await _context.Books
                .Include(b => b.Pages)
                .ToListAsync();
        }

        // 3. Explicit Loading Solution
        public async Task LoadPagesForSpecificBookAsync(Guid bookId)
        {
            var book = await _context.Books.FindAsync(bookId);
            
            if (book != null)
            {
                // Explicitly load the Pages collection for this specific book.
                // Useful when you don't want to load pages for all books in a list.
                await _context.Entry(book)
                    .Collection(b => b.Pages)
                    .LoadAsync();
            }
        }

        // 4. Selective Eager Loading (Projection)
        public async Task<object> GetBookDataProjectionAsync(Guid bookId)
        {
            // Only retrieves specific columns (Title and Page Text), not the entire entity graph.
            // Significantly reduces data transfer and memory usage.
            return await _context.Books
                .Where(b => b.Id == bookId)
                .Select(b => new 
                { 
                    b.Title, 
                    PageTexts = b.Pages.Select(p => p.Text).ToList() 
                })
                .FirstOrDefaultAsync();
        }
    }
}
