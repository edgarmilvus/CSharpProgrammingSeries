
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

# Source File: project_advanced_application_script.cs
# Description: Advanced Application Script
# ==========================================

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HighPerformanceAITokenProcessing
{
    /// <summary>
    /// Real-World Context: High-Throughput AI Inference Engine.
    /// 
    /// Problem: An AI inference service processes millions of user prompts daily.
    /// Each prompt requires tokenization (splitting text into words/sub-words), 
    /// model inference (generating response tokens), and serialization (formatting JSON).
    /// 
    /// GC Challenge: 
    /// 1. Frequent allocation of small objects (strings, arrays for tokens) fills Gen0/Gen1 rapidly, 
    ///    causing "Stop-the-World" pauses that block the inference thread.
    /// 2. Large payloads (e.g., 85KB+ context windows) end up on the Large Object Heap (LOH).
    ///    The LOH is not compacted by default in older .NET versions (or only partially in .NET Core), 
    ///    leading to memory fragmentation and OutOfMemoryExceptions under load.
    /// 
    /// Solution: This application simulates a token processing pipeline that utilizes:
    /// - Object Pooling (Span<T> logic) to reuse buffers and reduce Gen0 pressure.
    /// - ArrayPool<T> to manage large arrays without frequent LOH allocations.
    /// - Struct-based Token processing to stay off the heap entirely where possible.
    /// - Explicit GC tuning to favor low-latency modes suitable for AI inference.
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Initializing High-Performance AI Token Processor...");
            Console.WriteLine($"GC Mode: {GetGCMode()}");
            
            // 1. Configure GC for high throughput/low latency
            ConfigureGC();

            // 2. Instantiate the engine with object pooling enabled
            var engine = new InferenceEngine(useObjectPooling: true);

            // 3. Simulate a stream of incoming requests (High Volume)
            // In a real scenario, this would be an async HTTP listener loop.
            Console.WriteLine("\nStarting Processing Simulation (Press Ctrl+C to stop)...");
            
            var stopwatch = Stopwatch.StartNew();
            long totalTokensProcessed = 0;
            int requestCount = 0;

            try
            {
                while (true)
                {
                    // Simulate a batch of requests
                    for (int i = 0; i < 100; i++)
                    {
                        // Generate a random prompt (simulating user input)
                        string prompt = GenerateRandomPrompt();
                        
                        // Process the prompt (Tokenize -> Infer -> Serialize)
                        // This method is designed to minimize heap allocations.
                        string result = engine.ProcessPrompt(prompt);
                        
                        totalTokensProcessed += result.Length / 4; // Rough estimate
                        requestCount++;
                    }

                    // Force a Gen0 collection check to demonstrate low pressure
                    // (In production, we rely on the GC's own triggers, but this helps visualize stability)
                    if (requestCount % 1000 == 0)
                    {
                        long mem = GC.GetTotalMemory(false);
                        Console.WriteLine($"[Stats] Requests: {requestCount,6} | " +
                                          $"Mem: {mem / 1024,6} KB | " +
                                          $"Gen0: {GC.CollectionCount(0),4} | " +
                                          $"Gen1: {GC.CollectionCount(1),4} | " +
                                          $"Gen2: {GC.CollectionCount(2),4}");
                        
                        // Simulate occasional large object allocation (LOH test)
                        if (requestCount % 5000 == 0) engine.ProcessLargeContextWindow();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Simulation stopped: {ex.Message}");
            }
        }

        // --- GC Configuration & Helpers ---

        static void ConfigureGC()
        {
            // In .NET Core 5+, we can use LatencyMode to reduce GC frequency during critical processing.
            // GCLatencyMode.LowLatency minimizes Gen2 collections (which are expensive and cause pauses).
            // Note: This is a trade-off; memory usage might grow if the heap fills up.
            if (GCSettings.IsServerGC)
            {
                Console.WriteLine("Server GC is enabled (Good for multi-core throughput).");
            }
            
            // Setting latency mode for the current thread context
            GCSettings.LatencyMode = GCLatencyMode.LowLatency;
        }

        static string GetGCMode()
        {
            return GCSettings.IsServerGC ? "Server GC" : "Workstation GC";
        }

        static string GenerateRandomPrompt()
        {
            // Simulating input data without heavy allocation in the simulation loop
            // In a real app, this comes from network buffers.
            string[] vocab = { "AI", "Generate", "Code", "Optimize", "Memory", "Span", "Token", "Inference", "Model", "Response" };
            var sb = new StringBuilder();
            var rnd = new Random();
            int len = rnd.Next(5, 20);
            for (int i = 0; i < len; i++)
            {
                sb.Append(vocab[rnd.Next(vocab.Length)]);
                if (i < len - 1) sb.Append(' ');
            }
            return sb.ToString();
        }
    }

    /// <summary>
    /// Represents a single token. 
    /// Using a struct ensures this data lives on the Stack (or inside arrays) 
    /// rather than creating a heap object for every token.
    /// This drastically reduces Gen0 pressure.
    /// </summary>
    public struct Token
    {
        public int Id;
        public float LogProb;
        public string Text; // Strings are reference types, but the struct wrapper helps grouping.

        public Token(int id, string text, float logProb)
        {
            Id = id;
            Text = text;
            LogProb = logProb;
        }
    }

    /// <summary>
    /// Manages the inference pipeline.
    /// Focuses on reusing memory buffers to avoid LOH fragmentation and Gen0/Gen1 GC pauses.
    /// </summary>
    public class InferenceEngine
    {
        private readonly bool _usePooling;
        private readonly Tokenizer _tokenizer;
        private readonly ModelSimulator _model;
        private readonly Serializer _serializer;

        public InferenceEngine(bool useObjectPooling)
        {
            _usePooling = useObjectPooling;
            _tokenizer = new Tokenizer(useObjectPooling);
            _model = new ModelSimulator(useObjectPooling);
            _serializer = new Serializer(useObjectPooling);
        }

        /// <summary>
        /// Orchestrates the flow: Tokenization -> Inference -> Serialization.
        /// </summary>
        public string ProcessPrompt(string prompt)
        {
            // 1. Tokenization (Input Processing)
            // Returns a Span<Token> if pooling is active, avoiding List<Token> allocations.
            var tokens = _tokenizer.Encode(prompt);

            // 2. Inference (Model Generation)
            // Generates new tokens. Uses pooled arrays for intermediate state.
            var generatedTokens = _model.Generate(tokens);

            // 3. Serialization (Output Formatting)
            // Converts tokens to JSON string. Uses StringBuilder pooling.
            string result = _serializer.ToJson(generatedTokens);

            // 4. Cleanup: If using explicit pools, return arrays/buffers here.
            // In a real high-perf scenario, we might use 'using' blocks or custom allocators.
            if (_usePooling)
            {
                _tokenizer.ReturnBuffer(tokens);
                _model.ReturnBuffer(generatedTokens);
            }

            return result;
        }

        /// <summary>
        /// Simulates processing a very large context window (> 85KB).
        /// This forces allocation on the Large Object Heap (LOH).
        /// </summary>
        public void ProcessLargeContextWindow()
        {
            Console.WriteLine("\n[WARNING] Processing Large Context Window (LOH Allocation)...");
            
            // Allocating a large array of bytes (simulating a large JSON payload)
            // In .NET, objects > 85,000 bytes go to the LOH.
            // The LOH is not compacted by default, leading to fragmentation.
            byte[] largeBuffer = new byte[100 * 1024]; // 100 KB -> LOH
            
            // Simulate work
            for (int i = 0; i < largeBuffer.Length; i++) largeBuffer[i] = 1;
            
            // In a real app, we would use ArrayPool<byte>.Shared.Rent(100 * 1024) 
            // to avoid this LOH allocation entirely, or use Memory<T>/Span<T> over pinned memory.
            // Since this is a demonstration of the *problem*, we allocate directly here.
            
            Console.WriteLine("Large Context Processed. (Check Gen2 count increase if LOH is collected)");
            
            // Note: In .NET Core 3.0+, LOH compaction is enabled but requires a full blocking GC.
            // This is a heavy operation and causes significant pauses.
        }
    }

    /// <summary>
    /// Simulates breaking text into tokens.
    /// Uses ArrayPool to reuse token arrays, preventing frequent Gen0 allocations.
    /// </summary>
    public class Tokenizer
    {
        private readonly bool _usePooling;
        private static readonly System.Buffers.ArrayPool<Token> _tokenPool = System.Buffers.ArrayPool<Token>.Shared;

        public Tokenizer(bool usePooling) => _usePooling = usePooling;

        /// <summary>
        /// Encodes string into tokens.
        /// Returns Span<Token> to allow stack-based or pooled-array access without heap allocation.
        /// </summary>
        public Span<Token> Encode(string input)
        {
            // Simple heuristic: split by space. In reality, this uses BPE algorithms.
            string[] parts = input.Split(' ');
            int count = parts.Length;

            Token[] buffer;
            
            if (_usePooling)
            {
                // Rent from the shared pool. This avoids 'new Token[]' on the heap.
                // If the pool is empty, it falls back to allocating.
                buffer = _tokenPool.Rent(count);
            }
            else
            {
                buffer = new Token[count];
            }

            // Fill the buffer
            for (int i = 0; i < count; i++)
            {
                // Hashing string to ID (simulation)
                int id = parts[i].GetHashCode() & 0xFFFF; 
                buffer[i] = new Token(id, parts[i], 0.95f);
            }

            // Return a Span over the valid portion of the buffer.
            // This is safe and doesn't expose the underlying array directly if we slice it.
            return buffer.AsSpan(0, count);
        }

        public void ReturnBuffer(Span<Token> tokens)
        {
            if (_usePooling && tokens.TryGetArray(out var segment))
            {
                // Return the array to the pool so it can be reused.
                _tokenPool.Return(segment.Array);
            }
        }
    }

    /// <summary>
    /// Simulates the Neural Network inference step.
    /// </summary>
    public class ModelSimulator
    {
        private readonly bool _usePooling;
        private static readonly System.Buffers.ArrayPool<Token> _tokenPool = System.Buffers.ArrayPool<Token>.Shared;
        private readonly Random _rnd = new Random();

        public ModelSimulator(bool usePooling) => _usePooling = usePooling;

        /// <summary>
        /// Generates new tokens based on input context.
        /// </summary>
        public Span<Token> Generate(Span<Token> inputTokens)
        {
            // Simulate generating 10 new tokens
            int newTokenCount = 10;
            Token[] buffer;

            if (_usePooling)
            {
                buffer = _tokenPool.Rent(newTokenCount);
            }
            else
            {
                buffer = new Token[newTokenCount];
            }

            for (int i = 0; i < newTokenCount; i++)
            {
                // Simulate prediction logic
                int id = _rnd.Next(1000, 2000);
                buffer[i] = new Token(id, $"gen_{id}", 0.8f);
            }

            return buffer.AsSpan(0, newTokenCount);
        }

        public void ReturnBuffer(Span<Token> tokens)
        {
            if (_usePooling && tokens.TryGetArray(out var segment))
            {
                _tokenPool.Return(segment.Array);
            }
        }
    }

    /// <summary>
    /// Serializes tokens to JSON.
    /// Uses StringBuilder pooling to reduce LOH allocations from large strings.
    /// </summary>
    public class Serializer
    {
        private readonly bool _usePooling;
        // StringBuilder pool is custom because .NET doesn't provide a built-in one.
        // In production, use Microsoft.Extensions.ObjectPool or similar.
        private readonly Stack<StringBuilder> _builderPool = new Stack<StringBuilder>();

        public Serializer(bool usePooling) => _usePooling = usePooling;

        public string ToJson(Span<Token> tokens)
        {
            StringBuilder sb;
            if (_usePooling)
            {
                lock (_builderPool)
                {
                    sb = _builderPool.Count > 0 ? _builderPool.Pop() : new StringBuilder(1024);
                }
            }
            else
            {
                sb = new StringBuilder();
            }

            sb.Clear();
            sb.Append("{\"tokens\":[");

            for (int i = 0; i < tokens.Length; i++)
            {
                sb.Append("{\"id\":");
                sb.Append(tokens[i].Id);
                sb.Append(",\"text\":\"");
                sb.Append(tokens[i].Text);
                sb.Append("\"}");
                if (i < tokens.Length - 1) sb.Append(',');
            }

            sb.Append("]}");

            string json = sb.ToString();

            if (_usePooling)
            {
                lock (_builderPool)
                {
                    _builderPool.Push(sb);
                }
            }

            return json;
        }
    }
}
