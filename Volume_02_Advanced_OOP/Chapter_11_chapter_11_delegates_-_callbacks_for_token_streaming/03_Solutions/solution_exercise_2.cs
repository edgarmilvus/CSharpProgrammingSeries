
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
using System.IO;

namespace DelegateExercises
{
    // 1. Modified delegate to return bool
    public delegate bool TokenStreamDelegate(string token);

    public class LLMGenerator
    {
        public void Generate(TokenStreamDelegate callback)
        {
            string[] tokens = { "Model", " is", " processing", " stop", " data" };

            foreach (var token in tokens)
            {
                // 2. Check return value to control flow
                bool shouldContinue = callback(token);
                if (!shouldContinue) 
                {
                    Console.WriteLine("\n[Stream Interrupted by Delegate]");
                    break;
                }
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var generator = new LLMGenerator();
            var logPath = "log.txt";
            if (File.Exists(logPath)) File.Delete(logPath);

            // 3. Lambda expressions for specific behaviors
            TokenStreamDelegate logger = (token) => 
            {
                File.AppendAllText(logPath, token + "\n");
                return true; 
            };

            TokenStreamDelegate uiUpdater = (token) => 
            {
                Console.Write("#");
                return true;
            };

            // 6. Edge Case Logic (Lambda)
            TokenStreamDelegate stopper = (token) => 
            {
                if (token == " stop")
                {
                    Console.WriteLine("\nStop command detected!");
                    return false; // Breaks the loop
                }
                return true;
            };

            // 4. Multicast combination using +=
            TokenStreamDelegate pipeline = logger;
            pipeline += uiUpdater;
            pipeline += stopper;

            // 5. Invoke
            generator.Generate(pipeline);
            
            Console.WriteLine("\nDone.");
        }
    }
}
