
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
using System.Collections.Immutable;

public class Exercise3Runner
{
    public static void Run()
    {
        Console.WriteLine("\n--- Exercise 3: Structural Sharing in ImmutableList ---");
        
        // 1. Create Builder for efficient batch insertion (O(N log N) total)
        var builder = ImmutableList.CreateBuilder<int>();
        for (int i = 0; i < 100; i++)
        {
            builder.Add(i);
        }
        
        // 2. Create the immutable list
        ImmutableList<int> list1 = builder.ToImmutable();
        
        // 3. Update item 50 (creates a new list root, shares most nodes)
        // ImmutableList is internally a Balanced Binary Tree (usually Red-Black).
        // Updating index 50 requires traversing the tree (O(log N)) and creating new nodes on the path.
        ImmutableList<int> list2 = list1.SetItem(50, 999);

        Console.WriteLine($"List 1 (Original) Item 50: {list1[50]}");
        Console.WriteLine($"List 2 (Modified) Item 50: {list2[50]}");
        
        // Conceptual check: In a full immutable array, we would copy 100 elements (O(N)).
        // In a tree-based immutable list, we only copy the path from root to leaf (log N nodes).
        // The rest of the tree is shared between list1 and list2.
        
        Console.WriteLine("Explanation: 'list2' shares all nodes with 'list1' except the nodes on the path to index 50.");
        Console.WriteLine("This is significantly more memory efficient than copying arrays for large datasets.");
    }
}
