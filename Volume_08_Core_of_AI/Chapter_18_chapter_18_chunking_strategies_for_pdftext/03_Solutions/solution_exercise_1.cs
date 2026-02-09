
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

# Source File: solution_exercise_1.cs
# Description: Solution for Exercise 1
# ==========================================

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace ChunkingExercises
{
    public class FixedSizeChunker
    {
        /// <summary>
        /// Splits text into chunks based on max token count with an overlapping region.
        /// </summary>
        /// <param name="text">The input text to chunk.</param>
        /// <param name="maxTokens">Maximum tokens per chunk.</param>
        /// <param name="overlapTokens">Number of tokens to overlap between consecutive chunks.</param>
        /// <returns>A list of text chunks.</returns>
        public static List<string> SplitTextWithOverlap(string text, int maxTokens, int overlapTokens)
        {
            if (string.IsNullOrWhiteSpace(text))
                return new List<string>();

            // 1. Tokenize the entire text
            var tokens = GPT3Tokenizer.Encode(text);
            var chunks = new List<string>();

            // 2. Validate overlap constraint
            if (overlapTokens >= maxTokens)
            {
                throw new ArgumentException("Overlap tokens must be less than max tokens.");
            }

            int currentIndex = 0;

            while (currentIndex < tokens.Count)
            {
                // Determine the end index for the current chunk
                int endIndex = Math.Min(currentIndex + maxTokens, tokens.Count);
                
                // Extract the token slice for this chunk
                var chunkTokens = tokens.GetRange(currentIndex, endIndex - currentIndex);
                
                // Decode tokens back to text
                string chunkText = GPT3Tokenizer.Decode(chunkTokens);
                chunks.Add(chunkText);

                // If we've reached the end of the text, break
                if (endIndex == tokens.Count) break;

                // Calculate the next starting index (applying overlap)
                // We move forward by (maxTokens - overlapTokens)
                int step = maxTokens - overlapTokens;
                
                // Ensure we don't get stuck in an infinite loop if step is 0 or negative
                if (step <= 0) step = 1; 

                currentIndex += step;
            }

            return chunks;
        }

        // Helper method to simulate reading a file (since we can't access the file system directly in this snippet)
        public static string GetSampleLegalText()
        {
            return @"In consideration of the mutual covenants contained herein, the parties hereby agree as follows. 
            Section 1: Definitions. 'Agreement' refers to this document. 'Party' refers to either the client or the service provider. 
            Section 2: Confidentiality. Both parties agree to maintain strict confidentiality regarding all proprietary information exchanged during the term of this agreement. 
            Breach of confidentiality shall result in immediate termination and potential legal action for damages. 
            Section 3: Termination. This agreement may be terminated by either party with 30 days written notice. 
            Upon termination, all outstanding payments become immediately due. 
            Section 4: Governing Law. This agreement shall be governed by the laws of the State of New York.";
        }
    }

    // Entry point for the console application
    public class Program
    {
        public static void Main(string[] args)
        {
            string text = FixedSizeChunker.GetSampleLegalText();
            
            // Configuration
            int maxTokens = 50;
            int overlapTokens = 10;

            Console.WriteLine($"Processing text with MaxTokens: {maxTokens}, Overlap: {overlapTokens}\n");

            try
            {
                var chunks = FixedSizeChunker.SplitTextWithOverlap(text, maxTokens, overlapTokens);

                for (int i = 0; i < chunks.Count; i++)
                {
                    // Re-calculate token count for verification/display
                    var tokenCount = GPT3Tokenizer.Encode(chunks[i]).Count;
                    Console.WriteLine($"--- Chunk {i + 1} ---");
                    Console.WriteLine($"Token Count: {tokenCount}");
                    Console.WriteLine($"Content: {chunks[i].Trim()}\n");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}
