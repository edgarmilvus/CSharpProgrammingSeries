
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

# Source File: solution_exercise_3.cs
# Description: Solution for Exercise 3
# ==========================================

using System;

class Program
{
    static void Main()
    {
        // 1. Declare a 2D array (3 rows, 3 columns)
        char[,] board = new char[3, 3];

        // 2. Initialize values
        // Row 0
        board[0, 0] = 'X'; board[0, 1] = 'O'; board[0, 2] = 'X';
        // Row 1
        board[1, 0] = 'O'; board[1, 1] = 'X'; board[1, 2] = 'O';
        // Row 2
        board[2, 0] = 'X'; board[2, 1] = 'O'; board[2, 2] = 'X';

        Console.WriteLine("Game Board:");

        // 3. Iterate using foreach
        int elementCount = 0;
        
        foreach (char cell in board)
        {
            // 4. Print the character
            Console.Write(cell + " ");
            
            elementCount++;

            // 5. Logic to detect end of row
            // Since the width is 3, every 3rd element completes a row.
            if (elementCount % 3 == 0)
            {
                Console.WriteLine(); // Move to the next line
            }
        }
    }
}
