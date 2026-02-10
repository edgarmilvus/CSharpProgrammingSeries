
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

using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace SemanticKernelYamlApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // 1. SETUP: Initialize the Kernel with Azure OpenAI configuration.
            // We use basic string literals for configuration to avoid external config files.
            var builder = Kernel.CreateBuilder();
            builder.AddAzureOpenAIChatCompletion(
                deploymentName: "gpt-4o-mini", // Replace with your deployment name
                endpoint: "https://your-resource.openai.azure.com/", // Replace with your endpoint
                apiKey: "YOUR_API_KEY" // Replace with your API key
            );
            Kernel kernel = builder.Build();

            // 2. YAML DEFINITION: Define the prompt structure as a string.
            // This represents the "Core of AI Engineering" concept: version-controllable prompts.
            // We are creating a specialized 'Reviewer' prompt that enforces specific output formats.
            string yamlPrompt = @"
name: Reviewer
description: A prompt that reviews text and provides a structured critique.
template: |
  <message role=""system"">
  You are a professional editor. You review text for clarity, grammar, and tone.
  You must respond ONLY with a JSON object containing two keys: ""score"" (0-10) and ""feedback"" (string).
  </message>
  <message role=""user"">
  Please review the following text: {{$input}}
  </message>
template_format: semantic-kernel
input_variables:
  - name: input
    description: The text to review
    default: ''
";

            // 3. SERIALIZATION: Convert the YAML string into a KernelPromptTemplateConfig object.
            // This mimics the deserialization process used by the Kernel to load .yaml files.
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            // We manually parse the YAML into a config object. 
            // In a production app, Kernel.ImportPluginFromPromptDirectory handles this automatically.
            var promptConfig = deserializer.Deserialize<PromptTemplateConfig>(yamlPrompt);

            // 4. PROMPT TEMPLATE CREATION: Create the template object using the deserialized config.
            // This bridges the gap between the serialized YAML and the executable Kernel function.
            var promptTemplate = new KernelPromptTemplate(promptConfig);

            // 5. EXECUTION: Render the prompt and invoke the AI model.
            // We pass the 'input' variable required by the YAML definition.
            Console.WriteLine("--- Starting Review Process ---");
            string userText = "The quick brown fox jump over the lazy dog. It was very fast.";
            
            // Render the prompt (injecting variables)
            string renderedPrompt = await promptTemplate.RenderAsync(kernel, new KernelArguments { ["input"] = userText });
            Console.WriteLine("\n[DEBUG] Rendered Prompt Structure:");
            Console.WriteLine(renderedPrompt);

            // Execute the AI request
            var executionSettings = new OpenAIPromptExecutionSettings
            {
                MaxTokens = 200,
                Temperature = 0.1
            };

            // Create a function manually to invoke the rendered prompt
            var function = kernel.CreateFunctionFromPrompt(promptConfig, executionSettings);
            var result = await kernel.InvokeAsync(function, new KernelArguments { ["input"] = userText });

            Console.WriteLine("\n--- Review Result ---");
            Console.WriteLine(result.ToString());

            // 6. SERIALIZATION BACK: Demonstrate how to serialize a modified config back to YAML.
            // This is useful for saving dynamic prompt variations created during runtime.
            Console.WriteLine("\n--- Serialized Configuration (For Version Control) ---");
            var serializer = new SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();
            
            string serializedYaml = serializer.Serialize(promptConfig);
            Console.WriteLine(serializedYaml);
        }
    }

    // 7. DATA MODELS: Classes required for YAML Serialization/Deserialization.
    // These map directly to the YAML structure defined in Chapter 10.
    public class PromptTemplateConfig
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Template { get; set; }
        public string TemplateFormat { get; set; }
        public InputVariable[] InputVariables { get; set; }
    }

    public class InputVariable
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Default { get; set; }
    }
}
