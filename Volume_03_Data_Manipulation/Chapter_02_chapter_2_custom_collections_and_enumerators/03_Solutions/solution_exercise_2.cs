
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

# Source File: solution_exercise_2.cs
# Description: Solution for Exercise 2
# ==========================================

using System;
using System.Collections.Generic;

public class Suspect
{
    public string Name { get; set; }
    public List<Suspect> Connections { get; set; } = new List<Suspect>();
    
    // Essential for HashSet to work correctly on custom objects
    public override int GetHashCode() => Name.GetHashCode();
    public override bool Equals(object obj) => obj is Suspect s && s.Name == Name;
}

public class NetworkTracer
{
    public int FindConnectionDepth(Suspect start, Suspect target)
    {
        if (start == target) return 0;

        // FIFO Queue for BFS. We store a tuple to track the depth (distance) of the node.
        Queue<(Suspect node, int depth)> bfsQueue = new Queue<(Suspect, int)>();
        
        // HashSet to track visited nodes to prevent cycles and redundant processing.
        HashSet<Suspect> visited = new HashSet<Suspect>();

        bfsQueue.Enqueue((start, 0));
        visited.Add(start);

        while (bfsQueue.Count > 0)
        {
            var (current, depth) = bfsQueue.Dequeue();

            foreach (var neighbor in current.Connections)
            {
                // Check if we found the target immediately
                if (neighbor.Equals(target))
                {
                    return depth + 1; // Return distance to neighbor
                }

                // If not visited, add to queue to explore later
                if (!visited.Contains(neighbor))
                {
                    visited.Add(neighbor);
                    bfsQueue.Enqueue((neighbor, depth + 1));
                }
            }
        }

        return -1; // Target not reachable
    }
}
