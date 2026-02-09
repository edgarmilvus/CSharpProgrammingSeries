
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

# Source File: project_advanced_application_script.cs
# Description: Advanced Application Script
# ==========================================

using System;
using System.Collections.Generic; // Required for List<T> (Chapter 20)

namespace TextProcessingAdvanced
{
    class Program
    {
        // Global constants for configuration (Chapter 19)
        // We define these here so they can be changed easily later.
        const string Delimiter = ",";
        const string OutputSeparator = " | ";

        static void Main(string[] args)
        {
            Console.WriteLine("--- Advanced Application Script: User Data Processor ---");
            Console.WriteLine("This script parses raw user input, cleans it, and formats it for display.");
            Console.WriteLine("Enter 'exit' to stop the program.\n");

            // Main processing loop (Chapter 8)
            while (true)
            {
                Console.Write("Enter raw data (Name,Age,Email): ");
                string input = Console.ReadLine();

                // Check for exit condition (Chapter 6)
                if (input == "exit")
                {
                    Console.WriteLine("Exiting application...");
                    break;
                }

                // Skip empty inputs immediately (Chapter 6)
                if (string.IsNullOrEmpty(input))
                {
                    Console.WriteLine("Input is empty. Please try again.\n");
                    continue;
                }

                // Process the single input string
                ProcessSingleRecord(input);
                Console.WriteLine(); // Add spacing for readability
            }
        }

        /// <summary>
        /// Handles the logic for parsing, validating, and displaying a single record.
        /// </summary>
        /// <param name="rawData">The raw string input from the user.</param>
        static void ProcessSingleRecord(string rawData)
        {
            // 1. SPLITTING THE STRING
            // We use the Split method (Chapter 23) to break the string into an array of strings.
            // The comma is our delimiter. We expect exactly 3 parts: Name, Age, Email.
            string[] parts = rawData.Split(Delimiter);

            // 2. VALIDATION
            // We must check if the split resulted in the correct number of tokens.
            // If the user entered "John,25" (2 parts) or "John,25,john@email.com,USA" (4 parts), we reject it.
            if (parts.Length != 3)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error: Expected 3 fields separated by '{Delimiter}', but found {parts.Length}.");
                Console.ResetColor();
                return; // Exit this method early
            }

            // 3. EXTRACTION AND CLEANING
            // We extract the tokens using array indexing (Chapter 11).
            // We also trim whitespace (Chapter 23) to handle inputs like "John , 25".
            string name = parts[0].Trim();
            string ageString = parts[1].Trim();
            string email = parts[2].Trim();

            // 4. TYPE CONVERSION AND LOGIC
            // We need to validate the age is a number (Chapter 5 & 6).
            int age = 0;
            bool isAgeValid = false;

            // Basic validation loop (Chapter 8)
            // We attempt to parse the string. If it fails, we flag it.
            try
            {
                // Note: We are using int.Parse as allowed in Chapter 5.
                // In a real scenario with beginners, we might use a loop to re-prompt,
                // but for this single-entry script, we flag the error.
                age = int.Parse(ageString);
                isAgeValid = true;
            }
            catch
            {
                // Catch block handles cases where input is not a number (e.g., "abc")
                // Since we haven't learned advanced error handling, we just flag it.
                isAgeValid = false;
            }

            // 5. DECISION LOGIC
            if (!isAgeValid)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"Warning: Age '{ageString}' is not a valid number. Record marked as invalid.");
                Console.ResetColor();
                return;
            }

            // Check for logical constraints (e.g., Age must be positive)
            if (age <= 0)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Warning: Age must be greater than 0.");
                Console.ResetColor();
                return;
            }

            // 6. ADVANCED TOKENIZATION: EMAIL VALIDATION
            // We check if the email contains an '@' symbol.
            // We use Substring (Chapter 23) to find specific parts or logic to check existence.
            bool hasAtSymbol = false;
            // We iterate through the email string using a for loop (Chapter 9)
            for (int i = 0; i < email.Length; i++)
            {
                if (email[i] == '@')
                {
                    hasAtSymbol = true;
                    break; // Found it, stop looping (Chapter 10)
                }
            }

            if (!hasAtSymbol)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"Warning: Email '{email}' appears invalid (missing '@').");
                Console.ResetColor();
                // We continue despite the warning to show the formatting logic
            }

            // 7. RE-FORMATTING (JOINING)
            // We construct a new, standardized output string using String Interpolation (Chapter 3)
            // and Join (Chapter 23).
            
            // Let's create an array of the cleaned parts to join them back together
            string[] cleanParts = new string[3]; // Array declaration (Chapter 11)
            cleanParts[0] = name;
            cleanParts[1] = age.ToString(); // Convert int back to string (Chapter 5)
            cleanParts[2] = email;

            // Join the parts with a nicer separator
            string formattedOutput = string.Join(OutputSeparator, cleanParts);

            // 8. DISPLAY RESULTS
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Processed Record: {formattedOutput}");
            Console.ResetColor();
        }
    }
}
