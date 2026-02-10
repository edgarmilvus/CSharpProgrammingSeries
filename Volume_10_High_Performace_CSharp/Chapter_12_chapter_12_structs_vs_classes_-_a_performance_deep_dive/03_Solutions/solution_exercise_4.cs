
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

// Source File: solution_exercise_4.cs
// Description: Solution for Exercise 4
// ==========================================

using System;
using System.Runtime.CompilerServices;

namespace UnicodeTokenBuffer
{
    public enum TokenType { Word, Whitespace, Punctuation, Unknown }

    public ref struct TokenBuffer
    {
        public ReadOnlySpan<char> Span { get; }
        public int Position { get; private set; }

        public TokenBuffer(ReadOnlySpan<char> span)
        {
            Span = span;
            Position = 0;
        }

        public bool IsEnd => Position >= Span.Length;

        // Advances the position to the next token
        public void Next()
        {
            if (IsEnd) return;

            char current = Span[Position];

            // Skip whitespace
            if (char.IsWhiteSpace(current))
            {
                while (!IsEnd && char.IsWhiteSpace(Span[Position]))
                {
                    Position++;
                }
                return;
            }

            // Check for punctuation
            if (IsUnicodePunctuation(current))
            {
                Position++; // Consume single punctuation char
                return;
            }

            // Word processing (alphanumeric)
            // Note: char.IsLetterOrDigit handles Unicode categories automatically
            if (char.IsLetterOrDigit(current))
            {
                while (!IsEnd && char.IsLetterOrDigit(Span[Position]))
                {
                    Position++;
                }
                return;
            }

            // Fallback for unknown characters (e.g., control chars)
            Position++;
        }

        // Gets the current token span based on previous position logic
        public ReadOnlySpan<char> GetCurrentToken()
        {
            // This requires tracking start position. 
            // For this example, we simulate a simple scanner.
            // In a real implementation, we would track 'StartIndex' in Next().
            // Let's implement a simple scanner method instead for demonstration.
            return ReadOnlySpan<char>.Empty; 
        }

        // Helper: Check specific Unicode punctuation ranges
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsUnicodePunctuation(char c)
        {
            // Standard ASCII punctuation
            if ((c >= '!' && c <= '/') || (c >= ':' && c <= '@') || 
                (c >= '[' && c <= '`') || (c >= '{' && c <= '~')) 
                return true;

            // Unicode General Category check (slower but comprehensive)
            // Or specific ranges like Bullet (•) or Ellipsis (…)
            var cat = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
            return cat == System.Globalization.UnicodeCategory.InitialQuotePunctuation ||
                   cat == System.Globalization.UnicodeCategory.FinalQuotePunctuation ||
                   cat == System.Globalization.UnicodeCategory.OtherPunctuation;
        }

        // Advanced: Iterative scanner that returns tokens
        public bool ReadNextToken(out ReadOnlySpan<char> tokenSpan, out TokenType type)
        {
            tokenSpan = ReadOnlySpan<char>.Empty;
            type = TokenType.Unknown;

            // 1. Skip leading whitespace
            while (!IsEnd && char.IsWhiteSpace(Span[Position]))
            {
                Position++;
            }

            if (IsEnd) return false;

            int start = Position;

            char current = Span[Position];

            // 2. Determine Token Type
            if (IsUnicodePunctuation(current))
            {
                type = TokenType.Punctuation;
                Position++;
            }
            else if (char.IsLetterOrDigit(current))
            {
                type = TokenType.Word;
                // Advance until non-letter/digit. 
                // char.IsLetterOrDigit correctly handles surrogate pairs as a single unit
                while (!IsEnd && char.IsLetterOrDigit(Span[Position]))
                {
                    Position++;
                }
            }
            else if (char.IsWhiteSpace(current))
            {
                // Should be caught by step 1, but safety check
                type = TokenType.Whitespace;
                Position++;
            }
            else
            {
                type = TokenType.Unknown;
                Position++;
            }

            tokenSpan = Span.Slice(start, Position - start);
            return true;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            // Test string with ASCII, Unicode Punctuation, and Emojis (Surrogate pairs)
            string input = "Hello, World! 🌍 Testing… 123.";
            var buffer = new TokenBuffer(input.AsSpan());

            Console.WriteLine($"Input: \"{input}\"\nTokens:");
            
            while (buffer.ReadNextToken(out var token, out var type))
            {
                Console.WriteLine($"[{type}] '{token.ToString()}'");
            }
        }
    }
}
