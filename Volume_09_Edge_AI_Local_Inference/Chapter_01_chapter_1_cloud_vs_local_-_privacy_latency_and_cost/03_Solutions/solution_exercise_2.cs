
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

# Source File: solution_exercise_2.cs
# Description: Solution for Exercise 2
# ==========================================

using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace LatencyBenchmark
{
    public class BenchmarkResult
    {
        public string Operation { get; set; }
        public List<double> Latencies { get; set; } = new List<double>();
        
        public double Average => Latencies.Average();
        public double StdDev 
        {
            get
            {
                if (Latencies.Count < 2) return 0;
                var avg = Average;
                var sumSquares = Latencies.Sum(x => Math.Pow(x - avg, 2));
                return Math.Sqrt(sumSquares / (Latencies.Count - 1));
            }
        }
        public double Min => Latencies.Min();
        public double Max => Latencies.Max();
    }

    class Program
    {
        private const int Runs = 10;
        private static readonly HttpClient _httpClient = new HttpClient();

        static async Task Main(string[] args)
        {
            Console.WriteLine($"Starting Benchmark ({Runs} runs)...\n");

            // 1. Simulated Cloud Call
            var cloudResult = await BenchmarkCloudCalls();

            // 2. Local Inference
            // Note: We need the model file from Exercise 1.
            var localResult = BenchmarkLocalInference();

            // 3. Output Comparison
            PrintResults(cloudResult, localResult);
        }

        static async Task<BenchmarkResult> BenchmarkCloudCalls()
        {
            var result = new BenchmarkResult { Operation = "Simulated Cloud API" };
            var url = "https://jsonplaceholder.typicode.com/todos/1";

            // Warmup
            try { await _httpClient.GetAsync(url); } catch { }

            for (int i = 0; i < Runs; i++)
            {
                var sw = Stopwatch.StartNew();
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                await response.Content.ReadAsStringAsync();
                sw.Stop();
                result.Latencies.Add(sw.Elapsed.TotalMilliseconds);
            }
            return result;
        }

        static BenchmarkResult BenchmarkLocalInference()
        {
            var result = new BenchmarkResult { Operation = "Local Inference" };
            string modelPath = "phi-3-mini-4k-instruct.onnx";

            if (!System.IO.File.Exists(modelPath))
            {
                Console.WriteLine("Local model not found. Skipping local benchmark.");
                return result;
            }

            // Warmup (Crucial for loading model into memory)
            using (var engine = new LocalInferenceEngineWrapper(modelPath))
            {
                engine.RunInference("Warmup");
            }

            for (int i = 0; i < Runs; i++)
            {
                // We re-create the engine to simulate a fresh request context, 
                // or we could keep it open. For strict timing of the 'Run' phase:
                using (var engine = new LocalInferenceEngineWrapper(modelPath))
                {
                    var sw = Stopwatch.StartNew();
                    engine.RunInference("Test prompt " + i);
                    sw.Stop();
                    result.Latencies.Add(sw.Elapsed.TotalMilliseconds);
                }
            }
            return result;
        }

        static void PrintResults(BenchmarkResult cloud, BenchmarkResult local)
        {
            Console.WriteLine("{0,-25} | {1,-10} | {2,-10} | {3,-10} | {4,-10}", 
                "Operation", "Avg (ms)", "StdDev", "Min", "Max");
            Console.WriteLine(new string('-', 75));

            foreach (var res in new[] { cloud, local })
            {
                Console.WriteLine("{0,-25} | {1,-10:F2} | {2,-10:F2} | {3,-10:F2} | {4,-10:F2}",
                    res.Operation, res.Average, res.StdDev, res.Min, res.Max);
            }
        }
    }

    // Wrapper to expose the logic from Exercise 1 for benchmarking
    public class LocalInferenceEngineWrapper : IDisposable
    {
        private readonly InferenceSession _session;
        public LocalInferenceEngineWrapper(string modelPath) 
            => _session = new InferenceSession(modelPath);

        public string RunInference(string prompt)
        {
            var inputIds = new List<int> { 1 };
            inputIds.AddRange(prompt.Select(c => (int)c));
            inputIds.Add(2);
            var tensor = new DenseTensor<int>(inputIds.ToArray(), new long[] { 1, inputIds.Count });
            var inputs = new List<NamedOnnxValue> { NamedOnnxValue.CreateFromTensor("input_ids", tensor) };
            using var results = _session.Run(inputs);
            return "Simulated Output"; // Simplified for benchmark speed
        }
        public void Dispose() => _session?.Dispose();
    }
}
