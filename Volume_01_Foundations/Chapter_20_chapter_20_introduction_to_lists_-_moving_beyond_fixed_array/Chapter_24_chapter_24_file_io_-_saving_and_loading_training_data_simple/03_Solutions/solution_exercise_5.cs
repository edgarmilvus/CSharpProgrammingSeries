
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
using System.IO;
using System.Collections.Generic; // Required for List<T>

namespace FileIO_Exercises
{
    class Program
    {
        static void Main(string[] args)
        {
            string filePath = "training_log.txt";

            if (!File.Exists(filePath))
            {
                Console.WriteLine("No data to filter.");
                return;
            }

            // 1. Read existing data into an array
            string[] lines = File.ReadAllLines(filePath);
            
            // 2. Create a dynamic list to store valid entries
            // We use List<string> because we don't know the final count of passing entries
            List<string> passingEntries = new List<string>();

            foreach (string line in lines)
            {
                string[] parts = line.Split(',');
                
                if (parts.Length == 2)
                {
                    // 3. Convert score string to integer
                    int score = int.Parse(parts[1]);

                    // 4. Filter logic: Check if score is 50 or higher
                    if (score >= 50)
                    {
                        // Add the original line format to our list
                        passingEntries.Add(line);
                    }
                }
            }

            // 5. Overwrite the file with only passing data
            // We convert the List<string> to an array using .ToArray()
            File.WriteAllLines(filePath, passingEntries.ToArray());

            Console.WriteLine($"Filtering complete. {passingEntries.Count} passing records saved.");
        }
    }
}
