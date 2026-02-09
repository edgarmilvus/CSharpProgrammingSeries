
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

# Source File: solution_exercise_4.cs
# Description: Solution for Exercise 4
# ==========================================

using System;

namespace Chapter8_Exercises
{
    class Program
    {
        static void Main(string[] args)
        {
            string correctPassword = "CSharp101";
            int attempts = 3;
            string inputPassword;

            Console.WriteLine($"You have {attempts} attempts to enter the correct password.");

            // Loop runs as long as attempts remaining is greater than 0
            while (attempts > 0)
            {
                Console.Write($"Attempt {4 - attempts}: Enter password: ");
                inputPassword = Console.ReadLine();

                if (inputPassword == correctPassword)
                {
                    Console.WriteLine("Access Granted!");
                    break; // Exit loop immediately on success
                }
                else
                {
                    attempts--; // Decrement attempts
                    if (attempts > 0)
                    {
                        Console.WriteLine($"Wrong password. You have {attempts} attempts left.");
                    }
                }
            }

            // Check if the loop finished because attempts reached 0
            if (attempts == 0)
            {
                Console.WriteLine("Access Denied. System Locked.");
            }
        }
    }
}
