
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
using System.Threading;

public class DataProcessor
{
    public void ProcessData(Action<int> onProgress, Action<string> onComplete)
    {
        int totalChunks = 10;
        
        for (int i = 1; i <= totalChunks; i++)
        {
            // Simulate work
            Thread.Sleep(100); 

            // Calculate percentage
            int percentage = (i * 100) / totalChunks;

            // Invoke the callback safely
            // Null-conditional operator (?.Invoke) prevents NullReferenceException if no subscriber exists
            onProgress?.Invoke(percentage);
        }

        // Invoke completion callback
        onComplete?.Invoke($"Processing finished. {totalChunks * 100} records analyzed.");
    }
}

public class Program
{
    public static void Main()
    {
        DataProcessor processor = new DataProcessor();

        // Define Lambda 1: Progress Bar Visualizer
        Action<int> progressBar = (percent) =>
        {
            // Simple string formatting to create a visual bar
            int barWidth = 20;
            int filledWidth = (percent * barWidth) / 100;
            string bar = new string('=', filledWidth).PadRight(barWidth);
            Console.Write($"\r[{bar}] {percent}%");
        };

        // Define Lambda 2: Logger
        Action<string> logger = (message) =>
        {
            Console.WriteLine($"\n[LOG]: {message}");
        };

        Console.WriteLine("Starting Data Processing...");
        
        // Pass lambdas to the processor
        processor.ProcessData(progressBar, logger);

        Console.WriteLine("Program execution continues immediately after processing...");
    }
}
