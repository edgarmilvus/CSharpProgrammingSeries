
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

public class HistoryManager<T>
{
    private LinkedList<T> _history = new LinkedList<T>();
    private LinkedListNode<T> _currentPointer = null;

    public void Push(T state)
    {
        // If we are in the middle of history (due to Undo), 
        // pushing a new state invalidates the "Redo" path (future).
        if (_currentPointer != null && _currentPointer.Next != null)
        {
            var next = _currentPointer.Next;
            while (next != null)
            {
                var toDelete = next;
                next = next.Next;
                _history.Remove(toDelete);
            }
        }

        // Add new state
        _history.AddLast(state);
        _currentPointer = _history.Last;
    }

    public bool CanUndo() => _currentPointer != null && _currentPointer.Previous != null;
    public bool CanRedo() => _currentPointer != null && _currentPointer.Next != null;

    public T Undo()
    {
        if (!CanUndo()) throw new InvalidOperationException("No history to undo.");
        _currentPointer = _currentPointer.Previous;
        return _currentPointer.Value;
    }

    public T Redo()
    {
        if (!CanRedo()) throw new InvalidOperationException("No future state to redo.");
        _currentPointer = _currentPointer.Next;
        return _currentPointer.Value;
    }

    public T GetCurrent() => _currentPointer?.Value;

    // Custom Iterator using yield return
    // This decouples the traversal logic from the internal state management.
    public IEnumerable<T> TraverseHistory()
    {
        LinkedListNode<T> current = _history.First;
        while (current != null)
        {
            yield return current.Value;
            current = current.Next;
        }
    }
}
