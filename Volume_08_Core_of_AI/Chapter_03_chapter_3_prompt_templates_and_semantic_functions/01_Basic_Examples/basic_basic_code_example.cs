
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

// ==========================================================
// File: BasicSemanticFunctionExample.cs
// Objective: Demonstrate creating and invoking a basic 
//            Semantic Function using Microsoft Semantic Kernel.
// ==========================================================

// 1. Import the core Semantic Kernel orchestration library.
//    This namespace contains the 'Kernel' class and function-related primitives.
using Microsoft.SemanticKernel;
// 2. Import the necessary runtime services.
//    We need this to enable dependency injection for the Kernel configuration.
using Microsoft.Extensions.DependencyInjection;
// 3. Import the logging abstractions.
//    Essential for debugging the internal workings of the Kernel.
using Microsoft.Extensions.Logging;
// 4. Import the console logger implementation.
using Microsoft.Extensions.Logging.Console;

// 5. Define the program entry point.
//    We use 'async Task' to support asynchronous API calls to the LLM.
public class Program
{
    public static async Task Main(string[] args)
    {
        // 6. Define the path where our prompt definition file exists.
        //    In a real app, this might be a secure configuration or environment variable.
        //    For this standalone example, we will write the file to disk first to ensure it runs.
        string pluginDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Plugins");
        string promptPath = Path.Combine(pluginDirectory, "Summarize", "skprompt.txt");

        // 7. Ensure the directory structure exists.
        Directory.CreateDirectory(Path.GetDirectoryName(promptPath)!);

        // 8. Create the prompt file content.
        //    This is a "Zero-Shot" prompt. We are giving the AI a role and a task without examples.
        string promptContent = """
            You are a helpful assistant that summarizes text into a single, concise sentence.
            
            Input: {{$input}}
            Summary:
            """;

        // 9. Write the prompt to the file system so the Kernel can load it.
        await File.WriteAllTextAsync(promptPath, promptContent);

        // 10. Configure the Kernel Builder.
        //     This is the modern 'Expert Mode' approach using dependency injection.
        IKernelBuilder kernelBuilder = Kernel.CreateBuilder();

        // 11. Add Logging (Console).
        //     Critical for seeing internal Kernel events, token usage, and errors.
        kernelBuilder.Services.AddLogging(builder => builder
            .AddConsole()
            .SetMinimumLevel(LogLevel.Debug));

        // 12. Add the AI Service (Azure OpenAI used here as the example).
        //     NOTE: You must replace these placeholders with valid credentials or use OpenAI directly.
        kernelBuilder.AddAzureOpenAIChatCompletion(
            deploymentName: "gpt-4o-mini", // Your model deployment name
            endpoint: "https://your-resource.openai.azure.com/", // Your endpoint
            apiKey: "your-api-key" // Your API Key
        );

        // 13. Build the Kernel instance.
        //     The Kernel is the central nervous system of your AI application.
        IKernel kernel = kernelBuilder.Build();

        // 14. Define the Plugin Name.
        //     Plugins are logical groupings of functions (like a class in OOP).
        string pluginName = "MyTextPlugin";

        // 15. Import the Semantic Function from the file system.
        //     The Kernel automatically parses the 'skprompt.txt' and configures the execution settings.
        //     'promptDirectory' tells the kernel where to look for 'config.json' and 'skprompt.txt'.
        var summarizeFunction = kernel.ImportPluginFromPromptDirectory(pluginDirectory, pluginName)["Summarize"];

        // 16. Define the input data.
        //     In Semantic Kernel, inputs are passed as a dictionary of key-value pairs.
        //     The key '$input' is the default parameter name if not specified otherwise.
        string longText = "Microsoft Semantic Kernel is an open-source SDK that lets you easily build AI applications " +
                          "that can call your existing code. It provides a modern, composable architecture for " +
                          "orchestrating AI models and plugins to create powerful, context-aware applications.";

        // 17. Create the Kernel Arguments object.
        //     This is the modern replacement for the older 'ContextVariables' class.
        KernelArguments arguments = new()
        {
            ["input"] = longText
        };

        // 18. Invoke the function.
        //     This triggers the 'Function Calling Flow':
        //     1. Kernel prepares the prompt using the arguments.
        //     2. Kernel sends the request to the configured AI service.
        //     3. Kernel receives the response.
        FunctionResult result = await kernel.InvokeAsync(summarizeFunction, arguments);

        // 19. Extract and display the result.
        //     The result object contains metadata, usage statistics, and the actual content.
        Console.WriteLine("--------------------------------------------------");
        Console.WriteLine($"Original Length: {longText.Length} chars");
        Console.WriteLine($"Summary: {result}");
        Console.WriteLine("--------------------------------------------------");
    }
}
