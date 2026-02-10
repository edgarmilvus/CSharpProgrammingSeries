
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

namespace GradeAnalyzer
{
    class Program
    {
        static void Main(string[] args)
        {
            // ==========================================
            // PART 1: DATA SIMULATION (The "Database")
            // ==========================================
            // Since we cannot use Arrays (forbidden in Book 1), 
            // we simulate a list of student grades using individual variables.
            // In a real application, this data would come from a database or user input.
            
            int student01_Grade = 85;
            int student02_Grade = 92;
            int student03_Grade = 78;
            int student04_Grade = 64;
            int student05_Grade = 88;
            int student06_Grade = 55;
            int student07_Grade = 99;
            int student08_Grade = 72;
            int student09_Grade = 81;
            int student10_Grade = 45;

            // Configuration variables
            int passingThreshold = 60;
            int totalStudents = 10;

            // ==========================================
            // PART 2: ANALYSIS VARIABLES
            // ==========================================
            // We need variables to store the results of our loop calculations.
            
            double sumOfGrades = 0.0; // Must be double to calculate decimal average later
            int highestGrade = 0;     // We will update this as we loop
            int lowestGrade = 100;    // We assume 100 is the max possible score initially
            int passCount = 0;        // Counter for students who passed
            int failCount = 0;        // Counter for students who failed

            // ==========================================
            // PART 3: THE COUNTED LOOP (The Core Logic)
            // ==========================================
            // We use a 'for' loop to iterate through the 10 students.
            // The loop counter 'i' represents the student number (1 to 10).
            
            for (int i = 1; i <= totalStudents; i++)
            {
                // Temporary variable to hold the current student's grade
                int currentGrade = 0;

                // Inside the loop, we must select the correct variable based on 'i'.
                // This replaces the array indexing (arr[i]) we would use in future chapters.
                if (i == 1)
                {
                    currentGrade = student01_Grade;
                }
                else if (i == 2)
                {
                    currentGrade = student02_Grade;
                }
                else if (i == 3)
                {
                    currentGrade = student03_Grade;
                }
                else if (i == 4)
                {
                    currentGrade = student04_Grade;
                }
                else if (i == 5)
                {
                    currentGrade = student05_Grade;
                }
                else if (i == 6)
                {
                    currentGrade = student06_Grade;
                }
                else if (i == 7)
                {
                    currentGrade = student07_Grade;
                }
                else if (i == 8)
                {
                    currentGrade = student08_Grade;
                }
                else if (i == 9)
                {
                    currentGrade = student09_Grade;
                }
                else if (i == 10)
                {
                    currentGrade = student10_Grade;
                }

                // -------------------------------------------------
                // LOGIC STEP 1: Accumulate the Sum
                // -------------------------------------------------
                // We add the current grade to our running total.
                sumOfGrades += currentGrade;

                // -------------------------------------------------
                // LOGIC STEP 2: Find the Highest Grade
                // -------------------------------------------------
                // Check if the current grade is greater than the highest seen so far.
                if (currentGrade > highestGrade)
                {
                    highestGrade = currentGrade;
                }

                // -------------------------------------------------
                // LOGIC STEP 3: Find the Lowest Grade
                // -------------------------------------------------
                // Check if the current grade is lower than the lowest seen so far.
                if (currentGrade < lowestGrade)
                {
                    lowestGrade = currentGrade;
                }

                // -------------------------------------------------
                // LOGIC STEP 4: Count Passes and Fails
                // -------------------------------------------------
                // We use the logical operator '>=' (greater than or equal to).
                if (currentGrade >= passingThreshold)
                {
                    passCount++; // Increment pass counter
                }
                else
                {
                    failCount++; // Increment fail counter
                }
            }

            // ==========================================
            // PART 4: CALCULATING THE AVERAGE
            // ==========================================
            // We must convert the integer total to a double before division
            // to avoid "integer division" (where 85 / 100 becomes 0).
            
            double averageGrade = sumOfGrades / totalStudents;

            // ==========================================
            // PART 5: DISPLAYING RESULTS
            // ==========================================
            // Formatting the output to look professional.
            
            Console.WriteLine("========================================");
            Console.WriteLine("   GRADE ANALYZER 9000 - REPORT   ");
            Console.WriteLine("========================================");
            Console.WriteLine($"Total Students Analyzed: {totalStudents}");
            Console.WriteLine(""); // Empty line for spacing
            
            Console.WriteLine("--- Statistical Summary ---");
            Console.WriteLine($"Highest Grade: {highestGrade}");
            Console.WriteLine($"Lowest Grade:  {lowestGrade}");
            Console.WriteLine($"Average Grade: {averageGrade:F2}"); // F2 formats to 2 decimal places
            Console.WriteLine("");
            
            Console.WriteLine("--- Performance Breakdown ---");
            Console.WriteLine($"Passed (>= {passingThreshold}): {passCount}");
            Console.WriteLine($"Failed (< {passingThreshold}):  {failCount}");
            Console.WriteLine("========================================");
        }
    }
}
