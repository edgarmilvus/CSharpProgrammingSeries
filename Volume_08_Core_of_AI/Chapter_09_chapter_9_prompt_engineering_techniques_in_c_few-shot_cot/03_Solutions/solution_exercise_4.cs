
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

using Microsoft.SemanticKernel;
using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PromptEngineeringExercises;

// 1. Define the Data Model
public record TicketData(
    [property: JsonPropertyName("userId")] string? UserId,
    [property: JsonPropertyName("userName")] string? UserName,
    [property: JsonPropertyName("feature")] string? Feature,
    [property: JsonPropertyName("page")] string? Page,
    [property: JsonPropertyName("error")] string? Error,
    [property: JsonPropertyName("priority")] string Priority
);

public class SupportTicketParserPlugin
{
    [KernelFunction, Description("Extracts structured details from a support ticket.")]
    public async Task<string> ExtractTicketDetails(
        Kernel kernel,
        [Description("The unstructured support ticket text")] string ticketText)
    {
        // The prompt enforces strict JSON output using Few-Shot examples.
        var prompt = """
            You are a data extraction assistant. Extract specific entities from the support ticket text.
            
            You must respond ONLY with the JSON object, with no additional text, greetings, or explanations.
            Use the following schema:
            {
                "userId": string?,
                "userName": string?,
                "feature": string?,
                "page": string?,
                "error": string?,
                "priority": string
            }

            Examples:
            1. Input: "User 101, John Doe, reports that the 'Export to PDF' feature is failing on the reports page. He gets a 500 error. Priority is high."
               Output: {"userId": "101", "userName": "John Doe", "feature": "Export to PDF", "page": "reports", "error": "500", "priority": "High"}

            2. Input: "I can't log in. My username is sarah.w and I'm getting an 'invalid credentials' message."
               Output: {"userId": null, "userName": "sarah.w", "feature": "Login", "page": null, "error": "invalid credentials", "priority": "Medium"}

            Input Ticket:
            {{$ticketText}}
            """;

        var result = await kernel.InvokePromptAsync<string>(prompt, new KernelArguments { ["ticketText"] = ticketText });
        return result ?? "{}";
    }
}

public static class Program
{
    public static async Task Main(string[] args)
    {
        var builder = Kernel.CreateBuilder();
        // builder.AddAzureOpenAIChatCompletion(...);
        var kernel = builder.Build();

        var parserPlugin = new SupportTicketParserPlugin();

        var ticket = "Hi, my name is Alex. I'm on the dashboard and the 'Save Settings' button is grayed out. I'm a premium user. Please help.";

        Console.WriteLine("--- Data Extraction Result ---");

        try
        {
            var jsonString = await parserPlugin.ExtractTicketDetails(kernel, ticket);
            Console.WriteLine($"Raw JSON Response:\n{jsonString}\n");

            // 2. Advanced C# Integration: Deserialization
            try
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var ticketData = JsonSerializer.Deserialize<TicketData>(jsonString, options);

                if (ticketData != null)
                {
                    Console.WriteLine("--- Deserialized C# Object ---");
                    Console.WriteLine($"User: {ticketData.UserName} (ID: {ticketData.UserId})");
                    Console.WriteLine($"Feature: {ticketData.Feature} on Page: {ticketData.Page}");
                    Console.WriteLine($"Error: {ticketData.Error}");
                    Console.WriteLine($"Priority: {ticketData.Priority}");
                }
            }
            catch (JsonException jsonEx)
            {
                Console.WriteLine($"Failed to parse JSON: {jsonEx.Message}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error invoking LLM: {ex.Message}");
        }
    }
}
