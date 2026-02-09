
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

# Source File: solution_exercise_24.cs
# Description: Solution for Exercise 24
# ==========================================

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Text;

async Task RunMultiModalAgentAsync()
{
    var builder = Kernel.CreateBuilder();
    builder.AddOpenAIChatCompletion("gpt-4o", "api-key-here");
    var kernel = builder.Build();

    var artCritic = new ChatCompletionAgent(kernel, "You are an art critic. Analyze the attached image. Describe style, colors, and mood.");

    // Simulate Image Input
    // In a real scenario, you would load the file bytes and create an ImageContent object.
    // For this text-based environment, we will simulate the LLM's capability to read an image description.
    
    string imagePath = "sample_art.jpg";
    // string base64Image = Convert.ToBase64String(File.ReadAllBytes(imagePath));
    
    Console.WriteLine($"Analyzing image: {imagePath}");

    // Constructing the prompt with image context (Simulated)
    // Real SK usage: chatHistory.AddUserMessage(new TextContent("Analyze this"), new ImageContent(base64Image));
    var imageDescription = "[Image Data: A surrealist painting with melting clocks in a desert landscape]";
    var prompt = $"Analyze this image: {imageDescription}";

    var analysis = await artCritic.InvokeAsync(prompt);
    Console.WriteLine($"Art Critic Analysis: {analysis}");
}
