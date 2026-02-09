
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

namespace DelegateExercises
{
    // 1. Interface definition
    public interface ITokenDelegate
    {
        void OnToken(string token);
    }

    // 2. Concrete implementation 1: Console Output
    public class ConsoleOutputDelegate : ITokenDelegate
    {
        public void OnToken(string token)
        {
            Console.WriteLine($"Output: {token}");
        }
    }

    // 3. Concrete implementation 2: Stateful Metrics
    public class AggregateMetricsDelegate : ITokenDelegate
    {
        public int TokenCount { get; private set; } = 0;
        public double AverageLength { get; private set; } = 0;
        private double _currentTotalLength = 0;

        public void OnToken(string token)
        {
            TokenCount++;
            _currentTotalLength += token.Length;
            AverageLength = _currentTotalLength / TokenCount;
            
            Console.WriteLine($"[Metrics] Count: {TokenCount}, Avg Len: {AverageLength:F2}");
        }
    }

    public class LLMGenerator
    {
        // 4. Accepting the interface instead of a delegate
        public void Generate(ITokenDelegate callback)
        {
            string[] tokens = { "Tensor", "Flow", "Data" };
            foreach (var token in tokens)
            {
                callback.OnToken(token);
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var generator = new LLMGenerator();

            // 5. Dependency Injection style instantiation
            ITokenDelegate consoleDelegate = new ConsoleOutputDelegate();
            ITokenDelegate metricsDelegate = new AggregateMetricsDelegate();

            Console.WriteLine("--- Run 1 (Console) ---");
            generator.Generate(consoleDelegate);

            Console.WriteLine("\n--- Run 2 (Metrics) ---");
            generator.Generate(metricsDelegate);
        }
    }
}
