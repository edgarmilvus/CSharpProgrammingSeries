
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

using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Text;

public class TraceVisualizer
{
    private readonly LlmLogContext _context;

    public TraceVisualizer(LlmLogContext context)
    {
        _context = context;
    }

    // 1. Data Retrieval
    public ChainExecution? GetExecutionTrace(Guid executionId)
    {
        // Eager loading of the hierarchy.
        // We use .AsSplitQuery() to avoid cartesian explosion if a step has many children/tags.
        return _context.ChainExecutions
            .Include(e => e.Steps)
                .ThenInclude(s => s.Children) // Recursive nesting
            .Include(e => e.Steps)
                .ThenInclude(s => s.Tags)     // Load tags for coloring
            .AsSplitQuery()
            .FirstOrDefault(e => e.Id == executionId);
    }

    // 2. Graph Generation
    public string GenerateDotGraph(ChainExecution execution)
    {
        var sb = new StringBuilder();
        
        sb.AppendLine("digraph ExecutionTrace {");
        sb.AppendLine("  rankdir=TB;"); // Top to Bottom layout
        sb.AppendLine("  node [fontname=\"Arial\"];");

        if (execution.Steps != null)
        {
            foreach (var step in execution.Steps)
            {
                AppendNodeRecursive(sb, step);
            }
        }

        sb.AppendLine("}");
        return sb.ToString();
    }

    private void AppendNodeRecursive(StringBuilder sb, Step step)
    {
        // Define Node ID (sanitize Guid for Graphviz)
        var nodeId = $"step_{step.StepId.ToString("N")}";
        
        // Determine Shape
        string shape = step.StepType == "LLM" ? "ellipse" : "box";
        
        // Determine Color based on Duration
        string color = "black";
        if (step.DurationMs > 500) color = "red";
        else if (step.DurationMs < 100) color = "green";

        // Construct Label
        var label = $"\"{step.Name}\\n{step.DurationMs}ms\"";

        // Node Definition
        sb.AppendLine($"  {nodeId} [label={label}, shape={shape}, color={color}];");

        // Handle Parent-Child Edges
        if (step.Children != null)
        {
            foreach (var child in step.Children)
            {
                var childId = $"step_{child.StepId.ToString("N")}";
                sb.AppendLine($"  {nodeId} -> {childId};");
                
                // Recursive call for depth
                AppendNodeRecursive(sb, child);
            }
        }
    }
}
