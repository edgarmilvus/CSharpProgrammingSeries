
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

// Source File: solution_exercise_5.cs
// Description: Solution for Exercise 5
// ==========================================

using System;
using System.IO;

public struct BenchmarkResult
{
    public string Format;
    public double ModelSizeMB;
    public double EstimatedMemoryMB;
    public double InferenceTimeMs;
}

public class BenchmarkSimulator
{
    public static (BenchmarkResult Result, string GraphVizDot) SimulateBenchmark(string modelPath)
    {
        var fileInfo = new FileInfo(modelPath);
        double sizeMB = fileInfo.Length / (1024.0 * 1024.0);

        BenchmarkResult result = new BenchmarkResult
        {
            ModelSizeMB = sizeMB
        };

        string format = fileInfo.Extension.ToLower();
        
        if (format == ".onnx")
        {
            result.Format = "ONNX";
            // ONNX typically has higher overhead due to session creation and generic optimization
            result.EstimatedMemoryMB = sizeMB * 1.5; 
            // Simulating GPU acceleration (fast)
            result.InferenceTimeMs = 50.0; 
        }
        else if (format == ".gguf")
        {
            result.Format = "GGUF";
            // GGUF is memory mapped, lower overhead
            result.EstimatedMemoryMB = sizeMB * 1.2;
            // Simulating CPU bound processing (scales with model size)
            result.InferenceTimeMs = sizeMB * 2.0;
        }
        else
        {
            throw new ArgumentException("Unsupported file format for benchmarking.");
        }

        // Generate Graphviz DOT Diagram
        // We use string interpolation to build the graph structure
        string dotGraph = $@"
digraph BenchmarkFlow {{
    rankdir=TB;
    node [shape=box, style=""rounded,filled"", fillcolor=""#E3F2FD"", color=""#1565C0""];

    // Data Flow Nodes
    FS [label=""File System\n({format})""];
    Parser [label=""Metadata Parser\n(Size: {sizeMB:F2} MB)""];
    Sim [label=""Simulator\n(Heuristics)""];
    Report [label=""Reporter\n(Results)""];

    // Connections
    FS -> Parser [label=""Read Header""];
    Parser -> Sim [label=""Size/Type""];
    Sim -> Report [label=""Metrics""];
    
    // Add metrics as a separate node or label
    Metrics [label=""Est. Mem: {result.EstimatedMemoryMB:F2} MB\nLatency: {result.InferenceTimeMs:F2} ms"", fillcolor=""#FFF3E0"", color=""#EF6C00""];
    Sim -> Metrics [style=""dashed""];
}}";

        return (result, dotGraph);
    }
}

// Example Usage
// var (res, dot) = BenchmarkSimulator.SimulateBenchmark("model.gguf");
// Console.WriteLine(dot);
