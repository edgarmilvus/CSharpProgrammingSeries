
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

# Source File: solution_exercise_4.cs
# Description: Solution for Exercise 4
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
            bool running = true;

            // 1. Main program loop
            while (running)
            {
                Console.WriteLine("\n--- MENU ---");
                Console.WriteLine("1. Add Training Data");
                Console.WriteLine("2. View Training Data");
                Console.WriteLine("3. Exit");
                Console.Write("Select an option: ");

                string choice = Console.ReadLine();

                // 2. Switch expression for menu logic
                switch (choice)
                {
                    case "1":
                        // --- ADD DATA ---
                        Console.Write("Enter Name: ");
                        string name = Console.ReadLine();
                        
                        Console.Write("Enter Score: ");
                        string score = Console.ReadLine();

                        // Format: Name,Score\n
                        string entry = $"{name},{score}\n";
                        
                        // Append to file
                        File.AppendAllText(filePath, entry);
                        Console.WriteLine("Data added successfully.");
                        break;

                    case "2":
                        // --- VIEW DATA ---
                        if (File.Exists(filePath))
                        {
                            string[] lines = File.ReadAllLines(filePath);
                            
                            if (lines.Length == 0)
                            {
                                Console.WriteLine("File is empty.");
                            }
                            else
                            {
                                Console.WriteLine("\n--- Records ---");
                                foreach (string line in lines)
                                {
                                    // Basic parsing logic from Exercise 3
                                    string[] parts = line.Split(',');
                                    if (parts.Length == 2)
                                    {
                                        Console.WriteLine($"Name: {parts[0]} | Score: {parts[1]}");
                                    }
                                }
                            }
                        }
                        else
                        {
                            Console.WriteLine("No data file found.");
                        }
                        break;

                    case "3":
                        // --- EXIT ---
                        Console.WriteLine("Goodbye!");
                        running = false;
                        break;

                    default:
                        Console.WriteLine("Invalid option. Please try again.");
                        break;
                }
            }
        }
    }
}
