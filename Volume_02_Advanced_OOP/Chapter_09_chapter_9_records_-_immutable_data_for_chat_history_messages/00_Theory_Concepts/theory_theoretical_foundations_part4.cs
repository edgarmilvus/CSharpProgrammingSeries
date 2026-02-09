
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

# Source File: theory_theoretical_foundations_part4.cs
# Description: Theoretical Foundations
# ==========================================

public record UserMessage(string Text, string UserId) : ImmutableMessage;
public record SystemMessage(string Instruction) : ImmutableMessage;
public record ImageMessage(byte[] ImageData, string Caption) : ImmutableMessage;

public void RouteMessage(ImmutableMessage msg)
{
    // Pattern matching on the record type and properties
    switch (msg)
    {
        case UserMessage u when u.Text.Length > 1000:
            Console.WriteLine("Routing to LongContextProcessor");
            break;
        
        case UserMessage u:
            Console.WriteLine("Routing to StandardChatProcessor");
            break;
            
        case ImageMessage img:
            Console.WriteLine("Routing to VisionModelProcessor");
            break;
            
        case SystemMessage sys:
            Console.WriteLine("Ignoring system instruction in history");
            break;
            
        default:
            Console.WriteLine("Unknown message type");
            break;
    }
}
