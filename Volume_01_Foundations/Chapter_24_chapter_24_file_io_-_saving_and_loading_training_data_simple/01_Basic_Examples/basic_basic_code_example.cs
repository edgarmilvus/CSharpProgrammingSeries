
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

using System;
using System.IO; // Required for file operations

namespace FileIOExample
{
    class Program
    {
        static void Main(string[] args)
        {
            // Define the file path where we will store the data
            string filePath = "training_data.txt";

            // --- PART 1: SAVING DATA ---
            // We will write three lines of text to the file.
            // In a real scenario, this could be sensor readings or user input.
            
            // Open a stream to write text to the file (creates it if it doesn't exist)
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                writer.WriteLine("Dataset: Iris Flower");
                writer.WriteLine("Sample Count: 150");
                writer.WriteLine("Features: Sepal Length, Sepal Width");
            }
            // The 'using' block automatically closes the file when done.
            Console.WriteLine("Data saved successfully to " + filePath);

            // --- PART 2: LOADING DATA ---
            // Now we will read the data back to verify it was saved.
            Console.WriteLine("\nReading data from file:");
            
            // Open a stream to read text from the file
            using (StreamReader reader = new StreamReader(filePath))
            {
                // Loop until we reach the end of the file
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    // Print each line to the console
                    Console.WriteLine("Read: " + line);
                }
            }
            // The 'using' block automatically closes the file when done.
        }
    }
}
