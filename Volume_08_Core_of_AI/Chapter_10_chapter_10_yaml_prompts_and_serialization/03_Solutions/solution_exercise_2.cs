
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

using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.PromptTemplates.Handlebars;
using Microsoft.SemanticKernel.Yaml;

namespace Exercise2_ModularPrompts
{
    // Metadata class to help with serialization
    public class PromptMetadata
    {
        [JsonPropertyName("version")]
        public string Version { get; set; } = "1.0";
    }

    class Program
    {
        static async Task Main(string[] args)
        {
            // 1. Create YAML files (simulated)
            string systemYaml = @"
version: 1.0
name: SystemPrompt
template: You are a Senior Software Architect. Provide high-level, scalable solutions.
";

            string taskYaml = @"
version: 1.0
name: TaskPrompt
input_parameters:
  - name: context
    type: string
    required: true
  - name: format
    type: string
    required: true
template: |
  Given the following context: {{context}}
  Format the response as {{format}}.
";

            // 2. Load and Validate System Prompt Version
            var systemConfig = KernelPromptTemplateFactory.CreatePromptTemplateConfig(systemYaml);
            
            // Version Check Logic
            var systemVersion = systemConfig.Extensions?["version"]?.ToString();
            if (systemVersion != "1.0")
            {
                throw new InvalidOperationException($"Unsupported system prompt version: {systemVersion}");
            }

            // 3. Load Task Prompt
            var taskConfig = KernelPromptTemplateFactory.CreatePromptTemplateConfig(taskYaml);

            // 4. Combine Configurations
            // In a real scenario, we might merge the input parameters or concatenate templates.
            // Here, we execute them sequentially or combine logic.
            // For demonstration, we will execute the task prompt, but the system prompt context is "baked in".
            
            var kernel = Kernel.CreateBuilder().Build();
            
            // We use the Task Prompt as the execution entry point, 
            // but we could inject the System Prompt text into the arguments.
            var function = kernel.CreateFunctionFromPrompt(taskConfig, new HandlebarsPromptTemplateFactory());

            // Inject System Prompt into the context for this execution
            var result = await kernel.InvokeAsync(function, new KernelArguments
            {
                ["context"] = "Refactor the monolithic legacy codebase into microservices.",
                ["format"] = "bullet points"
            });

            Console.WriteLine($"Execution Result:\n{result}");

            // 5. Serialization Strategy for Audit Logging
            // To serialize the combined state, we create a new PromptTemplateConfig 
            // that merges the relevant parts.
            var combinedConfig = new PromptTemplateConfig
            {
                Name = "Combined_Architect_Workflow",
                Description = "System prompt v1.0 combined with Task prompt v1.0",
                Template = $"{systemConfig.Template} {taskConfig.Template}",
                InputParameters = taskConfig.InputParameters // Merging input params if necessary
            };

            // Add version metadata to extensions for persistence
            combinedConfig.Extensions ??= new KernelJsonSchema();
            combinedConfig.Extensions["version"] = "1.0";
            combinedConfig.Extensions["source_system_version"] = systemVersion;
            combinedConfig.Extensions["source_task_version"] = taskConfig.Extensions?["version"]?.ToString();

            // Serialize back to YAML
            string serializedYaml = KernelPromptTemplateFactory.SerializeToYaml(combinedConfig);
            Console.WriteLine("\n--- Serialized Combined State (Audit Log) ---");
            Console.WriteLine(serializedYaml);
        }
    }
}
