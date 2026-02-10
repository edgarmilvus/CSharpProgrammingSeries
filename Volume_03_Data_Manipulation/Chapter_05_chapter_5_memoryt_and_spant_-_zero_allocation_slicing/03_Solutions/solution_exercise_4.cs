
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

// Source File: solution_exercise_4.cs
// Description: Solution for Exercise 4
// ==========================================

using System.Collections.Generic;

namespace Book3_Chapter5_PracticalExercises
{
    public class Exercise4_Solution
    {
        public class SequenceAnalyzer
        {
            public void RunComparison()
            {
                // List: Contiguous memory, O(n) insertion in middle
                List<int> list = new List<int> { 1, 2, 3 };
                // Inserting at index 1 requires shifting elements 2 and 3 to the right.
                // This involves memory copying.
                list.Insert(1, 99); 

                // LinkedList: Non-contiguous (nodes), O(1) insertion given the node
                LinkedList<int> linkedList = new LinkedList<int>();
                var node1 = linkedList.AddFirst(1);
                var node2 = linkedList.AddAfter(node1, 2);
                linkedList.AddAfter(node2, 3);
                
                // Inserting 99 after node1 is O(1). 
                // We only update pointers (node1.Next and new node.Prev).
                linkedList.AddAfter(node1, 99);
            }

            public class LogBuffer
            {
                private LinkedList<string> _logs = new LinkedList<string>();
                private const int MaxLogs = 5;

                public void AddLog(string message)
                {
                    // AddLast is O(1)
                    _logs.AddLast(message);
                    
                    // RemoveFirst is O(1)
                    // Contrast with List.RemoveFirst which is O(n) due to shifting
                    if (_logs.Count > MaxLogs)
                    {
                        _logs.RemoveFirst(); 
                    }
                }

                public IEnumerable<string> GetLogs()
                {
                    // Traversing the linked list using an iterator
                    LinkedListNode<string> current = _logs.First;
                    while (current != null)
                    {
                        yield return current.Value;
                        current = current.Next;
                    }
                }
            }
        }
    }
}
