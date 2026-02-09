
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

# Source File: solution_exercise_5.cs
# Description: Solution for Exercise 5
# ==========================================

using System;

// The Class Blueprint
public class Student
{
    // 1. Fields (State) - Kept private for encapsulation
    private string _name;
    private int[] _grades;
    private int _gradeCount;

    // 2. Constructor
    public Student(string name)
    {
        _name = name;
        // Fixed size array as per constraints
        _grades = new int[3]; 
        _gradeCount = 0;
    }

    // 3. Instance Methods (Behavior)
    public void AddGrade(int grade)
    {
        // Check if array is full
        if (_gradeCount < _grades.Length)
        {
            _grades[_gradeCount] = grade;
            _gradeCount++;
        }
        else
        {
            Console.WriteLine($"Cannot add more grades for {_name}. Array full.");
        }
    }

    public double CalculateAverage()
    {
        if (_gradeCount == 0) return 0.0;

        int total = 0;
        // Sum grades using a for loop
        for (int i = 0; i < _gradeCount; i++)
        {
            total += _grades[i];
        }
        
        // Cast to double for decimal result
        return (double)total / _gradeCount;
    }

    public void DisplayReport()
    {
        Console.WriteLine($"Student: {_name}, Average: {CalculateAverage()}");
    }
}

public class Program
{
    public static void Main()
    {
        // 4. Instantiate and use the object
        Student student1 = new Student("John Doe");

        student1.AddGrade(85);
        student1.AddGrade(90);
        student1.AddGrade(78);
        
        // Try to add a 4th grade (logic handles this safely)
        student1.AddGrade(95);

        student1.DisplayReport();

        // Demonstrate creating a second independent student
        Student student2 = new Student("Jane Smith");
        student2.AddGrade(100);
        student2.DisplayReport();
    }
}
