
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

// Source File: solution_exercise_1.cs
// Description: Solution for Exercise 1
// ==========================================

using System;
using System.Collections.Generic;
using System.Threading;

namespace DelegateExercises
{
    // 1. Define the delegate type
    public delegate void TokenReceivedDelegate(string token);

    public class LLMGenerator
    {
        // 3. The generation method accepting the delegate
        public void Generate(TokenReceivedDelegate callback)
        {
            // 4. Simulate streaming tokens
            List<string> tokens = new List<string> { "The", " quick", " brown", " fox" };

            foreach (var token in tokens)
            {
                // Invoke the callback
                callback(token);
                // Simulate network latency
                Thread.Sleep(200); 
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            // 5. Create an instance of the generator
            var generator = new LLMGenerator();

            // 6. Create a method matching the delegate signature
            void PrintToken(string t) 
            { 
                Console.WriteLine($"Received: {t}"); 
            }

            // 7. Register the method and call Generate
            generator.Generate(PrintToken);
        }
    }
}
