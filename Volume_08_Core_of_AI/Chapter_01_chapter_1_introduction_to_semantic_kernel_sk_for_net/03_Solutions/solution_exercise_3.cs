
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

# Source File: solution_exercise_3.cs
# Description: Solution for Exercise 3
# ==========================================

// File: Program.cs
using Microsoft.SemanticKernel;

// 1. Ensure the directory structure exists:
//    /Plugins/StoryTeller/skprompt.txt
//    /Plugins/StoryTeller/config.json

var kernel = Kernel.CreateBuilder()
    .AddAzureOpenAIChatCompletion("deployment-name", "https://endpoint", "key") // Placeholder
    .Build();

// 2. Load the semantic function from the directory
try
{
    var storyPlugin = kernel.Plugins.AddFromPromptDirectory("Plugins/StoryTeller");
    var storyFunction = storyPlugin["StoryTeller"]; // Assuming function name matches directory or config

    // 3. Prepare arguments
    var arguments = new KernelArguments
    {
        ["genre"] = "Science Fiction",
        ["keyword"] = "Time Travel",
        // Interactive Challenge: Optional tone parameter
        ["tone"] = "Dark and suspenseful" 
    };

    // 4. Invoke the function
    var result = await kernel.InvokeAsync(storyFunction, arguments);

    Console.WriteLine("Generated Story Outline:");
    Console.WriteLine(result.ToString());
}
catch (Exception ex)
{
    Console.WriteLine($"Error invoking semantic function: {ex.Message}");
}
