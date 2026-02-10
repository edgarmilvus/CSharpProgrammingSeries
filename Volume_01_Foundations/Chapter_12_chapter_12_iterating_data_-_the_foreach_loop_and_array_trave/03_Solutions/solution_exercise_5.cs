
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

// Source File: solution_exercise_5.cs
// Description: Solution for Exercise 5
// ==========================================

using System;

class Program
{
    static void Main()
    {
        // 1. Create array
        int[] tickets = { 5, 12, 0, 8, 99, 3 };

        Console.WriteLine("Starting Ticket Processing...");

        // 2. Iterate
        foreach (int ticket in tickets)
        {
            // 3. Check for invalid ticket (0)
            if (ticket == 0)
            {
                Console.WriteLine("Skipping invalid ticket 0.");
                continue; // Skips the rest of the loop body and moves to next iteration
            }

            // 4. Check for emergency stop (99)
            if (ticket == 99)
            {
                Console.WriteLine("Emergency Stop! Ticket 99 found.");
                break; // Terminates the loop entirely
            }

            // 5. Normal processing
            Console.WriteLine($"Processing ticket: {ticket}");
        }

        Console.WriteLine("End of program.");
    }
}
