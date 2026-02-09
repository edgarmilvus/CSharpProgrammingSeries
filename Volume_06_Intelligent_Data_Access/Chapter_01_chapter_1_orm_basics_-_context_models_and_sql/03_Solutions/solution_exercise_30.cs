
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

# Source File: solution_exercise_30.cs
# Description: Solution for Exercise 30
# ==========================================

using System;
using System.Collections.Generic;
using System.Linq;

// 1. Operational Transformation (OT) for Text
public class TextOperation
{
    public int Position { get; set; }
    public string Insert { get; set; }
    public string Delete { get; set; }
}

// 2. Vector Update Strategy
public class CollaborativeVectorEngine
{
    // In a collaborative text editor, multiple users type.
    // The text changes, so the vector must update.
    
    // Conflict: User A types "cat", User B types "dog" at the same time.
    // The text becomes "cdogat" or "catdog".
    
    public Document ApplyOperations(Document doc, List<TextOperation> operations)
    {
        // 1. Apply OT to text
        // Sort operations by position and apply
        foreach (var op in operations.OrderBy(o => o.Position))
        {
            if (!string.IsNullOrEmpty(op.Insert))
            {
                doc.Content = doc.Content.Insert(op.Position, op.Insert);
            }
        }

        // 2. Mark Vector for Update
        // We don't update the vector immediately (too expensive).
        // We mark it "stale".
        doc.VectorStatus = VectorStatus.RecalculationNeeded;

        return doc;
    }

    public async Task<VectorUpdateJob> QueueVectorUpdate(Document doc)
    {
        if (doc.VectorStatus == VectorStatus.RecalculationNeeded)
        {
            // Add to processing queue (see Exercise 5)
            return new VectorUpdateJob { DocumentId = doc.Id, Priority = 1 };
        }
        return null;
    }
}

public enum VectorStatus { Fresh, Stale, RecalculationNeeded }
