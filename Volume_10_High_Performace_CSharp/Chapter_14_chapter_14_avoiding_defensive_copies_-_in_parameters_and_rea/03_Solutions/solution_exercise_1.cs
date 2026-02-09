
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
using System.Diagnostics;

namespace Exercise1
{
    // Step 1: Define the struct (mutable initially)
    public struct TokenInfo
    {
        public int TokenID;
        public int Position;
        public int Score;
    }

    // Step 4: Refactored to readonly struct
    public readonly struct ReadonlyTokenInfo
    {
        public readonly int TokenID;
        public readonly int Position;
        public readonly int Score;

        public ReadonlyTokenInfo(int id, int pos, int score)
        {
            TokenID = id;
            Position = pos;
            Score = score;
        }
    }

    public class HashCalculator
    {
        // Step 2: Method accepting by value (causes copy)
        public static int CalculateHashByValue(TokenInfo info)
        {
            // Simulate work
            return (info.TokenID * 31 + info.Position) * 31 + info.Score;
        }

        // Step 2: Method accepting by reference (no copy)
        public static int CalculateHashByIn(in ReadonlyTokenInfo info)
        {
            // info.TokenID++; // Compiler Error: Cannot modify readonly member
            return (info.TokenID * 31 + info.Position) * 31 + info.Score;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            const int iterations = 1_000_000;
            var rand = new Random();
            
            // Step 3: Initialize array
            var tokens = new TokenInfo[iterations];
            var readonlyTokens = new ReadonlyTokenInfo[iterations];
            
            for (int i = 0; i < iterations; i++)
            {
                tokens[i] = new TokenInfo { TokenID = i, Position = i * 2, Score = rand.Next(100) };
                readonlyTokens[i] = new ReadonlyTokenInfo(i, i * 2, rand.Next(100));
            }

            // Benchmark By Value
            var sw = Stopwatch.StartNew();
            long sumByValue = 0;
            for (int i = 0; i < iterations; i++)
            {
                sumByValue += HashCalculator.CalculateHashByValue(tokens[i]);
            }
            sw.Stop();
            Console.WriteLine($"Passing by Value: {sw.ElapsedMilliseconds} ms (Sum: {sumByValue})");

            // Benchmark By 'in'
            sw.Restart();
            long sumByIn = 0;
            for (int i = 0; i < iterations; i++)
            {
                sumByIn += HashCalculator.CalculateHashByIn(in readonlyTokens[i]);
            }
            sw.Stop();
            Console.WriteLine($"Passing by 'in': {sw.ElapsedMilliseconds} ms (Sum: {sumByIn})");
        }
    }
}
