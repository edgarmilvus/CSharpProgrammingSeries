
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
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncFileReading
{
    public class AsyncFileReader : IAsyncDisposable
    {
        private readonly string _filepath;
        private StreamReader? _reader;

        public AsyncFileReader(string filepath)
        {
            _filepath = filepath;
        }

        // Mimics Python's __aenter__
        public async Task<AsyncFileReader> OpenAsync()
        {
            // In .NET, File.OpenText is synchronous. 
            // For true non-blocking I/O, we use FileStream with StreamReader.
            var stream = new FileStream(_filepath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true);
            _reader = new StreamReader(stream);
            return this;
        }

        // Mimics Python's async generator
        public async IAsyncEnumerable<string> ReadLinesAsync()
        {
            if (_reader == null) throw new InvalidOperationException("Reader not initialized. Call OpenAsync first.");

            string? line;
            while ((line = await _reader.ReadLineAsync()) != null)
            {
                yield return line;
            }
        }

        // Mimics Python's __aexit__
        public async ValueTask DisposeAsync()
        {
            if (_reader != null)
            {
                await _reader.DisposeAsync();
                _reader = null;
            }
        }
    }

    public static class Program
    {
        // Simulates processing (e.g., sending to an LLM or DB)
        public static async Task<string> ProcessLineAsync(string line)
        {
            await Task.Delay(10); // Simulated I/O latency (10ms)
            return line.ToUpper();
        }

        public static async Task Main()
        {
            const string filePath = "large_dummy.txt";
            const int lineCount = 10000;

            // 1. Create a dummy file for testing
            await File.WriteAllLinesAsync(filePath, 
                Enumerable.Range(0, lineCount).Select(i => $"Line {i}"));

            Console.WriteLine("Starting Async Execution...");
            
            // 2. Async Execution
            var stopwatch = Stopwatch.StartNew();
            
            // Using the AsyncFileReader with the 'using' pattern for disposal
            await using (var asyncReader = new AsyncFileReader(filePath))
            {
                await asyncReader.OpenAsync();
                
                var tasks = new List<Task<string>>();
                
                // Reading lines and simulating processing
                // Note: In a real high-throughput scenario, we might use a SemaphoreSlim 
                // to limit concurrency if processing is CPU intensive or rate-limited.
                await foreach (var line in asyncReader.ReadLinesAsync())
                {
                    tasks.Add(ProcessLineAsync(line));
                }

                // Wait for all processing tasks to complete
                var results = await Task.WhenAll(tasks);
            }
            
            stopwatch.Stop();
            Console.WriteLine($"Async execution time: {stopwatch.Elapsed.TotalSeconds:F4}s");

            // 3. Synchronous Counterpart
            Console.WriteLine("\nStarting Sync Execution...");
            stopwatch.Restart();

            // Standard blocking file read
            var lines = File.ReadAllLines(filePath);
            var syncResults = new string[lines.Length];
            for (int i = 0; i < lines.Length; i++)
            {
                // Blocking wait
                Thread.Sleep(10); 
                syncResults[i] = lines[i].ToUpper();
            }

            stopwatch.Stop();
            Console.WriteLine($"Sync execution time: {stopwatch.Elapsed.TotalSeconds:F4}s");
        }
    }
}
