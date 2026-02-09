
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

# Source File: solution_exercise_3.cs
# Description: Solution for Exercise 3
# ==========================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Yaml;

namespace Exercise3_DynamicConstraints
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // 1. Updated YAML with 'constraints' array and Handlebars iterator
            string yamlContent = @"
name: TravelItineraryPlanner
description: Generates a travel itinerary based on destination and constraints.
input_parameters:
  - name: destination
    type: string
    required: true
  - name: duration
    type: string
    required: true
  - name: constraints
    type: array
    required: false
template: |
  Create a {{duration}} travel itinerary for {{destination}}.
  
  {{#each constraints}}
  - Special constraint: {{this}}
  {{/each}}
  
  Ensure the itinerary respects all constraints listed above.
";

            var kernel = Kernel.CreateBuilder().Build();

            // 2. Function to generate itinerary with constraints
            await GenerateItineraryWithConstraints(kernel, "Paris", "3 days", new List<string> { "vegetarian", "budget-friendly" });
            
            // 3. Test with empty constraints
            await GenerateItineraryWithConstraints(kernel, "Tokyo", "5 days", new List<string>());
        }

        static async Task GenerateItineraryWithConstraints(Kernel kernel, string destination, string duration, List<string> constraints)
        {
            Console.WriteLine($"\n--- Planning for {destination} ({duration}) with {constraints.Count} constraints ---");

            try
            {
                var function = KernelPromptTemplateFactory.CreateFunctionFromYaml(yamlContent);
                
                // 4. Map List<string> to the YAML parameter
                // Note: The Semantic Kernel argument mapping handles IEnumerable as arrays for Handlebars
                var arguments = new KernelArguments
                {
                    ["destination"] = destination,
                    ["duration"] = duration,
                    ["constraints"] = constraints 
                };

                var result = await kernel.InvokeAsync(function, arguments);
                Console.WriteLine(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}
