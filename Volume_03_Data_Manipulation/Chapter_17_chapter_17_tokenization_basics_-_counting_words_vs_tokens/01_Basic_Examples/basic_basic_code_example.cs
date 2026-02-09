
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

# Source File: basic_basic_code_example.cs
# Description: Basic Code Example
# ==========================================

using System;
using System.Buffers;
using System.Numerics;
using System.Text;

// Example: Zero-Allocation Tokenization and Vectorized Counting
// Context: Processing a high-volume text stream for AI model input.
// Goal: Avoid heap allocations (GC pressure) and leverage SIMD for counting.
public class ZeroAllocationTokenizer
{
    public static void ProcessText()
    {
        // Simulating a large input buffer (e.g., from a file or network stream).
        // In a real scenario, this might be a MemoryMappedFile view or a rented ArrayPool buffer.
        string rawText = "The quick brown fox jumps over the lazy dog. The dog, however, wasn't lazy!";
        
        // 1. Convert to ReadOnlySpan<char> to enable zero-allocation slicing.
        // NO heap allocation for the string data itself; we are just creating a view.
        ReadOnlySpan<char> textSpan = rawText.AsSpan();

        // 2. Rent a buffer from the ArrayPool to store tokens.
        // This avoids 'new byte[]' allocations on the heap.
        // We estimate an upper bound: raw length is usually enough for tokens.
        char[] tokenBuffer = ArrayPool<char>.Shared.Rent(textSpan.Length);
        
        // 3. Tokenize using a stack-allocated span for delimiters (SIMD-friendly).
        // We define punctuation as delimiters.
        // Using stackalloc for small, fixed-size arrays avoids heap allocation entirely.
        Span<char> delimiters = stackalloc char[] { ' ', '.', ',', '!', '?' };

        try
        {
            // Tokenization Logic: Manual loop over Span for zero allocation.
            // We iterate through the textSpan, identifying word boundaries.
            int tokenCount = 0;
            int start = 0;
            
            // We will store tokens as (start index, length) tuples in a stack buffer for this example.
            // In a real high-performance scenario, we might process tokens immediately or stream them.
            Span<(int start, int length)> tokenIndices = stackalloc (int, int)[128]; // Stack allocated indices

            for (int i = 0; i < textSpan.Length; i++)
            {
                char c = textSpan[i];
                bool isDelimiter = false;
                
                // Check against delimiters (SIMD potential here, but simple loop for clarity)
                foreach (char d in delimiters)
                {
                    if (c == d)
                    {
                        isDelimiter = true;
                        break;
                    }
                }

                if (isDelimiter)
                {
                    if (i > start)
                    {
                        // Found a token
                        if (tokenCount < tokenIndices.Length)
                        {
                            tokenIndices[tokenCount] = (start, i - start);
                            tokenCount++;
                        }
                    }
                    start = i + 1;
                }
            }

            // Handle the last token if text doesn't end with a delimiter
            if (start < textSpan.Length)
            {
                if (tokenCount < tokenIndices.Length)
                {
                    tokenIndices[tokenCount] = (start, textSpan.Length - start);
                    tokenCount++;
                }
            }

            // 4. Vectorized Counting (SIMD)
            // Goal: Count vowels in the tokenized text without allocating strings.
            // We process the original textSpan directly using System.Numerics.Vector<T>.
            
            int vowelCount = 0;
            int i = 0;
            
            // Determine if Vector<T> is supported (hardware acceleration)
            int vectorSize = Vector<char>.Count;
            
            // Define vowels for SIMD comparison (SIMD requires loading constants into vectors)
            // We create a vector of 'a' to compare against chunks of the text.
            // Note: Handling case-insensitivity in SIMD requires careful masking or pre-processing.
            // For this example, we count lowercase vowels.
            
            Vector<char> aVec = new Vector<char>('a');
            Vector<char> eVec = new Vector<char>('e');
            Vector<char> iVec = new Vector<char>('i');
            Vector<char> oVec = new Vector<char>('o');
            Vector<char> uVec = new Vector<char>('u');

            // Process the textSpan in Vector-sized chunks
            for (; i <= textSpan.Length - vectorSize; i += vectorSize)
            {
                var chunk = new Vector<char>(textSpan.Slice(i, vectorSize));
                
                // SIMD Comparison: Compare chunk with each vowel vector
                // Vector.Equals returns a mask where matching elements are all 1s (0xFFFF)
                vowelCount += Vector.Sum(Vector.Equals(chunk, aVec));
                vowelCount += Vector.Sum(Vector.Equals(chunk, eVec));
                vowelCount += Vector.Sum(Vector.Equals(chunk, iVec));
                vowelCount += Vector.Sum(Vector.Equals(chunk, oVec));
                vowelCount += Vector.Sum(Vector.Equals(chunk, uVec));
            }

            // Process remaining elements (tail) using a standard loop
            for (; i < textSpan.Length; i++)
            {
                char c = textSpan[i];
                if (c == 'a' || c == 'e' || c == 'i' || c == 'o' || c == 'u')
                {
                    vowelCount++;
                }
            }

            // Output results
            Console.WriteLine($"Token Count: {tokenCount}");
            Console.WriteLine($"Total Vowels (Vectorized): {vowelCount}");
            
            // Demonstrate accessing a specific token without allocation
            // We slice the original span using the indices we found earlier
            if (tokenCount > 0)
            {
                var firstTokenInfo = tokenIndices[0];
                ReadOnlySpan<char> firstToken = textSpan.Slice(firstTokenInfo.start, firstTokenInfo.length);
                Console.Write("First Token: ");
                Console.WriteLine(firstToken); // Console.WriteLine handles ReadOnlySpan<char> efficiently in .NET Core
            }
        }
        finally
        {
            // CRITICAL: Return the rented array to the pool to prevent memory leaks
            // and allow reuse by other parts of the application.
            ArrayPool<char>.Shared.Return(tokenBuffer);
        }
    }
}
