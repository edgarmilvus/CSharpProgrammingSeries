
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

// Source File: project_advanced_application_script.cs
// Description: Advanced Application Script
// ==========================================

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.Text;
using System.Text.Json;

// Real-World Context:
// Imagine a logistics company that needs to generate daily dispatch instructions for drivers.
// These instructions must be dynamic based on weather conditions, package priority, and traffic.
// Using Handlebars templates allows us to separate the "structure" of the prompt from the "data",
// enabling non-technical staff to update the logic without rewriting C# code.

class Program
{
    static async Task Main(string[] args)
    {
        // 1. SETUP: Initialize the Kernel.
        // We use a dummy endpoint here for demonstration. In a real app, load from config.
        // We are NOT using dependency injection here to keep the example self-contained and procedural.
        var builder = Kernel.CreateBuilder();
        builder.AddOpenAIChatCompletion(
            modelId: "gpt-4o-mini",
            apiKey: "fake-key-for-demo-only"
        );
        var kernel = builder.Build();

        // 2. DATA PREPARATION: Create the dynamic data payload.
        // This represents the real-time data coming from sensors or databases.
        var dispatchData = new
        {
            DriverName = "Alex",
            Date = DateTime.Now.ToString("yyyy-MM-dd"),
            Weather = "Rainy", // Options: Sunny, Rainy, Snowy
            PriorityLoad = true,
            Stops = new[]
            {
                new { Name = "Warehouse A", Address = "123 Industrial Way", Priority = "High" },
                new { Name = "Client B", Address = "456 Downtown Ave", Priority = "Low" },
                new { Name = "Client C", Address = "789 Suburb St", Priority = "Medium" }
            }
        };

        // 3. TEMPLATE DEFINITION: The Handlebars Prompt.
        // We use Handlebars syntax for logic:
        // {{#if}} ... {{/if}} for conditional weather advice.
        // {{#each}} ... {{/each}} to iterate over stops.
        // {{variable}} to inject data.
        string handlebarsTemplate = """
            Write a daily dispatch instruction for driver {{DriverName}} on {{Date}}.

            {{#if (eq Weather "Rainy")}}
            IMPORTANT: Roads are slippery. Advise driver to reduce speed by 20% and maintain safe following distance.
            {{/if}}

            {{#if (eq Weather "Sunny")}}
            Weather is clear. Standard driving protocols apply.
            {{/if}}

            {{#if PriorityLoad}}
            WARNING: This is a priority load. Ensure strict adherence to time windows.
            {{/if}}

            Delivery Schedule:
            {{#each Stops}}
            - Stop {{@index}}: {{Name}} ({{Address}})
              Priority: {{Priority}}
              {{#if (eq Priority "High")}}
              Action: Verify signature required immediately upon delivery.
              {{/if}}
            {{/each}}

            Summary: Total stops: {{Stops.length}}.
            """;

        // 4. PROMPT RENDERING: Using the Kernel's Handlebars Engine.
        // The Kernel compiles the template with the data.
        // Note: We are simulating the rendering step here. In a full Semantic Kernel implementation,
        // we would typically use a PromptTemplateConfig and a HandlebarsPromptTemplateFactory.
        // However, for this "Application Script" subsection, we demonstrate the logic flow explicitly.
        
        Console.WriteLine("--- Generating Dispatch Instructions ---");
        
        // Simulating the Handlebars rendering process manually to demonstrate the logic flow
        // without relying on external libraries for this specific console output.
        string renderedPrompt = RenderHandlebarsTemplate(handlebarsTemplate, dispatchData);

        Console.WriteLine("\n[RENDERED PROMPT SENT TO AI]:\n");
        Console.WriteLine(renderedPrompt);

        // 5. AI INVOCATION: Sending the rendered prompt to the LLM.
        // In a production environment, this is where we execute the kernel function.
        try 
        {
            // We simulate the AI response here to avoid API call failures with dummy keys.
            // In a real app: var result = await kernel.InvokePromptAsync(renderedPrompt);
            string simulatedAIResponse = SimulateAIResponse(renderedPrompt);
            
            Console.WriteLine("\n[AI RESPONSE]:\n");
            Console.WriteLine(simulatedAIResponse);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error invoking AI: {ex.Message}");
        }
    }

    // Helper method to simulate Handlebars rendering logic for demonstration.
    // This mimics the internal logic of the Semantic Kernel's Handlebars engine.
    static string RenderHandlebarsTemplate(string template, object data)
    {
        var sb = new StringBuilder();
        var properties = data.GetType().GetProperties();
        var dataDict = properties.ToDictionary(p => p.Name, p => p.GetValue(data));

        // Simple regex-based parsing for demonstration (Production uses actual Handlebars parser)
        // This handles {{#if}}, {{#each}}, and variables.
        
        // 1. Handle Variables and simple logic
        string processed = template;
        
        // Replace simple variables {{Variable}}
        foreach (var kvp in dataDict)
        {
            processed = processed.Replace($"{{{{{kvp.Key}}}}}", kvp.Value?.ToString() ?? "");
        }

        // 2. Handle {{#if Weather "Rainy"}}
        // Note: A real parser is complex. We are hardcoding specific logic for this demo context.
        if (dataDict.ContainsKey("Weather") && dataDict["Weather"]?.ToString() == "Rainy")
        {
            // Keep the content inside the Rainy block
            processed = System.Text.RegularExpressions.Regex.Replace(
                processed, 
                @"{{#if \(eq Weather ""Rainy""\)}}(.*?){{/if}}", 
                "$1", 
                System.Text.RegularExpressions.RegexOptions.Singleline);
        }
        else
        {
            // Remove the Rainy block
            processed = System.Text.RegularExpressions.Regex.Replace(
                processed, 
                @"{{#if \(eq Weather ""Rainy""\)}}.*?{{/if}}", 
                "", 
                System.Text.RegularExpressions.RegexOptions.Singleline);
        }

        // Handle {{#if PriorityLoad}}
        if (dataDict.ContainsKey("PriorityLoad") && (bool)dataDict["PriorityLoad"])
        {
            processed = System.Text.RegularExpressions.Regex.Replace(
                processed, 
                @"{{#if PriorityLoad}}(.*?){{/if}}", 
                "$1", 
                System.Text.RegularExpressions.RegexOptions.Singleline);
        }
        else
        {
            processed = System.Text.RegularExpressions.Regex.Replace(
                processed, 
                @"{{#if PriorityLoad}}.*?{{/if}}", 
                "", 
                System.Text.RegularExpressions.RegexOptions.Singleline);
        }

        // 3. Handle {{#each Stops}}
        // We need to extract the block content and repeat it.
        var eachMatch = System.Text.RegularExpressions.Regex.Match(
            processed, 
            @"{{#each Stops}}(.*?){{/each}}", 
            System.Text.RegularExpressions.RegexOptions.Singleline);

        if (eachMatch.Success)
        {
            var blockContent = eachMatch.Groups[1].Value;
            var stops = dataDict["Stops"] as IEnumerable<object>;
            var replacement = new StringBuilder();

            if (stops != null)
            {
                int index = 1;
                foreach (var stop in stops)
                {
                    var stopProps = stop.GetType().GetProperties();
                    var stopDict = stopProps.ToDictionary(p => p.Name, p => p.GetValue(stop));
                    
                    string stopBlock = blockContent;
                    
                    // Replace variables inside the block
                    foreach (var kvp in stopDict)
                    {
                        stopBlock = stopBlock.Replace($"{{{{{kvp.Key}}}}}", kvp.Value?.ToString() ?? "");
                    }
                    
                    // Replace @index
                    stopBlock = stopBlock.Replace("{{@index}}", index.ToString());

                    // Handle nested {{#if Priority "High"}}
                    if (stopDict.ContainsKey("Priority") && stopDict["Priority"]?.ToString() == "High")
                    {
                        stopBlock = System.Text.RegularExpressions.Regex.Replace(
                            stopBlock, 
                            @"{{#if \(eq Priority ""High""\)}}(.*?){{/if}}", 
                            "$1", 
                            System.Text.RegularExpressions.RegexOptions.Singleline);
                    }
                    else
                    {
                        stopBlock = System.Text.RegularExpressions.Regex.Replace(
                            stopBlock, 
                            @"{{#if \(eq Priority ""High""\)}}.*?{{/if}}", 
                            "", 
                            System.Text.RegularExpressions.RegexOptions.Singleline);
                    }

                    replacement.AppendLine(stopBlock.Trim());
                    index++;
                }
            }

            processed = processed.Replace(eachMatch.Value, replacement.ToString());
        }

        // Clean up remaining empty lines
        return string.Join("\n", processed.Split('\n').Where(l => !string.IsNullOrWhiteSpace(l)));
    }

    static string SimulateAIResponse(string prompt)
    {
        // Simulating an AI response based on the prompt content
        if (prompt.Contains("Rainy"))
        {
            return "Dispatch Instructions Generated:\n\n" +
                   "1. Safety First: Roads are slippery. Reduce speed by 20%.\n" +
                   "2. Priority Load Active: Strict time windows enforced.\n" +
                   "3. Route:\n" +
                   "   - Stop 1: Warehouse A (High Priority). Signature required.\n" +
                   "   - Stop 2: Client B (Low Priority).\n" +
                   "   - Stop 3: Client C (Medium Priority).\n" +
                   "Total Stops: 3.";
        }
        return "Standard dispatch generated.";
    }
}
