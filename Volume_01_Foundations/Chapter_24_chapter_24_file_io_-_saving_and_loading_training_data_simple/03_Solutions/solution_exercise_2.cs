
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
using System.IO;

namespace FileIO_Exercises
{
    class Program
    {
        static void Main(string[] args)
        {
            string filePath = "training_log.txt";

            // 1. Check if the file exists before trying to read
            if (File.Exists(filePath))
            {
                // 2. Read all lines into an array of strings
                // Each element in the array represents one line from the file
                string[] lines = File.ReadAllLines(filePath);

                Console.WriteLine("--- Training Data ---");

                // 3. Iterate through the array using a foreach loop
                foreach (string line in lines)
                {
                    // Print the raw line
                    Console.WriteLine(line);
                }
            }
            else
            {
                // 4. Handle the missing file scenario
                Console.WriteLine("File not found. Please run Exercise 1 first to generate the data.");
            }
        }
    }
}
