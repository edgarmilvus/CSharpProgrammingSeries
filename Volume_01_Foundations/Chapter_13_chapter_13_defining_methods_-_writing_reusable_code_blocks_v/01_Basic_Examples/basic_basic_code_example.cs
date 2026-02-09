
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

namespace MethodBasics
{
    class Program
    {
        // The Main method is the entry point of the program.
        // It controls the flow of execution.
        static void Main(string[] args)
        {
            Console.WriteLine("--- Visitor Check-in Started ---");

            // Call the void method defined below.
            // Notice we don't use '=' or a variable here; we are executing the block of code.
            GreetVisitor();

            Console.WriteLine("Visitor check-in complete.");
        }

        // METHOD DEFINITION:
        // This is a static void method named "GreetVisitor".
        // It takes no parameters (empty parentheses) and returns no value (void).
        static void GreetVisitor()
        {
            // Logic inside the method:
            // We use string interpolation to format the output.
            Console.WriteLine($"Welcome to the building! Please sign in.");
            
            // We can perform other allowed actions here, like a simple calculation.
            int year = 2024;
            Console.WriteLine($"Current year: {year}");
        }
    }
}
