
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

# Source File: solution_exercise_3.cs
# Description: Solution for Exercise 3
# ==========================================

using System;
using System.IO;

namespace FileIO_Exercises
{
    class Program
    {
        static void Main(string[] args)
        {
            string filePath = "training_log.txt";

            if (File.Exists(filePath))
            {
                string[] lines = File.ReadAllLines(filePath);

                Console.WriteLine("--- Parsed Training Data ---");

                foreach (string line in lines)
                {
                    // 1. Split the line by the comma
                    // Example: "Alice,95" becomes ["Alice", "95"]
                    string[] parts = line.Split(',');

                    // Safety check: Ensure we have exactly 2 parts
                    if (parts.Length == 2)
                    {
                        string name = parts[0];
                        string scoreString = parts[1];

                        // 2. Convert score string to integer using int.Parse
                        int score = int.Parse(scoreString);

                        // 3. Print formatted output using String Interpolation
                        Console.WriteLine($"Student: {name} | Score: {score}");
                    }
                    else
                    {
                        // Inform user if a line is malformed
                        Console.WriteLine($"Skipping invalid line format: {line}");
                    }
                }
            }
            else
            {
                Console.WriteLine("File not found.");
            }
        }
    }
}
