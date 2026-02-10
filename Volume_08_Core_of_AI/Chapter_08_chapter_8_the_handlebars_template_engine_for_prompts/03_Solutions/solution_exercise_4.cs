
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

using HandlebarsDotNet;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.PromptTemplates.Handlebars;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class RefactoredAgent
{
    public record ToolDefinition(string Name, string Description, string[] Keywords);

    public static async Task RunChallengeAsync()
    {
        var kernel = Kernel.CreateBuilder()
            .AddOpenAIChatCompletion(modelId: "gpt-4", apiKey: Environment.GetEnvironmentVariable("OPENAI_API_KEY"))
            .Build();

        // 1. The Refactored Handlebars Template
        string template = """
            You are an intelligent router. Analyze the user input and select the appropriate tool.
            
            User Input: "{{userInput}}"
            
            Available Tools:
            {{#each tools}}
            - {{Name}}: {{Description}}
            {{/each}}

            Decision Logic:
            {{#each tools}}
                {{#if (contains userInput Keywords)}}
                The user is asking about {{Name}}. You MUST invoke the {{Name}} tool immediately.
                {{/if}}
            {{/each}}
            
            If no tool matches, respond with a clarification question.
            """;

        // 2. Data Setup
        var tools = new List<ToolDefinition>
        {
            new("WeatherPlugin", "Gets the current weather", new[] { "weather", "rain", "sun" }),
            new("StockPlugin", "Gets stock prices", new[] { "stock", "market", "shares" }),
            new("NewsPlugin", "Gets latest news", new[] { "news", "headline", "report" })
        };
        
        string userInput = "What is the weather like today?";

        // 3. Implementation
        // We define a custom factory to register the 'contains' helper.
        var factory = new HandlebarsPromptTemplateFactory((config) =>
        {
            config.Helpers.Add("contains", (writer, context, arguments) =>
            {
                // Arguments: 0 = haystack (userInput), 1 = needle array (keywords)
                if (arguments.Length < 2) return;

                var haystack = arguments[0]?.ToString() ?? "";
                // Handlebars passes arrays as Object[] or similar
                var needles = arguments[1] as IEnumerable<object>;

                if (needles == null) return;

                // Perform the check
                bool match = needles.Any(n => 
                    haystack.Contains(n?.ToString() ?? "", StringComparison.OrdinalIgnoreCase));

                // Handlebars helpers work by writing to the output stream if true.
                // However, in Handlebars.Net, helpers used in {{#if}} blocks 
                // should return a boolean value or use the options.fn/else.
                // The standard way to support {{#if}} with a custom helper is slightly different 
                // in Handlebars.Net, but Semantic Kernel's usage often relies on 
                // basic truthy checks. 
                
                // Correct approach for Handlebars.Net block helpers:
                // We need to use the context of the helper invocation.
                // Note: The HandlebarsDotNet API for block helpers is complex.
                // For simplicity in this exercise, we will assume a simpler function helper 
                // or use the 'truthy' return mechanism if the helper is invoked in a standard {{#if}} context.
                
                // *Correction for Handlebars.Net block logic:*
                // Standard Handlebars.Net requires registering a block helper specifically.
                // But for the sake of this exercise and Semantic Kernel compatibility, 
                // we will implement a simple check that writes "true" if match, 
                // relying on Handlebars' truthy evaluation.
                
                if (match) writer.Write("true"); 
            });
        });

        // Note: The above helper implementation is a simplification. 
        // In a robust Handlebars.Net implementation, we would register a specific BlockHelper.
        // However, Semantic Kernel's abstraction often simplifies this. 
        // If strict block helper behavior is required, we would use:
        // config.BlockHelpers.Add("contains", (output, context, args, templateFn) => { ... });
        
        // Let's stick to a functional approach that works with {{#if}} in Semantic Kernel:
        // We will register a standard helper that returns a boolean via the writer (as string "true") 
        // or rely on the fact that Semantic Kernel might expose specific helpers.
        
        // Re-implementing with a Block Helper for strict correctness:
        factory = new HandlebarsPromptTemplateFactory((config) =>
        {
            config.BlockHelpers.Add("contains", (writer, options, context, arguments) =>
            {
                if (arguments.Length < 2) return;
                var haystack = arguments[0]?.ToString() ?? "";
                var needles = arguments[1] as IEnumerable<object>;

                if (needles != null && needles.Any(n => haystack.Contains(n?.ToString() ?? "", StringComparison.OrdinalIgnoreCase)))
                {
                    // Write the content inside the block if true
                    options.Template(writer, context);
                }
            });
        });

        var config = new PromptTemplateConfig
        {
            Template = template,
            TemplateFormat = "handlebars"
        };

        var templateEngine = await factory.CreateAsync(config);
        
        var kernelArguments = new KernelArguments
        {
            ["userInput"] = userInput,
            ["tools"] = tools
        };

        var result = await templateEngine.RenderAsync(kernel, kernelArguments);
        Console.WriteLine(result);
    }
}
