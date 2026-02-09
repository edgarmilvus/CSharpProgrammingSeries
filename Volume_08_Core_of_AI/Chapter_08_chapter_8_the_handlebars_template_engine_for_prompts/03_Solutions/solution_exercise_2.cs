
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

# Source File: solution_exercise_2.cs
# Description: Solution for Exercise 2
# ==========================================

using Microsoft.SemanticKernel.PromptTemplates.Handlebars;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class OrderStatusPrompt
{
    public record OrderItem(string ProductName, int Quantity, decimal Price);

    public static async Task GeneratePromptAsync()
    {
        // 1. Define the Handlebars Template
        string template = """
            User inquiry: "What is in my order?"
            
            Order Details:
            {{#each items}}
            - {{ProductName}}: Qty: {{Quantity}} @ ${{Price}}
            {{/each}}

            Total Value: ${{total}}
            """;

        // 2. Prepare Data
        var orderItems = new List<OrderItem>
        {
            new("Wireless Mouse", 1, 25.99m),
            new("Mechanical Keyboard", 1, 120.50m),
            new("USB-C Cable", 2, 15.00m)
        };
        
        // In a real scenario, total might be calculated in C#, but we pass it as data here.
        decimal total = 161.49m;

        // 3. Render the template
        var factory = new HandlebarsPromptTemplateFactory();
        var config = new PromptTemplateConfig
        {
            Template = template,
            TemplateFormat = "handlebars"
        };

        var templateEngine = factory.CreateAsync(config).Result;
        
        // Semantic Kernel passes data via KernelArguments
        var arguments = new KernelArguments
        {
            ["items"] = orderItems,
            ["total"] = total
        };

        // We use the Kernel context (null is acceptable for pure rendering without Kernel services)
        var renderedPrompt = await templateEngine.RenderAsync(null, arguments);

        Console.WriteLine(renderedPrompt);
    }
}
