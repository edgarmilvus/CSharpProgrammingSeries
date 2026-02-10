
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

// Source File: basic_basic_code_example.cs
// Description: Basic Code Example
// ==========================================

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace HighPerformanceAI.TokenProcessing
{
    // ---------------------------------------------------------
    // CONTEXT: AI Token Processing
    // ---------------------------------------------------------
    // In an AI inference engine, we process a sequence of tokens.
    // Each token has a vocabulary ID and a weight (logit).
    // We need to perform vectorized operations (like scaling weights)
    // on these tokens millions of times per second.
    //
    // PROBLEM: Using classes creates heap allocations and pointer chasing,
    // which destroys CPU cache locality. Using structs allows data to be
    // packed contiguously in memory, enabling SIMD (Single Instruction, Multiple Data).

    /// <summary>
    /// Represents a Token as a CLASS (Reference Type).
    /// This is the "slow" path for high-throughput numeric processing.
    /// </summary>
    public class TokenClass
    {
        public int Id;
        public float Weight;

        public TokenClass(int id, float weight)
        {
            Id = id;
            Weight = weight;
        }
    }

    /// <summary>
    /// Represents a Token as a STRUCT (Value Type).
    /// This is the "fast" path for high-throughput numeric processing.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)] // Ensures specific memory layout
    public struct TokenStruct
    {
        public int Id;
        public float Weight;

        public TokenStruct(int id, float weight)
        {
            Id = id;
            Weight = weight;
        }
    }

    public class TokenBenchmark
    {
        const int ITERATIONS = 1_000_000; // 1 Million tokens

        public static void RunDemo()
        {
            Console.WriteLine($"--- Token Processing Benchmark (Iterations: {ITERATIONS:N0}) ---\n");

            // 1. BENCHMARK CLASS-BASED PROCESSING
            // ---------------------------------------------------------
            // We allocate an array of references. Each token is a separate object
            // allocated on the Managed Heap.
            TokenClass[] tokenClasses = new TokenClass[ITERATIONS];
            
            // Pre-fill to ensure allocation overhead is accounted for
            for (int i = 0; i < ITERATIONS; i++)
            {
                tokenClasses[i] = new TokenClass(i, 1.0f);
            }

            Stopwatch sw = Stopwatch.StartNew();
            
            // SCENARIO: Apply a temperature scaling factor to the weights.
            // In a real AI model, this is a vectorized operation.
            for (int i = 0; i < ITERATIONS; i++)
            {
                // Accessing a class involves:
                // 1. Load the reference from the array.
                // 2. Dereference the pointer to find the object on the heap.
                // 3. Access the field.
                tokenClasses[i].Weight *= 0.5f; 
            }
            
            sw.Stop();
            long classTime = sw.ElapsedMilliseconds;
            Console.WriteLine($"[Class] Processing Time: {classTime} ms");

            // 2. BENCHMARK STRUCT-BASED PROCESSING
            // ---------------------------------------------------------
            // We allocate an array of values. The structs are packed contiguously
            // in memory (no pointers, no heap objects).
            TokenStruct[] tokenStructs = new TokenStruct[ITERATIONS];

            // Pre-fill
            for (int i = 0; i < ITERATIONS; i++)
            {
                tokenStructs[i] = new TokenStruct(i, 1.0f);
            }

            sw.Restart();

            // SCENARIO: Apply the same temperature scaling.
            for (int i = 0; i < ITERATIONS; i++)
            {
                // Accessing a struct involves:
                // 1. Calculate offset in the contiguous array.
                // 2. Access the data directly (CPU cache friendly).
                // Note: We must copy the struct to the stack to modify it,
                // then copy it back. However, the JIT optimizer often optimizes
                // this in tight loops to direct memory manipulation.
                TokenStruct t = tokenStructs[i];
                t.Weight *= 0.5f;
                tokenStructs[i] = t;
            }

            sw.Stop();
            long structTime = sw.ElapsedMilliseconds;
            Console.WriteLine($"[Struct] Processing Time: {structTime} ms");

            // 3. ADVANCED: SIMD OPTIMIZATION (The "Why")
            // ---------------------------------------------------------
            // Structs allow us to use System.Numerics.Vector<T> for SIMD.
            // We cannot easily do this with arrays of classes because the data
            // is scattered all over the heap.
            Console.WriteLine("\n--- SIMD Optimization (Vector<T>) ---");
            
            // Reset data for fair comparison
            for (int i = 0; i < ITERATIONS; i++) tokenStructs[i] = new TokenStruct(i, 1.0f);

            sw.Restart();
            ProcessStructsSimd(tokenStructs);
            sw.Stop();
            long simdTime = sw.ElapsedMilliseconds;
            Console.WriteLine($"[Struct + SIMD] Processing Time: {simdTime} ms");

            // 4. MEMORY LAYOUT VISUALIZATION
            // ---------------------------------------------------------
            VisualizeMemoryLayout();
        }

        /// <summary>
        /// Optimized processing using SIMD (Vectorization).
        /// Requires the data to be contiguous (structs in an array).
        /// </summary>
        private static void ProcessStructsSimd(TokenStruct[] tokens)
        {
            // In a real scenario, we would use Span<T> and Vector<T>.
            // This example simulates the concept by processing chunks.
            // Note: We cannot use Vector<T> directly on custom structs easily
            // without unsafe code or explicit layout, but for floats, 
            // we can treat the memory as floats if we ignore the ID.
            
            // For this demo, we will simply iterate, but imagine using:
            // Vector<float> scale = new Vector<float>(0.5f);
            // This processes 8 floats (AVX2) or 16 floats (AVX-512) at once.
            
            for (int i = 0; i < tokens.Length; i++)
            {
                // In SIMD, this loop would be unrolled and vectorized automatically
                // by the JIT if we were using Vector<T> types.
                tokens[i].Weight *= 0.5f;
            }
        }

        private static void VisualizeMemoryLayout()
        {
            Console.WriteLine("\n--- Memory Layout Visualization ---");
            Console.WriteLine("Generating DOT diagram for memory representation...");

            string dot = @"
digraph MemoryLayout {
    rankdir=TB;
    node [shape=record, fontname=""Helvetica""];

    // Class Layout (Heap)
    subgraph cluster_heap {
        label = ""Managed Heap (TokenClass Array)"";
        style = filled;
        color = lightgrey;

        ArrayObj [label=""Array Object Header|Data: [ref, ref, ref, ...]""];
        
        Obj1 [label=""Object 1 Header|Id: 100|Weight: 0.5f""];
        Obj2 [label=""Object 2 Header|Id: 101|Weight: 0.5f""];
        Obj3 [label=""Object 3 Header|Id: 102|Weight: 0.5f""];
        
        ArrayObj -> Obj1 [style=dotted, label=""Ref"", arrowhead=vee];
        ArrayObj -> Obj2 [style=dotted, label=""Ref""];
        ArrayObj -> Obj3 [style=dotted, label=""Ref""];
    }

    // Struct Layout (Stack/Contiguous)
    subgraph cluster_stack {
        label = ""Contiguous Memory (TokenStruct Array)"";
        style = filled;
        color = lightblue;

        StructBlock [label=""<f0> Id: 100|<f1> Weight: 0.5f|<f2> Id: 101|<f3> Weight: 0.5f|<f4> Id: 102|<f5> Weight: 0.5f"", height=2];
    }
    
    // Cache Line Visualization
    subgraph cluster_cache {
        label = ""CPU Cache Line (64 Bytes)"";
        style = dashed;
        
        CacheLine [label=""[Struct 1][Struct 2][Struct 3]...""];
    }

    StructBlock -> CacheLine [label=""Prefetcher loves this"", color=green, fontcolor=green];
}
";
            Console.WriteLine("

[ERROR: Failed to render diagram.]

");
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            TokenBenchmark.RunDemo();
        }
    }
}
