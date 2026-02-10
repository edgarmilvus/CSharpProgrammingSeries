
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

// Source File: solution_exercise_1.cs
// Description: Solution for Exercise 1
// ==========================================

using System;
using System.Runtime.InteropServices;

public struct DataPoint
{
    public double Value;
    public DateTime Timestamp;

    public DataPoint(double val)
    {
        Value = val;
        Timestamp = DateTime.Now;
    }
}

public class SystemMonitor
{
    public void LogData(object data)
    {
        Console.WriteLine($"Logging object of type: {data.GetType().Name}");
        
        // Even though the original type was a struct, 
        // the parameter 'data' is a reference to a boxed object.
        if (data.GetType().IsValueType)
        {
            Console.WriteLine(" -> This is a value type (likely boxed).");
        }
    }
}

public class Program
{
    public static void Main()
    {
        SystemMonitor monitor = new SystemMonitor();
        DataPoint dp = new DataPoint(42.5);

        Console.WriteLine("--- 1. Passing Struct Directly ---");
        // Implicit boxing occurs here. The struct is copied to the heap.
        monitor.LogData(dp);

        Console.WriteLine("\n--- 2. Passing Explicitly Boxed Struct ---");
        // Explicit boxing. We create a reference variable pointing to the heap object.
        object boxedDp = (object)dp;
        monitor.LogData(boxedDp);
        
        // Analysis of references
        Console.WriteLine("\n--- Reference Analysis ---");
        Console.WriteLine("Value types are stored on the Stack (in this scope).");
        Console.WriteLine("Boxed objects are stored on the Heap.");
        Console.WriteLine("Every boxing operation creates a NEW object on the heap.");
    }
}
