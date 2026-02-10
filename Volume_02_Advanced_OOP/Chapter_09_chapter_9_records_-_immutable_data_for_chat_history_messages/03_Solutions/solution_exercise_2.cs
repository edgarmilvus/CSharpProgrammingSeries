
/*
#
# These sources are part of the "C# Programming Series" by Edgar Milvus, 
# for additional info, new volumes, link to stores:
# https://github.com/edgarmilvus/CSharpProgrammingSeries
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
*/

// Source File: solution_exercise_2.cs
// Description: Solution for Exercise 2
// ==========================================

using System;
using System.Collections.Generic;

namespace ChatSystem.Core
{
    public static class MessageRouter
    {
        public static string Route(ChatMessage<object> msg)
        {
            // Using a switch expression for declarative logic
            return msg switch
            {
                // Check property pattern first (Role)
                { Role: "System" } => "System command ignored",

                // Check type pattern for Content (List<string>)
                { Content: List<string> } => "Processing Multi-modal content...",

                // Check type pattern for Content (string)
                { Content: string } => "Processing Text content...",

                // Check for null
                { Content: null } => "Empty Message",

                // Fallback
                _ => "Unknown message type"
            };
        }
    }

    public class Exercise2Runner
    {
        public static void Run()
        {
            // Helper to create messages easily
            ChatMessage<object> CreateMsg(string role, object content) 
                => new ChatMessage<object> { Role = role, Content = content, Id = Guid.NewGuid(), Timestamp = DateTime.Now };

            var textMsg = CreateMsg("User", "Hello");
            var multiMsg = CreateMsg("User", new List<string> { "img1.jpg", "img2.jpg" });
            var sysMsg = CreateMsg("System", "Reset");
            var nullMsg = CreateMsg("User", null);

            Console.WriteLine(MessageRouter.Route(textMsg));   // Output: Processing Text content...
            Console.WriteLine(MessageRouter.Route(multiMsg));  // Output: Processing Multi-modal content...
            Console.WriteLine(MessageRouter.Route(sysMsg));    // Output: System command ignored
            Console.WriteLine(MessageRouter.Route(nullMsg));   // Output: Empty Message
        }
    }
}
