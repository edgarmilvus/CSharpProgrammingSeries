
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

# Source File: solution_exercise_1.cs
# Description: Solution for Exercise 1
# ==========================================

using System;
using System.IO; // Required for File operations

namespace FileIO_Exercises
{
    class Program
    {
        static void Main(string[] args)
        {
            // 1. Prompt user for name
            Console.Write("Enter your name: ");
            string name = Console.ReadLine();

            // 2. Prompt user for score
            Console.Write("Enter your score: ");
            string score = Console.ReadLine();

            // 3. Format the data line
            // We use string interpolation to create the CSV format
            // \n ensures the next entry starts on a new line
            string dataLine = $"{name},{score}\n";

            // 4. Define the file path
            string filePath = "training_log.txt";

            // 5. Append the text to the file
            // File.AppendAllText creates the file if it doesn't exist
            // or adds to the end if it does.
            File.AppendAllText(filePath, dataLine);

            // 6. Confirm action
            Console.WriteLine($"Data saved to {filePath}.");
        }
    }
}
