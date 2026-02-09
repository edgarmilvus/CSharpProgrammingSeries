
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

# Source File: solution_exercise_3.cs
# Description: Solution for Exercise 3
# ==========================================

using System;
using System.Collections.Generic;

namespace ChatSystem.Core
{
    // 1. & 2. Define nested records
    public record SafetyReport(bool IsSafe, string Reason);
    public record TokenUsage(string Model, int InputTokens, int OutputTokens);

    // 3. Main record
    public record ChatLogEntry
    {
        public ChatMessage<string> Message { get; init; }
        public SafetyReport Safety { get; init; }
        public IEnumerable<TokenUsage> Usage { get; init; }
    }

    public static class LogFactory
    {
        // 4. Factory method
        public static ChatLogEntry CreateSafeEntry(string role, string content)
        {
            return new ChatLogEntry
            {
                Message = new ChatMessage<string>
                {
                    Id = Guid.NewGuid(),
                    Role = role,
                    Content = content,
                    Timestamp = DateTime.UtcNow
                },
                Safety = new SafetyReport(true, "Passed automated checks"),
                // Using collection expression syntax
                Usage = [ new TokenUsage("GPT-4-Turbo", 10, 50) ]
            };
        }
    }

    public class Exercise3Runner
    {
        public static void Run()
        {
            var entry = LogFactory.CreateSafeEntry("User", "Explain quantum physics.");
            
            Console.WriteLine($"Role: {entry.Message.Role}");
            Console.WriteLine($"Safe: {entry.Safety.IsSafe}");
            
            // Iterating the collection
            foreach(var u in entry.Usage)
            {
                Console.WriteLine($"Model: {u.Model}, Output: {u.OutputTokens}");
            }
        }
    }
}
