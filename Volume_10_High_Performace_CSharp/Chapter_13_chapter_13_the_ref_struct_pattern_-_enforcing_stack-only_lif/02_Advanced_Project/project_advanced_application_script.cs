
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
using System.Text;

namespace HighPerformanceTokenProcessing
{
    // REAL-WORLD CONTEXT:
    // In AI token processing, we frequently parse raw text streams into discrete tokens (words, punctuation).
    // This happens in tight loops where memory allocation overhead is unacceptable.
    // We will use 'ref struct' to enforce stack-only allocation, ensuring zero heap allocations and zero GC pressure.
    class Program
    {
        static void Main(string[] args)
        {
            // 1. INPUT DATA: Simulating a stream of text from an AI model.
            // We use a standard string here, but we will treat it as a sequence of characters (ReadOnlySpan).
            string rawText = "AI models process tokens efficiently. Ref structs help!";

            // 2. PARSER INITIALIZATION:
            // We instantiate our Tokenizer. Since it is a ref struct, it lives entirely on the stack.
            // It holds a reference to the input data without copying it.
            Tokenizer tokenizer = new Tokenizer(rawText.AsSpan());

            // 3. PROCESSING LOOP:
            // We iterate through tokens. Notice we do not allocate strings or arrays here.
            // The 'GetNextToken' method returns a 'Token' ref struct, which is also stack-allocated.
            Console.WriteLine("--- Parsed Tokens ---");
            while (tokenizer.TryGetNextToken(out Token token))
            {
                // We must convert the token span to a string only for display purposes.
                // In a real high-performance pipeline, we would process the Span<byte> directly.
                string tokenValue = token.Text.ToString();
                Console.WriteLine($"Token: '{tokenValue}' | Type: {token.Type} | Length: {token.Text.Length}");
            }
        }
    }

    // DEFINITION: Token Type Enum
    // Simple classification for our tokens.
    public enum TokenType
    {
        Word,
        Punctuation,
        Whitespace,
        Unknown
    }

    // DEFINITION: The 'ref struct' Pattern
    // CRITICAL CONSTRAINT: A ref struct cannot be boxed, cannot be a field in a class, 
    // and cannot be used in async methods. This guarantees it lives on the stack.
    public ref struct Token
    {
        public ReadOnlySpan<char> Text { get; }
        public TokenType Type { get; }

        // Constructor to initialize the immutable token data.
        public Token(ReadOnlySpan<char> text, TokenType type)
        {
            Text = text;
            Type = type;
        }
    }

    // DEFINITION: Tokenizer State Machine
    // This ref struct manages the parsing state and the current position in the input text.
    public ref struct Tokenizer
    {
        private ReadOnlySpan<char> _remainingText;
        private int _currentIndex;

        public Tokenizer(ReadOnlySpan<char> input)
        {
            _remainingText = input;
            _currentIndex = 0;
        }

        // LOGIC: TryGetNextToken
        // This method advances the state of the tokenizer. 
        // It returns false when the input is exhausted.
        public bool TryGetNextToken(out Token token)
        {
            token = default; // Initialize to default state.

            // 1. CHECK BOUNDS:
            // If we have processed all characters, stop.
            if (_currentIndex >= _remainingText.Length)
            {
                return false;
            }

            // 2. IDENTIFY CURRENT CHARACTER:
            // We peek at the current character to decide the token type.
            char currentChar = _remainingText[_currentIndex];

            // 3. DEFINE BOUNDARIES:
            // We need to find the start and end of the current token.
            int tokenStart = _currentIndex;
            int tokenLength = 1; // Default to single character if unknown.

            // LOGIC: Word Detection (Alphanumeric)
            if (char.IsLetterOrDigit(currentChar))
            {
                // Advance until we hit a non-letter/digit.
                int scanIndex = _currentIndex + 1;
                while (scanIndex < _remainingText.Length && char.IsLetterOrDigit(_remainingText[scanIndex]))
                {
                    scanIndex++;
                }
                tokenLength = scanIndex - tokenStart;
                token = new Token(_remainingText.Slice(tokenStart, tokenLength), TokenType.Word);
            }
            // LOGIC: Punctuation Detection
            else if (char.IsPunctuation(currentChar))
            {
                // In this simple parser, we treat each punctuation mark as a separate token.
                token = new Token(_remainingText.Slice(tokenStart, 1), TokenType.Punctuation);
            }
            // LOGIC: Whitespace Detection
            else if (char.IsWhiteSpace(currentChar))
            {
                // Advance until we hit a non-whitespace.
                int scanIndex = _currentIndex + 1;
                while (scanIndex < _remainingText.Length && char.IsWhiteSpace(_remainingText[scanIndex]))
                {
                    scanIndex++;
                }
                tokenLength = scanIndex - tokenStart;
                token = new Token(_remainingText.Slice(tokenStart, tokenLength), TokenType.Whitespace);
            }
            // LOGIC: Fallback
            else
            {
                token = new Token(_remainingText.Slice(tokenStart, 1), TokenType.Unknown);
            }

            // 4. UPDATE STATE:
            // Advance the current index for the next call.
            _currentIndex += tokenLength;

            return true;
        }
    }
}
