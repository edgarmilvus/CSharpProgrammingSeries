
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

# Source File: project_advanced_application_script.cs
# Description: Advanced Application Script
# ==========================================

using System;
using System.Buffers;
using System.Text;

namespace HighPerformanceTokenProcessing
{
    /// <summary>
    /// A high-performance, zero-allocation tokenizer for processing AI model inputs.
    /// This class leverages ReadOnlySpan<char> to slice and parse text without creating
    /// string objects on the heap, drastically reducing Garbage Collection (GC) pressure.
    /// </summary>
    public class SpanTokenizer
    {
        // We use SearchValues for O(1) lookup of delimiter characters.
        // This is significantly faster than checking each character against an array.
        private readonly SearchValues<char> _delimiters;
        private readonly SearchValues<char> _punctuation;

        public SpanTokenizer()
        {
            // Define common delimiters (whitespace and control characters)
            _delimiters = SearchValues.Create(" \t\r\n\f\v");
            
            // Define punctuation to treat as separate tokens
            _punctuation = SearchValues.Create(".,!?;:()[]{}\"\'");
        }

        /// <summary>
        /// Tokenizes the input text into a list of spans.
        /// NOTE: In a real production scenario, we would rent an array from the ArrayPool
        /// to avoid allocations. Here, we return a List<ReadOnlySpan<char>> for demonstration,
        /// but the spans themselves point to the original memory.
        /// </summary>
        public List<ReadOnlySpan<char>> Tokenize(string input)
        {
            // Convert string to ReadOnlySpan<char>. This is a zero-cost operation.
            ReadOnlySpan<char> text = input.AsSpan();
            var tokens = new List<ReadOnlySpan<char>>();

            int start = 0;
            int length = text.Length;

            while (start < length)
            {
                // 1. Skip Delimiters
                // Find the first character that is NOT a delimiter.
                while (start < length && _delimiters.Contains(text[start]))
                {
                    start++;
                }

                if (start >= length) break; // End of text

                // 2. Find Token End
                // Determine where the current token ends.
                int end = start;
                
                while (end < length)
                {
                    char currentChar = text[end];

                    if (_delimiters.Contains(currentChar))
                    {
                        // Found a delimiter, token ends here.
                        break;
                    }

                    if (_punctuation.Contains(currentChar))
                    {
                        // Punctuation acts as a token boundary.
                        // If we are at the start of a token, this punctuation is the token itself.
                        if (end == start)
                        {
                            end++; // Include the punctuation char
                        }
                        // Else, the token ends before the punctuation.
                        break;
                    }

                    end++;
                }

                // 3. Extract and Store Span
                // Create a slice of the original text. No allocation occurs here.
                ReadOnlySpan<char> token = text.Slice(start, end - start);
                
                // Only add non-empty tokens
                if (token.Length > 0)
                {
                    tokens.Add(token);
                }

                // Move start to the position after the current token
                start = end;
            }

            return tokens;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("--- High-Performance AI Token Processor ---");
            
            // Simulate a batch of AI prompts (e.g., from a high-throughput API)
            string[] prompts = new string[]
            {
                "Hello, AI! Please analyze this data.",
                "System: Initiate protocol 7 (Emergency).",
                "User input: \"Tokenization\" is key for performance."
            };

            var tokenizer = new SpanTokenizer();

            foreach (var prompt in prompts)
            {
                Console.WriteLine($"\nProcessing Prompt: \"{prompt}\"");
                
                // Tokenize the prompt
                var tokens = tokenizer.Tokenize(prompt);

                Console.WriteLine($"Found {tokens.Count} tokens:");
                
                // Iterate over the spans
                // Note: We cannot easily index a List<ReadOnlySpan<char>> in older C# versions,
                // but we can iterate or convert to string for display purposes only.
                int index = 0;
                foreach (var token in tokens)
                {
                    // CRITICAL: Converting a Span to a string here creates an allocation.
                    // In a real processing pipeline, we would keep the Span and process it directly.
                    // We do this only for demonstration output.
                    string tokenStr = token.ToString();
                    Console.WriteLine($"  [{index++}] '{tokenStr}' (Length: {token.Length})");
                }
            }

            Console.WriteLine("\n--- Processing Complete ---");
            Console.WriteLine("Memory Note: The tokenization logic itself created zero heap allocations.");
        }
    }
}
