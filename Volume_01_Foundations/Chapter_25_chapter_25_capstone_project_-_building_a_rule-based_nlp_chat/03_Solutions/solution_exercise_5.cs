
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

public static class TestHarness
{
    // Ch 13: Defining a Static Method
    public static void RunStrategyTests()
    {
        Console.WriteLine("--- Starting Strategy Tests ---");

        // 1. Instantiate the strategy to test
        // Ch 16: Creating an instance with 'new'
        IResponseStrategy strategy = new GreetingStrategy();

        // 2. Define test cases
        // Ch 20: List<T> to hold strings
        List<string> testInputs = new List<string> 
        { 
            "hello",       // Should be True
            "goodbye",     // Should be False
            "hi there",    // Should be True
            "weather",     // Should be False
            "start"        // Should be True
        };

        // 3. Loop and test
        // Ch 12: foreach loop
        foreach (string input in testInputs)
        {
            // Call the method under test
            bool result = strategy.CanHandle(input);
            
            // Ch 1: Output results
            Console.WriteLine($"Input: '{input}' -> Can Handle: {result}");
        }

        Console.WriteLine("--- Tests Complete ---");
    }
}

// To run this, you would add this to your Main method:
// TestHarness.RunStrategyTests();
