
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

# Source File: solution_exercise_1.cs
# Description: Solution for Exercise 1
# ==========================================

using System;
using System.Threading.Tasks;

// 1. Define a custom exception class
public class ForbiddenKeywordException : Exception
{
    public string Keyword { get; }

    public ForbiddenKeywordException(string keyword)
        : base($"Forbidden keyword '{keyword}' detected.")
    {
        Keyword = keyword;
    }
}

public class PromptValidator
{
    // 2. & 3. Implement async method to check for "malicious"
    public async Task<bool> ValidatePromptAsync(string prompt)
    {
        // Simulate network latency
        await Task.Delay(100);

        if (prompt.Contains("malicious", StringComparison.OrdinalIgnoreCase))
        {
            // 3. Throw specific custom exception
            throw new ForbiddenKeywordException("malicious");
        }

        return true;
    }
}

public class Program
{
    public static async Task Main()
    {
        var validator = new PromptValidator();
        string prompt = "This is a malicious prompt.";

        Console.WriteLine("--- Testing Specific Exception Catch ---");
        try
        {
            await validator.ValidatePromptAsync(prompt);
        }
        // 5. Catch specifically ForbiddenKeywordException
        catch (ForbiddenKeywordException ex)
        {
            // 5. Log the offending keyword using the custom property
            Console.WriteLine($"Caught specific exception: {ex.Message}");
            Console.WriteLine($"Offending keyword: {ex.Keyword}");
        }

        Console.WriteLine("\n--- Testing Generic Exception Catch ---");
        try
        {
            await validator.ValidatePromptAsync(prompt);
        }
        // 6. Demonstrate catching generic Exception
        catch (Exception ex)
        {
            Console.WriteLine($"Caught generic exception: {ex.GetType().Name}");
            // The specific property 'Keyword' is not accessible here without casting
            if (ex is ForbiddenKeywordException fkEx)
            {
                Console.WriteLine($"Keyword (accessed via casting): {fkEx.Keyword}");
            }
            else
            {
                Console.WriteLine("Cannot access specific keyword details directly.");
            }
        }
    }
}
