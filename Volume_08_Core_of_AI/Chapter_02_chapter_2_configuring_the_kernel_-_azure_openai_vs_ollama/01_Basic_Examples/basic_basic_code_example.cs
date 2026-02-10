
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

// Source File: basic_basic_code_example.cs
// Description: Basic Code Example
// ==========================================

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Connectors.Ollama;
using System.ComponentModel;

// --- CONFIGURATION ---
// In a real application, these would come from secure configuration (e.g., Azure Key Vault, appsettings.json)
const string AZURE_OPENAI_DEPLOYMENT_NAME = "gpt-4o-mini";
const string AZURE_OPENAI_ENDPOINT = "https://your-resource.openai.azure.com/";
const string AZURE_OPENAI_API_KEY = "YOUR_AZURE_API_KEY";

const string OLLAMA_MODEL_NAME = "llama3.2";
const string OLLAMA_ENDPOINT = "http://localhost:11434";

// --- KERNEL FACTORY PATTERN ---
// This method encapsulates the logic for creating a Kernel instance configured for a specific provider.
// It demonstrates the abstraction layer: the calling code doesn't need to know which provider is used.
public static class KernelFactory
{
    public static Kernel CreateAzureOpenAIService()
    {
        // 1. Instantiate the Kernel builder.
        var builder = Kernel.CreateBuilder();

        // 2. Add the Azure OpenAI Chat Completion service.
        // This registers the service with the DI container within the kernel.
        builder.AddAzureOpenAIChatCompletion(
            deploymentName: AZURE_OPENAI_DEPLOYMENT_NAME,
            endpoint: AZURE_OPENAI_ENDPOINT,
            apiKey: AZURE_OPENAI_API_KEY);

        // 3. Build the Kernel.
        return builder.Build();
    }

    public static Kernel CreateOllamaService()
    {
        // 1. Instantiate the Kernel builder.
        var builder = Kernel.CreateBuilder();

        // 2. Add the Ollama Chat Completion service.
        // Note: Ollama typically runs locally, so no API key is required by default.
        builder.AddOllamaChatCompletion(
            modelId: OLLAMA_MODEL_NAME,
            endpoint: new Uri(OLLAMA_ENDPOINT));

        // 3. Build the Kernel.
        return builder.Build();
    }
}

// --- PLUG-IN DEFINITION ---
// A simple plugin to demonstrate kernel execution.
public class TimePlugin
{
    [KernelFunction, Description("Retrieves the current local time.")]
    public string GetCurrentTime() => DateTime.Now.ToString("T");
}

// --- MAIN EXECUTION LOGIC ---
// This simulates an application that needs to perform an AI task.
// It abstracts the provider choice, allowing us to swap Azure OpenAI for Ollama with minimal code change.
public class Program
{
    public static async Task Main(string[] args)
    {
        Console.WriteLine("Select AI Provider:\n1. Azure OpenAI\n2. Ollama");
        var choice = Console.ReadLine();

        // Determine which Kernel to create based on user input.
        // In a production app, this decision might be driven by configuration flags or runtime conditions.
        Kernel kernel = choice?.Trim() == "2" 
            ? KernelFactory.CreateOllamaService() 
            : KernelFactory.CreateAzureOpenAIService();

        // Import the plugin into the kernel.
        kernel.ImportPluginFromObject(new TimePlugin(), "time");

        // Define the prompt. 
        // We use a simple prompt that asks the AI to utilize the 'time' plugin.
        string prompt = "What is the current time? Please use the time plugin.";

        Console.WriteLine($"\n--- Executing Prompt: '{prompt}' ---");
        
        // Create execution settings.
        // We explicitly request the AI to call a function if necessary.
        var executionSettings = new OpenAIPromptExecutionSettings
        {
            ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
        };

        try
        {
            // Invoke the kernel. This is the core interaction point.
            // The kernel routes the prompt to the configured AI service, 
            // processes the response (including function calls), and returns the result.
            var result = await kernel.InvokePromptAsync(prompt, executionSettings);

            Console.WriteLine($"\n--- Result ---");
            Console.WriteLine(result);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n--- Error ---");
            Console.WriteLine($"An error occurred: {ex.Message}");
            // In a real app, log the full exception stack trace.
        }
    }
}
