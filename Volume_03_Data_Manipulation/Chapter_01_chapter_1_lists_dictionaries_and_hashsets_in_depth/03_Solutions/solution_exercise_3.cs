
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

# Source File: solution_exercise_3.cs
# Description: Solution for Exercise 3
# ==========================================

using System;
using System.Collections.Generic;

public class GraphTraverser
{
    public static void Run()
    {
        // Adjacency List
        var graph = new Dictionary<string, List<string>>
        {
            { "A", new List<string> { "B", "C" } },
            { "B", new List<string> { "A", "D", "E" } },
            { "C", new List<string> { "A", "F" } },
            { "D", new List<string> { "B" } },
            { "E", new List<string> { "B", "F" } },
            { "F", new List<string> { "C", "E" } },
            { "G", new List<string> { "H" } } // Disconnected component
        };

        Console.WriteLine($"Shortest path from A to F: {BFS(graph, "A", "F")} hops."); // Should be 2 (A->C->F or A->E->F)
        Console.WriteLine($"Shortest path from A to G: {BFS(graph, "A", "G")} hops."); // Should be -1
    }

    public static int BFS(Dictionary<string, List<string>> graph, string start, string target)
    {
        if (start == target) return 0;

        // To track distance, we pair the node with its distance in the queue
        // Note: Tuples are allowed as they are generic types.
        var queueWithDist = new Queue<(string Node, int Dist)>();
        queueWithDist.Enqueue((start, 0));

        // HashSet for visited nodes (prevents cycles)
        var visited = new HashSet<string>();
        visited.Add(start);

        while (queueWithDist.Count > 0)
        {
            var (current, dist) = queueWithDist.Dequeue();

            // Check neighbors
            if (graph.ContainsKey(current))
            {
                foreach (var neighbor in graph[current])
                {
                    if (neighbor == target) return dist + 1;

                    if (!visited.Contains(neighbor))
                    {
                        visited.Add(neighbor);
                        queueWithDist.Enqueue((neighbor, dist + 1));
                    }
                }
            }
        }

        return -1; // Not found
    }
}
