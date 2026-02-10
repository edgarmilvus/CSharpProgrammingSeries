
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
using System.Collections.Generic;
using System.Text;

// Modified LazyChain with Visualization capability
public class LazyChain<TInput, TOutput>
{
    private readonly Func<TInput, TOutput> _transformation;
    private readonly List<string> _stepLabels; // Stores labels for visualization

    // Default constructor
    public LazyChain(Func<TInput, TOutput> initialTransformation = null)
    {
        _transformation = initialTransformation ?? (input => (TOutput)(object)input);
        _stepLabels = new List<string>();
    }

    // Private constructor for chaining
    private LazyChain(Func<TInput, TOutput> transformation, List<string> labels)
    {
        _transformation = transformation;
        _stepLabels = labels;
    }

    // Modified Then method to accept a label
    public LazyChain<TInput, TNewOutput> Then<TNewOutput>(Func<TOutput, TNewOutput> nextTransformation, string label)
    {
        Func<TInput, TNewOutput> composedFunc = input => nextTransformation(_transformation(input));
        
        // Clone the list and add the new label
        var newLabels = new List<string>(_stepLabels) { label };

        return new LazyChain<TInput, TNewOutput>(composedFunc, newLabels);
    }

    // Overload for backward compatibility (anonymous steps)
    public LazyChain<TInput, TNewOutput> Then<TNewOutput>(Func<TOutput, TNewOutput> nextTransformation)
    {
        return Then(nextTransformation, $"Step {_stepLabels.Count + 1}");
    }

    public TOutput Run(TInput input)
    {
        return _transformation(input);
    }

    // Generate Graphviz DOT code
    public string Visualize()
    {
        var sb = new StringBuilder();
        sb.AppendLine("digraph Chain {");
        sb.AppendLine("  rankdir=TB;"); // Top to Bottom layout
        sb.AppendLine("  Node [shape=box, style=filled, fillcolor=lightblue];");

        // Start Node
        sb.AppendLine("  Start [label=\"Input: " + typeof(TInput).Name + "\", fillcolor=lightgrey];");

        // Process Nodes
        for (int i = 0; i < _stepLabels.Count; i++)
        {
            sb.AppendLine($"  Step{i+1} [label=\"{ _stepLabels[i] }\"];");
        }

        // End Node
        sb.AppendLine($"  End [label=\"Output: " + typeof(TOutput).Name + "\", fillcolor=lightgrey];");

        // Edges
        sb.AppendLine("  Start -> Step1;");
        for (int i = 1; i < _stepLabels.Count; i++)
        {
            sb.AppendLine($"  Step{i} -> Step{i+1};");
        }
        sb.AppendLine($"  Step{_stepLabels.Count} -> End;");

        sb.AppendLine("}");
        return sb.ToString();
    }
}

public class Program
{
    public static void Main()
    {
        // Create chain with labeled steps
        var chain = new LazyChain<double, double>()
            .Then(x => x * 2, "Multiply by 2")
            .Then(x => x + 10, "Add 10")
            .Then(x => x * x, "Square Result");

        // Run logic
        double result = chain.Run(5.0);
        Console.WriteLine($"Chain Result: {result}"); // Should be 400

        // Visualize logic
        Console.WriteLine("\n--- Graphviz DOT Code ---");
        string dotCode = chain.Visualize();
        Console.WriteLine(dotCode);
        
        // Instructions for rendering
        Console.WriteLine("\n--- Instructions ---");
        Console.WriteLine("Copy the code above into a Graphviz viewer (e.g., graphviz.org or VSCode extension) to see the diagram.");
    }
}
