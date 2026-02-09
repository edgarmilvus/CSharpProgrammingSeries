
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

public class Student
{
    // Public properties
    public string Name { get; set; }
    public string StudentId { get; private set; }

    // Private fields for internal state management
    private double[] _grades;
    private int _gradeCount; // Tracks the number of grades added

    // Constructor
    public Student(string name, string studentId, int maxGrades)
    {
        Name = name;
        StudentId = studentId;
        
        // Initialize the array with a fixed size
        _grades = new double[maxGrades];
        _gradeCount = 0;
    }

    // Method to add a grade with validation
    public void AddGrade(double grade)
    {
        // 1. Validate grade range (0.0 to 100.0)
        if (grade < 0.0 || grade > 100.0)
        {
            Console.WriteLine($"Error: Grade {grade} is invalid. Must be between 0 and 100.");
            return; // Exit method early
        }

        // 2. Check if array is full
        if (_gradeCount >= _grades.Length)
        {
            Console.WriteLine($"Error: Cannot add grade. Maximum capacity ({_grades.Length}) reached.");
            return; // Exit method early
        }

        // 3. Add grade to the array and increment counter
        _grades[_gradeCount] = grade;
        _gradeCount++;
        Console.WriteLine($"Added grade: {grade}");
    }

    // Property to calculate GPA dynamically
    public double Gpa
    {
        get
        {
            // Guard clause: Prevent division by zero
            if (_gradeCount == 0)
            {
                return 0.0;
            }

            // Calculate sum of only the valid grades added
            double sum = 0.0;
            for (int i = 0; i < _gradeCount; i++)
            {
                sum += _grades[i];
            }

            // Return the average
            return sum / _gradeCount;
        }
    }
}

public class Program
{
    public static void Main()
    {
        // Create a student with capacity for 3 grades
        Student student = new Student("Alice", "S12345", 3);

        Console.WriteLine($"Student: {student.Name}, ID: {student.StudentId}");
        Console.WriteLine($"Initial GPA: {student.Gpa:F2}"); // Formatted to 2 decimal places

        // Add valid grades
        student.AddGrade(85.5);
        student.AddGrade(92.0);
        student.AddGrade(78.5);

        // Try to add an invalid grade (out of range)
        student.AddGrade(105.0); 

        // Try to add a grade to a full array
        student.AddGrade(88.0); 

        // Display final calculated GPA
        Console.WriteLine($"Final GPA: {student.Gpa:F2}");

        // Verify StudentId is read-only (uncommenting the next line causes an error)
        // student.StudentId = "S99999";
    }
}
