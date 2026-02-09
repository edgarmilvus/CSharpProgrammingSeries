
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

# Source File: theory_theoretical_foundations_part7.cs
# Description: Theoretical Foundations
# ==========================================

using System;
using System.IO;
using System.Collections.Generic;

public class Student
{
    public string Name { get; set; }
    public int Score { get; set; }
}

public class Program
{
    public static void Main()
    {
        string filePath = "students.txt";
        List<Student> studentRoster = new List<Student>();

        // Ensure the file exists for this example
        if (!File.Exists(filePath))
        {
            // Create a dummy file for demonstration
            File.WriteAllText(filePath, "Alice,85\nBob,92\nCharlie,78");
        }

        using (StreamReader reader = new StreamReader(filePath))
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                // 1. Split the line by the comma
                // Chapter 23: String.Split
                string[] parts = line.Split(',');

                // 2. Validate data (Basic logic from Chapter 6)
                if (parts.Length == 2)
                {
                    string name = parts[0];
                    
                    // 3. Parse the string score to an integer
                    // Chapter 5: Type Conversion (int.Parse)
                    int score = int.Parse(parts[1]);

                    // 4. Create the object and add to list
                    // Chapter 16: Class definition, Chapter 20: List<T>
                    Student s = new Student();
                    s.Name = name;
                    s.Score = score;
                    
                    studentRoster.Add(s);
                }
            }
        }

        // Verify the data loaded
        foreach (Student s in studentRoster)
        {
            Console.WriteLine($"Student: {s.Name}, Score: {s.Score}");
        }
    }
}
