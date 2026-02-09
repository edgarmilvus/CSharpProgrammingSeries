
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

class Program
{
    static void Main(string[] args)
    {
        Random random = new Random();
        int secretNumber = random.Next(1, 11);
        int guess = 0;

        // Call method to print introduction
        PrintIntro();

        // Loop continues until the guess matches the secret number
        while (guess != secretNumber)
        {
            // Pass 'guess' by reference to update its value inside the method
            MakeGuess(ref guess);
            
            // Check the result of the guess
            CheckGuess(guess, secretNumber);
        }
    }

    static void PrintIntro()
    {
        Console.WriteLine("I'm thinking of a number between 1 and 10.");
    }

    static void MakeGuess(ref int guess)
    {
        Console.Write("Enter your guess: ");
        string input = Console.ReadLine();
        guess = int.Parse(input);
    }

    static void CheckGuess(int guess, int secretNumber)
    {
        if (guess < secretNumber)
        {
            Console.WriteLine("Too low!");
        }
        else if (guess > secretNumber)
        {
            Console.WriteLine("Too high!");
        }
        else
        {
            Console.WriteLine("You guessed it!");
        }
    }
}
