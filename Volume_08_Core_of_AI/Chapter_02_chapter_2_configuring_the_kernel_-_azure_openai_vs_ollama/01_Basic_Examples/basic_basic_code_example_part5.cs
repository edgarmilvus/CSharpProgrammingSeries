
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

// Source File: basic_basic_code_example_part5.cs
// Description: Basic Code Example
// ==========================================

public class Program
{
    public static async Task Main(string[] args)
    {
        Console.WriteLine("Select AI Provider:\n1. Azure OpenAI\n2. Ollama");
        var choice = Console.ReadLine();

        Kernel kernel = choice?.Trim() == "2" 
            ? KernelFactory.CreateOllamaService() 
            : KernelFactory.CreateAzureOpenAIService();

        kernel.ImportPluginFromObject(new TimePlugin(), "time");

        string prompt = "What is the current time? Please use the time plugin.";

        var executionSettings = new OpenAIPromptExecutionSettings
        {
            ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
        };

        try
        {
            var result = await kernel.InvokePromptAsync(prompt, executionSettings);
            Console.WriteLine($"\n--- Result ---");
            Console.WriteLine(result);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n--- Error ---");
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }
}
