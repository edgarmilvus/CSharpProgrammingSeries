
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

// Source File: solution_exercise_3.cs
// Description: Solution for Exercise 3
// ==========================================

using System;
using System.Collections.Generic;

// A simple struct to hold the item and its priority
public struct PrioritizedItem<T>
{
    public T Item;
    public int Priority; // Lower integer = Higher priority
}

public class PriorityQueue<T>
{
    private List<PrioritizedItem<T>> _items = new List<PrioritizedItem<T>>();

    public void Enqueue(T item, int priority)
    {
        _items.Add(new PrioritizedItem<T> { Item = item, Priority = priority });
    }

    public T Dequeue()
    {
        if (_items.Count == 0) throw new InvalidOperationException("Queue is empty");

        int minIndex = 0;
        int minPriority = _items[0].Priority;

        // Linear scan to find the item with lowest priority value
        for (int i = 1; i < _items.Count; i++)
        {
            if (_items[i].Priority < minPriority)
            {
                minPriority = _items[i].Priority;
                minIndex = i;
            }
        }

        T result = _items[minIndex].Item;
        _items.RemoveAt(minIndex);
        return result;
    }

    public int Count => _items.Count;
}

public class GraphTraversal
{
    // Adjacency list: Node -> List of (Neighbor, Weight)
    private Dictionary<int, List<Tuple<int, int>>> _adjacencyList;

    public GraphTraversal()
    {
        _adjacencyList = new Dictionary<int, List<Tuple<int, int>>>();
    }

    public void AddEdge(int from, int to, int weight)
    {
        if (!_adjacencyList.ContainsKey(from)) _adjacencyList[from] = new List<Tuple<int, int>>();
        _adjacencyList[from].Add(new Tuple<int, int>(to, weight));
    }

    // Simulate Dijkstra's Algorithm using our Custom PriorityQueue
    public void FindShortestPath(int startNode, int targetNode)
    {
        // Priority Queue stores (Node, Current Cost)
        var pq = new PriorityQueue<int>();
        pq.Enqueue(startNode, 0);

        // Keep track of visited nodes to avoid cycles (simplified for this exercise)
        HashSet<int> visited = new HashSet<int>();

        Console.WriteLine($"Starting traversal from {startNode}...");

        while (pq.Count > 0)
        {
            // Dequeue the node with the lowest cost
            int currentNode = pq.Dequeue();

            if (visited.Contains(currentNode)) continue;
            visited.Add(currentNode);

            Console.WriteLine($"Visiting Node {currentNode}");

            if (currentNode == targetNode)
            {
                Console.WriteLine("Target reached!");
                return;
            }

            // Explore neighbors
            if (_adjacencyList.ContainsKey(currentNode))
            {
                foreach (var edge in _adjacencyList[currentNode])
                {
                    int neighbor = edge.Item1;
                    int weight = edge.Item2;
                    
                    // In a real Dijkstra, we track total distance. 
                    // Here we simulate priority by adding weight to a dummy accumulator 
                    // or just using weight as priority for demonstration.
                    // For this exercise, we enqueue neighbor with the edge weight as priority.
                    pq.Enqueue(neighbor, weight);
                }
            }
        }
    }
}

public class Exercise3Runner
{
    public static void Run()
    {
        var graph = new GraphTraversal();
        // Graph: 0 -> 1 (cost 4), 0 -> 2 (cost 1)
        //        1 -> 3 (cost 2)
        //        2 -> 1 (cost 2), 2 -> 3 (cost 5)
        graph.AddEdge(0, 1, 4);
        graph.AddEdge(0, 2, 1);
        graph.AddEdge(1, 3, 2);
        graph.AddEdge(2, 1, 2);
        graph.AddEdge(2, 3, 5);

        // The PriorityQueue ensures we process the lowest cost edges first (conceptually)
        // Note: In this simplified version, we don't track cumulative cost, just priority order.
        graph.FindShortestPath(0, 3);
    }
}
