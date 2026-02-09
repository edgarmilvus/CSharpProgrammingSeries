
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

# Source File: theory_theoretical_foundations_part16.cs
# Description: Theoretical Foundations
# ==========================================

using System;

class ChatbotFilter
{
    static void Main()
    {
        // Simulated AI response
        string aiResponse = "I cannot help with that request.";
        bool isSafe = true; // Assume safe until proven otherwise

        // Check 1: Is the response empty?
        if (aiResponse == "")
        {
            isSafe = false;
        }

        // Check 2: Does it contain a banned word? (Simple check)
        // Note: This is a basic example. Real AI filters are much more complex.
        if (aiResponse.Contains("banned_word")) 
        {
            // We are simulating a method call here. 
            // In a real app, this would be a string method.
            isSafe = false;
        }

        // Decision: Show response or error
        if (isSafe)
        {
            Console.WriteLine($"Bot: {aiResponse}");
        }
        else
        {
            Console.WriteLine("Bot: I am sorry, I cannot generate a response to that.");
        }
    }
}
