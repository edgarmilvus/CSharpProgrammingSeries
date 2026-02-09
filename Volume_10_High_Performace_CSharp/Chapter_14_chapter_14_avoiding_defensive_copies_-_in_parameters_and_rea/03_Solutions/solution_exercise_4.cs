
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
using System.Diagnostics;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Exercise4
{
    public struct AttentionScore
    {
        // Simulating a struct containing multiple vectors (hypothetical large struct)
        public Vector256<float> QueryVector;
        public Vector256<float> KeyVector;
        
        public AttentionScore(Vector256<float> q, Vector256<float> k)
        {
            QueryVector = q;
            KeyVector = k;
        }
    }

    public class AttentionOptimizer
    {
        // Original: Passing by value (Copy)
        public static float ComputeScoreByValue(AttentionScore score)
        {
            // Simulate dot product
            return Avx.Sum(Avx.Multiply(score.QueryVector, score.KeyVector));
        }

        // Refactored: Passing by 'in' (Reference)
        public static float ComputeScoreByIn(in AttentionScore score)
        {
            // No copy of the struct occurs. 
            // The JIT can keep the vectors in registers if possible.
            return Avx.Sum(Avx.Multiply(score.QueryVector, score.KeyVector));
        }
    }

    class Program
    {
        static void Main()
        {
            // Initialize data
            var q = Vector256.Create(1.0f, 2.0f, 3.0f, 4.0f, 5.0f, 6.0f, 7.0f, 8.0f);
            var k = Vector256.Create(8.0f, 7.0f, 6.0f, 5.0f, 4.0f, 3.0f, 2.0f, 1.0f);
            var scoreStruct = new AttentionScore(q, k);
            
            const long iterations = 10_000_000;
            var sw = new Stopwatch();

            // Benchmark By Value
            sw.Start();
            float resultVal = 0;
            for (long i = 0; i < iterations; i++)
            {
                resultVal = AttentionOptimizer.ComputeScoreByValue(scoreStruct);
            }
            sw.Stop();
            Console.WriteLine($"By Value: {sw.ElapsedMilliseconds} ms");

            // Benchmark By 'in'
            sw.Restart();
            float resultIn = 0;
            for (long i = 0; i < iterations; i++)
            {
                resultIn = AttentionOptimizer.ComputeScoreByIn(in scoreStruct);
            }
            sw.Stop();
            Console.WriteLine($"By 'in': {sw.ElapsedMilliseconds} ms");
        }
    }
}
