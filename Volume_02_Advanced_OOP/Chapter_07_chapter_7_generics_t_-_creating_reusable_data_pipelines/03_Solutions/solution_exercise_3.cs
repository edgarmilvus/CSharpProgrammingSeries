
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

// Source File: solution_exercise_3.cs
// Description: Solution for Exercise 3
// ==========================================

using System;
using System.Collections.Generic;

// 1. Generic Interface
public interface Collector<T>
{
    void Collect(T item);
    void PrintContents();
}

// 2. Concrete Implementation
public class ObjectCollector : Collector<object>
{
    private List<object> _storage = new List<object>();

    public void Collect(object item)
    {
        _storage.Add(item);
        Console.WriteLine($"Collected object: {item}");
    }

    public void PrintContents()
    {
        Console.WriteLine("Collector Contents:");
        foreach (var item in _storage)
        {
            Console.WriteLine($" - {item}");
        }
    }
}

public class Exercise3Runner
{
    // 3. Variance Challenge Method
    // In Java: void drainToCollector(List<Double> source, Collector<? super Double> dest)
    // In C#: We accept 'Collector<object>' because 'object' is a supertype of 'double'.
    // This allows us to pass an ObjectCollector (which implements Collector<object>)
    // and add double values to it.
    
    public static void DrainToCollector(List<double> source, Collector<object> dest)
    {
        foreach (double d in source)
        {
            dest.Collect(d);
        }
    }

    public static void Run()
    {
        var doubles = new List<double> { 1.1, 2.2, 3.3 };
        var objectCollector = new ObjectCollector();

        // We pass an ObjectCollector where a Collector<object> is expected
        DrainToCollector(doubles, objectCollector);

        objectCollector.PrintContents();
    }
}
