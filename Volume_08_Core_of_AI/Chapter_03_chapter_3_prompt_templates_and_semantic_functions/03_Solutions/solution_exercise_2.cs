
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

using Microsoft.SemanticKernel;
using System.Text.Json;
using System.Text.Json.Serialization;

// Data Models for JSON Parsing
public class ReportOutline
{
    [JsonPropertyName("introduction")]
    public string Introduction { get; set; } = "";
    
    [JsonPropertyName("sections")]
    public List<Section> Sections { get; set; } = new();
    
    [JsonPropertyName("conclusion")]
    public string Conclusion { get; set; } = "";
}

public class Section
{
    [JsonPropertyName("title")]
    public string Title { get; set; } = "";
    
    [JsonPropertyName("summary")]
    public string Summary { get; set; } = "";
}

class Program
{
    static async Task Main(string[] args)
    {
        var builder = Kernel.CreateBuilder();
        // Add your AI connector here (e.g., builder.AddOpenAIChatCompletion(...))
        var kernel = builder.Build();

        // 1. Define Functions
        var generateOutlineFn = kernel.CreateFunctionFromPrompt(
            "Generate a structured outline for a report on the topic: {{topic}}. Include an Introduction, 3 Main Body Sections, and a Conclusion. Output in JSON format.",
            new OpenAIPromptExecutionSettings { MaxTokens = 500, Temperature = 0.3 }
        );

        var expandSectionFn = kernel.CreateFunctionFromPrompt(
            "Expand the following section of a report. Section Title: {{title}}. Context: {{context}}. Generate 3-5 paragraphs of detailed content.",
            new OpenAIPromptExecutionSettings { MaxTokens = 800, Temperature = 0.7 }
        );

        var polishReportFn = kernel.CreateFunctionFromPrompt(
            "Take the following draft content and polish it for a professional audience. Ensure a consistent tone and fix grammatical errors. Draft: {{draft_content}}",
            new OpenAIPromptExecutionSettings { MaxTokens = 1000, Temperature = 0.3 }
        );

        // 2. Execution with Error Handling (Interactive Challenge)
        string topic = "The impact of AI on Software Development";
        ReportOutline? outline = null;
        int retries = 0;
        const int maxRetries = 2;

        while (retries <= maxRetries)
        {
            try
            {
                Console.WriteLine($"Generating Outline (Attempt {retries + 1})...");
                var result = await kernel.InvokeAsync(generateOutlineFn, new() { ["topic"] = topic });
                
                // Attempt to parse JSON
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                outline = JsonSerializer.Deserialize<ReportOutline>(result.ToString(), options);
                
                if (outline != null) break; // Success
            }
            catch (JsonException)
            {
                Console.WriteLine("Failed to parse JSON. Retrying with correction instruction...");
                retries++;
                
                // Modify prompt for retry
                if (retries <= maxRetries)
                {
                    // Re-create function with stricter instructions or append to arguments if supported
                    // For this exercise, we simulate the retry by updating the prompt text dynamically
                    var retryPrompt = $"Generate a structured outline for a report on the topic: {topic}. Output STRICTLY in valid JSON format. Do not include markdown code blocks.";
                    generateOutlineFn = kernel.CreateFunctionFromPrompt(retryPrompt, new OpenAIPromptExecutionSettings { MaxTokens = 500, Temperature = 0.3 });
                }
            }
        }

        if (outline == null) throw new Exception("Failed to generate valid outline after retries.");

        // 3. Chaining: Expand Sections
        var fullDraft = new System.Text.StringBuilder();
        
        // Add Introduction
        fullDraft.AppendLine(await kernel.InvokeAsync(expandSectionFn, new() 
            { ["title"] = "Introduction", ["context"] = outline.Introduction }));

        // Iterate through main sections
        foreach (var section in outline.Sections)
        {
            Console.WriteLine($"Expanding section: {section.Title}...");
            var sectionContent = await kernel.InvokeAsync(expandSectionFn, new() 
                { ["title"] = section.Title, ["context"] = section.Summary });
            fullDraft.AppendLine(sectionContent);
        }

        // Add Conclusion
        fullDraft.AppendLine(await kernel.InvokeAsync(expandSectionFn, new() 
            { ["title"] = "Conclusion", ["context"] = outline.Conclusion }));

        // 4. Chaining: Polish
        Console.WriteLine("Polishing final report...");
        var finalReport = await kernel.InvokeAsync(polishReportFn, new() 
            { ["draft_content"] = fullDraft.ToString() });

        Console.WriteLine("\n--- Final Polished Report ---");
        Console.WriteLine(finalReport);
    }
}
