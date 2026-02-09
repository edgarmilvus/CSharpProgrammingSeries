
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
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Yaml;

namespace Exercise1_YamlValidation
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // 1. Create the YAML content (simulating reading from a file)
            string yamlContent = @"
name: MarketingEmailGenerator
description: Generates a marketing email based on product details.
input_parameters:
  - name: Product
    type: string
    required: true
  - name: Tone
    type: string
    required: true
    enum: [Friendly, Professional, Urgent]
  - name: TargetAudience
    type: string
    required: true
template: |
  Write a {{Tone}} marketing email promoting our {{Product}}.
  The target audience is {{TargetAudience}}.
  Keep it concise and engaging.
";

            // Initialize Semantic Kernel
            var kernel = Kernel.CreateBuilder().Build();

            try
            {
                // 2. Deserialize YAML into KernelFunction
                var factory = new KernelPromptTemplateFactory();
                var function = KernelPromptTemplateFactory.CreateFunctionFromYaml(yamlContent);

                Console.WriteLine("--- Test 1: Valid Arguments ---");
                
                // 3. Invoke with valid arguments
                var result = await kernel.InvokeAsync(function, new KernelArguments
                {
                    ["Product"] = "Eco-Friendly Water Bottle",
                    ["Tone"] = "Friendly",
                    ["TargetAudience"] = "Hikers"
                });

                Console.WriteLine($"Result: {result}");
            }
            catch (ValidationException ex)
            {
                Console.WriteLine($"Validation Error: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"General Error: {ex.Message}");
            }

            Console.WriteLine("\n--- Test 2: Missing Required Parameter ---");

            try
            {
                // 4. Attempt invocation with missing TargetAudience
                var function = KernelPromptTemplateFactory.CreateFunctionFromYaml(yamlContent);
                
                // Intentionally omitting "TargetAudience"
                var result = await kernel.InvokeAsync(function, new KernelArguments
                {
                    ["Product"] = "Eco-Friendly Water Bottle",
                    ["Tone"] = "Friendly"
                });
                
                Console.WriteLine($"Result: {result}");
            }
            catch (ValidationException ex)
            {
                // 5. Catch validation exception
                Console.WriteLine($"Validation Error Caught: {ex.Message}");
            }
        }
    }
}
