
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

using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

public class StructuredInferenceService
{
    // 1. Accept a System Prompt
    public async Task<string> RefactorCodeAsync(string userCode)
    {
        // Prompt Engineering per requirements
        string systemPrompt = "You are a C# expert. Refactor the code to use modern syntax. " +
                              "Return ONLY the code wrapped in <code> tags. Do not add conversation.";
        
        string userPrompt = $"Input: {userCode}";

        // Simulate LLM stream
        var fullResponse = new StringBuilder();
        
        // Simulating the LLM returning the structure
        // In reality, this comes from GenerateCodeAsync
        var stream = SimulateLlmStream(systemPrompt, userPrompt);
        
        await foreach (var chunk in stream)
        {
            fullResponse.Append(chunk);
        }

        // 2. Implement PostProcessor
        return ExtractCodeFromXml(fullResponse.ToString());
    }

    // 3 & 4. Parsing and Defensive Coding
    private string ExtractCodeFromXml(string rawOutput)
    {
        // Regex to find content between <code> and </code>
        // RegexOptions.Singleline ensures . matches newlines
        var match = Regex.Match(rawOutput, @"<code>(.*?)</code>", RegexOptions.Singleline | RegexOptions.IgnoreCase);

        if (match.Success)
        {
            // 4. Defensive Coding: Trim whitespace
            return match.Groups[1].Value.Trim();
        }

        // Fallback: If tags are missing or unclosed, return raw output or a specific error message
        // This prevents the UI from showing nothing if the LLM hallucinates.
        Console.WriteLine("Warning: Failed to parse structured output. Displaying raw content.");
        return rawOutput; 
    }

    // Mock stream for demonstration
    private async IAsyncEnumerable<string> SimulateLlmStream(string sys, string user)
    {
        var response = "<code>\npublic class Calculator {\n    public int Add(int a, int b) => a + b;\n}\n</code>";
        foreach (var c in response)
        {
            yield return c.ToString();
            await Task.Delay(10); // Simulate typing
        }
    }
}

// Usage in View Model
public class RefactorViewModel
{
    private readonly StructuredInferenceService _service;

    public async Task PerformRefactor(string selectedCode)
    {
        // 3. Interactive Challenge: Refactor Command
        var refactored = await _service.RefactorCodeAsync(selectedCode);
        
        // Update UI with ONLY the extracted code
        // UI.Text = refactored;
    }
}
