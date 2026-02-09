
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

namespace ChatSystem.Core
{
    public class GenericValidator
    {
        // 2. Constraint: T must be a reference type
        public static bool IsValid<T>(ChatMessage<T> message) where T : class
        {
            if (message == null) return false;

            // 4. Pattern matching on the Content property
            return message.Content switch
            {
                string s => !string.IsNullOrWhiteSpace(s),
                List<string> l => l.Count > 0,
                null => false,
                _ => true // Valid for other reference types
            };
        }
    }

    public class Exercise5Runner
    {
        public static void Run()
        {
            // Helper to create messages
            ChatMessage<T> CreateMsg<T>(T content) where T : class 
                => new ChatMessage<T> { Role = "User", Content = content, Id = Guid.NewGuid(), Timestamp = DateTime.Now };

            var validText = CreateMsg("Hello");
            var invalidText = CreateMsg("");
            var validList = CreateMsg(new List<string> { "img.png" });
            var invalidList = CreateMsg(new List<string>());

            Console.WriteLine($"Text Valid: {GenericValidator.IsValid(validText)}"); // True
            Console.WriteLine($"Text Invalid: {GenericValidator.IsValid(invalidText)}"); // False
            Console.WriteLine($"List Valid: {GenericValidator.IsValid(validList)}"); // True
            Console.WriteLine($"List Invalid: {GenericValidator.IsValid(invalidList)}"); // False
        }
    }
}
