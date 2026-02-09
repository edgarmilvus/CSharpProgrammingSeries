
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
using System.Collections.Generic;

public class DataUtils
{
    // 1. Generic method with constraint
    // The 'where T : IComparable<T>' constraint ensures T has a CompareTo method.
    public static T FindMax<T>(List<T> collection) where T : IComparable<T>
    {
        if (collection == null || collection.Count == 0)
            throw new ArgumentException("Collection cannot be empty");

        T max = collection[0];
        for (int i = 1; i < collection.Count; i++)
        {
            // CompareTo is available because of the constraint
            if (collection[i].CompareTo(max) > 0)
            {
                max = collection[i];
            }
        }
        return max;
    }

    // 2. Filtering method
    // 'where U : T' ensures U is a subtype of T (or T itself).
    public static List<U> FilterByType<T, U>(List<T> collection) where U : T
    {
        List<U> result = new List<U>();
        foreach (var item in collection)
        {
            if (item is U typedItem)
            {
                result.Add(typedItem);
            }
        }
        return result;
    }
}

// Helper class for testing
public class Score : IComparable<Score>
{
    public int Value { get; set; }
    public int CompareTo(Score other) => this.Value.CompareTo(other.Value);
}

public class Exercise5Runner
{
    public static void Run()
    {
        // Test FindMax
        var scores = new List<Score> 
        { 
            new Score { Value = 10 }, 
            new Score { Value = 50 }, 
            new Score { Value = 25 } 
        };
        Score maxScore = DataUtils.FindMax(scores);
        Console.WriteLine($"Max Score: {maxScore.Value}");

        // Test FilterByType
        var mixedList = new List<object> { 1, "Hello", 2.5, "World", 4 };
        // T is object, U is string
        var strings = DataUtils.FilterByType<object, string>(mixedList);
        Console.WriteLine($"Filtered Strings: {strings.Count}");
        foreach(var s in strings) Console.WriteLine(s);
    }
}
