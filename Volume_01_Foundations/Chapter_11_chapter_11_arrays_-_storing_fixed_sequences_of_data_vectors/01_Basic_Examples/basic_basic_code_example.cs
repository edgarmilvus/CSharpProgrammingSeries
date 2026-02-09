
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

# Source File: basic_basic_code_example.cs
# Description: Basic Code Example
# ==========================================

using System;

class Program
{
    static void Main()
    {
        // 1. Declare an array to hold 5 integer scores.
        // We use 'int[]' to specify an array of integers.
        // The number '5' sets the fixed size of the array.
        int[] scores = new int[5];

        // 2. Assign individual scores using the array index.
        // Array indices start at 0 and go up to (size - 1).
        // This is like putting numbers into specific numbered boxes.
        scores[0] = 85;
        scores[1] = 92;
        scores[2] = 78;
        scores[3] = 95;
        scores[4] = 88;

        // 3. Calculate the total score.
        // We use a 'for' loop to visit every index from 0 to 4.
        // We add the score at the current index to our running total.
        int total = 0;
        for (int i = 0; i < 5; i++)
        {
            total = total + scores[i];
        }

        // 4. Calculate the average.
        // We divide the total by the number of items (5).
        // We cast the total to a double to ensure we get a decimal result.
        double average = (double)total / 5;

        // 5. Display the results.
        Console.WriteLine($"Total Score: {total}");
        Console.WriteLine($"Class Average: {average}");
    }
}
