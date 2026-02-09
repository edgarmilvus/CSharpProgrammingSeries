
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

namespace Exercise4
{
    class Program
    {
        static void Main(string[] args)
        {
            // Inputs from the previous example
            Console.Write("Enter Role (Admin, Editor, Viewer): ");
            string role = Console.ReadLine();

            Console.Write("Enter Status (Active, Inactive): ");
            string status = Console.ReadLine();

            // New Security Requirement
            bool isSuspicious = true; // Set to true to test the block

            // LOGIC FLOW:
            // 1. Check the global security flag (isSuspicious).
            // 2. If true, deny immediately.
            // 3. If false, evaluate the role/status switch.
            
            string accessMessage;

            if (!isSuspicious) // "!" means "NOT". If isSuspicious is false, this is true.
            {
                // Original logic from the book example
                accessMessage = role switch
                {
                    "Admin" => status == "Active" ? "Full Access" : "Read-Only Access",
                    "Editor" => status == "Active" ? "Edit Access" : "No Access",
                    "Viewer" => status == "Active" ? "View Only" : "No Access",
                    _ => "Unknown Role"
                };
            }
            else
            {
                accessMessage = "System Locked: Suspicious Activity Detected";
            }

            Console.WriteLine($"Result: {accessMessage}");
        }
    }
}
