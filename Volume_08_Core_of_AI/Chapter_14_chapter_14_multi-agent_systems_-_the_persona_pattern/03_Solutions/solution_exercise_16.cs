
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

// Source File: solution_exercise_16.cs
// Description: Solution for Exercise 16
// ==========================================

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.Text.Json;

async Task RunRoleBasedAccessControlAsync()
{
    var builder = Kernel.CreateBuilder();
    builder.AddOpenAIChatCompletion("gpt-4o", "api-key-here");
    var kernel = builder.Build();

    // Mock Database
    var db = new List<object> {
        new { Name = "Alice", Dept = "Engineering", Salary = 100000 },
        new { Name = "Bob", Dept = "Sales", Salary = 90000 }
    };

    // Agents
    var dbAgent = new ChatCompletionAgent(kernel, "Query the database and return raw JSON.");
    var proxyAgent = new ChatCompletionAgent(kernel, "You are a security filter. If the data contains 'Salary', redact it. Return sanitized JSON.");
    var publicAnalyst = new ChatCompletionAgent(kernel, "Analyze employee departments.");

    // 1. Public Analyst Request
    var request = "Get employee details.";
    
    // 2. Proxy Intercepts
    // In a real scenario, the Proxy would call the DB agent. 
    // Here we simulate the DB response manually for the example.
    string rawDbJson = JsonSerializer.Serialize(db);
    Console.WriteLine($"[Database Agent Raw]: {rawDbJson}");

    // 3. Proxy Sanitizes
    var sanitizationPrompt = $"Sanitize this JSON: {rawDbJson}";
    var sanitizedJson = await proxyAgent.InvokeAsync(sanitizationPrompt);
    
    Console.WriteLine($"[Proxy Agent Sanitized]: {sanitizedJson}");

    // 4. Public Analyst Receives
    var analysis = await publicAnalyst.InvokeAsync($"Analyze this data: {sanitizedJson}");
    Console.WriteLine($"[Public Analyst]: {analysis}");
}
