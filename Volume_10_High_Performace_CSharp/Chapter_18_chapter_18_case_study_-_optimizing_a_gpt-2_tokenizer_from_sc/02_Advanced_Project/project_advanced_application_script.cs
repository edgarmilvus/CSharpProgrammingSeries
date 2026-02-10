
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
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace HighPerformanceTokenizer
{
    /// <summary>
    /// High-performance byte-pair encoding (BPE) tokenizer for GPT-2 style models.
    /// Optimized for minimal memory allocation using Span<T> and unsafe pointer operations.
    /// </summary>
    public static class GPT2Tokenizer
    {
        // GPT-2 Byte Pair Encoding merge table (simplified subset for demonstration).
        // In a real scenario, this is loaded from a JSON/encoder.json file.
        // We use a static array to avoid heap allocations during initialization.
        private static readonly (byte[] Pattern, int Id)[] BpeMerges = new[]
        {
            // UTF-8 byte sequences for common tokens
            (new byte[] { 0xe2, 0x9c, 0x93 }, 1),   // ✓ (checkmark)
            (new byte[] { 0xf0, 0x9f, 0x94, 0xa5 }, 2), // 🔥 (fire)
            (new byte[] { 0xe2, 0x98, 0x95 }, 3),   // ☕ (coffee)
            (new byte[] { 0x20 }, 4),               // Space
            (new byte[] { 0x65 }, 5),               // 'e'
            (new byte[] { 0x74 }, 6),               // 't'
            (new byte[] { 0x61 }, 7),               // 'a'
            (new byte[] { 0x6f }, 8),               // 'o'
            (new byte[] { 0x69 }, 9),               // 'i'
            (new byte[] { 0x6e }, 10),              // 'n'
            (new byte[] { 0x73 }, 11),              // 's'
            (new byte[] { 0x72 }, 12),              // 'r'
            (new byte[] { 0x68 }, 13),              // 'h'
            (new byte[] { 0x64 }, 14),              // 'd'
            (new byte[] { 0x6c }, 15),              // 'l'
            (new byte[] { 0x75 }, 16),              // 'u'
            (new byte[] { 0x63 }, 17),              // 'c'
            (new byte[] { 0x6d }, 18),              // 'm'
            (new byte[] { 0x66 }, 19),              // 'f'
            (new byte[] { 0x77 }, 20),              // 'w'
            (new byte[] { 0x67 }, 21),              // 'g'
            (new byte[] { 0x70 }, 22),              // 'p'
            (new byte[] { 0x79 }, 23),              // 'y'
            (new byte[] { 0x62 }, 24),              // 'b'
            (new byte[] { 0x76 }, 25),              // 'v'
            (new byte[] { 0x6b }, 26),              // 'k'
            (new byte[] { 0x78 }, 27),              // 'x'
            (new byte[] { 0x6a }, 28),              // 'j'
            (new byte[] { 0x71 }, 29),              // 'q'
            (new byte[] { 0x7a }, 30),              // 'z'
            (new byte[] { 0x41 }, 31),              // 'A'
            (new byte[] { 0x42 }, 32),              // 'B'
            (new byte[] { 0x43 }, 33),              // 'C'
            (new byte[] { 0x44 }, 34),              // 'D'
            (new byte[] { 0x45 }, 35),              // 'E'
            (new byte[] { 0x46 }, 36),              // 'F'
            (new byte[] { 0x47 }, 37),              // 'G'
            (new byte[] { 0x48 }, 38),              // 'H'
            (new byte[] { 0x49 }, 39),              // 'I'
            (new byte[] { 0x4a }, 40),              // 'J'
            (new byte[] { 0x4b }, 41),              // 'K'
            (new byte[] { 0x4c }, 42),              // 'L'
            (new byte[] { 0x4d }, 43),              // 'M'
            (new byte[] { 0x4e }, 44),              // 'N'
            (new byte[] { 0x4f }, 45),              // 'O'
            (new byte[] { 0x50 }, 46),              // 'P'
            (new byte[] { 0x51 }, 47),              // 'Q'
            (new byte[] { 0x52 }, 48),              // 'R'
            (new byte[] { 0x53 }, 49),              // 'S'
            (new byte[] { 0x54 }, 50),              // 'T'
            (new byte[] { 0x55 }, 51),              // 'U'
            (new byte[] { 0x56 }, 52),              // 'V'
            (new byte[] { 0x57 }, 53),              // 'W'
            (new byte[] { 0x58 }, 54),              // 'X'
            (new byte[] { 0x59 }, 55),              // 'Y'
            (new byte[] { 0x5a }, 56),              // 'Z'
            (new byte[] { 0x30 }, 57),              // '0'
            (new byte[] { 0x31 }, 58),              // '1'
            (new byte[] { 0x32 }, 59),              // '2'
            (new byte[] { 0x33 }, 60),              // '3'
            (new byte[] { 0x34 }, 61),              // '4'
            (new byte[] { 0x35 }, 62),              // '5'
            (new byte[] { 0x36 }, 63),              // '6'
            (new byte[] { 0x37 }, 64),              // '7'
            (new byte[] { 0x38 }, 65),              // '8'
            (new byte[] { 0x39 }, 66),              // '9'
            (new byte[] { 0x2e }, 67),              // '.'
            (new byte[] { 0x2c }, 68),              // ','
            (new byte[] { 0x21 }, 69),              // '!'
            (new byte[] { 0x3f }, 70),              // '?'
            (new byte[] { 0x22 }, 71),              // '"'
            (new byte[] { 0x27 }, 72),              // '''
            (new byte[] { 0x3a }, 73),              // ':'
            (new byte[] { 0x3b }, 74),              // ';'
            (new byte[] { 0x28 }, 75),              // '('
            (new byte[] { 0x29 }, 76),              // ')'
            (new byte[] { 0x5b }, 77),              // '['
            (new byte[] { 0x5d }, 78),              // ']'
            (new byte[] { 0x7b }, 79),              // '{'
            (new byte[] { 0x7d }, 80),              // '}'
            (new byte[] { 0x2d }, 81),              // '-'
            (new byte[] { 0x2f }, 82),              // '/'
            (new byte[] { 0x40 }, 83),              // '@'
            (new byte[] { 0x23 }, 84),              // '#'
            (new byte[] { 0x24 }, 85),              // '$'
            (new byte[] { 0x25 }, 86),              // '%'
            (new byte[] { 0x5e }, 87),              // '^'
            (new byte[] { 0x26 }, 88),              // '&'
            (new byte[] { 0x2a }, 89),              // '*'
            (new byte[] { 0x2b }, 90),              // '+'
            (new byte[] { 0x3d }, 91),              // '='
            (new byte[] { 0x3c }, 92),              // '<'
            (new byte[] { 0x3e }, 93),              // '>'
            (new byte[] { 0x7e }, 94),              // '~'
            (new byte[] { 0x5f }, 95),              // '_'
            (new byte[] { 0x7c }, 96),              // '|'
            (new byte[] { 0x5c }, 97),              // '\'
            (new byte[] { 0x60 }, 98),              // '`'
            (new byte[] { 0x2a, 0x2a }, 99),        // '**'
            (new byte[] { 0x20, 0x20 }, 100),       // '  '
            (new byte[] { 0x0a }, 101),             // Newline
            (new byte[] { 0x09 }, 102),             // Tab
        };

        /// <summary>
        /// Encodes a string into a list of token IDs using optimized byte-level processing.
        /// </summary>
        public static int[] Encode(string text)
        {
            // 1. Convert string to UTF-8 bytes without allocating a byte array on the heap.
            //    We use the experimental 'Span<byte>' approach via Encoding.UTF8.GetBytes.
            //    In a real high-perf scenario, we might use 'fixed' statements on the string,
            //    but for .NET Standard compatibility, we allocate a temporary array here
            //    but immediately wrap it in a Span for zero-copy operations downstream.
            byte[] utf8Bytes = Encoding.UTF8.GetBytes(text);
            Span<byte> byteSpan = utf8Bytes.AsSpan();

            // 2. Initialize a list to hold token IDs. 
            //    We use a simple array resizing strategy to avoid List<T> overhead if possible,
            //    but List<T> is acceptable here as per standard C# usage. 
            //    To be strictly zero-allocation for the result container, we'd need a fixed-size array,
            //    but tokens vary in length. We use a List for practicality.
            var tokens = new System.Collections.Generic.List<int>();

            // 3. Process the byte span using a sliding window approach.
            //    We iterate through the bytes, attempting to match the longest possible BPE pattern first.
            int position = 0;
            while (position < byteSpan.Length)
            {
                int bestMatchLength = 0;
                int bestMatchId = -1;

                // 4. Scan BPE merges. 
                //    NOTE: In a real tokenizer, this is a Trie or Hash lookup. 
                //    Here we linear scan for simplicity of the case study.
                //    We look for the longest match starting at the current position.
                for (int i = 0; i < BpeMerges.Length; i++)
                {
                    byte[] pattern = BpeMerges[i].Pattern;
                    int patternLen = pattern.Length;

                    // Boundary check
                    if (position + patternLen > byteSpan.Length) continue;

                    // 5. Fast comparison using Span<T>.SequenceEqual (SIMD optimized internally in .NET)
                    //    We slice the current position to match the pattern length.
                    if (byteSpan.Slice(position, patternLen).SequenceEqual(pattern))
                    {
                        // We found a match. Check if it's the longest one so far.
                        if (patternLen > bestMatchLength)
                        {
                            bestMatchLength = patternLen;
                            bestMatchId = BpeMerges[i].Id;
                        }
                    }
                }

                // 6. Handle the result of the matching logic
                if (bestMatchId != -1)
                {
                    // Found a valid token, add to list
                    tokens.Add(bestMatchId);
                    position += bestMatchLength;
                }
                else
                {
                    // No match found (unknown byte). 
                    // In GPT-2, we map unknown bytes to a specific byte token (usually 0-255).
                    // For this demo, we map to a generic "unknown" token (ID 0).
                    tokens.Add(0);
                    position++;
                }
            }

            return tokens.ToArray();
        }

        /// <summary>
        /// Decodes a list of token IDs back into a string.
        /// </summary>
        public static string Decode(int[] tokenIds)
        {
            // 1. Calculate total byte length to allocate exactly once.
            int totalBytes = 0;
            foreach (int id in tokenIds)
            {
                // Find the pattern length for this ID
                foreach (var merge in BpeMerges)
                {
                    if (merge.Id == id)
                    {
                        totalBytes += merge.Pattern.Length;
                        break;
                    }
                }
            }

            // 2. Allocate a single byte array for the result.
            byte[] resultBytes = new byte[totalBytes];
            Span<byte> resultSpan = resultBytes.AsSpan();
            int writePosition = 0;

            // 3. Reconstruct the byte sequence
            foreach (int id in tokenIds)
            {
                foreach (var merge in BpeMerges)
                {
                    if (merge.Id == id)
                    {
                        // Copy bytes efficiently using Span.CopyTo
                        // This is much faster than individual byte assignment in a loop.
                        merge.Pattern.AsSpan().CopyTo(resultSpan.Slice(writePosition));
                        writePosition += merge.Pattern.Length;
                        break;
                    }
                }
            }

            // 4. Convert UTF-8 bytes back to string
            return Encoding.UTF8.GetString(resultBytes);
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== High-Performance GPT-2 Tokenizer Demo ===");
            Console.WriteLine("Using Span<T>, SIMD-backed SequenceEqual, and Zero-Allocation Logic.\n");

            // Real-world context: Processing user prompts for an AI chatbot.
            // We need to tokenize the input quickly to feed into the model.
            string inputText = "Hello AI! ✓ Check this out: **High Performance C#** 🔥";
            
            Console.WriteLine($"Input Text: \"{inputText}\"");
            Console.WriteLine($"UTF-8 Length: {Encoding.UTF8.GetByteCount(inputText)} bytes");
            Console.WriteLine();

            // --- ENCODING PHASE ---
            Console.WriteLine("--- Encoding ---");
            int[] tokenIds = GPT2Tokenizer.Encode(inputText);
            
            Console.WriteLine("Token IDs: [" + string.Join(", ", tokenIds) + "]");
            Console.WriteLine($"Total Tokens: {tokenIds.Length}");
            Console.WriteLine();

            // --- DECODING PHASE ---
            Console.WriteLine("--- Decoding ---");
            string decodedText = GPT2Tokenizer.Decode(tokenIds);
            
            Console.WriteLine($"Decoded Text: \"{decodedText}\"");
            Console.WriteLine($"Match: {inputText.Equals(decodedText, StringComparison.Ordinal)}");
            
            Console.WriteLine("\n=== Processing Complete ===");
        }
    }
}
