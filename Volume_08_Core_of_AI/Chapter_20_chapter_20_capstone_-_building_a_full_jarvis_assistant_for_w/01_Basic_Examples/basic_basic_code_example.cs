
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
using Microsoft.SemanticKernel.Memory;
using Microsoft.SemanticKernel.Connectors.Memory.Sqlite;
using System.Text;
using System.Text.Json;

namespace JarvisMini;

public class Program
{
    // Entry point for the console application
    public static async Task Main(string[] args)
    {
        // 1. Setup: Initialize the Kernel with essential services
        var kernel = new KernelBuilder()
            // Using OpenAI GPT-3.5 Turbo for text generation (requires environment variable OPENAI_API_KEY)
            .WithOpenAIChatCompletionService("gpt-3.5-turbo", Environment.GetEnvironmentVariable("OPENAI_API_KEY")!)
            // Using a local SQLite database for persistent memory storage
            .WithMemoryStorage(new SqliteMemoryStorage("jarvis_memory.db"))
            .Build();

        // 2. Context: Load or create persistent user preferences
        // We simulate a "real-world" scenario where the assistant remembers user preferences.
        // We use a specific collection name to organize our data.
        const string memoryCollection = "UserPreferences";
        
        // Retrieve the user's preferred greeting style
        var greetingPreference = await kernel.Memory.GetAsync(memoryCollection, "GreetingStyle");
        
        string greetingStyle;
        if (greetingPreference == null)
        {
            // First run: Default to formal, and save it for next time
            greetingStyle = "formal";
            await kernel.Memory.SaveInformationAsync(memoryCollection, "formal", "GreetingStyle");
        }
        else
        {
            greetingStyle = greetingPreference.Metadata.Text;
        }

        // 3. Agentic Workflow: Define a simple plan for the assistant
        // In a full Jarvis system, the Planner would break down complex requests.
        // Here, we simulate a plan to handle a user request to "Summarize my day".
        var userRequest = "Summarize my day based on the meeting notes I have in memory.";

        // 4. Execution: Orchestrating the Kernel to process the request
        Console.WriteLine($"[System]: Assistant initialized. Memory loaded. Style: {greetingStyle}");
        Console.WriteLine($"[User]: {userRequest}");
        
        // Create a prompt template that incorporates the retrieved memory context
        // This demonstrates "Stateful Memory & Personalization"
        var promptTemplate = $"""
            You are a helpful desktop assistant.
            The user prefers a {{$style}} greeting style.
            The user request is: "{{$input}}".
            
            Please respond to the user request.
            If the request mentions memory or notes, acknowledge that you are retrieving context.
            Keep the response concise and in the style defined by the greeting preference.
            """;

        // Execute the function
        var result = await kernel.RunAsync(
            promptTemplate,
            new KernelArguments(new KernelPromptTemplateConfig())
            {
                ["input"] = userRequest,
                ["style"] = greetingStyle
            }
        );

        // 5. Output: Displaying the result
        Console.WriteLine($"\n[Jarvis]: {result.Result}");
        
        // 6. Demonstration of Plugin Integration (Simulated)
        // In the full capstone, we would invoke native C# plugins here.
        // For this example, we simulate a "SystemNotification" plugin call.
        await SimulateSystemNotification(kernel, "Jarvis Assistant", "Task completed successfully.");
    }

    // Simulates a native Windows system plugin (e.g., Toast Notification)
    private static async Task SimulateSystemNotification(IKernel kernel, string title, string message)
    {
        // In a real scenario, this would be a native C# method registered as a skill.
        // We are using a local function here to keep the example self-contained.
        Console.WriteLine($"\n[System Plugin]: Triggering notification -> Title: {title}, Message: {message}");
        await Task.CompletedTask; // Simulating async IO
    }
}
