
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

// Define the class that tracks actions
public class ActionTracker
{
    // Static field: Shared across the entire application, not tied to a specific object instance.
    // Initialized to 0.
    public static int TotalActions = 0;

    // Static method: Can be called directly on the class name.
    public static void LogAction()
    {
        // Increment the shared static field
        TotalActions++;
        // Print the current count
        Console.WriteLine($"Action logged. Total actions: {TotalActions}");
    }
}

public class Program
{
    public static void Main(string[] args)
    {
        // We do not use 'new ActionTracker()'. We call the method directly on the class.
        ActionTracker.LogAction();
        ActionTracker.LogAction();
        ActionTracker.LogAction();

        // Access the static field directly to print the final result
        Console.WriteLine($"Final total from Main: {ActionTracker.TotalActions}");
    }
}
