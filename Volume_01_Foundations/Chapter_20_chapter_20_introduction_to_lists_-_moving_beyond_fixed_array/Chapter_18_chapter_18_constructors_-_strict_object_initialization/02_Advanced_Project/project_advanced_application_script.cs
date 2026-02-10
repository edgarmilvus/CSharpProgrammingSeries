
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
    // DEFINITION: The Class Blueprint
    // We are defining a 'Book' that exists in the Heap memory.
    public class Book
    {
        // --- FIELDS AND PROPERTIES (Chapter 17) ---
        
        // We use 'private set' to prevent modification from outside the class.
        // However, for strict initialization, we will use 'init' (Chapter 18 specific).
        
        // The Title is a string. It is mandatory.
        // 'init' means this value can ONLY be set when the object is first created.
        public string Title { get; init; }

        // The ID is an integer. It is mandatory.
        public int Id { get; init; }

        // The Available status. Defaults to true.
        public bool IsAvailable { get; set; }

        // --- CONSTRUCTOR (Chapter 18) ---
        
        // This is the Constructor. It has the same name as the class.
        // It takes parameters to enforce strict initialization.
        // If you try to create a Book without providing a title and id, the code will not compile.
        public Book(string initialTitle, int initialId)
        {
            // LOGIC: Validation
            // We check if the input is valid. This is "Guard Clause" logic.
            if (initialTitle == null || initialTitle == "")
            {
                // We throw an exception to stop execution immediately.
                // This prevents an invalid object from ever existing.
                throw new ArgumentException("Title cannot be empty.");
            }

            if (initialId <= 0)
            {
                throw new ArgumentException("ID must be a positive number.");
            }

            // LOGIC: Assignment
            // We assign the parameters to the 'init' properties.
            // Once this block finishes, these properties lock.
            Title = initialTitle;
            Id = initialId;
            IsAvailable = true; // Default state
        }
    }

    // The Main Program
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("--- Library System Initialized ---");
            Console.WriteLine("");

            // SCENARIO 1: The Correct Way (Strict Initialization)
            // We provide the mandatory arguments required by the constructor.
            try
            {
                Console.WriteLine("Attempting to create a valid book...");
                
                // The 'new' keyword calls the constructor.
                Book validBook = new Book("The C# Foundations", 101);

                Console.WriteLine($"Success! Book Created: {validBook.Title} (ID: {validBook.Id})");
                
                // Let's try to change the title? 
                // validBook.Title = "New Title"; // This would cause a compile error because of 'init'.
                
                // Let's change availability (This is allowed because IsAvailable has a 'set')
                validBook.IsAvailable = false;
                Console.WriteLine($"Availability updated to: {validBook.IsAvailable}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

            Console.WriteLine("");

            // SCENARIO 2: The Incorrect Way (Missing Data)
            // We try to create a book without the required constructor arguments.
            // This will fail immediately.
            try
            {
                Console.WriteLine("Attempting to create a book with missing data...");
                
                // This line will not compile if you uncomment it:
                // Book badBook = new Book(); 
                
                // So we simulate a user entering empty data:
                string userInput = ""; 
                int userInt = 0; // Simulating bad ID

                // We attempt to pass this bad data to the constructor
                Book invalidBook = new Book(userInput, userInt);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Caught Error: {ex.Message}");
            }

            Console.WriteLine("");

            // SCENARIO 3: The Library Inventory (Using Arrays)
            // We will store these books in an array (Chapter 11).
            // We need to manage the index manually because we don't have Lists yet.
            
            Console.WriteLine("--- Generating Inventory Report ---");

            // Define an array to hold exactly 3 books.
            Book[] libraryInventory = new Book[3];

            // Manually adding books using the strict constructor.
            // We use a loop (Chapter 9) to populate the array.
            for (int i = 0; i < libraryInventory.Length; i++)
            {
                if (i == 0)
                {
                    libraryInventory[i] = new Book("Clean Code", 201);
                }
                else if (i == 1)
                {
                    libraryInventory[i] = new Book("Design Patterns", 202);
                }
                else
                {
                    libraryInventory[i] = new Book("Refactoring", 203);
                }
            }

            // Iterating through the inventory (Chapter 12: foreach)
            foreach (Book b in libraryInventory)
            {
                // We can access the properties because they are readable.
                Console.WriteLine($"ID: {b.Id} | Title: {b.Title} | Available: {b.IsAvailable}");
            }

            Console.WriteLine("");
            Console.WriteLine("--- End of Script ---");
        }
    }
}
