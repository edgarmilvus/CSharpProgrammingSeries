
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

// Source File: project_advanced_application_script.cs
// Description: Advanced Application Script
// ==========================================

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace HighPerformanceAITokenProcessing
{
    class Program
    {
        static void Main(string[] args)
        {
            // 1. Setup: Create a synthetic "AI Model Prompt" buffer.
            // In a real scenario, this might come from a network stream or file.
            // We use a char array to simulate text data.
            char[] buffer = new char[1024 * 1024]; // 1 MB buffer
            Random.Shared.NextChars(buffer); // Fill with random text for demonstration
            
            // Inject specific delimiters to test logic
            buffer[100] = '.';
            buffer[1000] = '!';
            buffer[5000] = '?';
            buffer[buffer.Length - 1] = '.';

            Console.WriteLine($"Processing {buffer.Length} characters of text data...");
            Console.WriteLine("--------------------------------------------------");

            // 2. Execution: Run the optimized Span-based processor.
            // We avoid converting the char[] to string to prevent heap allocations.
            ProcessTokens(buffer);

            Console.WriteLine("--------------------------------------------------");
            Console.WriteLine("Processing complete. Zero heap allocations for tokenization.");
        }

        /// <summary>
        /// Processes a buffer of text to identify sentence boundaries and calculate
        /// token statistics using zero-allocation slicing with Span<T>.
        /// </summary>
        static void ProcessTokens(char[] data)
        {
            // Convert array to Span. This is a stack-only view, no allocation.
            ReadOnlySpan<char> view = data.AsSpan();
            
            int sentenceCount = 0;
            int tokenCount = 0;
            int maxTokenLength = 0;

            // 3. Tokenization Loop: Iterate through the buffer.
            // We track the 'start' index of the current token.
            int start = 0;
            
            // Use a ref struct for the enumerator to maintain zero-allocation guarantees.
            // In a real high-performance scenario, we might use a custom struct enumerator,
            // but for clarity, we use a simple index loop here.
            for (int i = 0; i < view.Length; i++)
            {
                char c = view[i];

                // Check for sentence delimiters (simple tokenization logic)
                if (c == '.' || c == '!' || c == '?')
                {
                    // We found a sentence boundary.
                    // The token is the slice from 'start' to 'i'.
                    ReadOnlySpan<char> sentence = view.Slice(start, i - start);

                    // 4. Analysis: Process the slice without allocating strings.
                    if (sentence.Length > 0)
                    {
                        AnalyzeSentence(sentence, ref tokenCount, ref maxTokenLength);
                        sentenceCount++;
                    }

                    // Move start to the character after the delimiter
                    start = i + 1;
                }
            }

            // 5. Reporting: Output the statistics.
            Console.WriteLine($"Total Sentences: {sentenceCount}");
            Console.WriteLine($"Total Tokens (Words): {tokenCount}");
            Console.WriteLine($"Max Token Length: {maxTokenLength} chars");
        }

        /// <summary>
        /// Analyzes a single sentence slice to count words and find the longest word.
        /// Uses SIMD-friendly logic where applicable.
        /// </summary>
        static void AnalyzeSentence(ReadOnlySpan<char> sentence, ref int totalTokenCount, ref int globalMaxTokenLength)
        {
            // We treat ' ' (space) as a word separator.
            // This is a simplified tokenizer for demonstration.
            
            int wordStart = 0;
            bool inWord = false;

            // 6. SIMD Optimization: Check for whitespace using Vector operations.
            // We process the span in chunks (Vector<T>.Count) to minimize branching.
            // Note: Vector.IsHardwareAccelerated checks if SIMD is supported.
            if (Vector.IsHardwareAccelerated && sentence.Length >= Vector<ushort>.Count)
            {
                // We treat char as ushort (System.Char is essentially uint16)
                // We look for the space character ' ' (ASCII 32)
                ushort spaceChar = (ushort)' ';
                var spaceVector = new Vector<ushort>(spaceChar);

                int i = 0;
                int lastWordEnd = 0;

                // Process in SIMD chunks
                for (; i <= sentence.Length - Vector<ushort>.Count; i += Vector<ushort>.Count)
                {
                    var chunk = sentence.Slice(i, Vector<ushort>.Count);
                    
                    // Load data into a Vector. 
                    // Unsafe.As is used to reinterpret the memory layout safely.
                    // This avoids per-element access overhead.
                    var vectorChunk = Unsafe.ReadUnaligned<Vector<ushort>>(ref Unsafe.As<char, ushort>(ref MemoryMarshal.GetReference(chunk)));
                    
                    // Compare with space vector
                    var mask = Vector.Equals(vectorChunk, spaceVector);
                    
                    // If any element in the mask is true (non-zero), we have a space.
                    // In a full production system, we would extract the mask and iterate bits.
                    // For this demo, we fall back to scalar processing if a space is detected in the chunk.
                    if (mask != Vector<ushort>.Zero)
                    {
                        // Fallback to scalar for the chunk containing spaces to keep logic simple
                        // in this constrained example, but the SIMD load/check was done.
                        for (int j = 0; j < Vector<ushort>.Count; j++)
                        {
                            char current = chunk[j];
                            if (current == ' ')
                            {
                                if (inWord)
                                {
                                    int len = (i + j) - wordStart;
                                    UpdateStats(len, ref totalTokenCount, ref globalMaxTokenLength);
                                    inWord = false;
                                }
                            }
                            else
                            {
                                if (!inWord)
                                {
                                    wordStart = i + j;
                                    inWord = true;
                                }
                            }
                        }
                    }
                    else
                    {
                        // No spaces in this chunk. If we were in a word, it continues.
                        // In a real specialized kernel, we would handle run-length encoding here.
                        // For this demo, we just ensure we flag that we are in a word 
                        // if the previous chunk ended in one.
                        if (inWord)
                        {
                             // The word spans across the SIMD boundary.
                             // We don't finalize it here, just let it continue.
                        }
                    }
                }
                
                // Process remaining elements (tail) scalarly
                ProcessScalarTail(sentence, i, ref wordStart, ref inWord, ref totalTokenCount, ref globalMaxTokenLength);
            }
            else
            {
                // Hardware acceleration not available, pure scalar processing
                ProcessScalarTail(sentence, 0, ref wordStart, ref inWord, ref totalTokenCount, ref globalMaxTokenLength);
            }
        }

        /// <summary>
        /// Helper to process characters scalarly (used for tails or non-SIMD paths).
        /// </summary>
        static void ProcessScalarTail(
            ReadOnlySpan<char> sentence, 
            int startIndex, 
            ref int wordStart, 
            ref bool inWord, 
            ref int totalTokenCount, 
            ref int globalMaxTokenLength)
        {
            for (int i = startIndex; i < sentence.Length; i++)
            {
                char c = sentence[i];
                if (c == ' ')
                {
                    if (inWord)
                    {
                        int len = i - wordStart;
                        UpdateStats(len, ref totalTokenCount, ref globalMaxTokenLength);
                        inWord = false;
                    }
                }
                else
                {
                    if (!inWord)
                    {
                        wordStart = i;
                        inWord = true;
                    }
                }
            }

            // Check for word ending at the very end of the sentence
            if (inWord)
            {
                int len = sentence.Length - wordStart;
                UpdateStats(len, ref totalTokenCount, ref globalMaxTokenLength);
            }
        }

        /// <summary>
        /// Updates the running statistics.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void UpdateStats(int length, ref int count, ref int max)
        {
            count++;
            if (length > max)
            {
                max = length;
            }
        }
    }

    // Extension method to fill array with random chars for the demo
    public static class RandomExtensions
    {
        public static void NextChars(this Random random, char[] buffer)
        {
            for (int i = 0; i < buffer.Length; i++)
            {
                // Generate readable ASCII chars (A-Z, a-z, space)
                int r = random.Next(0, 53);
                if (r == 52) buffer[i] = ' ';
                else if (r < 26) buffer[i] = (char)('A' + r);
                else buffer[i] = (char)('a' + (r - 26));
            }
        }
    }
}
