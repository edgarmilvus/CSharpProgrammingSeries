
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
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace PolymorphismVsStructs
{
    // --- Class-Based Polymorphism ---

    public abstract class AIToken
    {
        public int TokenId { get; set; }
        public abstract void Process();
    }

    public class TextToken : AIToken
    {
        public string Text { get; set; }
        public override void Process() { /* Simulate text processing */ var len = Text.Length; }
    }

    public class EmbeddingToken : AIToken
    {
        public float[] Embedding { get; set; }
        public override void Process() { /* Simulate embedding normalization */ var sum = Embedding[0] + Embedding[1]; }
    }

    // --- Struct-Based Union (Simulated Polymorphism) ---

    public enum TokenType { Text, Embedding }

    // Explicit layout allows overlapping memory (union behavior)
    [StructLayout(LayoutKind.Explicit)]
    public struct HybridToken
    {
        [FieldOffset(0)] public TokenType Type;
        
        // String reference (8 bytes on 64-bit)
        [FieldOffset(8)] public int TextId; // Using ID instead of string to keep struct pure value type for this demo
        // Note: Storing strings in a union struct is tricky because strings are immutable reference types.
        // For this exercise, we simulate data with value types to show memory packing.
        
        [FieldOffset(8)] public float EmbeddingX;
        [FieldOffset(12)] public float EmbeddingY;
        [FieldOffset(16)] public float EmbeddingZ;

        // To strictly fit the exercise requirement of "data layout", we assume we are processing
        // simple data. If we used strings, we would store references, but the struct size must be fixed.
        // Let's adjust the struct to hold data relevant to the scenario.
        
        public HybridToken(TokenType type)
        {
            // Initialize all fields to zero
            Type = type;
            TextId = 0;
            EmbeddingX = 0; EmbeddingY = 0; EmbeddingZ = 0;
        }
    }

    public class ComparisonDemo
    {
        const int Count = 50_000;

        public static void RunClassBased()
        {
            var list = new List<AIToken>(Count);
            var rng = new Random(42);

            for (int i = 0; i < Count; i++)
            {
                if (rng.Next(2) == 0)
                {
                    list.Add(new TextToken { TokenId = i, Text = "Some text data" });
                }
                else
                {
                    list.Add(new EmbeddingToken { TokenId = i, Embedding = new float[] { 1.0f, 2.0f, 3.0f } });
                }
            }

            var sw = Stopwatch.StartNew();
            foreach (var token in list)
            {
                token.Process();
            }
            sw.Stop();
            Console.WriteLine($"Class-Based Polymorphism: {sw.ElapsedMilliseconds}ms");
        }

        public static void RunStructBased()
        {
            var array = new HybridToken[Count];
            var rng = new Random(42);

            for (int i = 0; i < Count; i++)
            {
                var type = rng.Next(2) == 0 ? TokenType.Text : TokenType.Embedding;
                array[i] = new HybridToken(type);
                // Simulate data assignment
                if (type == TokenType.Text) array[i].TextId = i;
                else { array[i].EmbeddingX = 1.0f; array[i].EmbeddingY = 2.0f; }
            }

            var sw = Stopwatch.StartNew();
            foreach (var token in array)
            {
                // Switch expression simulates polymorphism
                switch (token.Type)
                {
                    case TokenType.Text:
                        // Process text (using TextId)
                        var len = token.TextId.ToString().Length; 
                        break;
                    case TokenType.Embedding:
                        var sum = token.EmbeddingX + token.EmbeddingY;
                        break;
                }
            }
            sw.Stop();
            Console.WriteLine($"Struct-Based Union: {sw.ElapsedMilliseconds}ms");
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            // Warmup
            ComparisonDemo.RunClassBased();
            ComparisonDemo.RunStructBased();

            Console.WriteLine("--- Performance Test ---");
            ComparisonDemo.RunClassBased();
            ComparisonDemo.RunStructBased();

            Console.WriteLine("\n--- Memory Layout Analysis ---");
            Console.WriteLine($"Size of AIToken reference: {IntPtr.Size} bytes");
            Console.WriteLine($"Size of TextToken object: ~24 bytes + string data (Scattered in Heap)");
            Console.WriteLine($"Size of HybridToken struct: {Marshal.SizeOf<HybridToken>()} bytes (Contiguous)");
        }
    }
}
