
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

// Source File: theory_theoretical_foundations_part12.cs
// Description: Theoretical Foundations
// ==========================================

using System;

class Program
{
    static void Main()
    {
        // 1. Store data: Scores for words found in an email
        // "Free" = 5 points, "Money" = 3 points, "Urgent" = 4 points
        int[] wordScores = { 5, 3, 4, 2, 5 }; 

        // 2. Process: Sum the scores (using a for loop)
        int totalScore = 0;
        
        for (int i = 0; i < wordScores.Length; i++)
        {
            totalScore = totalScore + wordScores[i];
        }

        // 3. Decision: Logic from Chapter 6
        int spamThreshold = 10;

        if (totalScore > spamThreshold)
        {
            Console.WriteLine($"Spam detected! Score: {totalScore}");
        }
        else
        {
            Console.WriteLine($"Email is safe. Score: {totalScore}");
        }
    }
}
