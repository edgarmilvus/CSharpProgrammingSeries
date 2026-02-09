
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
        // 1. Variable Declaration and Assignment
        // We declare variables to store the user's name and age.
        // Allowed types: string, int, double, bool.
        string name = "Alex";
        int age = 25;

        // 2. String Concatenation (Traditional Method)
        // We use the '+' operator to join strings and variables.
        // Note: We must manually add spaces.
        Console.WriteLine("Hello, " + name + "! You are " + age + " years old.");

        // 3. String Interpolation (Modern Method)
        // We use the '$' prefix and curly braces {} to embed variables directly.
        // This is often easier to read and write.
        Console.WriteLine($"Hello, {name}! You are {age} years old.");

        // 4. Handling Special Characters (Escape Chars)
        // We use the backslash (\) to include quotes inside a string.
        // This prints: She said, "Hello!"
        Console.WriteLine("She said, \"Hello!\"");
        
        // 5. Combining Interpolation and Escape Chars
        // We can use both features together.
        Console.WriteLine($"She said, \"Hello, {name}!\"");
    }
}
