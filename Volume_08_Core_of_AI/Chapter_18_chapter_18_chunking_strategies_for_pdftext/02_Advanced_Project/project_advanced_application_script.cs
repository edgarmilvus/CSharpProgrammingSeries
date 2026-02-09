
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
using System.Collections.Generic;
using System.Text;

namespace ChunkingStrategiesApp
{
    // Real-world context:
    // A legal tech startup processes thousands of PDF contracts daily for a RAG (Retrieval-Augmented Generation) pipeline.
    // The contracts vary in length and structure. To ensure the AI model can accurately retrieve relevant clauses
    // without hitting token limits or losing context, we need a robust chunking strategy.
    // This application simulates the ingestion of a raw text contract and applies a hybrid chunking approach:
    // 1. Semantic Chunking (by logical sections like "Clause" or "Article").
    // 2. Fixed-Size Chunking (for sections that are too large, ensuring they fit within model constraints).
    // This mimics the logic found in Microsoft Semantic Kernel's document connectors but implements the core algorithms manually.

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== Legal Document Chunking Processor ===");
            Console.WriteLine("Initializing text splitter and semantic analyzer...\n");

            // 1. Simulate raw text extraction from a PDF.
            // In a real scenario, this would come from a PDF parser like iTextSharp or PdfPig.
            string rawLegalText = GenerateMockContract();

            // 2. Define our chunking parameters.
            // Max token limit is roughly 4 chars per token (conservative estimate for GPT models).
            int maxTokensPerChunk = 500; 
            int maxCharsPerChunk = maxTokensPerChunk * 4;

            // 3. Step A: Semantic Splitting.
            // We split the document by logical boundaries first (e.g., "ARTICLE X").
            // This preserves the semantic meaning better than arbitrary character cuts.
            List<string> semanticSections = SplitBySemanticBoundaries(rawLegalText);

            Console.WriteLine($"[INFO] Found {semanticSections.Count} semantic sections.");

            // 4. Step B: Recursive / Fixed-Size Splitting.
            // If a semantic section exceeds the max character limit, we split it further
            // using a sliding window approach to preserve overlapping context.
            List<string> finalChunks = new List<string>();

            foreach (var section in semanticSections)
            {
                if (section.Length <= maxCharsPerChunk)
                {
                    finalChunks.Add(section);
                }
                else
                {
                    // Section is too large, apply recursive fixed-size splitting.
                    var subChunks = SplitByFixedSize(section, maxCharsPerChunk, overlapSize: 50);
                    finalChunks.AddRange(subChunks);
                }
            }

            // 5. Output Results
            Console.WriteLine("\n=== Processing Complete ===");
            Console.WriteLine($"Total Chunks Generated: {finalChunks.Count}");
            Console.WriteLine("--------------------------------------------------");

            for (int i = 0; i < finalChunks.Count; i++)
            {
                Console.WriteLine($"CHUNK {i + 1} (Length: {finalChunks[i].Length} chars):");
                // Truncate for display purposes
                string displayText = finalChunks[i].Length > 100 
                    ? finalChunks[i].Substring(0, 100) + "..." 
                    : finalChunks[i];
                Console.WriteLine($"\"{displayText}\"");
                Console.WriteLine("--------------------------------------------------");
            }
        }

        // --- Core Logic Methods ---

        /// <summary>
        /// Simulates a semantic chunker that splits text based on logical document structure.
        /// In Semantic Kernel, this is akin to using the TextChunker with specific separators.
        /// </summary>
        static List<string> SplitBySemanticBoundaries(string text)
        {
            List<string> sections = new List<string>();

            // Define logical separators common in legal docs.
            // Priority: Articles > Clauses > Paragraphs.
            string[] separators = { "ARTICLE ", "CLAUSE ", "\n\n" };

            // We iterate through separators to find the most granular logical split.
            // For this simulation, we will use "ARTICLE " as the primary semantic boundary.
            // This mimics the "RecursiveCharacterTextSplitter" logic where we try splitting by a list of separators.
            
            int startIndex = 0;
            int searchIndex = 0;

            while (searchIndex < text.Length)
            {
                int nextSeparatorIndex = -1;
                string foundSeparator = "";

                // Look for the next occurrence of any separator after the current start index
                foreach (var sep in separators)
                {
                    int idx = text.IndexOf(sep, searchIndex);
                    if (idx != -1 && (nextSeparatorIndex == -1 || idx < nextSeparatorIndex))
                    {
                        nextSeparatorIndex = idx;
                        foundSeparator = sep;
                    }
                }

                if (nextSeparatorIndex != -1)
                {
                    // If the separator is not at the very beginning, we found a chunk.
                    // However, for semantic chunking, we usually want the separator to be the start of the chunk
                    // or part of the previous chunk. Here, we include the separator at the start of the next chunk.
                    
                    // If we are at the start, we don't have a previous chunk yet.
                    if (nextSeparatorIndex > startIndex)
                    {
                        string section = text.Substring(startIndex, nextSeparatorIndex - startIndex);
                        if (!string.IsNullOrWhiteSpace(section))
                        {
                            sections.Add(section.Trim());
                        }
                    }

                    // Move search index past this separator to avoid infinite loops
                    searchIndex = nextSeparatorIndex + foundSeparator.Length;
                    startIndex = nextSeparatorIndex; // Start next chunk at the separator
                }
                else
                {
                    // No more separators found, add the rest of the text.
                    string remaining = text.Substring(startIndex);
                    if (!string.IsNullOrWhiteSpace(remaining))
                    {
                        sections.Add(remaining.Trim());
                    }
                    break;
                }
            }

            return sections;
        }

        /// <summary>
        /// Splits a large string into smaller chunks of a fixed maximum size.
        /// Implements a sliding window with overlap to maintain context between chunks.
        /// </summary>
        static List<string> SplitByFixedSize(string text, int maxChunkSize, int overlapSize)
        {
            List<string> chunks = new List<string>();

            if (string.IsNullOrEmpty(text)) return chunks;

            int currentPos = 0;
            while (currentPos < text.Length)
            {
                // Determine the length of the current chunk.
                // We want to avoid cutting words in half, so we look for the last space within the limit.
                int remainingLength = text.Length - currentPos;
                int currentChunkSize = Math.Min(maxChunkSize, remainingLength);
                
                int endPos = currentPos + currentChunkSize;

                // If we are not at the end of the text, try to find a good break point (space or punctuation)
                if (endPos < text.Length)
                {
                    // Backtrack to find the last space to avoid splitting words
                    while (endPos > currentPos && !char.IsWhiteSpace(text[endPos - 1]))
                    {
                        endPos--;
                    }

                    // If we backtracked too much (e.g., a very long word), just hard cut at max size
                    if (endPos == currentPos)
                    {
                        endPos = currentPos + currentChunkSize;
                    }
                }

                string chunk = text.Substring(currentPos, endPos - currentPos);
                chunks.Add(chunk);

                // Update position for next chunk
                currentPos = endPos;

                // Apply Overlap Logic:
                // If we are moving to the next chunk, we want to "rewind" slightly
                // to include context from the previous chunk.
                if (currentPos < text.Length && overlapSize > 0)
                {
                    // Rewind the current position by the overlap size
                    int rewind = Math.Min(overlapSize, currentPos);
                    
                    // Ensure we don't go back into the previous chunk's territory too much
                    // (Simple sliding window logic)
                    currentPos -= rewind;
                    
                    // However, we must ensure we don't create an infinite loop if overlap > chunk size.
                    // In a real kernel implementation, this is handled by the text splitter's internal buffer.
                    if (currentPos <= 0) currentPos = endPos; // Reset if overlap logic fails
                }
            }

            return chunks;
        }

        // --- Mock Data Generator ---

        static string GenerateMockContract()
        {
            // Generates a dummy legal text to simulate a PDF extraction.
            StringBuilder sb = new StringBuilder();
            
            sb.AppendLine("MASTER SERVICES AGREEMENT");
            sb.AppendLine("This Master Services Agreement (the 'Agreement') is entered into by Party A and Party B.");
            sb.AppendLine("Effective Date: January 1, 2024.");
            sb.AppendLine();

            // Article 1: Definitions (Short, should pass as one chunk)
            sb.AppendLine("ARTICLE 1: DEFINITIONS");
            sb.AppendLine("For the purposes of this Agreement, 'Service' refers to the cloud infrastructure provided.");
            sb.AppendLine("'Confidential Information' includes all non-public data shared between parties.");
            sb.AppendLine("The 'Term' refers to the duration of this contract.");
            sb.AppendLine();

            // Article 2: Scope of Services (Long, will require splitting)
            // We intentionally make this section very long to trigger the fixed-size splitter.
            sb.AppendLine("ARTICLE 2: SCOPE OF SERVICES");
            sb.AppendLine("Provider agrees to deliver comprehensive cloud computing services, including but not limited to storage, processing power, and network bandwidth. ");
            sb.AppendLine("The services shall be available 99.9% of the time as defined in the Service Level Agreement (SLA). ");
            sb.AppendLine("Performance metrics will be monitored monthly. ");
            
            // Repeating text to simulate length
            for (int i = 0; i < 20; i++)
            {
                sb.AppendLine($"Additional operational requirement {i}: The provider must ensure data redundancy across multiple geographic regions to prevent data loss during catastrophic events. ");
            }
            sb.AppendLine("All data must be encrypted at rest and in transit using AES-256 standard.");

            // Article 3: Payment Terms (Medium length)
            sb.AppendLine("ARTICLE 3: PAYMENT TERMS");
            sb.AppendLine("Invoices shall be issued on the first of every month. Payment is due within 30 days.");
            sb.AppendLine("Late payments may incur a 5% interest fee.");
            sb.AppendLine("All prices are exclusive of applicable taxes.");

            return sb.ToString();
        }
    }
}
