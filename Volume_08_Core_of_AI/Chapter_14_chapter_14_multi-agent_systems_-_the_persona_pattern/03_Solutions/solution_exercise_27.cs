
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

# Source File: solution_exercise_27.cs
# Description: Solution for Exercise 27
# ==========================================

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

public class Document { public string Id { get; set; } = ""; public string Content { get; set; } = ""; }

async Task RunKnowledgeGraphAsync()
{
    var builder = Kernel.CreateBuilder();
    builder.AddOpenAIChatCompletion("gpt-4o", "api-key-here");
    var kernel = builder.Build();

    // Mock Knowledge Base
    var kb = new List<Document> {
        new Document { Id = "FAQ1", Content = "Password reset is located in Settings > Security." },
        new Document { Id = "FAQ2", Content = "Billing is handled via Stripe dashboard." }
    };

    var supportAgent = new ChatCompletionAgent(kernel, "Answer questions based strictly on the provided context.");

    string userQuery = "How do I reset my password?";

    // 1. Retrieval (Simple string matching for demo)
    var relevantDocs = kb.Where(d => d.Content.Contains("password", StringComparison.OrdinalIgnoreCase)).ToList();
    string context = string.Join("\n", relevantDocs.Select(d => d.Content));

    Console.WriteLine($"Retrieved Context: {context}");

    // 2. Generation
    var prompt = $"Context: {context}\n\nQuestion: {userQuery}";
    var answer = await supportAgent.InvokeAsync(prompt);

    Console.WriteLine($"Agent Answer: {answer}");
}
