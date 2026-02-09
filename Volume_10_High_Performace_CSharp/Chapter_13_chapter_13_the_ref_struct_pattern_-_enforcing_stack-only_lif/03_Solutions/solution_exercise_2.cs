
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

# Source File: solution_exercise_2.cs
# Description: Solution for Exercise 2
# ==========================================

using System;

public ref struct TokenNormalizer
{
    // Since the logic is stateless, we can use static methods.
    // However, keeping it as a ref struct satisfies the exercise requirement.
    
    public static Span<byte> NormalizeInPlace(Span<byte> tokenBuffer)
    {
        // 1. Convert lowercase ASCII to uppercase
        // We iterate directly over the span.
        for (int i = 0; i < tokenBuffer.Length; i++)
        {
            byte b = tokenBuffer[i];
            // Check for 'a' (0x61) through 'z' (0x7A)
            if (b >= 0x61 && b <= 0x7A)
            {
                // Subtract 0x20 to convert to uppercase (ASCII specific)
                tokenBuffer[i] = (byte)(b - 0x20);
            }
        }

        // 2. Remove trailing whitespace (0x20)
        int newLength = tokenBuffer.Length;
        while (newLength > 0 && tokenBuffer[newLength - 1] == 0x20)
        {
            newLength--;
        }

        // Return the sliced span. This does not allocate; it just updates the pointer/length.
        return tokenBuffer.Slice(0, newLength);
    }
}

public static class NormalizerTestHarness
{
    public static void RunTest()
    {
        // Create a buffer large enough to hold our test data plus padding
        byte[] heapBuffer = new byte[] { 
            (byte)'h', (byte)'e', (byte)'l', (byte)'l', (byte)'o', 
            0x20, 0x20 // Trailing spaces
        };

        // Create a slice. This is a view into the array, not a copy.
        // We pass the slice to the method.
        Span<byte> slice = heapBuffer.AsSpan();

        Console.WriteLine($"Original Array Content: {System.Text.Encoding.UTF8.GetString(heapBuffer)}");
        Console.WriteLine($"Original Array Length: {heapBuffer.Length}");

        // Perform zero-allocation normalization
        Span<byte> result = TokenNormalizer.NormalizeInPlace(slice);

        Console.WriteLine($"Resulting Span Content: {System.Text.Encoding.UTF8.GetString(result)}");
        Console.WriteLine($"Resulting Span Length: {result.Length}");
        
        // Verify the original array was modified in place
        Console.WriteLine($"Array Modified? {heapBuffer[0] == (byte)'H'}"); // Should be true
    }
}
