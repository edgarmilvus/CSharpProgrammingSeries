
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

// Source File: solution_exercise_1.cs
// Description: Solution for Exercise 1
// ==========================================

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.PromptTemplates.Handlebars;
using System;
using System.Threading.Tasks;

public class LegalSummarizer
{
    public static async Task RunAsync()
    {
        // 1. Initialize Kernel
        var kernel = Kernel.CreateBuilder()
            .AddOpenAIChatCompletion(modelId: "gpt-4", apiKey: Environment.GetEnvironmentVariable("OPENAI_API_KEY"))
            .Build();

        // 2. Define the Handlebars Template String
        // Note: We use the 'eq' helper (standard in Semantic Kernel's Handlebars integration) 
        // to compare strings safely.
        string template = """
            You are a precise legal assistant. 
            {{#if domain}}
                {{#if (eq domain "Intellectual Property")}}
                    Focus on patent claims, copyright dates, and trademark distinctiveness.
                {{else if (eq domain "Contract Law")}}
                    Focus on obligations, liabilities, termination clauses, and effective dates.
                {{else}}
                    Provide a neutral summary of the key legal points.
                {{/if}}
            {{else}}
                Provide a neutral summary of the key legal points.
            {{/if}}

            Here is the text to summarize:
            {{text}}
            """;

        // 3. Create PromptTemplateConfig and HandlebarsPromptTemplateFactory
        var promptConfig = new PromptTemplateConfig
        {
            Template = template,
            TemplateFormat = "handlebars"
        };

        var factory = new HandlebarsPromptTemplateFactory();

        // 4. Render the prompt for different domains
        
        // Scenario A: Intellectual Property
        var ipArguments = new KernelArguments
        {
            ["domain"] = "Intellectual Property",
            ["text"] = "Patent US-2023-12345 covers a method for AI-assisted code generation."
        };

        var ipPrompt = await factory.CreateAsync(promptConfig).RenderAsync(kernel, ipArguments);
        Console.WriteLine("--- Intellectual Property Output ---");
        Console.WriteLine(ipPrompt);
        Console.WriteLine();

        // Scenario B: Contract Law
        var contractArguments = new KernelArguments
        {
            ["domain"] = "Contract Law",
            ["text"] = "Section 3.1: The Vendor shall deliver goods by Dec 31, 2023."
        };

        var contractPrompt = await factory.CreateAsync(promptConfig).RenderAsync(kernel, contractArguments);
        Console.WriteLine("--- Contract Law Output ---");
        Console.WriteLine(contractPrompt);
        Console.WriteLine();

        // Scenario C: Missing Domain (Edge Case)
        var genericArguments = new KernelArguments
        {
            // "domain" is intentionally omitted
            ["text"] = "The defendant was found liable."
        };

        var genericPrompt = await factory.CreateAsync(promptConfig).RenderAsync(kernel, genericArguments);
        Console.WriteLine("--- Generic Output (Missing Domain) ---");
        Console.WriteLine(genericPrompt);
    }
}
