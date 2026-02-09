
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

using Microsoft.KernelMemory;
using Microsoft.SemanticKernel;
using System;
using System.IO;
using System.Threading.Tasks;

// The main entry point of our application
class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("=== Kernel Memory RAG Example ===\n");

        // 1. SETUP: Initialize the Kernel Memory builder with default settings.
        // By default, this uses volatile memory (RAM) for both text storage and vector embeddings.
        // This is perfect for testing, but in production, you would swap in Azure AI Search and Azure OpenAI.
        var memory = new KernelMemoryBuilder()
            .WithOpenAIDefaults(Environment.GetEnvironmentVariable("OPENAI_API_KEY"))
            .Build();

        // 2. INGESTION: Prepare a dummy document to simulate an employee handbook.
        // In a real scenario, this would be a PDF, Word doc, or Markdown file.
        string documentPath = "EmployeeHandbook.txt";
        await File.WriteAllTextAsync(documentPath, 
            "Welcome to the company! \n" +
            "Vacation Policy: Employees accrue 15 days per year. \n" +
            "IT Support: Contact helpdesk@company.com for issues. \n" +
            "Dress Code: Business casual is required in the office.");

        Console.WriteLine($"[1] Ingesting document: {documentPath}");

        // 3. PROCESSING: Import the document into Kernel Memory.
        // KM automatically chunks the text, generates embeddings, and stores them.
        // We assign a unique ID ("doc001") to reference it later.
        var ingestionResult = await memory.ImportDocumentAsync(
            new Document("doc001")
                .AddFile(documentPath)
        );

        // Wait for the background processing to complete (simulated here by checking status)
        // In a real async system, you might use events or polling.
        while (!await memory.IsDocumentReadyAsync(documentId: "doc001"))
        {
            await Task.Delay(100); // Wait 100ms
        }
        Console.WriteLine("[2] Ingestion complete. Document is indexed.\n");

        // 4. RETRIEVAL: Query the memory for specific information.
        // We ask a question that requires understanding context (semantic search).
        string question = "How many vacation days do I get?";
        Console.WriteLine($"[3] Querying: \"{question}\"");

        var answer = await memory.AskAsync(question);

        // 5. SYNTHESIS: Display the result.
        // The 'AskAsync' method retrieves relevant chunks and uses the LLM to synthesize an answer.
        Console.WriteLine($"\n[4] Result:\n{answer.Result}");

        // Cleanup
        if (File.Exists(documentPath)) File.Delete(documentPath);
    }
}
