
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

using System;
using System.Collections.Generic;

public class LRUCache<K, V>
{
    // Internal class to link Dictionary keys to LinkedList nodes
    private class CacheNode
    {
        public K Key;
        public V Value;
        public LinkedListNode<K> ListNode; // Reference to the node in the usage list
    }

    private Dictionary<K, CacheNode> _map;
    private LinkedList<K> _usageList; // Head = MRU, Tail = LRU
    private int _capacity;

    public LRUCache(int capacity)
    {
        _capacity = capacity;
        _map = new Dictionary<K, CacheNode>(capacity);
        _usageList = new LinkedList<K>();
    }

    public V Get(K key)
    {
        if (_map.TryGetValue(key, out CacheNode node))
        {
            // Access occurred: Move to front (MRU)
            MarkMostRecentlyUsed(node);
            return node.Value;
        }
        return default(V); 
    }

    public void Put(K key, V value)
    {
        if (_map.TryGetValue(key, out CacheNode existingNode))
        {
            // Key exists: Update value and move to front
            existingNode.Value = value;
            MarkMostRecentlyUsed(existingNode);
        }
        else
        {
            // New Key: Check capacity
            if (_map.Count >= _capacity)
            {
                EvictLeastRecentlyUsed();
            }

            // Create new node
            CacheNode newNode = new CacheNode { Key = key, Value = value };
            // Add to front of Linked List
            newNode.ListNode = _usageList.AddFirst(key);
            
            _map.Add(key, newNode);
        }
    }

    private void MarkMostRecentlyUsed(CacheNode node)
    {
        // Optimization: LinkedList.Remove(node) is O(1) because we have the reference.
        _usageList.Remove(node.ListNode);
        // Add to head
        _usageList.AddFirst(node.ListNode);
    }

    private void EvictLeastRecentlyUsed()
    {
        // Tail is the LRU
        LinkedListNode<K> lruNode = _usageList.Last;
        if (lruNode != null)
        {
            _usageList.Remove(lruNode);
            // Remove from Dictionary using the Key stored in the Linked List node
            _map.Remove(lruNode.Value);
        }
    }
}
