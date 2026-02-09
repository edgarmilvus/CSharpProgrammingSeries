
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

namespace LoraVisualization
{
    public class LoraGraphVizGenerator
    {
        public string GenerateDotDiagram()
        {
            var sb = new StringBuilder();

            // Graph Header
            sb.AppendLine("digraph LoraArchitecture {");
            sb.AppendLine("  rankdir=TB;"); // Top to Bottom
            sb.AppendLine("  node [fontname=\"Arial\", fontsize=10];");
            sb.AppendLine("  edge [fontname=\"Arial\", fontsize=8];");
            
            // Subgraph for Base Model (Left side, distinct color)
            sb.AppendLine("  subgraph cluster_base {");
            sb.AppendLine("    label = \"Base Model Path\";");
            sb.AppendLine("    style = filled;");
            sb.AppendLine("    color = lightgrey;");
            
            sb.AppendLine("    Input [shape=ellipse, style=filled, fillcolor=white];");
            sb.AppendLine("    BaseWeight [shape=box, label=\"W_base (MatMul)\", style=filled, fillcolor=lightblue];");
            sb.AppendLine("    BaseOutput [shape=ellipse, label=\"Base Output\", style=filled, fillcolor=white];");
            
            sb.AppendLine("    Input -> BaseWeight;");
            sb.AppendLine("    BaseWeight -> BaseOutput;");
            sb.AppendLine("  }");

            // Subgraph for LoRA Adapter (Right side, distinct color)
            sb.AppendLine("  subgraph cluster_lora {");
            sb.AppendLine("    label = \"LoRA Adapter Path\";");
            sb.AppendLine("    style = filled;");
            sb.AppendLine("    color = lightblue;");
            
            sb.AppendLine("    LoraA [shape=box, label=\"Lora_A (r=64)\", style=filled, fillcolor=orange];");
            sb.AppendLine("    LoraB [shape=box, label=\"Lora_B (r=64)\", style=filled, fillcolor=orange];");
            sb.AppendLine("    Scale [shape=ellipse, label=\"Scale (α/r)\", style=filled, fillcolor=yellow];");
            
            // Connections within LoRA path
            sb.AppendLine("    Input -> LoraA [style=dashed];"); // Input shared
            sb.AppendLine("    LoraA -> LoraB;");
            sb.AppendLine("    LoraB -> Scale;");
            sb.AppendLine("  }");

            // Merging Node (Outside clusters)
            sb.AppendLine("  Add [shape=ellipse, label=\"Add (+)\", style=filled, fillcolor=lightgreen];");
            
            // Final Output
            sb.AppendLine("  FinalOutput [shape=box, label=\"Hidden States\", style=filled, fillcolor=white];");

            // Connecting the paths to the Merge
            sb.AppendLine("  BaseOutput -> Add;");
            sb.AppendLine("  Scale -> Add;");
            sb.AppendLine("  Add -> FinalOutput;");

            // Styling specific edges
            sb.AppendLine("  BaseWeight -> Add [color=blue, penwidth=2.0];");
            sb.AppendLine("  Scale -> Add [color=orange, penwidth=2.0];");

            sb.AppendLine("}");

            return sb.ToString();
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var generator = new LoraGraphVizGenerator();
            string dotDiagram = generator.GenerateDotDiagram();
            
            Console.WriteLine("--- Graphviz DOT Code ---");
            Console.WriteLine(dotDiagram);
            Console.WriteLine("\n--- Render Instructions ---");
            Console.WriteLine("Copy the code above into a Graphviz viewer (e.g., graphviz.org or VS Code extension) to see the diagram.");
        }
    }
}
