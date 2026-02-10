
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

// Source File: basic_basic_code_example_part2.cs
// Description: Basic Code Example
// ==========================================

using System;

class AgeVerifier
{
    static void Main()
    {
        // Variable to store the user's age
        int age;
        
        // Variable to track if the input is valid
        bool isValid = false;

        // We use a do-while loop because we must ask for input at least once
        do
        {
            // Prompt the user
            Console.Write("Please enter your age (1-120): ");
            string input = Console.ReadLine();

            // Try to convert the string input to an integer
            // We use a logical OR (||) to check for two error conditions
            if (input == "" || (input != "" && !int.TryParse(input, out age)))
            {
                // If input is empty or cannot be parsed, show an error
                Console.WriteLine("Invalid input. Please enter a whole number.");
            }
            else
            {
                // If parsing succeeded, 'age' now holds the number.
                // Now check if the number is within the valid range.
                if (age >= 1 && age <= 120)
                {
                    isValid = true; // Set flag to true to exit the loop
                }
                else
                {
                    // Number was parsed but is out of range
                    Console.WriteLine("Age must be between 1 and 120.");
                }
            }

        } while (!isValid); // Continue looping as long as isValid is false

        // Final confirmation message
        Console.WriteLine($"Thank you. Age {age} recorded.");
    }
}
