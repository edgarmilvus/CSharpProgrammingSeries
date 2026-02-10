
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

using System;
using System.Collections.Generic;

public class LRUCache
{
    // Internal node class for Doubly Linked List
    public class ListNode
    {
        public int Key;
        public int Value;
        public ListNode Prev;
        public ListNode Next;

        public ListNode(int key, int value) { Key = key; Value = value; }
    }

    private Dictionary<int, ListNode> cache;
    private int capacity;
    
    // Dummy head and tail nodes to simplify boundary operations
    private ListNode head;
    private ListNode tail;

    public LRUCache(int capacity)
    {
        this.capacity = capacity;
        this.cache = new Dictionary<int, ListNode>(capacity);
        
        // Initialize dummy nodes
        head = new ListNode(0, 0);
        tail = new ListNode(0, 0);
        head.Next = tail;
        tail.Prev = head;
    }

    // Helper to remove a node from the linked list
    private void RemoveNode(ListNode node)
    {
        ListNode prevNode = node.Prev;
        ListNode nextNode = node.Next;
        prevNode.Next = nextNode;
        nextNode.Prev = prevNode;
    }

    // Helper to add a node right after the head (Most Recently Used)
    private void AddNodeToHead(ListNode node)
    {
        node.Prev = head;
        node.Next = head.Next;
        head.Next.Prev = node;
        head.Next = node;
    }

    // Helper to move a node to the front (update usage)
    private void MoveToHead(ListNode node)
    {
        RemoveNode(node);
        AddNodeToHead(node);
    }

    // Helper to get the tail node (LRU)
    private ListNode GetTailNode()
    {
        return tail.Prev;
    }

    public int Get(int key)
    {
        if (cache.TryGetValue(key, out ListNode node))
        {
            MoveToHead(node);
            return node.Value;
        }
        return -1;
    }

    public void Put(int key, int value)
    {
        // 1. Check if key exists. If so, update value and move to head.
        if (cache.TryGetValue(key, out ListNode existingNode))
        {
            existingNode.Value = value;
            MoveToHead(existingNode);
        }
        else
        {
            // 2. If key does not exist:
            //    a. Check capacity. If full, remove the tail node (LRU).
            if (cache.Count >= capacity)
            {
                ListNode lruNode = GetTailNode();
                if (lruNode != null && lruNode != head) // Ensure we don't remove dummy tail
                {
                    RemoveNode(lruNode);
                    cache.Remove(lruNode.Key);
                }
            }

            //    b. Create new node and add to head.
            ListNode newNode = new ListNode(key, value);
            AddNodeToHead(newNode);
            
            //    c. Add to Dictionary.
            cache[key] = newNode;
        }
    }

    public void PrintCacheState()
    {
        Console.WriteLine("Current Cache State (MRU -> LRU):");
        var curr = head.Next;
        while (curr != tail)
        {
            Console.Write($"[Key:{curr.Key}, Val:{curr.Value}] -> ");
            curr = curr.Next;
        }
        Console.WriteLine("END");
    }
}

public class LRUCacheDemo
{
    public static void Run()
    {
        var lru = new LRUCache(3);
        lru.Put(1, 10); // Cache: [1]
        lru.Put(2, 20); // Cache: [2 -> 1]
        lru.Put(3, 30); // Cache: [3 -> 2 -> 1]
        lru.PrintCacheState();

        lru.Get(1);     // Accessed 1. Cache: [1 -> 3 -> 2]
        lru.PrintCacheState();

        lru.Put(4, 40); // Capacity reached. Evicts 2 (Least Recently Used). Cache: [4 -> 1 -> 3]
        lru.PrintCacheState();
    }
}
