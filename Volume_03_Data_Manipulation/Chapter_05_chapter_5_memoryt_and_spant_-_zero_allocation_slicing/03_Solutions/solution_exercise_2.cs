
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

// Source File: solution_exercise_2.cs
// Description: Solution for Exercise 2
// ==========================================

using System.Collections.Generic;

namespace Book3_Chapter5_PracticalExercises
{
    public class Exercise2_Solution
    {
        public class GraphNode
        {
            public string Id;
            public List<GraphNode> Neighbors;

            public GraphNode(string id)
            {
                Id = id;
                Neighbors = new List<GraphNode>();
            }
        }

        public class GraphTraversal
        {
            public List<string> BFS(GraphNode startNode)
            {
                List<string> visitedOrder = new List<string>();
                
                // Queue manages the frontier of exploration (FIFO)
                Queue<GraphNode> queue = new Queue<GraphNode>();
                
                // HashSet provides O(1) check if a node was already processed
                HashSet<GraphNode> visited = new HashSet<GraphNode>();

                queue.Enqueue(startNode);
                visited.Add(startNode);

                while (queue.Count > 0)
                {
                    GraphNode current = queue.Dequeue();
                    visitedOrder.Add(current.Id);

                    // Iterate through neighbors
                    foreach (var neighbor in current.Neighbors)
                    {
                        // Check existence in HashSet is O(1)
                        if (!visited.Contains(neighbor))
                        {
                            visited.Add(neighbor);
                            queue.Enqueue(neighbor);
                        }
                    }
                }

                return visitedOrder;
            }
        }
    }
}
