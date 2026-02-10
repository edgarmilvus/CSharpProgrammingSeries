
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

// Source File: solution_exercise_20.cs
// Description: Solution for Exercise 20
// ==========================================

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel Functions;

public class WeatherPlugin
{
    [KernelFunction, Description("Get the current weather for a city")]
    public string GetWeather([Description("The city name")] string city)
    {
        // Simulated API call
        return $"The weather in {city} is sunny, 22°C.";
    }
}

async Task RunApiWrapperAsync()
{
    var builder = Kernel.CreateBuilder();
    builder.AddOpenAIChatCompletion("gpt-4o", "api-key-here");
    
    // Import the plugin
    builder.Plugins.AddFromType<WeatherPlugin>("WeatherPlugin");
    
    var kernel = builder.Build();

    var agent = new ChatCompletionAgent(
        kernel: kernel,
        instructions: "You are a helpful assistant. Use the weather tool if the user asks about weather.",
        executionSettings: new OpenAIPromptExecutionSettings 
        { 
            ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions 
        }
    );

    // User queries
    string[] queries = { "What is the weather in London?", "What about Tokyo?" };
    
    foreach(var query in queries)
    {
        Console.WriteLine($"\nUser: {query}");
        var response = await agent.InvokeAsync(query);
        Console.WriteLine($"Agent: {response}");
    }
}
