
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
using System.Threading.Tasks;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Memory;
using Microsoft.SemanticKernel.Connectors.AI.OpenAI;
using Microsoft.SemanticKernel.Plugins.Memory;

public class LegalEmailProcessor
{
    // Main entry point for the application
    public static async Task Main(string[] args)
    {
        // 1. Initialize the Kernel with necessary services
        // We use OpenAI for text generation and a volatile memory store for demonstration
        var kernel = new KernelBuilder()
            .WithOpenAIChatCompletionService("gpt-4", Environment.GetEnvironmentVariable("OPENAI_API_KEY"))
            .WithOpenAITextEmbeddingGenerationService("text-embedding-ada-002", Environment.GetEnvironmentVariable("OPENAI_API_KEY"))
            .WithMemoryStorage(new VolatileMemoryStore())
            .Build();

        // 2. Define the input data (Simulating an incoming email)
        string emailContent = @"
            Subject: Urgent: Contract Breach on Project Alpha
            Dear Legal Team,
            We have identified a significant breach of contract by Vendor X regarding the delivery dates 
            specified in the agreement signed on 2023-10-15. The clause 4.2 regarding late penalties 
            needs immediate review. Please advise on next steps.
            Sincerely, John Doe (Operations Manager)
        ";

        Console.WriteLine("--- Processing Incoming Email ---");
        Console.WriteLine(emailContent);
        Console.WriteLine("---------------------------------\n");

        // 3. Ingest the raw text into Kernel Memory
        // This handles chunking, embedding, and storing the data for retrieval
        var memoryCollectionName = "legal_emails";
        var emailId = Guid.NewGuid().ToString();
        
        await kernel.Memory.SaveInformationAsync(
            collection: memoryCollectionName,
            id: emailId,
            text: emailContent,
            description: "Legal dispute regarding contract breach"
        );

        // 4. Extract Key Entities using a Native Plugin
        // We define a local class to handle specific logic
        var extractionPlugin = kernel.ImportPluginFromObject(new EmailAnalysisPlugin());

        // 5. Execute the Agentic Workflow
        // Step A: Extract specific entities (Date and Clause)
        var context = new KernelArguments { ["email"] = emailContent };
        var extractionResult = await kernel.InvokeAsync(extractionPlugin["ExtractEntities"], context);
        
        // Step B: Retrieve relevant context from memory (Semantic Search)
        // We search for "contract breach" to find related clauses or precedents
        var searchQuery = "contract breach clause 4.2";
        var memoryResults = kernel.Memory.SearchAsync(memoryCollectionName, searchQuery, limit: 1);

        string retrievedContext = "";
        await foreach (var item in memoryResults)
        {
            retrievedContext = item.Metadata.Text;
        }

        // Step C: Synthesize a final summary using the LLM
        // Combining extracted entities, retrieved context, and the original email
        var summaryPlugin = kernel.ImportPluginFromObject(new SummaryGeneratorPlugin());
        var summaryArgs = new KernelArguments 
        {
            ["entities"] = extractionResult.ToString(),
            ["context"] = retrievedContext,
            ["email"] = emailContent
        };
        
        var finalReport = await kernel.InvokeAsync(summaryPlugin["GenerateReport"], summaryArgs);

        Console.WriteLine("\n--- Final Legal Report ---");
        Console.WriteLine(finalReport.ToString());
    }
}

// Plugin 1: Handles extraction of specific data points from text
public class EmailAnalysisPlugin
{
    // Note: In a production scenario, this might call a specialized Named Entity Recognition model.
    // Here, we simulate extraction logic using Kernel functions.
    [KernelFunction]
    public string ExtractEntities([KernelFunctionName] string email)
    {
        // Basic parsing logic (avoiding complex RegEx for brevity, focusing on flow)
        string date = "Not Found";
        string clause = "Not Found";

        if (email.Contains("2023-10-15")) date = "2023-10-15";
        if (email.Contains("clause 4.2")) clause = "Clause 4.2";

        return $"Date: {date}, Clause: {clause}";
    }
}

// Plugin 2: Handles the final synthesis of the report
public class SummaryGeneratorPlugin
{
    [KernelFunction]
    public async Task<string> GenerateReport(
        [KernelFunctionName] string entities, 
        [KernelFunctionName] string context,
        [KernelFunctionName] string email,
        Kernel kernel)
    {
        // Construct a prompt to the LLM to synthesize the data
        string prompt = $@"
            You are a legal assistant. Summarize the following legal issue.
            Extracted Entities: {entities}
            Retrieved Precedent/Context: {context}
            Original Email: {email}
            
            Generate a concise report for the case file.
        ";

        // Use the kernel's chat completion service directly
        var result = await kernel.GetRequiredService<IChatCompletionService>()
            .GetChatMessageContentsAsync(prompt);
            
        return result[0].Content;
    }
}
