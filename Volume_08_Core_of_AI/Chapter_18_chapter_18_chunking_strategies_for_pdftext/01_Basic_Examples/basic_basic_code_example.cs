
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

// Source File: basic_basic_code_example.cs
// Description: Basic Code Example
// ==========================================

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.Text;
using System.Text.Json;

// ==========================================
// SCENARIO: The "Legal Brief" Aggregator
// ==========================================
// Problem: A junior associate needs to quickly understand the key clauses in a 50-page PDF contract.
// Manual reading is time-consuming. We want to feed the text into an AI, but the entire document
// exceeds the context window of standard LLMs (e.g., 4k/8k/128k tokens).
// Solution: We use Semantic Kernel's TextChunker to split the document into manageable pieces
// that preserve context, allowing an AI agent to analyze each section individually.

namespace ChunkingDemo
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // 1. SETUP: Initialize the Semantic Kernel
            // We use a dummy key here because we aren't calling an LLM, just using the Kernel's utilities.
            var kernel = Kernel.CreateBuilder()
                .AddOpenAIChatCompletion("gpt-4", "fake-api-key") 
                .Build();

            // 2. DATA: Simulate a large PDF text extraction
            // In a real app, this would come from a PDF parser like PdfPig or Azure Document Intelligence.
            // We create a long string with paragraphs to demonstrate chunking boundaries.
            string rawText = GenerateMockLegalText();

            Console.WriteLine("--- ORIGINAL TEXT (Simulated PDF Extraction) ---");
            Console.WriteLine(rawText.Substring(0, Math.Min(500, rawText.Length)) + "...\n");
            Console.WriteLine($"Total Length: {rawText.Length} characters\n");

            // 3. CHUNKING: Apply the TextChunker
            // We will use the 'RecursiveCharacter' strategy, which tries to split by paragraphs,
            // then lines, then spaces, then characters, ensuring semantic units stay together as long as possible.
            var chunker = new Microsoft.SemanticKernel.Text.TextChunker();
            
            // Configuration:
            // - MaxTokens: 100 (Small for demonstration, usually 500-1000 for RAG)
            // - ChunkingStrategy: Recursive (Smart splitting)
            // - Overlap: 15 tokens (To prevent cutting sentences in half)
            int maxTokensPerChunk = 100;
            int overlapTokens = 15;

            // The Split method returns a list of strings (chunks).
            // Note: We pass a custom tokenizer. In production, you'd use the specific model's tokenizer (e.g., Tiktoken).
            // For this demo, we use a simple whitespace tokenizer approximation.
            var chunks = chunker.Split(
                rawText, 
                maxTokensPerChunk, 
                overlapTokens, 
                new SimpleTokenizer()
            );

            // 4. OUTPUT: Display the chunks
            Console.WriteLine($"--- CHUNKING RESULTS (Strategy: Recursive, MaxTokens: {maxTokensPerChunk}, Overlap: {overlapTokens}) ---\n");

            for (int i = 0; i < chunks.Count; i++)
            {
                Console.WriteLine($"[CHUNK {i + 1}]");
                Console.WriteLine($"Length: {chunks[i].Length} chars");
                Console.WriteLine("Content:");
                Console.WriteLine(chunks[i]);
                Console.WriteLine(new string('-', 40));
            }

            // 5. AGENTIC PATTERN: Simulate processing chunks
            // In a real agentic workflow, these chunks would be passed to a loop of AI calls.
            Console.WriteLine("\n--- AGENTIC SIMULATION ---");
            await ProcessChunksWithAgent(chunks);
        }

        // Helper: Generates a long text simulating a legal contract
        static string GenerateMockLegalText()
        {
            var sb = new StringBuilder();
            sb.AppendLine("SECTION 1: DEFINITIONS");
            sb.AppendLine("1.1 'Agreement' refers to this Master Service Agreement between the Parties.");
            sb.AppendLine("1.2 'Confidential Information' means any data disclosed that is marked confidential.");
            sb.AppendLine("1.3 'Effective Date' is the date first written above.");
            sb.AppendLine();
            sb.AppendLine("SECTION 2: SERVICES");
            sb.AppendLine("2.1 The Provider agrees to deliver the services specified in Exhibit A.");
            sb.AppendLine("2.2 Service levels are guaranteed at 99.9% uptime, excluding scheduled maintenance.");
            sb.AppendLine("2.3 The Client shall provide necessary access to systems within 5 business days.");
            sb.AppendLine();
            sb.AppendLine("SECTION 3: PAYMENT");
            sb.AppendLine("3.1 Fees are due net 30 days from invoice date.");
            sb.AppendLine("3.2 Late payments incur a 1.5% monthly interest charge.");
            sb.AppendLine("3.3 All fees are non-refundable unless termination is caused by Provider breach.");
            sb.AppendLine();
            sb.AppendLine("SECTION 4: TERMINATION");
            sb.AppendLine("4.1 Either party may terminate with 60 days written notice.");
            sb.AppendLine("4.2 Immediate termination is allowed for breach of confidentiality.");
            sb.AppendLine("4.3 Upon termination, all Confidential Information must be returned or destroyed.");
            sb.AppendLine();
            sb.AppendLine("SECTION 5: LIABILITY");
            sb.AppendLine("5.1 Provider liability is capped at the total fees paid in the preceding 12 months.");
            sb.AppendLine("5.2 Neither party is liable for indirect or consequential damages.");
            sb.AppendLine("5.3 This limitation applies even if a remedy fails of its essential purpose.");
            sb.AppendLine();
            // Repeat sections to simulate a long document
            for (int i = 0; i < 5; i++)
            {
                sb.AppendLine($"APPENDIX {i + 1}: TECHNICAL SPECIFICATIONS");
                sb.AppendLine($"Specification {i + 1} requires compliance with ISO 27001 standards.");
                sb.AppendLine($"Audit logs must be retained for a minimum of {365 + (i * 10)} days.");
                sb.AppendLine("Data encryption at rest must use AES-256 bit algorithms.");
                sb.AppendLine();
            }
            
            return sb.ToString();
        }

        // Helper: Simulates an Agent processing chunks one by one
        static async Task ProcessChunksWithAgent(List<string> chunks)
        {
            foreach (var chunk in chunks)
            {
                // In a real scenario, we would call:
                // var result = await kernel.InvokeAsync("SummarizePlugin", "Summarize", new KernelArguments { ["input"] = chunk });
                
                // For this demo, we just simulate the delay and output.
                Console.Write($"Processing chunk of {chunk.Length} chars... ");
                await Task.Delay(200); // Simulate network latency
                Console.WriteLine("Done.");
            }
            Console.WriteLine("All chunks processed by Agent.");
        }
    }

    // ==========================================
    // CUSTOM TOKENIZER (Simple Implementation)
    // ==========================================
    // Semantic Kernel requires a tokenizer to calculate token counts.
    // For this 'Hello World' example, we implement a basic whitespace tokenizer.
    // In production, use: Microsoft.SemanticKernel.Connectors.OpenAI.Tokenizer
    public class SimpleTokenizer : Microsoft.SemanticKernel.Text.ITokenizer
    {
        public int CountTokens(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return 0;
            // Split by whitespace and punctuation to approximate tokens
            var tokens = text.Split(new[] { ' ', '\t', '\n', '\r', '.', ',', ';', ':', '!', '?' }, 
                                    StringSplitOptions.RemoveEmptyEntries);
            return tokens.Length;
        }
    }
}
