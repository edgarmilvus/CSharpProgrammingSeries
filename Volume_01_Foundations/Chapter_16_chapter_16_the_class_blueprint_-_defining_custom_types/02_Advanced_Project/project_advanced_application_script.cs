
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

using System;

namespace LibrarySystem
{
    // 1. THE BOOK CLASS BLUEPRINT
    // This defines the structure for every book in our library.
    public class Book
    {
        // Fields (Private Data Storage)
        // We keep these private to protect the data from direct outside modification.
        private string title;
        private string author;
        private bool isCheckedOut;

        // Constructor
        // This runs when we create a new instance (new Book(...)).
        // It sets the initial state of the object.
        public Book(string bookTitle, string bookAuthor)
        {
            title = bookTitle;
            author = bookAuthor;
            isCheckedOut = false; // Default: A new book is available.
        }

        // Method: Check Availability
        // Returns true if available, false if checked out.
        public bool IsAvailable()
        {
            return !isCheckedOut;
        }

        // Method: Checkout
        // Changes the state of the book.
        public void Checkout()
        {
            if (isCheckedOut == false)
            {
                isCheckedOut = true;
                Console.WriteLine($"Success: '{title}' has been checked out.");
            }
            else
            {
                Console.WriteLine($"Error: '{title}' is already checked out.");
            }
        }

        // Method: ReturnBook
        // Resets the state of the book.
        public void ReturnBook()
        {
            if (isCheckedOut == true)
            {
                isCheckedOut = false;
                Console.WriteLine($"Success: '{title}' has been returned.");
            }
            else
            {
                Console.WriteLine($"Error: '{title}' was not checked out.");
            }
        }

        // Method: GetDetails
        // Returns a formatted string describing the book.
        public string GetDetails()
        {
            string status = isCheckedOut ? "Checked Out" : "Available";
            // Using String Interpolation (Ch 3) and Ternary Operator (Ch 6/7 logic)
            return $"Title: {title} | Author: {author} | Status: {status}";
        }
    }

    // 2. THE LIBRARY CLASS BLUEPRINT
    // This manages a collection of books.
    public class Library
    {
        // Field: Array of Books
        // We use a fixed-size array (Ch 11) to hold our books.
        private Book[] collection;
        
        // Field: Counter to track how many books we have added
        private int currentBookCount;

        // Constructor
        public Library(int capacity)
        {
            // Initialize the array with a specific size
            collection = new Book[capacity];
            currentBookCount = 0;
        }

        // Method: AddBook
        // Adds a book to the collection if there is space.
        public void AddBook(Book newBook)
        {
            if (currentBookCount < collection.Length)
            {
                collection[currentBookCount] = newBook;
                currentBookCount++;
                Console.WriteLine($"Library: Added '{newBook.GetDetails().Substring(7, 10)}...' to collection.");
            }
            else
            {
                Console.WriteLine("Library is full! Cannot add more books.");
            }
        }

        // Method: FindBook
        // Searches for a book by title (Linear Search - Ch 9/10 logic).
        // Returns the Book object if found, or null if not.
        public Book FindBook(string searchTitle)
        {
            // Loop through the array (Ch 9)
            for (int i = 0; i < collection.Length; i++)
            {
                // Check if the slot is occupied (not null)
                if (collection[i] != null)
                {
                    // We need to access the book's internal title.
                    // Since 'title' is private in the Book class, we cannot do collection[i].title directly.
                    // We must use a public method or property to get data. 
                    // However, we didn't make a GetTitle() method. 
                    // We will simulate checking by using the GetDetails method string manipulation 
                    // or we will assume for this script that we can access it if we designed it better.
                    // *Correction for Beginner Level*: Let's add a public helper method to the Book class 
                    // to check the title without exposing everything.
                    
                    // NOTE: To keep this strictly within Ch 16 beginner constraints, 
                    // we will assume the Book class has a public method 'CheckTitle' (added below).
                    if (collection[i].CheckTitle(searchTitle))
                    {
                        return collection[i];
                    }
                }
            }
            return null; // Book not found
        }

        // Method: DisplayAllBooks
        // Iterates through the collection and prints details.
        public void DisplayAllBooks()
        {
            Console.WriteLine("\n--- Current Library Inventory ---");
            for (int i = 0; i < collection.Length; i++)
            {
                if (collection[i] != null)
                {
                    // Accessing the public method of the Book object
                    Console.WriteLine(collection[i].GetDetails());
                }
            }
            Console.WriteLine("---------------------------------");
        }
    }

    // MAIN PROGRAM EXECUTION
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Welcome to the Automated Library System");
            Console.WriteLine("--------------------------------------");

            // 1. Instantiate the Library
            // We create an object of type Library using 'new'.
            // We decide the capacity is 3 books for this example.
            Library myLibrary = new Library(3);

            // 2. Create Book Objects
            // We create instances of the Book class.
            Book book1 = new Book("The C# Foundations", "John Doe");
            Book book2 = new Book("Clean Code", "Robert Martin");
            Book book3 = new Book("Design Patterns", "Gang of Four");
            Book book4 = new Book("Extra Book", "Author X"); // This one won't fit

            // 3. Add Books to the Library
            myLibrary.AddBook(book1);
            myLibrary.AddBook(book2);
            myLibrary.AddBook(book3);
            myLibrary.AddBook(book4); // Should fail (Library full)

            // 4. Display Inventory
            myLibrary.DisplayAllBooks();

            // 5. Simulate a User Checkout
            Console.WriteLine("\n--- User Action: Checkout 'Clean Code' ---");
            // Find the book first
            Book foundBook = myLibrary.FindBook("Clean Code");
            
            if (foundBook != null)
            {
                // Call the Checkout method on the specific book object
                foundBook.Checkout();
            }
            else
            {
                Console.WriteLine("Book not found in library.");
            }

            // 6. Try to Checkout the same book again
            Console.WriteLine("\n--- User Action: Checkout 'Clean Code' Again ---");
            // Note: 'foundBook' still points to the same object in memory (Heap)
            // We don't need to search again to see the updated state.
            foundBook.Checkout(); // Should fail

            // 7. Return the book
            Console.WriteLine("\n--- User Action: Return 'Clean Code' ---");
            foundBook.ReturnBook();

            // 8. Display Inventory Again
            myLibrary.DisplayAllBooks();

            // 9. Search for a book that doesn't exist
            Console.WriteLine("\n--- Search for 'Unknown Book' ---");
            Book missingBook = myLibrary.FindBook("Unknown Book");
            if (missingBook == null)
            {
                Console.WriteLine("Search Result: Book not found.");
            }

            // Keep console open
            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }
    }

    // EXTENSION TO BOOK CLASS (Added for functionality)
    // This is a helper method to allow the Library to search by title.
    public partial class Book
    {
        public bool CheckTitle(string titleToCheck)
        {
            // Simple string comparison (Ch 6)
            // Note: This is case-sensitive.
            if (title == titleToCheck)
            {
                return true;
            }
            return false;
        }
    }
}
