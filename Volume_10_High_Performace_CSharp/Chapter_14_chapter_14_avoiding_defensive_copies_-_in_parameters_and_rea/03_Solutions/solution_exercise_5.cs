
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

namespace Exercise5
{
    // Requirement 1: Generic Interface
    public interface ITokenProcessor<T> where T : struct
    {
        // Requirement 2: Method with 'in'
        T Process(in T input);
    }

    // Requirement 3: Concrete Struct
    public struct FastTokenizer : ITokenProcessor<FastTokenizer>
    {
        private int _internalCounter;

        public FastTokenizer(int counter)
        {
            _internalCounter = counter;
        }

        // Requirement 3: Implementation
        public FastTokenizer Process(in FastTokenizer input)
        {
            // We return a new struct. 
            // Since 'input' is 'in', we can read it efficiently without copying.
            return new FastTokenizer(input._internalCounter + 1);
        }
    }

    // Requirement 4: Generic Static Method
    public static class PipelineExecutor
    {
        public static TOutput ExecutePipeline<TInput, TOutput>(TOutput processor, in TInput input)
            where TOutput : struct, ITokenProcessor<TInput>
            where TInput : struct
        {
            // Requirement 5: Call the method
            return processor.Process(in input);
        }
    }

    class Program
    {
        static void Main()
        {
            var tokenizer = new FastTokenizer(0);
            var inputToken = new FastTokenizer(100);

            // Execute
            var result = PipelineExecutor.ExecutePipeline(tokenizer, in inputToken);
        }
    }
}
