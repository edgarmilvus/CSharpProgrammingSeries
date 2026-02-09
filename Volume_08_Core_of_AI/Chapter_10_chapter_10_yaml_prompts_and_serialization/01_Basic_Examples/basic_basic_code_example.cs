
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

# Source File: basic_basic_code_example.cs
# Description: Basic Code Example
# ==========================================

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.PromptTemplates.Yaml;
using System.ComponentModel;
using System.Text.Json;
using System.Threading.Tasks;

// 1. Define a simple plugin class with a native function.
// In a real scenario, this might be a database lookup or an API call.
public class EmailPlugin
{
    [KernelFunction, Description("Returns the raw text of the latest customer feedback email.")]
    public string GetLatestFeedback() => "The app is great, but the login button is hard to see on mobile.";
}

class Program
{
    static async Task Main(string[] args)
    {
        // 2. Initialize the Kernel. 
        // Note: We use a dummy model ID for demonstration; real usage requires Azure/OpenAI keys.
        var builder = Kernel.CreateBuilder();
        builder.AddAzureOpenAIChatCompletion(
            deploymentName: "gpt-4o-mini", 
            endpoint: "https://dummy.openai.azure.com/", 
            apiKey: "dummy"); 
        var kernel = builder.Build();

        // 3. Register the native plugin.
        kernel.ImportPluginFromObject(new EmailPlugin(), "email");

        // 4. Define the YAML prompt template.
        // This is the core of the example. We use a raw string here for self-containment,
        // but typically this would be loaded from a .yaml file.
        string yamlPrompt = """
            name: SummarizeFeedback
            template: |
                <message role="system">You are a helpful assistant summarizing customer feedback.</message>
                <message role="user">
                    Please summarize the following feedback concisely:
                    {{$feedback}}
                </message>
            template_format: semantic-kernel
            input_variables:
                - name: feedback
                  description: The raw feedback text to summarize.
                  default: "No feedback provided."
            execution_settings:
                default:
                    max_tokens: 100
                    temperature: 0.7
            """;

        // 5. Create the Prompt Template Configuration from the YAML string.
        var promptConfig = PromptTemplateConfig.FromYaml(yamlPrompt);

        // 6. Create the Kernel Function using the configuration.
        // This step performs the serialization/deserialization of the YAML structure.
        var summarizeFunction = kernel.CreateFunctionFromPrompt(promptConfig);

        // 7. Prepare the arguments.
        // We retrieve the feedback using the native function we registered earlier.
        var feedback = kernel.Plugins["email"]["GetLatestFeedback"].InvokeAsync<string>();
        var arguments = new KernelArguments
        {
            ["feedback"] = await feedback
        };

        // 8. Invoke the function.
        // The Kernel handles the LLM call using the structured prompt defined in YAML.
        var result = await kernel.InvokeAsync(summarizeFunction, arguments);

        // 9. Output the result.
        Console.WriteLine("--- YAML Prompt Execution Result ---");
        Console.WriteLine(result);
    }
}
