
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

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Planning;
using Microsoft.SemanticKernel.Planning.Handlebars;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System;

public class TravelAssistant
{
    public static async Task ExecuteAsync()
    {
        // 1. Kernel Setup
        var kernel = Kernel.CreateBuilder()
            .AddAzureOpenAIChatCompletion(
                deploymentName: "gpt-4o-mini",
                endpoint: Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT"),
                apiKey: Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY"))
            .Build();

        // 2. Function Definition (Semantic Function)
        // We define the prompt to return JSON as requested.
        var prompt = """
            Generate a list of 3 attractions in {{$city}} focused on {{$interest}}.
            Return the result as a JSON array of objects with properties "Name" and "Type".
            Example: [{{"Name": "Eiffel Tower", "Type": "Observation"}}]
            """;

        var attractionsFunction = kernel.CreateFunctionFromPrompt(
            prompt, 
            new OpenAIPromptExecutionSettings { MaxTokens = 200 });

        kernel.ImportPluginFromFunctions("TravelPlugin", [attractionsFunction]);

        // 3. Planner Configuration
        var planner = new HandlebarsPlanner(new HandlebarsPlannerConfig
        {
            // Allow recursive planning if sub-tasks are needed
            AllowLoops = true 
        });

        // 4. Execution
        var goal = "Generate a 3-day itinerary for Paris focusing on art and history. Include a morning, afternoon, and evening activity for each day.";
        
        Console.WriteLine($"Planning Goal: {goal}\n");

        // Create the plan
        var plan = await planner.CreatePlanAsync(kernel, goal);

        Console.WriteLine("--- Raw Handlebars Plan String ---");
        Console.WriteLine(plan.RawPlan);
        Console.WriteLine("\n----------------------------------");

        // 5. Output Parsing & Execution
        // We need to inspect the plan to understand the variables.
        // However, to execute it, we invoke the plan directly.
        
        // Interactive Challenge: Custom Helper
        // HandlebarsPlanner allows registering custom helpers. 
        // We will register a helper to format the JSON output nicely.
        
        var helperArgs = new HandlebarsPlannerArguments();
        helperArgs.Helpers.Add("formatTable", (writer, options, context, args) =>
        {
            // This helper simulates converting JSON data to a Markdown table
            // In a real execution, 'args' would contain the data.
            // Note: Handlebars helpers in C# Semantic Kernel are often defined 
            // via the configuration or passed during execution context.
            // For this exercise, we simulate the output transformation logic.
            
            if (args.Length > 0 && args[0] is string jsonStr)
            {
                try 
                {
                    var data = JsonSerializer.Deserialize<JsonElement>(jsonStr);
                    var output = "| Name | Type |\n|------|------|\n";
                    if (data.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var item in data.EnumerateArray())
                        {
                            var name = item.GetProperty("Name").GetString();
                            var type = item.GetProperty("Type").GetString();
                            output += $"| {name} | {type} |\n";
                        }
                    }
                    writer.Write(output);
                }
                catch { writer.Write("Error formatting table"); }
            }
        });

        // Execute the plan
        // Note: HandlebarsPlanner execution usually handles the rendering.
        // We pass the arguments if the plan requires specific inputs.
        var result = await plan.InvokeAsync(kernel);

        Console.WriteLine("--- Final Itinerary Output ---");
        Console.WriteLine(result);
    }
}

// Helper class for JSON structure
public class Attraction
{
    [JsonPropertyName("Name")]
    public string Name { get; set; } = "";
    
    [JsonPropertyName("Type")]
    public string Type { get; set; } = "";
}
