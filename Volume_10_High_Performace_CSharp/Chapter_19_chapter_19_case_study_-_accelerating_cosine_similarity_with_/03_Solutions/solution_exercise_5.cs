
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

// This is a text string containing the Graphviz DOT syntax.
// To view the diagram, copy the content between the triple backticks 
// and render it using a Graphviz viewer (e.g., viz-js.com, or local Graphviz installation).

public static class MemoryVisualizer
{
    public static string GetDiagramCode()
    {
        return @"
digraph MemoryLayout {
    rankdir=TB;
    node [shape=record, fontname=""Helvetica""];
    
    // 1. The Raw Array
    subgraph cluster_raw {
        label=""Raw Array: float[] data (Length 16)"";
        style=filled;
        color=lightgrey;
        
        // Define nodes for array indices 0-15
        node [width=0.4];
        arr0 [label=""0""];
        arr1 [label=""1""];
        arr2 [label=""2""];
        arr3 [label=""3"""];
        arr4 [label=""4"""];
        arr5 [label=""5"""];
        arr6 [label=""6"""];
        arr7 [label=""7"""];
        arr8 [label=""8"""];
        arr9 [label=""9"""];
        arr10 [label=""10"""];
        arr11 [label=""11"""];
        arr12 [label=""12"""];
        arr13 [label=""13"""];
        arr14 [label=""14"""];
        arr15 [label=""15"""];
        
        // Link them horizontally
        {rank=same; arr0 arr1 arr2 arr3 arr4 arr5 arr6 arr7 arr8 arr9 arr10 arr11 arr12 arr13 arr14 arr15}
    }

    // 2. The Span Slice
    subgraph cluster_span {
        label=""Span<float>: data.AsSpan(4, 8)"";
        style=filled;
        color=lightblue;
        
        span4 [label=""4""];
        span5 [label=""5"""];
        span6 [label=""6"""];
        span7 [label=""7"""];
        span8 [label=""8"""];
        span9 [label=""9"""];
        span10 [label=""10"""];
        span11 [label=""11"""];
        
        {rank=same; span4 span5 span6 span7 span8 span9 span10 span11}
    }

    // 3. SIMD Register (128-bit = 4 floats)
    subgraph cluster_reg {
        label=""SIMD Register (128-bit)"";
        style=filled;
        color=lightgreen;
        
        reg_node [label=""<f0> 0 | <f1> 1 | <f2> 2 | <f3> 3""];
    }

    // 4. Connections / Iterations
    // Iteration 1: Loads indices 4, 5, 6, 7
    arr4 -> reg_node:f0 [color=""red"", style=""dashed"", label=""Load 1""];
    arr5 -> reg_node:f1 [color=""red"", style=""dashed""];
    arr6 -> reg_node:f2 [color=""red"", style=""dashed""];
    arr7 -> reg_node:f3 [color=""red"", style=""dashed""];

    // Iteration 2: Loads indices 8, 9, 10, 11
    arr8 -> reg_node:f0 [color=""blue"", style=""dashed"", label=""Load 2""];
    arr9 -> reg_node:f1 [color=""blue"", style=""dashed""];
    arr10 -> reg_node:f2 [color=""blue"", style=""dashed""];
    arr11 -> reg_node:f3 [color=""blue"", style=""dashed""];

    // Highlight the Slice Boundaries visually
    {rank=same; arr3 span4}
    {rank=same; arr11 span11}
    
    // Annotations
    note1 [label=""Stride = 4 elements"", shape=note];
    note1 -> arr8 [arrowhead=none];
}
";
    }
}
