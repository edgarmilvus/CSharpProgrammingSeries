
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
using System.Collections.Generic;

public class HighScoreChallenge
{
    public static void Main()
    {
        List<int> scores = new List<int>();
        string response;
        
        // Use a do-while loop (Chapter 8) to ensure the user enters at least one score
        do
        {
            Console.Write("Enter a score: ");
            // Convert string input to integer (Chapter 5)
            int score = int.Parse(Console.ReadLine());
            scores.Add(score);
            
            Console.Write("Add another score? (yes/no): ");
            response = Console.ReadLine();
            
        } while (response == "yes");
        
        // Safety check: Ensure the list isn't empty to avoid errors
        if (scores.Count == 0)
        {
            Console.WriteLine("No scores entered.");
            return;
        }
        
        // Initialize variables for calculation
        int sum = 0;
        int max = scores[0]; // Assume the first score is the highest initially
        
        // Iterate through the list to calculate sum and find max
        foreach (int score in scores)
        {
            sum += score; // Add to total sum
            
            // Check if current score is greater than our current max
            if (score > max)
            {
                max = score; // Update max
            }
        }
        
        // Calculate average (Cast to double to keep decimal precision)
        double average = (double)sum / scores.Count;
        
        Console.WriteLine($"\nHighest Score: {max}");
        Console.WriteLine($"Average Score: {average:F2}"); // F2 formats to 2 decimal places
    }
}
