
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

using System;

public class Book
{
    // Properties (Get only = Immutable from outside)
    public string Title { get; }
    public string Author { get; }
    public int PageCount { get; }
    public string ISBN { get; }
    public bool IsCheckedOut { get; private set; }

    // Constructor
    public Book(string title, string author, int pageCount, string isbn)
    {
        Title = title;
        Author = author;
        ISBN = isbn;

        // Logic: Enforce minimum page count
        if (pageCount <= 0)
        {
            PageCount = 1;
        }
        else
        {
            PageCount = pageCount;
        }

        // Default state
        IsCheckedOut = false;
    }

    // Methods
    public void Checkout()
    {
        if (!IsCheckedOut)
        {
            IsCheckedOut = true;
            Console.WriteLine($"{Title} has been checked out.");
        }
        else
        {
            Console.WriteLine($"{Title} is already checked out.");
        }
    }

    public void ReturnBook()
    {
        if (IsCheckedOut)
        {
            IsCheckedOut = false;
            Console.WriteLine($"{Title} has been returned.");
        }
    }

    public string GetInfo()
    {
        return $"{Title} by {Author} ({PageCount} pages) - ISBN: {ISBN}";
    }
}

public class Program
{
    public static void Main()
    {
        Book b1 = new Book("The C# Foundations", "Dev Author", 350, "978-123456");
        Book b2 = new Book("Empty Book", "Unknown", 0, "000-000000");

        Console.WriteLine(b1.GetInfo());
        Console.WriteLine(b2.GetInfo()); // Should show 1 page

        b1.Checkout();
        Console.WriteLine($"Is b1 checked out? {b1.IsCheckedOut}");
        
        b1.ReturnBook();
        Console.WriteLine($"Is b1 checked out? {b1.IsCheckedOut}");
    }
}
