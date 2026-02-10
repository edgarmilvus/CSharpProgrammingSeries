
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

// Source File: project_advanced_application_script.cs
// Description: Advanced Application Script
// ==========================================

using System;

class Program
{
    // Main entry point of the program
    static void Main(string[] args)
    {
        // We define how many students we are processing.
        // We use an array to store the names for later display.
        // Note: We are restricted to a fixed size for this example.
        string[] studentNames = new string[3];
        double[] studentAverages = new double[3];

        // We use a for loop to iterate through each student slot.
        for (int i = 0; i < 3; i++)
        {
            Console.WriteLine($"--- Processing Student {i + 1} ---");

            // Step 1: Get the student's name
            string currentName = GetStudentName();
            studentNames[i] = currentName;

            // Step 2: Get the three test scores
            double score1 = GetScore("Test 1");
            double score2 = GetScore("Test 2");
            double score3 = GetScore("Test 3");

            // Step 3: Calculate the average score
            double average = CalculateAverage(score1, score2, score3);
            studentAverages[i] = average;

            // Step 4: Display the immediate result
            DisplayResult(currentName, average);
        }

        // Step 5: Display a summary of all students
        DisplayClassSummary(studentNames, studentAverages);
    }

    // ---------------------------------------------------------
    // METHOD 1: Get Student Name
    // Purpose: Prompts the user for a name and returns it.
    // ---------------------------------------------------------
    static string GetStudentName()
    {
        Console.Write("Enter student name: ");
        string name = Console.ReadLine();
        
        // Basic validation to ensure name isn't empty
        while (name.Length == 0)
        {
            Console.WriteLine("Name cannot be empty. Please try again.");
            Console.Write("Enter student name: ");
            name = Console.ReadLine();
        }
        
        return name;
    }

    // ---------------------------------------------------------
    // METHOD 2: Get Score
    // Purpose: Prompts for a specific test score and ensures it is valid.
    // Parameters: testLabel (e.g., "Test 1")
    // ---------------------------------------------------------
    static double GetScore(string testLabel)
    {
        double score;
        while (true) // Infinite loop until valid input is received
        {
            Console.Write($"Enter score for {testLabel}: ");
            string input = Console.ReadLine();

            // Attempt to convert string to double
            // We use double.Parse because we expect decimal numbers
            if (double.TryParse(input, out score))
            {
                // Check if score is within valid range (0-100)
                if (score >= 0 && score <= 100)
                {
                    return score; // Valid input, exit loop
                }
                else
                {
                    Console.WriteLine("Score must be between 0 and 100.");
                }
            }
            else
            {
                Console.WriteLine("Invalid input. Please enter a number.");
            }
        }
    }

    // ---------------------------------------------------------
    // METHOD 3: Calculate Average
    // Purpose: Takes three scores and returns the mathematical average.
    // ---------------------------------------------------------
    static double CalculateAverage(double s1, double s2, double s3)
    {
        // We use basic arithmetic operators
        double sum = s1 + s2 + s3;
        double average = sum / 3.0;
        return average;
    }

    // ---------------------------------------------------------
    // METHOD 4: Display Result
    // Purpose: Void method to print the grade report for a single student.
    // ---------------------------------------------------------
    static void DisplayResult(string name, double average)
    {
        Console.WriteLine($"\nResults for {name}:");
        Console.WriteLine($"Average Score: {average:F2}"); // Format to 2 decimal places

        // Determine Letter Grade using If/Else logic
        if (average >= 90)
        {
            Console.WriteLine("Letter Grade: A");
        }
        else if (average >= 80)
        {
            Console.WriteLine("Letter Grade: B");
        }
        else if (average >= 70)
        {
            Console.WriteLine("Letter Grade: C");
        }
        else if (average >= 60)
        {
            Console.WriteLine("Letter Grade: D");
        }
        else
        {
            Console.WriteLine("Letter Grade: F");
        }
        Console.WriteLine("-------------------------");
    }

    // ---------------------------------------------------------
    // METHOD 5: Display Class Summary
    // Purpose: Prints a final table of all students and their grades.
    // ---------------------------------------------------------
    static void DisplayClassSummary(string[] names, double[] averages)
    {
        Console.WriteLine("\n*** CLASS SUMMARY REPORT ***");
        
        // We use a for loop to iterate through the arrays
        for (int i = 0; i < names.Length; i++)
        {
            // We use a switch expression (allowed in Ch 7) to map average to letter
            string letterGrade = averages[i] >= 90 ? "A" :
                                 averages[i] >= 80 ? "B" :
                                 averages[i] >= 70 ? "C" :
                                 averages[i] >= 60 ? "D" : "F";

            // Formatting the output in columns
            Console.WriteLine($"Student: {names[i],-15} | Avg: {averages[i],-6:F2} | Grade: {letterGrade}");
        }
    }
}
