
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

// Source File: solution_exercise_5.cs
// Description: Solution for Exercise 5
// ==========================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ChunkingExercises
{
    public class MultiModalPreprocessor
    {
        // Dictionary to store extracted tables: Key = Placeholder ID, Value = Table Content
        private readonly Dictionary<string, string> _extractedTables = new Dictionary<string, string>();

        public List<string> ProcessPdfText(string rawText)
        {
            Console.WriteLine("--- Step 1: Extracting Tables ---");
            string processedText = ExtractStructures(rawText);

            Console.WriteLine("\n--- Step 2: Chunking Modified Text ---");
            // Using the recursive chunker from Exercise 2 (simulated here with simple splitting for brevity)
            // In a real scenario, pass 'processedText' to TextChunker.SplitPlainTextChunks
            var chunks = SimpleChunker.Split(processedText, 100); 

            Console.WriteLine("\n--- Step 3: Re-injecting Tables ---");
            var finalChunks = ReinjectTables(chunks);

            return finalChunks;
        }

        public string ExtractStructures(string text)
        {
            // Regex to match markdown-style tables or pipe-delimited text
            // Looks for lines starting and ending with |, containing | in between
            string pattern = @"(\|\s?.*?\s?\|(\r?\n\|\s?.*?\s?\|)*)";
            
            int tableIdCounter = 1;
            
            string modifiedText = Regex.Replace(text, pattern, match =>
            {
                string tableContent = match.Value;
                string placeholder = $"[TABLE_ID:{tableIdCounter}]";
                
                _extractedTables[placeholder] = tableContent;
                Console.WriteLine($"Found Table {tableIdCounter}. Replacing with placeholder: {placeholder}");
                
                tableIdCounter++;
                return placeholder;
            });

            return modifiedText;
        }

        public List<string> ReinjectTables(List<string> chunks)
        {
            var finalChunks = new List<string>();

            foreach (var chunk in chunks)
            {
                string finalContent = chunk;
                bool hasTable = false;

                foreach (var kvp in _extractedTables)
                {
                    if (chunk.Contains(kvp.Key))
                    {
                        hasTable = true;
                        
                        // Check size constraint
                        int estimatedTokens = Microsoft.SemanticKernel.Connectors.OpenAI.GPT3Tokenizer.Encode(kvp.Value).Count;
                        
                        // Arbitrary check: if table is > 50% of chunk capacity, warn
                        if (estimatedTokens > 600) // Assuming chunk limit is 1024
                        {
                            Console.WriteLine($"WARNING: Table {kvp.Key} is too large for chunk. Marking as Overflow.");
                            finalContent = finalContent.Replace(kvp.Key, $"\n[OVERFLOW_TABLE: {kvp.Key}]\n");
                        }
                        else
                        {
                            finalContent = finalContent.Replace(kvp.Key, $"\n{ kvp.Value }\n");
                        }
                    }
                }

                if (!hasTable) finalContent = chunk; // No change needed

                finalChunks.Add(finalContent);
            }

            return finalChunks;
        }
    }

    // Helper class for simulation
    public static class SimpleChunker
    {
        public static List<string> Split(string text, int maxChars)
        {
            var chunks = new List<string>();
            int index = 0;
            while (index < text.Length)
            {
                int length = Math.Min(maxChars, text.Length - index);
                chunks.Add(text.Substring(index, length));
                index += length;
            }
            return chunks;
        }
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            // Simulated raw PDF text extraction
            string rawPdfText = @"
            Section 1: Overview.
            The following table lists the system requirements.
            | Component | Requirement |
            |-----------|-------------|
            | CPU       | 2 GHz       |
            | RAM       | 8 GB        |
            End of table.
            Section 2: Installation.
            Please refer to the table above before installing.
            ";

            var preprocessor = new MultiModalPreprocessor();
            var finalChunks = preprocessor.ProcessPdfText(rawPdfText);

            Console.WriteLine("\n--- Final Output ---");
            foreach (var chunk in finalChunks)
            {
                Console.WriteLine($"Chunk: {chunk}");
            }

            Console.WriteLine("\n--- Extracted Tables Log ---");
            foreach (var kvp in preprocessor._extractedTables)
            {
                Console.WriteLine($"{kvp.Key}:\n{kvp.Value}");
            }
        }
    }
}
