
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

// Source File: solution_exercise_4.cs
// Description: Solution for Exercise 4
// ==========================================

using System;
using System.IO;
using System.Linq;

namespace DelegateExercises
{
    // 1. Define the delegate for Tensor streaming
    public delegate void TensorReceivedDelegate(float[] tensor, string token);

    // The "Advanced" Generator (Simulated as unmodifiable legacy code)
    public class TensorLLMGenerator
    {
        public void GenerateTokensWithTensors(TensorReceivedDelegate callback)
        {
            // Simulate 3 steps
            for (int i = 0; i < 3; i++)
            {
                string token = $"Token_{i}";
                // Simulate a tensor (hidden state of size 3)
                float[] tensor = new float[] { 
                    (float)Random.Shared.NextDouble(), 
                    (float)Random.Shared.NextDouble() * 2, 
                    (float)Random.Shared.NextDouble() 
                };
                
                // Invoke the callback
                callback(tensor, token);
            }
        }
    }

    // 2. Logger (Static Class)
    public static class TensorLogger
    {
        public static void LogToFile(float[] tensor, string token)
        {
            string data = $"[{token}] " + string.Join(", ", tensor.Select(f => f.ToString("F3"))) + "\n";
            File.AppendAllText("tensor_log.txt", data);
        }
    }

    // 3. Visualizer (Static Class)
    public static class TensorVisualizer
    {
        public static void DrawGrid(float[] tensor, string token)
        {
            Console.WriteLine($"--- Visualizing {token} ---");
            foreach (var val in tensor)
            {
                int bars = (int)(val * 10);
                Console.Write($"[{new string('|', bars)}]\n");
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var generator = new TensorLLMGenerator();

            // 4. & 6. Monitor Logic (Bonus using Lambda Expression)
            // We define the logic inline to check for high activation
            TensorReceivedDelegate monitor = (tensor, token) => 
            {
                if (tensor.Any(v => v > 0.9f))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"!!! ALERT: High Activation in {token} !!!");
                    Console.ResetColor();
                }
            };

            // 5. Constructing the Multicast Pipeline
            TensorReceivedDelegate pipeline = TensorLogger.LogToFile;
            pipeline += TensorVisualizer.DrawGrid;
            pipeline += monitor; // Adding the lambda-based monitor

            Console.WriteLine("Starting Tensor Generation Pipeline...\n");
            
            // Pass the pipeline to the unmodified generator
            generator.GenerateTokensWithTensors(pipeline);
        }
    }
}
