
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

// Source File: solution_exercise_2.cs
// Description: Solution for Exercise 2
// ==========================================

using Bogus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

public record Product(Guid Id, string Name, string Category, string Description);

public class LlmProductSeeder
{
    // Mock LLM API Service
    private class MockLlmService
    {
        public async Task<string> GetCompletionAsync(string prompt)
        {
            // Simulate network latency
            await Task.Delay(50); 
            // Simulate a response based on the prompt structure
            return $"Generated description for prompt: {prompt.Substring(0, Math.Min(50, prompt.Length))}... " +
                   "This product features high durability and premium material composition.";
        }
    }

    // 1. Prompt Engineering Helper
    private static string ConstructPrompt(string productName, string category)
    {
        return $"Generate a realistic, 2-sentence product description for a {category} item named {productName}. " +
               $"Include the following technical attributes: durability, material composition, and intended use.";
    }

    // 2. Description Generation with Validation & Retry
    private static async Task<string> GenerateDescriptionWithRetry(MockLlmService llm, string name, string category)
    {
        const int maxRetries = 1;
        const int maxCharLimit = 500;

        for (int i = 0; i <= maxRetries; i++)
        {
            var prompt = ConstructPrompt(name, category);
            var description = await llm.GetCompletionAsync(prompt);

            // Validation: Not null, not empty, within length limit
            if (!string.IsNullOrWhiteSpace(description) && description.Length <= maxCharLimit)
            {
                return description;
            }
        }

        // Fallback if retries fail
        return "Default description for testing purposes.";
    }

    public async Task<List<Product>> GenerateTestDatasetAsync()
    {
        var llmService = new MockLlmService();
        
        // 3. Generate Base Products
        var productFaker = new Faker<Product>()
            .RuleFor(p => p.Id, f => Guid.NewGuid())
            .RuleFor(p => p.Name, f => f.Commerce.ProductName())
            .RuleFor(p => p.Category, f => f.Commerce.Department())
            // Placeholder for description, to be filled later
            .RuleFor(p => p.Description, f => "Pending generation..."); 

        var products = productFaker.Generate(50);

        // 4. Optimization: Batching Strategy
        // We collect the inputs and process them to simulate a single batch API call 
        // or parallel processing to minimize total latency compared to sequential awaits.
        
        var descriptionTasks = products
            .Select(p => GenerateDescriptionWithRetry(llmService, p.Name, p.Category))
            .ToList();

        // Await all tasks concurrently
        var descriptions = await Task.WhenAll(descriptionTasks);

        // Map generated descriptions back to products
        var result = new List<Product>();
        for (int i = 0; i < products.Count; i++)
        {
            result.Add(products[i] with { Description = descriptions[i] });
        }

        return result;
    }
}
