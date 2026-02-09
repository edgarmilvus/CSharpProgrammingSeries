
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

# Source File: theory_theoretical_foundations_part8.cs
# Description: Theoretical Foundations
# ==========================================

using System;

class Program
{
    static void Main()
    {
        // 1. Integer Variable (Whole Number)
        int userAge = 30;
        
        // 2. Double Variable (Decimal Number)
        double userHeight = 5.9;
        
        // 3. String Variable (Text)
        string userName = "Jordan";
        
        // 4. Boolean Variable (True/False)
        bool isSubscribed = true;

        // Outputting the variables using Console.WriteLine (Ch 1 Concept)
        Console.WriteLine("User Name: " + userName);
        Console.WriteLine("User Age: " + userAge);
        Console.WriteLine("User Height: " + userHeight);
        Console.WriteLine("Subscribed: " + isSubscribed);

        // Using Console.Write to stay on the same line
        Console.Write("Status: ");
        Console.WriteLine(isSubscribed); // Prints "True"
    }
}
