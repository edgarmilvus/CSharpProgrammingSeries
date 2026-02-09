
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
using System.Text;

public static class MemoryVisualizer
{
    public static void Run()
    {
        // 1. Setup Data
        string originalString = "Hello World";
        char[] charArray = originalString.ToCharArray(); // Copy to new array
        
        // 2. Create Spans
        ReadOnlySpan<char> spanFromString = originalString.AsSpan();
        Span<char> spanFromArray = charArray.AsSpan();

        // 3. Generate DOT Graph
        // Note: We use object references (GetHashCode) as unique identifiers for the graph.
        // In a real memory map, these would be addresses, but GetHashCode provides a stable visual ID.
        
        var sb = new StringBuilder();
        sb.AppendLine("digraph MemoryLayout {");
        sb.AppendLine("  rankdir=LR;"); // Left to Right layout
        sb.AppendLine("  node [shape=box, style=filled, color=lightblue];");

        // Nodes
        sb.AppendLine($"  StringNode [label=\"String Object\\n\\\"{originalString}\\\"\\nRef: {originalString.GetHashCode()}\"];");
        sb.AppendLine($"  ArrayNode [label=\"Char Array\\nRef: {charArray.GetHashCode()}\"];");
        
        // Spans are not objects on the heap, but we visualize them as stack/temporary nodes
        sb.AppendLine($"  Span1Node [label=\"Span From String\\nPoints to: String Memory\", shape=note, color=lightyellow];");
        sb.AppendLine($"  Span2Node [label=\"Span From Array\\nPoints to: Array Memory\", shape=note, color=lightyellow];");

        // Edges (Relationships)
        // String -> Span1 (Pointer reference)
        sb.AppendLine("  StringNode -> Span1Node [label=\"AsSpan()\", style=dashed, arrowhead=vee];");
        
        // Array -> Span2 (Pointer reference)
        sb.AppendLine("  ArrayNode -> Span2Node [label=\"AsSpan()\", style=dashed, arrowhead=vee];");

        // String -> Array (Ownership/Copy relationship)
        // Note: In reality, the array is independent, but we show the creation dependency
        sb.AppendLine("  StringNode -> ArrayNode [label=\"ToCharArray()\", style=dotted, constraint=false];");

        sb.AppendLine("}");
        
        Console.WriteLine("=== GRAPHVIZ DOT CODE ===");
        Console.WriteLine(sb.ToString());
        Console.WriteLine("=========================");
        Console.WriteLine("Copy the code above and paste it into a Graphviz viewer (e.g., graphviz.org or VS Code extension).");
    }
}

// Entry point to run the visualizer
public class Program
{
    public static void Main()
    {
        MemoryVisualizer.Run();
    }
}
