
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

# Source File: solution_exercise_2.cs
# Description: Solution for Exercise 2
# ==========================================

using System;

namespace Chapter5_Exercise2
{
    class Program
    {
        static void Main(string[] args)
        {
            // 1. Get the bill amount
            Console.Write("Enter the total bill amount: ");
            string billInput = Console.ReadLine();
            
            // 2. Get the number of people
            Console.Write("How many people are splitting the bill? ");
            string peopleInput = Console.ReadLine();
            
            // 3. Convert types
            // We use Convert.ToDouble for the bill to handle decimals (money)
            double totalBill = Convert.ToDouble(billInput);
            
            // We use int.Parse for the count of people (must be a whole number)
            int numberOfPeople = int.Parse(peopleInput);
            
            // 4. Calculate share
            // Dividing a double by an int results in a double
            double share = totalBill / numberOfPeople;
            
            // 5. Output with currency formatting (C)
            Console.WriteLine($"Each person pays: {share:C}");
        }
    }
}
