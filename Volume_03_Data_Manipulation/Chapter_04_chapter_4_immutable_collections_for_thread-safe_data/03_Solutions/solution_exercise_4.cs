
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

# Source File: solution_exercise_4.cs
# Description: Solution for Exercise 4
# ==========================================

using System;
using System.Collections.Generic;

public class GraphNode
{
    public string Value { get; set; }
    public List<GraphNode> Neighbors { get; set; }

    public GraphNode(string value)
    {
        Value = value;
        Neighbors = new List<GraphNode>();
    }
}

public class GraphTraversal
{
    public static GraphNode BFS(GraphNode start, string targetValue)
    {
        if (start == null) return null;
        if (start.Value == targetValue) return start;

        // Queue for frontier (FIFO - First In, First Out)
        // Essential for BFS to explore level-by-level
        Queue<GraphNode> queue = new Queue<GraphNode>();
        queue.Enqueue(start);

        // HashSet for visited nodes (O(1) lookup)
        // Essential to handle cycles and avoid infinite loops
        HashSet<GraphNode> visited = new HashSet<GraphNode>();
        visited.Add(start);

        while (queue.Count > 0)
        {
            GraphNode current = queue.Dequeue();

            foreach (var neighbor in current.Neighbors)
            {
                // Check if already visited
                if (visited.Contains(neighbor)) continue;

                // Check target
                if (neighbor.Value == targetValue)
                {
                    return neighbor;
                }

                // Mark visited and enqueue
                visited.Add(neighbor);
                queue.Enqueue(neighbor);
            }
        }

        return null;
    }
}

public class Exercise4Runner
{
    public static void Run()
    {
        Console.WriteLine("\n--- Exercise 4: BFS Graph Traversal ---");
        
        // Construct a graph
        //      A
        //    /   \
        //   B     C
        //  / \     \
        // D   E     F
        var a = new GraphNode("A");
        var b = new GraphNode("B");
        var c = new GraphNode("C");
        var d = new GraphNode("D");
        var e = new GraphNode("E");
        var f = new GraphNode("F");

        a.Neighbors.AddRange(new[] { b, c });
        b.Neighbors.AddRange(new[] { d, e });
        c.Neighbors.Add(f);

        // Search for 'F'
        var result = GraphTraversal.BFS(a, "F");
        
        if (result != null)
        {
            Console.WriteLine($"Found node: {result.Value}");
        }
        else
        {
            Console.WriteLine("Node not found.");
        }

        // Complexity Analysis:
        // Each node is enqueued once.
        // Each edge is checked once (via Neighbors list).
        // Time Complexity: O(V + E) where V is vertices, E is edges.
        // Space Complexity: O(V) (Queue and HashSet can hold all nodes in worst case)
    }
}
