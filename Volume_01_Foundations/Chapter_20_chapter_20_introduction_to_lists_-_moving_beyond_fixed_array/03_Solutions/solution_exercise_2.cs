
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

// Source File: solution_exercise_2.cs
// Description: Solution for Exercise 2
// ==========================================

using System;
using System.Collections.Generic;

public class UndoSimulation
{
    public static void Main()
    {
        List<string> history = new List<string>();
        
        // Use a for loop (Chapter 9) to add 5 items
        for (int i = 1; i <= 5; i++)
        {
            string action = $"Action {i}"; // String interpolation (Chapter 3)
            history.Add(action);
            Console.WriteLine($"Added: {action}");
        }
        
        Console.WriteLine("\n--- History Full ---");
        
        // To remove the last item, we need its index.
        // The Count property gives the total number of items.
        // Indices start at 0, so the last index is Count - 1.
        int lastIndex = history.Count - 1;
        
        // Remove the last element using .RemoveAt()
        history.RemoveAt(lastIndex);
        
        Console.WriteLine("\n--- Last Action Undone ---");
        
        // Display the remaining history
        foreach (string action in history)
        {
            Console.WriteLine(action);
        }
    }
}
