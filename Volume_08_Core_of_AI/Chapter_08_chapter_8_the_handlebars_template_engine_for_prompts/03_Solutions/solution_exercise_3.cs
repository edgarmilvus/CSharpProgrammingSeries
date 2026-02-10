
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

// Source File: solution_exercise_3.cs
// Description: Solution for Exercise 3
// ==========================================

using HandlebarsDotNet;
using Microsoft.SemanticKernel.PromptTemplates.Handlebars;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class MeetingMinutesGenerator
{
    public static async Task RunAsync()
    {
        // 1. Define Template
        string template = """
            Meeting Date: {{formatDate meetingDate "MMMM dd, yyyy"}}
            
            Action Items:
            {{#each actionItems}}
            * {{this}}
            {{/each}}
            """;

        // 2. Setup Data
        var meetingDate = DateTime.Now;
        var actions = new List<string> { "Finalize Q3 budget", "Review PR #42", "Schedule team offsite" };

        // 3. Custom Helper Registration Strategy
        // Semantic Kernel's HandlebarsPromptTemplateFactory allows customization via the 
        // underlying HandlebarsConfiguration. We create a factory instance and inject 
        // the helper registration logic before creating the prompt template.
        
        var factory = new HandlebarsPromptTemplateFactory((config) =>
        {
            // Register the 'formatDate' helper
            config.Helpers.Add("formatDate", (writer, context, arguments) =>
            {
                if (arguments.Length < 2) return;
                
                if (arguments[0] is DateTime dt && arguments[1] is string format)
                {
                    writer.WriteSafeString(dt.ToString(format));
                }
                else
                {
                    // Fallback or error handling
                    writer.WriteSafeString(arguments[0]?.ToString() ?? "");
                }
            });
        });

        var config = new PromptTemplateConfig
        {
            Template = template,
            TemplateFormat = "handlebars"
        };

        var templateEngine = await factory.CreateAsync(config);
        
        var arguments = new KernelArguments
        {
            ["meetingDate"] = meetingDate,
            ["actionItems"] = actions
        };

        var result = await templateEngine.RenderAsync(null, arguments);
        Console.WriteLine(result);
    }
}
