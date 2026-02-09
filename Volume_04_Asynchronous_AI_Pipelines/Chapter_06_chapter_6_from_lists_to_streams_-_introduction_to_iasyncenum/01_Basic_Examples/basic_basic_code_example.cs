
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
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public class Program
{
    public static async Task Main(string[] args)
    {
        Console.WriteLine("User: What is the capital of France?");
        Console.Write("AI: ");

        // 1. Consume the async stream
        // The 'await foreach' loop retrieves items one by one as they become available.
        await foreach (var token in GetStreamingResponseAsync("France"))
        {
            Console.Write(token);
            // Simulate human-like typing speed
            await Task.Delay(100); 
        }

        Console.WriteLine("\n\n[End of Stream]");
    }

    /// <summary>
    /// Simulates an AI model generating a response token by token.
    /// </summary>
    /// <param name="topic">The topic to generate text about.</param>
    /// <returns>An asynchronous stream of strings (tokens).</returns>
    public static async IAsyncEnumerable<string> GetStreamingResponseAsync(string topic)
    {
        // Simulated response data
        string[] tokens = { "The", " capital", " of", " ", topic, " is", " Paris." };

        foreach (string token in tokens)
        {
            // 2. Yield the current token immediately
            // This passes the data to the caller without blocking the loop.
            yield return token;

            // 3. Simulate asynchronous work (e.g., network latency or LLM inference time)
            // In a real scenario, this delay represents waiting for the next token 
            // from the API.
            await Task.Delay(200); 
        }
    }
}
