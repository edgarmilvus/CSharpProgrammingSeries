
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

// Source File: theory_theoretical_foundations_part5.cs
// Description: Theoretical Foundations
// ==========================================

public struct Node { public int Id; public float Weight; }

public void BfsTraversal(Span<Node> storageBuffer, int startNodeId)
{
    // We use the span as a backing store for our queue logic
    // This avoids the overhead of the Queue<T> class wrapper and resizing
    int head = 0;
    int tail = 0;
    
    storageBuffer[tail++] = new Node { Id = startNodeId, Weight = 1.0f };
    
    while (head < tail)
    {
        Node current = storageBuffer[head++];
        
        // Simulate visiting neighbors
        // In a real scenario, we would calculate neighbors and add them to storageBuffer[tail++]
        // ensuring we don't exceed storageBuffer.Length
    }
}
