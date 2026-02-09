
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

# Source File: theory_theoretical_foundations_part2.cs
# Description: Theoretical Foundations
# ==========================================

// Conceptual High-Performance Pattern
char[] buffer = ArrayPool<char>.Shared.Rent(4096);
int position = 0;

try {
    foreach (var token in tokens) {
        // Copy token into buffer
        // Check bounds, handle resizing if needed (returning old buffer, renting new one)
        token.CopyTo(buffer.AsSpan(position));
        position += token.Length;
        
        // If buffer is full, flush to HTTP stream and reset
        if (position > buffer.Length - maxTokenSize) {
            await response.WriteAsync(buffer, 0, position);
            position = 0;
        }
    }
    // Final flush
    if (position > 0) await response.WriteAsync(buffer, 0, position);
}
finally {
    // CRITICAL: Always return the buffer, even if exceptions occur
    ArrayPool<char>.Shared.Return(buffer);
}
