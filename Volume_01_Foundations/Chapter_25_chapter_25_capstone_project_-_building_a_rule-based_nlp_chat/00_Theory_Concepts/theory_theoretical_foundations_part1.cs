
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

# Source File: theory_theoretical_foundations_part1.cs
# Description: Theoretical Foundations
# ==========================================

using System;
using System.Collections.Generic; // Required for List<T> (Chapter 20)

public class ResponseRule
{
    // The pattern we look for in the user's input (Chapter 17: Fields/Properties)
    public string Pattern { get; set; }
    
    // The response to return if the pattern matches
    public string Response { get; set; }

    // Constructor to initialize the rule (Chapter 18: Constructors)
    public ResponseRule(string pattern, string response)
    {
        Pattern = pattern;
        Response = response;
    }

    // A method to check if this rule matches the input
    // We use String.Contains (Chapter 23: String methods) for simplicity
    public bool IsMatch(string userInput)
    {
        // Convert both to lowercase to make matching case-insensitive
        // (We will simulate this using logic, as ToLower() is slightly advanced, 
        // but we can compare manually or assume case sensitivity for now. 
        // For this example, we will use basic equality or substring checks).
        
        // Using String.Contains to see if the pattern exists in the input
        // Note: Contains is case-sensitive. We will handle casing in the engine.
        return userInput.Contains(Pattern);
    }
}
