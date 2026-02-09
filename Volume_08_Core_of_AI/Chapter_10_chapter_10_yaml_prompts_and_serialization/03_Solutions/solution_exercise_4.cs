
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

# Source File: solution_exercise_4.cs
# Description: Solution for Exercise 4
# ==========================================

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Yaml;

namespace Exercise4_ComplexTypes
{
    // 1. C# Classes matching the schema
    public class CodeFile
    {
        public string FileName { get; set; }
        public string Language { get; set; } // C#, Python, JavaScript
        public string SourceCode { get; set; }
    }

    public class ReviewContext
    {
        public List<CodeFile> Files { get; set; }
    }

    class Program
    {
        static async Task Main(string[] args)
        {
            // 2. YAML with JSON Schema for complex validation
            string yamlContent = @"
name: CodeReviewAssistant
description: Reviews code for quality and security.
input_parameters:
  - name: context
    type: object
    required: true
    schema:
      type: object
      properties:
        Files:
          type: array
          items:
            type: object
            properties:
              FileName:
                type: string
              Language:
                type: string
                enum: [C#, Python, JavaScript]
              SourceCode:
                type: string
                minLength: 10
      required:
        - Files
template: |
  Review the following code files:
  {{#each context.Files}}
  File: {{FileName}} ({{Language}})
  Code: {{SourceCode}}
  {{/each}}
  Provide a summary of improvements.
";

            var kernel = Kernel.CreateBuilder().Build();

            // 3. Create and Serialize Complex Object
            var reviewContext = new ReviewContext
            {
                Files = new List<CodeFile>
                {
                    new CodeFile { FileName = "Program.cs", Language = "C#", SourceCode = "public void Main() { Console.WriteLine('Hello'); }" },
                    // Uncomment below to trigger validation error for unsupported language
                    // new CodeFile { FileName = "Legacy.cobol", Language = "COBOL", SourceCode = "IDENTIFICATION DIVISION." }
                }
            };

            // Serialization Strategy: Pass the complex object as a JSON string or rely on Kernel's native mapping.
            // For YAML prompts, passing the serialized JSON is often the most robust way to ensure structure fidelity.
            string contextJson = JsonSerializer.Serialize(reviewContext);

            try
            {
                var function = KernelPromptTemplateFactory.CreateFunctionFromYaml(yamlContent);

                // 4. Invoke with complex data
                // Note: We pass the JSON string. The template accesses it via {{context}} if treated as string,
                // or the Kernel might deserialize it automatically if the schema matches.
                // To ensure compatibility with the YAML schema 'type: object', we pass the object directly.
                var result = await kernel.InvokeAsync(function, new KernelArguments
                {
                    ["context"] = reviewContext 
                });

                Console.WriteLine($"Review Result:\n{result}");
            }
            catch (ValidationException ex)
            {
                Console.WriteLine($"Validation Intercepted: {ex.Message}");
                Console.WriteLine("The LLM was not called because the input failed schema validation.");
            }
        }
    }
}
