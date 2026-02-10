
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
using System.Linq;

public class Review
{
    public string Text { get; set; }
}

public class DataCleaning
{
    public static void Main()
    {
        var rawReviews = new List<Review>
        {
            new Review { Text = "  Great product!  " },
            null, // Edge case: null object
            new Review { Text = null }, // Edge case: null property
            new Review { Text = "" },
            new Review { Text = "  Terrible experience.  " }
        };

        // Pure Functional Pipeline
        var cleanReviews = rawReviews
            .Where(r => r != null)                  // 1. Filter null objects
            .Select(r => r.Text)                    // 2. Extract text
            .Where(text => !string.IsNullOrWhiteSpace(text)) // 3. Filter empty/whitespace
            .Select(text => text.Trim().ToUpper())  // 4. Normalize
            .ToList();                              // 5. Materialize results

        // Output
        Console.WriteLine("Cleaned Reviews:");
        foreach (var review in cleanReviews)
        {
            Console.WriteLine($"'{review}'");
        }
    }
}
